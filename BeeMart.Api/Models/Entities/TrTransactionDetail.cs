namespace BeeMart.Api.Models.Entities;

public class TrTransactionDetail
{
    public string IdTransactionDetail { get; set; } = string.Empty;

    public string IdTransaction { get; set; } = string.Empty;

    public string IdProduct { get; set; } = string.Empty;

    public int Qty { get; set; } = 1;

    public decimal Price { get; set; }

    public decimal Discount { get; set; }

    public decimal SubTotal { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime DateIn { get; set; }

    public string? UserIn { get; set; }

    public DateTime? Dateup { get; set; }

    public string? Userup { get; set; }

    public TrTransaction Transaction { get; set; } = null!;

    public MsProduct Product { get; set; } = null!;
}
