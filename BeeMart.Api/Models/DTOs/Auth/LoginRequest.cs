using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Auth;

/// <summary>
/// Request payload for <c>POST /api/v1/auth/login</c> (<c>API-AUTH-01</c>).
/// </summary>
public class LoginRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
