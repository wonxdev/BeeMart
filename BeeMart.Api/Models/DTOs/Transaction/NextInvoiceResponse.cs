namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Response payload for <c>GET /api/v1/transactions/next-invoice</c>.
/// </summary>
public class NextInvoiceResponse
{
    public string InvoiceNo { get; set; } = string.Empty;
}
