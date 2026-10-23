using System.Security.Claims;

namespace BeeMart.Api.Common;

/// <summary>
/// Extension methods on <see cref="ClaimsPrincipal"/> to extract BeeMart user context claims.
/// </summary>
public static class ClaimsHelper
{
    public const string IdUserClaimType = "idUser";
    public const string UsernameClaimType = "username";
    public const string FullNameClaimType = "fullName";
    public const string RoleClaimType = "role";

    public const string AdminRole = "Admin";
    public const string CashierRole = "Cashier";

    public static string GetUserId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return string.Empty;
        }

        return principal.FindFirstValue(IdUserClaimType)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;
    }

    public static string GetUsername(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return string.Empty;
        }

        return principal.FindFirstValue(UsernameClaimType)
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;
    }

    public static string GetFullName(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return string.Empty;
        }

        return principal.FindFirstValue(FullNameClaimType) ?? string.Empty;
    }

    public static string GetRole(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return string.Empty;
        }

        return principal.FindFirstValue(RoleClaimType)
            ?? principal.FindFirstValue(ClaimTypes.Role)
            ?? string.Empty;
    }

    public static bool IsAdmin(this ClaimsPrincipal? principal) =>
        string.Equals(principal.GetRole(), AdminRole, StringComparison.Ordinal);

    public static bool IsCashier(this ClaimsPrincipal? principal) =>
        string.Equals(principal.GetRole(), CashierRole, StringComparison.Ordinal);
}
