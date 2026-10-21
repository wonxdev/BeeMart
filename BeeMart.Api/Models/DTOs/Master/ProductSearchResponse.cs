using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Master;

/// <summary>
/// Paginated response DTO for POS product search (<c>GET /api/v1/products/search</c>).
/// </summary>
public class ProductSearchResponse
{
    [JsonPropertyName("items")]
    public List<ProductSearchItemResponse> Items { get; set; } = new();

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
