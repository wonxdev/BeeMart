using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Transaction;

/// <summary>
/// Request ViewModel for submitting a new POS transaction to <c>POST /api/v1/transactions</c>.
/// </summary>
public class CheckoutRequestViewModel
{
    [Required(ErrorMessage = "Metode pembayaran wajib dipilih.")]
    [JsonPropertyName("idPaymentMethod")]
    public string IdPaymentMethod { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Uang diterima tidak valid.")]
    [JsonPropertyName("customerMoney")]
    public decimal CustomerMoney { get; set; }

    [Required(ErrorMessage = "Keranjang belanja tidak boleh kosong.")]
    [MinLength(1, ErrorMessage = "Keranjang belanja harus memiliki minimal 1 barang.")]
    [JsonPropertyName("items")]
    public List<CheckoutItemRequestViewModel> Items { get; set; } = new();
}

/// <summary>
/// Line-item payload inside <see cref="CheckoutRequestViewModel"/>.
/// </summary>
public class CheckoutItemRequestViewModel
{
    [Required]
    [JsonPropertyName("idProduct")]
    public string IdProduct { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Jumlah barang minimal 1.")]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
