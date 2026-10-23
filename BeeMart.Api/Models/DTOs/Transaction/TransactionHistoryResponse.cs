namespace BeeMart.Api.Models.DTOs.Transaction;

/// <summary>
/// Paginated response payload for <c>GET /api/v1/transactions/history</c>.
/// </summary>
public class TransactionHistoryResponse
{
    public List<TransactionHistoryItemResponse> Items { get; set; } = new();

    public int TotalItems { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}
