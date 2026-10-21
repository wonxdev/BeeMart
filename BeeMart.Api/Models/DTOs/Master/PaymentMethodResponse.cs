using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Master;

/// <summary>
/// Response DTO representing an active payment method (<c>GET /api/v1/payment-methods</c>).
/// </summary>
public class PaymentMethodResponse
{
    [JsonPropertyName("idPaymentMethod")]
    public string IdPaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("paymentMethodName")]
    public string PaymentMethodName { get; set; } = string.Empty;
}
