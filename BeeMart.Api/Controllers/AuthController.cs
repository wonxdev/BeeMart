using BeeMart.Api.Common;
using BeeMart.Api.Data;
using BeeMart.Api.Models.DTOs.Auth;
using BeeMart.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Controllers;

/// <summary>
/// Authentication endpoints (<c>POST /api/v1/auth/register</c> and <c>POST /api/v1/auth/login</c>).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly BeeMartDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        BeeMartDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest? request)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.FullName)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.RePassword)
            || string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(ApiResponse.Error("Username, Full Name, Password, Re-Password, and Role are required."));
        }

        if (!string.Equals(request.Password, request.RePassword, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse.Error("Password and confirmation password do not match"));
        }

        var trimmedRole = request.Role.Trim();
        if (!string.Equals(trimmedRole, ClaimsHelper.AdminRole, StringComparison.Ordinal)
            && !string.Equals(trimmedRole, ClaimsHelper.CashierRole, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse.Error("Role must be either Admin or Cashier"));
        }

        request.Username = request.Username.Trim();
        var trimmedFullName = request.FullName.Trim();

        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return BadRequest(ApiResponse.Error("username has already exists"));
        }

        var newIdUser = await GenerateNextUserIdAsync();
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var newUser = new MsUser
        {
            IdUser = newIdUser,
            Username = request.Username,
            FullName = trimmedFullName,
            PasswordHash = passwordHash,
            Role = trimmedRole,
            Stsrc = true,
            DateIn = DateTime.Now,
            UserIn = request.Username,
            Dateup = null,
            Userup = null
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        var responseData = new RegisterResponse
        {
            IdUser = newUser.IdUser,
            Username = newUser.Username,
            FullName = newUser.FullName,
            Role = newUser.Role
        };

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<RegisterResponse>.Success(responseData, "Account registered successfully"));
    }

    /// <summary>
    /// Authenticates a user and issues a JWT bearer token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(ApiResponse.Error("Username, Password, and Role are required."));
        }

        var trimmedUsername = request.Username.Trim();
        var trimmedRole = request.Role.Trim();

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Stsrc && u.Username == trimmedUsername);

        if (user is null
            || !string.Equals(user.Role, trimmedRole, StringComparison.OrdinalIgnoreCase)
            || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse.Error("Invalid login"));
        }

        var token = _jwtTokenService.GenerateToken(user);

        var responseData = new LoginResponse
        {
            IdUser = user.IdUser,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            Token = token
        };

        return Ok(ApiResponse<LoginResponse>.Success(responseData));
    }

    /// <summary>
    /// Generates a unique 5-digit numeric <c>IdUser</c> string formatted as <c>D5</c> (e.g., <c>"00001"</c>, <c>"78887"</c>).
    /// </summary>
    private async Task<string> GenerateNextUserIdAsync()
    {
        var existingIds = await _dbContext.Users
            .AsNoTracking()
            .Select(u => u.IdUser)
            .ToListAsync();

        var existingSet = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);

        var maxNumericId = 0;
        foreach (var id in existingIds)
        {
            if (int.TryParse(id, out var numericValue) && numericValue > maxNumericId && numericValue < 99999)
            {
                maxNumericId = numericValue;
            }
        }

        var candidate = maxNumericId + 1;
        while (candidate <= 99999)
        {
            var formatted = candidate.ToString("D5");
            if (!existingSet.Contains(formatted))
            {
                return formatted;
            }

            candidate++;
        }

        for (var fallback = 1; fallback <= 99999; fallback++)
        {
            var formatted = fallback.ToString("D5");
            if (!existingSet.Contains(formatted))
            {
                return formatted;
            }
        }

        throw new InvalidOperationException("No available 5-digit IdUser values remain in MsUser.");
    }
}
