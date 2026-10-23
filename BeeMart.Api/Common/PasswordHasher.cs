using BCrypt.Net;

namespace BeeMart.Api.Common;

/// <summary>
/// Abstraction for hashing and verifying user passwords using BCrypt.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// BCrypt implementation of <see cref="IPasswordHasher"/> using work factor 11.
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
