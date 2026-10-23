using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.Models.DTOs.Transaction;
using BeeMart.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

/// <summary>
/// Transaction endpoints (<c>GET /api/v1/transactions/next-invoice</c>, <c>POST /api/v1/transactions</c>, and <c>GET /api/v1/transactions/history</c>).
/// </summary>
[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly BeeMartDbContext _dbContext;

    public TransactionsController(BeeMartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Generates the next sequential invoice number for the current date.
    /// Format: <c>INV-YYYYMMDD-XXXX</c> (e.g., <c>INV-20261020-0001</c>).
    /// Accessible by authenticated <c>Cashier</c> and <c>Admin</c> users.
    /// </summary>
    [HttpGet("next-invoice")]
    public async Task<IActionResult> GetNextInvoice()
    {
        var invoiceNo = await GenerateNextInvoiceNoAsync(DateTime.Now);

        var responseData = new NextInvoiceResponse
        {
            InvoiceNo = invoiceNo
        };

        return Ok(ApiResponse<NextInvoiceResponse>.Success(responseData));
    }

    /// <summary>
    /// Validates and creates a new POS transaction atomically.
    /// Accessible by authenticated <c>Cashier</c> and <c>Admin</c> users.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest? request)
    {
        var userId = User.GetUserId();
        var username = User.GetUsername();
        var fullName = User.GetFullName();

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized(ApiResponse.Error("Unauthorized user context"));
        }

        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(ApiResponse.Error("Transaction must contain at least one item"));
        }

        foreach (var item in request.Items)
        {
            if (item is null || string.IsNullOrWhiteSpace(item.IdProduct))
            {
                return BadRequest(ApiResponse.Error("Product ID is required for each transaction item"));
            }

            if (item.Quantity <= 0)
            {
                return BadRequest(ApiResponse.Error("Item quantity must be greater than zero"));
            }
        }

        if (string.IsNullOrWhiteSpace(request.IdPaymentMethod)
            || !int.TryParse(request.IdPaymentMethod.Trim(), out var paymentMethodId))
        {
            return BadRequest(ApiResponse.Error("Invalid payment method"));
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        var paymentMethod = await _dbContext.PaymentMethods
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.IdPaymentMethod == paymentMethodId && pm.Stsrc);

        if (paymentMethod is null)
        {
            return BadRequest(ApiResponse.Error("Invalid payment method"));
        }

        var requestedProductIds = request.Items
            .Select(i => i.IdProduct.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var products = await _dbContext.Products
            .Where(p => p.Stsrc && requestedProductIds.Contains(p.IdProduct))
            .ToListAsync();

        var productMap = products.ToDictionary(p => p.IdProduct, StringComparer.OrdinalIgnoreCase);

        foreach (var item in request.Items)
        {
            var trimmedProductId = item.IdProduct.Trim();
            if (!productMap.ContainsKey(trimmedProductId))
            {
                return BadRequest(ApiResponse.Error($"Invalid product: {trimmedProductId}"));
            }
        }

        var now = DateTime.Now;
        var totalItem = 0;
        var totalPrice = 0m;
        var detailResponses = new List<TransactionDetailResponse>(request.Items.Count);
        var detailEntities = new List<TrTransactionDetail>(request.Items.Count);
        var transactionId = Guid.NewGuid().ToString();

        foreach (var item in request.Items)
        {
            var product = productMap[item.IdProduct.Trim()];

            if (product.Stock < item.Quantity)
            {
                return BadRequest(ApiResponse.Error($"Insufficient stock for product: {product.Name}"));
            }

            product.Stock -= item.Quantity;
            product.Dateup = now;
            product.Userup = username;

            var subtotal = product.Price * item.Quantity;
            totalItem += item.Quantity;
            totalPrice += subtotal;

            detailEntities.Add(new TrTransactionDetail
            {
                IdTransactionDetail = Guid.NewGuid().ToString(),
                IdTransaction = transactionId,
                IdProduct = product.IdProduct,
                Qty = item.Quantity,
                Price = product.Price,
                Discount = 0m,
                SubTotal = subtotal,
                Stsrc = true,
                DateIn = now,
                UserIn = username,
                Dateup = null,
                Userup = null
            });

            detailResponses.Add(new TransactionDetailResponse
            {
                IdProduct = product.IdProduct,
                ProductName = product.Name,
                Quantity = item.Quantity,
                Price = product.Price,
                Subtotal = subtotal
            });
        }

        if (request.CustomerMoney < totalPrice)
        {
            return BadRequest(ApiResponse.Error("Customer money is insufficient"));
        }

        var change = request.CustomerMoney - totalPrice;
        var invoiceNo = await GenerateNextInvoiceNoAsync(now);

        var transactionEntity = new TrTransaction
        {
            IdTransaction = transactionId,
            InvoiceNo = invoiceNo,
            TransactionDate = now,
            IdUser = userId,
            IdPaymentMethod = paymentMethod.IdPaymentMethod,
            TotalItem = totalItem,
            TotalPrice = totalPrice,
            Stsrc = true,
            DateIn = now,
            UserIn = username,
            Dateup = null,
            Userup = null
        };

        _dbContext.Transactions.Add(transactionEntity);
        _dbContext.TransactionDetails.AddRange(detailEntities);

        await _dbContext.SaveChangesAsync();
        await dbTransaction.CommitAsync();

        var cashierName = !string.IsNullOrWhiteSpace(fullName) ? fullName : username;

        var responseData = new CreateTransactionResponse
        {
            InvoiceNo = invoiceNo,
            TransactionDate = now,
            CashierId = userId,
            CashierName = cashierName,
            PaymentMethodName = paymentMethod.MethodName,
            TotalItem = totalItem,
            TotalPrice = totalPrice,
            CustomerMoney = request.CustomerMoney,
            Change = change,
            Details = detailResponses
        };

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CreateTransactionResponse>.Success(responseData, "Transaction created successfully"));
    }

    /// <summary>
    /// Retrieves paginated transaction history with role-based scoping.
    /// <c>Cashier</c> users can only view their own transactions; <c>Admin</c> users can view all transactions and optionally filter by <c>cashierId</c>.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? cashierId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();
        var isCashier = User.IsCashier();

        if (string.IsNullOrWhiteSpace(userId) || (!isAdmin && !isCashier))
        {
            return Unauthorized(ApiResponse.Error("Unauthorized user context"));
        }

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 1;
        }
        else if (pageSize > 50)
        {
            pageSize = 50;
        }

        var query = _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Stsrc);

        // Cashiers are scoped to their own IdUser; Admins can view all transactions or filter by cashierId.
        if (isCashier)
        {
            query = query.Where(t => t.IdUser == userId);
        }
        else if (isAdmin && !string.IsNullOrWhiteSpace(cashierId))
        {
            var trimmedCashierId = cashierId.Trim();
            query = query.Where(t => t.IdUser == trimmedCashierId);
        }

        if (startDate.HasValue)
        {
            var effectiveStartDate = startDate.Value.TimeOfDay == TimeSpan.Zero
                ? startDate.Value.Date
                : startDate.Value;
            query = query.Where(t => t.TransactionDate >= effectiveStartDate);
        }

        if (endDate.HasValue)
        {
            var effectiveEndDate = endDate.Value.TimeOfDay == TimeSpan.Zero
                ? endDate.Value.Date.AddDays(1).AddTicks(-1)
                : endDate.Value;
            query = query.Where(t => t.TransactionDate <= effectiveEndDate);
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling((double)totalItems / pageSize);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.InvoiceNo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.User)
            .Include(t => t.PaymentMethod)
            .Include(t => t.TransactionDetails.Where(d => d.Stsrc))
                .ThenInclude(d => d.Product)
            .ToListAsync();

        var items = transactions.Select(t =>
        {
            var details = t.TransactionDetails
                .Where(d => d.Stsrc)
                .Select(d => new TransactionHistoryItemDetailResponse
                {
                    IdProduct = d.IdProduct,
                    ProductName = d.Product?.Name ?? string.Empty,
                    Quantity = d.Qty,
                    Price = d.Price,
                    Subtotal = d.SubTotal
                })
                .ToList();

            return new TransactionHistoryItemResponse
            {
                IdTransaction = t.IdTransaction,
                InvoiceNo = t.InvoiceNo,
                TransactionDate = t.TransactionDate,
                CashierId = t.IdUser,
                CashierName = !string.IsNullOrWhiteSpace(t.User?.FullName)
                    ? t.User.FullName
                    : (t.User?.Username ?? string.Empty),
                PaymentMethodName = t.PaymentMethod?.MethodName ?? string.Empty,
                TotalItem = details.Sum(d => d.Quantity),
                TotalPrice = t.TotalPrice,
                Details = details
            };
        }).ToList();

        var responseData = new TransactionHistoryResponse
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };

        return Ok(ApiResponse<TransactionHistoryResponse>.Success(responseData));
    }

    /// <summary>
    /// Generates the next sequential invoice number for the specified date using format <c>INV-YYYYMMDD-XXXX</c>.
    /// </summary>
    private async Task<string> GenerateNextInvoiceNoAsync(DateTime date)
    {
        var datePrefix = $"INV-{date:yyyyMMdd}-";

        var existingInvoicesToday = await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.InvoiceNo.StartsWith(datePrefix))
            .Select(t => t.InvoiceNo)
            .ToListAsync();

        var maxSequence = 0;
        foreach (var invoice in existingInvoicesToday)
        {
            if (invoice.Length > datePrefix.Length)
            {
                var suffix = invoice.Substring(datePrefix.Length);
                if (int.TryParse(suffix, out var sequenceNumber) && sequenceNumber > maxSequence)
                {
                    maxSequence = sequenceNumber;
                }
            }
        }

        var nextSequence = maxSequence + 1;
        return $"{datePrefix}{nextSequence:D4}";
    }
}
