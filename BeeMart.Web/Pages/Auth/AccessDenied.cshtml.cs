using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeeMart.Web.Pages.Auth;

/// <summary>
/// PageModel for <c>/Auth/AccessDenied</c>.
/// Displays a friendly permission error message when a user lacks sufficient role privileges.
/// </summary>
[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
