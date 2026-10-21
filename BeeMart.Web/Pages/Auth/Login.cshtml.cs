using System.Security.Claims;
using BeeMart.Web.Models.Auth;
using BeeMart.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Auth;

/// <summary>
/// PageModel for <c>/Auth/Login</c> (<c>FEAT-AUTH-01</c>).
/// Authenticates the user via <c>POST /api/v1/auth/login</c> and issues a Cookie Authentication ticket containing the JWT token.
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IApiClient _apiClient;

    public LoginModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAfterLogin(returnUrl);
        }

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Input.Username) || string.IsNullOrWhiteSpace(Input.Password))
        {
            ErrorMessage = "Username dan Password wajib diisi.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Mohon lengkapi seluruh field login dengan benar.";
            return Page();
        }

        var loginRequest = new LoginRequest
        {
            Username = Input.Username.Trim(),
            Password = Input.Password,
            Role = Input.Role.Trim()
        };

        var apiResponse = await _apiClient.PostAsync<LoginRequest, LoginResponse>(
            "auth/login",
            loginRequest,
            cancellationToken);

        if (!apiResponse.IsSuccess || apiResponse.Data is null)
        {
            ErrorMessage = !string.IsNullOrWhiteSpace(apiResponse.Message)
                ? apiResponse.Message
                : "Invalid login";
            return Page();
        }

        var user = apiResponse.Data;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.IdUser),
            new("idUser", user.IdUser),
            new(ClaimTypes.Name, user.Username),
            new("username", user.Username),
            new("fullName", user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("role", user.Role),
            new(ApiClient.JwtClaimType, user.Token)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectAfterLogin(ReturnUrl);
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/Index");
    }
}
