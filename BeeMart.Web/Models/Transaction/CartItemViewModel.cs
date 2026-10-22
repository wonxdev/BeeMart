using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Transaction;

/// <summary>
/// Represents an item in the Cashier's POS shopping cart (<c>Daftar Belanjaan</c>).
/// </summary>
public class CartItemViewModel
{
    [JsonPropertyName("idProduct")]
    public string IdProduct { get; set; } = string.Empty;

    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("maxStock")]
    public int MaxStock { get; set; }

    [JsonPropertyName("subtotal")]
    public decimal Subtotal => Price * Quantity;
}
