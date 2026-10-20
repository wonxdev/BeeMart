using System.Text.Json.Serialization;

namespace BeeMart.Api.Models.DTOs.Auth;

/// <summary>
/// Request payload for <c>POST /api/v1/auth/register</c> (<c>API-AUTH-02</c>).
/// </summary>
public class RegisterRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("rePassword")]
    public string RePassword { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
