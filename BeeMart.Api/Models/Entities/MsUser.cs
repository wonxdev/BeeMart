namespace BeeMart.Api.Models.Entities;

public class MsUser
{
    public string IdUser { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool Stsrc { get; set; } = true;

    public DateTime DateIn { get; set; }

    public string? UserIn { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    public string? Userup { get; set; }

    public ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
}
