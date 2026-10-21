namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Line item detail in a created transaction response (<c>POST /api/v1/transactions</c>).
/// </summary>
public class TransactionDetailResponse
{
    public string IdProduct { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public decimal Subtotal { get; set; }
}
