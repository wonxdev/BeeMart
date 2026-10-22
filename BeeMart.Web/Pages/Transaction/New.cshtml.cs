using System.Security.Claims;
using BeeMart.Web.Models.Transaction;
using BeeMart.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Transaction;

/// <summary>
/// PageModel for the Cashier Point of Sale (POS) interface (<c>/Transaction/New</c>, <c>FEAT-TRX-01</c>).
/// Strictly restricted to users in the <c>Cashier</c> role.
/// All data operations flow exclusively through <see cref="IApiClient"/> (<c>NFR-ARCH-001</c>, <c>NFR-ARCH-002</c>).
/// </summary>
[Authorize(Roles = "Cashier")]
public class NewModel : PageModel
{
    private readonly IApiClient _apiClient;

    public NewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string CashierId { get; private set; } = string.Empty;

    public string CashierUsername { get; private set; } = string.Empty;

    public string CashierFullName { get; private set; } = string.Empty;

    public string NextInvoiceNo { get; private set; } = "-";

    public List<PaymentMethodResponse> PaymentMethods { get; private set; } = new();

    public ProductSearchResponse ProductCatalog { get; private set; } = new();

    [BindProperty(SupportsGet = true, Name = "search")]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true, Name = "page")]
    public int CurrentPage { get; set; } = 1;

    [BindProperty(SupportsGet = true, Name = "pageSize")]
    public int PageSize { get; set; } = 10;

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        PopulateCashierIdentity();

        if (CurrentPage < 1)
        {
            CurrentPage = 1;
        }

        if (PageSize < 1)
        {
            PageSize = 10;
        }
        else if (PageSize > 50)
        {
            PageSize = 50;
        }

        var nextInvoiceTask = _apiClient.GetAsync<NextInvoiceResponse>("transactions/next-invoice", cancellationToken);
        var paymentMethodsTask = _apiClient.GetAsync<List<PaymentMethodResponse>>("payment-methods", cancellationToken);

        var searchQuery = BuildSearchEndpoint(SearchTerm, CurrentPage, PageSize);
        var productsTask = _apiClient.GetAsync<ProductSearchResponse>(searchQuery, cancellationToken);

        await Task.WhenAll(nextInvoiceTask, paymentMethodsTask, productsTask);

        var invoiceResponse = await nextInvoiceTask;
        if (invoiceResponse.IsSuccess && invoiceResponse.Data is not null)
        {
            NextInvoiceNo = invoiceResponse.Data.InvoiceNo;
        }
        else if (!string.IsNullOrWhiteSpace(invoiceResponse.Message))
        {
            ErrorMessage = invoiceResponse.Message;
        }

        var paymentMethodsResponse = await paymentMethodsTask;
        if (paymentMethodsResponse.IsSuccess && paymentMethodsResponse.Data is not null)
        {
            PaymentMethods = paymentMethodsResponse.Data;
        }
        else if (string.IsNullOrWhiteSpace(ErrorMessage) && !string.IsNullOrWhiteSpace(paymentMethodsResponse.Message))
        {
            ErrorMessage = paymentMethodsResponse.Message;
        }

        var productsResponse = await productsTask;
        if (productsResponse.IsSuccess && productsResponse.Data is not null)
        {
            ProductCatalog = productsResponse.Data;
        }
        else if (string.IsNullOrWhiteSpace(ErrorMessage) && !string.IsNullOrWhiteSpace(productsResponse.Message))
        {
            ErrorMessage = productsResponse.Message;
        }

        return Page();
    }

    /// <summary>
    /// AJAX GET handler (<c>/Transaction/New?handler=SearchProducts</c>) that proxies product search to <c>GET /api/v1/products/search</c>.
    /// </summary>
    public async Task<IActionResult> OnGetSearchProductsAsync(
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }
        else if (pageSize > 50)
        {
            pageSize = 50;
        }

        var endpoint = BuildSearchEndpoint(search, page, pageSize);
        var apiResponse = await _apiClient.GetAsync<ProductSearchResponse>(endpoint, cancellationToken);

        return new JsonResult(apiResponse)
        {
            StatusCode = apiResponse.StatusCode > 0 ? apiResponse.StatusCode : 200
        };
    }

    /// <summary>
    /// AJAX GET handler (<c>/Transaction/New?handler=NextInvoice</c>) that proxies <c>GET /api/v1/transactions/next-invoice</c>.
    /// </summary>
    public async Task<IActionResult> OnGetNextInvoiceAsync(CancellationToken cancellationToken = default)
    {
        var apiResponse = await _apiClient.GetAsync<NextInvoiceResponse>("transactions/next-invoice", cancellationToken);

        return new JsonResult(apiResponse)
        {
            StatusCode = apiResponse.StatusCode > 0 ? apiResponse.StatusCode : 200
        };
    }

    /// <summary>
    /// AJAX POST handler (<c>/Transaction/New?handler=Checkout</c>) that submits the transaction payload to <c>POST /api/v1/transactions</c>.
    /// </summary>
    public async Task<IActionResult> OnPostCheckoutAsync(
        [FromBody] CheckoutRequestViewModel? request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            var emptyCartError = ApiResponse<CreateTransactionResponse>.Error(
                "Transaction must contain at least one item",
                StatusCodes.Status400BadRequest);
            return new JsonResult(emptyCartError) { StatusCode = StatusCodes.Status400BadRequest };
        }

        if (string.IsNullOrWhiteSpace(request.IdPaymentMethod))
        {
            var paymentError = ApiResponse<CreateTransactionResponse>.Error(
                "Invalid payment method",
                StatusCodes.Status400BadRequest);
            return new JsonResult(paymentError) { StatusCode = StatusCodes.Status400BadRequest };
        }

        var apiResponse = await _apiClient.PostAsync<CheckoutRequestViewModel, CreateTransactionResponse>(
            "transactions",
            request,
            cancellationToken);

        var statusCode = apiResponse.StatusCode > 0
            ? apiResponse.StatusCode
            : (apiResponse.IsSuccess ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest);

        return new JsonResult(apiResponse)
        {
            StatusCode = statusCode
        };
    }

    private void PopulateCashierIdentity()
    {
        CashierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("idUser")?.Value
                    ?? string.Empty;

        CashierUsername = User.Identity?.Name
                          ?? User.FindFirst("username")?.Value
                          ?? "Cashier";

        CashierFullName = User.FindFirst("fullName")?.Value
                          ?? CashierUsername;
    }

    private static string BuildSearchEndpoint(string? search, int page, int pageSize)
    {
        var query = $"products/search?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            query += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        return query;
    }
}
