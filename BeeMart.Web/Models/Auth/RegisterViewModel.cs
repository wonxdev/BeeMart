using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeeMart.Web.Models.Auth;

/// <summary>
/// ViewModel for the Register Razor Page (<c>/Auth/Register</c>, <c>FEAT-AUTH-02</c>).
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(50, ErrorMessage = "Username maksimal 50 karakter.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name wajib diisi.")]
    [StringLength(300, ErrorMessage = "Full Name maksimal 300 karakter.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Re-Password wajib diisi.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match")]
    [Display(Name = "Re-Password")]
    public string RePassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role wajib dipilih.")]
    [Display(Name = "Role")]
    public string Role { get; set; } = "Cashier";
}

/// <summary>
/// Request payload sent by <c>BeeMart.Web</c> to <c>POST /api/v1/auth/register</c> (<c>API-AUTH-02</c>).
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

/// <summary>
/// Response payload returned by <c>POST /api/v1/auth/register</c> (<c>API-AUTH-02</c>).
/// </summary>
public class RegisterResponse
{
    [JsonPropertyName("idUser")]
    public string IdUser { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
