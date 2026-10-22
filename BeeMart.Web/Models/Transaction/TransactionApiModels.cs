using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Transaction;

/// <summary>
/// Response DTO from <c>GET /api/v1/transactions/next-invoice</c>.
/// </summary>
public class NextInvoiceResponse
{
    [JsonPropertyName("invoiceNo")]
    public string InvoiceNo { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO from <c>GET /api/v1/payment-methods</c>.
/// </summary>
public class PaymentMethodResponse
{
    [JsonPropertyName("idPaymentMethod")]
    public string IdPaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("paymentMethodName")]
    public string PaymentMethodName { get; set; } = string.Empty;
}

/// <summary>
/// Single product item returned by <c>GET /api/v1/products/search</c>.
/// </summary>
public class ProductSearchItemResponse
{
    [JsonPropertyName("idProduct")]
    public string IdProduct { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("productImage")]
    public string? ProductImage { get; set; }
}

/// <summary>
/// Paginated product search response from <c>GET /api/v1/products/search</c>.
/// </summary>
public class ProductSearchResponse
{
    [JsonPropertyName("items")]
    public List<ProductSearchItemResponse> Items { get; set; } = new();

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 10;

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

/// <summary>
/// Line-item detail returned in <see cref="CreateTransactionResponse"/>.
/// </summary>
public class TransactionDetailResponse
{
    [JsonPropertyName("idProduct")]
    public string IdProduct { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Receipt payload returned by <c>POST /api/v1/transactions</c> (<c>HTTP 201 Created</c>).
/// </summary>
public class CreateTransactionResponse
{
    [JsonPropertyName("invoiceNo")]
    public string InvoiceNo { get; set; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTime TransactionDate { get; set; }

    [JsonPropertyName("cashierId")]
    public string CashierId { get; set; } = string.Empty;

    [JsonPropertyName("cashierName")]
    public string CashierName { get; set; } = string.Empty;

    [JsonPropertyName("paymentMethodName")]
    public string PaymentMethodName { get; set; } = string.Empty;

    [JsonPropertyName("totalItem")]
    public int TotalItem { get; set; }

    [JsonPropertyName("totalPrice")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("customerMoney")]
    public decimal CustomerMoney { get; set; }

    [JsonPropertyName("change")]
    public decimal Change { get; set; }

    [JsonPropertyName("details")]
    public List<TransactionDetailResponse> Details { get; set; } = new();
}
