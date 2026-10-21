using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Master;

/// <summary>
/// Response DTO representing a single product item in POS search results (<c>GET /api/v1/products/search</c>).
/// </summary>
public class ProductSearchItemResponse
{
    [JsonPropertyName("idProduct")]
    public string IdProduct { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("stock")]
    public int Stock { get; set; }

    [JsonPropertyName("productImage")]
    public string? ProductImage { get; set; }
}
