using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Auth;

/// <summary>
/// ViewModel for the Login Razor Page (<c>/Auth/Login</c>, <c>FEAT-AUTH-01</c>).
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    [Display(Name = "Role")]
    public string Role { get; set; } = "Cashier";
}

/// <summary>
/// Request payload sent by <c>BeeMart.Web</c> to <c>POST /api/v1/auth/login</c> (<c>API-AUTH-01</c>).
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

/// <summary>
/// Response payload returned by <c>POST /api/v1/auth/login</c> (<c>API-AUTH-01</c>).
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
