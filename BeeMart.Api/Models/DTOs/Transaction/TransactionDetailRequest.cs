namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Individual line item in a transaction creation request (<c>POST /api/v1/transactions</c>).
/// </summary>
public class TransactionDetailRequest
{
    public string IdProduct { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
