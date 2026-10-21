namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Single transaction item in a paginated transaction history response (<c>GET /api/v1/transactions/history</c>).
/// </summary>
public class TransactionHistoryItemResponse
{
    public string IdTransaction { get; set; } = string.Empty;

    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public string CashierId { get; set; } = string.Empty;

    public string CashierName { get; set; } = string.Empty;

    public string PaymentMethodName { get; set; } = string.Empty;

    public int TotalItem { get; set; }

    public decimal TotalPrice { get; set; }

    public List<TransactionHistoryItemDetailResponse> Details { get; set; } = new();
}
