namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Request payload for <c>POST /api/v1/transactions</c> (<c>API-TRX-02</c>).
/// </summary>
public class CreateTransactionRequest
{
    public string IdPaymentMethod { get; set; } = string.Empty;

    public decimal CustomerMoney { get; set; }

    public List<TransactionDetailRequest> Items { get; set; } = new();
}
