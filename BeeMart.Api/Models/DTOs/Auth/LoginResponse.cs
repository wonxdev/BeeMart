using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Auth;

/// <summary>
/// Response data payload for <c>POST /api/v1/auth/login</c> (<c>API-AUTH-01</c>).
/// </summary>
public class LoginResponse
{
    [JsonPropertyName("idUser")]
    public string IdUser { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
