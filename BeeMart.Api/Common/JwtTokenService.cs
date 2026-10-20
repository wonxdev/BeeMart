using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeeMart.Api.Models.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BeeMart.Api.Common;

/// <summary>
/// Service contract for generating JWT bearer tokens containing BeeMart user claims.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT token for the specified <see cref="MsUser"/> entity.
    /// </summary>
    string GenerateToken(MsUser user);

    /// <summary>
    /// Generates a signed JWT token for the specified user attributes.
    /// </summary>
    string GenerateToken(string idUser, string username, string fullName, string role);
}

/// <summary>
/// HMAC-SHA256 JWT token generator reading settings from <c>JwtSettings</c> configuration.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(MsUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return GenerateToken(user.IdUser, user.Username, user.FullName, user.Role);
    }

    public string GenerateToken(string idUser, string username, string fullName, string role)
    {
        var jwtSection = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        var issuer = jwtSection["Issuer"] ?? "BeeMartApi";
        var audience = jwtSection["Audience"] ?? "BeeMartApp";
        var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var parsedMinutes) && parsedMinutes > 0
            ? parsedMinutes
            : 1440;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, idUser),
            new(ClaimsHelper.IdUserClaimType, idUser),
            new(ClaimTypes.Name, username),
            new(ClaimsHelper.UsernameClaimType, username),
            new(ClaimsHelper.FullNameClaimType, fullName),
            new(ClaimTypes.Role, role),
            new(ClaimsHelper.RoleClaimType, role)
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
