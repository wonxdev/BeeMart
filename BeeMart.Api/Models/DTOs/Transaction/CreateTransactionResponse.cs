namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Response payload for <c>POST /api/v1/transactions</c>.
/// </summary>
public class CreateTransactionResponse
{
    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public string CashierId { get; set; } = string.Empty;

    public string CashierName { get; set; } = string.Empty;

    public string PaymentMethodName { get; set; } = string.Empty;

    public int TotalItem { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal CustomerMoney { get; set; }

    public decimal Change { get; set; }

    public List<TransactionDetailResponse> Details { get; set; } = new();
}
