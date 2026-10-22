using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Transaction;

/// <summary>
/// Line-item detail in a transaction history item returned by <c>GET /api/v1/transactions/history</c>.
/// </summary>
public class TransactionHistoryItemDetailViewModel
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
/// Single transaction header with line-item details returned by <c>GET /api/v1/transactions/history</c>.
/// </summary>
public class TransactionHistoryItemViewModel
{
    [JsonPropertyName("idTransaction")]
    public string IdTransaction { get; set; } = string.Empty;

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

    [JsonPropertyName("details")]
    public List<TransactionHistoryItemDetailViewModel> Details { get; set; } = new();
}

/// <summary>
/// Paginated API response payload from <c>GET /api/v1/transactions/history</c> (<c>API-HIST-01</c>).
/// </summary>
public class TransactionHistoryResponse
{
    [JsonPropertyName("items")]
    public List<TransactionHistoryItemViewModel> Items { get; set; } = new();

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
/// Represents an active cashier option in the Admin cashier filter dropdown.
/// </summary>
public class CashierFilterOptionViewModel
{
    public string CashierId { get; set; } = string.Empty;

    public string CashierName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(Username)
            ? $"{CashierName} ({CashierId})"
            : $"{CashierName} ({Username} - {CashierId})";
}

/// <summary>
/// ViewModel for the Transaction History page (<c>/Transaction/History</c>, <c>FEAT-HIST-01</c> &amp; <c>FEAT-HIST-02</c>).
/// Holds filter parameters, paginated transaction items with line-item details, and Admin cashier filter options.
/// </summary>
public class TransactionHistoryViewModel
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? CashierId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public List<TransactionHistoryItemViewModel> Items { get; set; } = new();

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public int CurrentPage { get; set; } = 1;

    public List<CashierFilterOptionViewModel> CashierOptions { get; set; } = new();
}
