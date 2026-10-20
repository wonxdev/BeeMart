using System.ComponentModel.DataAnnotations.Schema;

namespace BeeMart.Api.Models.Entities;

public class TrTransaction
{
    public string IdTransaction { get; set; } = string.Empty;

    public string InvoiceNo { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public string IdUser { get; set; } = string.Empty;

    public int IdPaymentMethod { get; set; }

    [NotMapped]
    public int TotalItem { get; set; }

    public decimal TotalPrice { get; set; }

    public bool Stsrc { get; set; } = true;

    public DateTime DateIn { get; set; }

    public string? UserIn { get; set; }

    public DateTime? Dateup { get; set; }

    public string? Userup { get; set; }

    public MsUser User { get; set; } = null!;

    public LtPaymentMethod PaymentMethod { get; set; } = null!;

    public ICollection<TrTransactionDetail> TransactionDetails { get; set; } = new List<TrTransactionDetail>();
}
