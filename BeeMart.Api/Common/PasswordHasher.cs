using BCrypt.Net;

namespace BeeMart.Api.Common;

/// <summary>
/// Abstraction for hashing and verifying user passwords using BCrypt (NFR-SEC-001).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using BCrypt with work factor 11.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain-text password against a stored BCrypt hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// BCrypt implementation of <see cref="IPasswordHasher"/> using work factor 11 (matching Phase 3 seed hashes).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
