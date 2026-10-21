using BeeMart.Web.Models.Auth;
using BeeMart.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Auth;

/// <summary>
/// PageModel for <c>/Auth/Register</c> (<c>FEAT-AUTH-02</c>).
/// Registers a new user account via <c>POST /api/v1/auth/register</c> and redirects to <c>/Auth/Login</c> on success.
/// </summary>
[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IApiClient _apiClient;

    public RegisterModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Input.Username)
            || string.IsNullOrWhiteSpace(Input.FullName)
            || string.IsNullOrWhiteSpace(Input.Password)
            || string.IsNullOrWhiteSpace(Input.RePassword)
            || string.IsNullOrWhiteSpace(Input.Role))
        {
            ErrorMessage = "Username, Full Name, Password, Re-Password, dan Role wajib diisi.";
            return Page();
        }

        if (!string.Equals(Input.Password, Input.RePassword, StringComparison.Ordinal))
        {
            ErrorMessage = "Password and confirmation password do not match";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            var firstModelError = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

            ErrorMessage = firstModelError ?? "Mohon periksa kembali data registrasi Anda.";
            return Page();
        }

        var registerRequest = new RegisterRequest
        {
            Username = Input.Username.Trim(),
            FullName = Input.FullName.Trim(),
            Password = Input.Password,
            RePassword = Input.RePassword,
            Role = Input.Role.Trim()
        };

        var apiResponse = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>(
            "auth/register",
            registerRequest,
            cancellationToken);

        if (!apiResponse.IsSuccess)
        {
            ErrorMessage = !string.IsNullOrWhiteSpace(apiResponse.Message)
                ? apiResponse.Message
                : "Gagal mendaftarkan akun.";
            return Page();
        }

        TempData["SuccessMessage"] = "Account registered successfully. Please login.";
        return RedirectToPage("/Auth/Login");
    }
}
