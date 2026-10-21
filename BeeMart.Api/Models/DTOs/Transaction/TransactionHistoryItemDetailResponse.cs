namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Line-item detail in a transaction history item (<c>GET /api/v1/transactions/history</c>).
/// </summary>
public class TransactionHistoryItemDetailResponse
{
    public string IdProduct { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal { get; set; }
}
