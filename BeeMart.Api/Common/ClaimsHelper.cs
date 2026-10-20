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

    /// <summary>
    /// Returns the 5-digit <c>IdUser</c> string from the authenticated principal.
    /// </summary>
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

    /// <summary>
    /// Returns the <c>Username</c> string from the authenticated principal.
    /// </summary>
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

    /// <summary>
    /// Returns the <c>FullName</c> string from the authenticated principal.
    /// </summary>
    public static string GetFullName(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return string.Empty;
        }

        return principal.FindFirstValue(FullNameClaimType) ?? string.Empty;
    }

    /// <summary>
    /// Returns the <c>Role</c> string (<c>"Admin"</c> or <c>"Cashier"</c>) from the authenticated principal.
    /// </summary>
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

    /// <summary>
    /// Returns <c>true</c> if the authenticated user has the <c>Admin</c> role.
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal? principal)
    {
        return string.Equals(principal.GetRole(), AdminRole, StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns <c>true</c> if the authenticated user has the <c>Cashier</c> role.
    /// </summary>
    public static bool IsCashier(this ClaimsPrincipal? principal)
    {
        return string.Equals(principal.GetRole(), CashierRole, StringComparison.Ordinal);
    }
}
