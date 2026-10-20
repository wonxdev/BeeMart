namespace BeeMart.Api.Models.Entities;

public class LtPaymentMethod
{
    public int IdPaymentMethod { get; set; }

    public string MethodName { get; set; } = string.Empty;

    public bool Stsrc { get; set; } = true;

    public DateTime DateIn { get; set; }

    public string? UserIn { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    public string? Userup { get; set; }

    public ICollection<TrTransaction> Transactions { get; set; } = new List<TrTransaction>();
}
