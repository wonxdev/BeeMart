namespace BeeMart.Api.Models.Entities;

public class MsProduct
{
    public string IdProduct { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Weight { get; set; }

    public int Stock { get; set; }

    public decimal Price { get; set; }

    public decimal Discount { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime DateIn { get; set; }

    public string? UserIn { get; set; } = "SYSTEM";

    public DateTime? Dateup { get; set; }

    public string? Userup { get; set; }

    public ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
