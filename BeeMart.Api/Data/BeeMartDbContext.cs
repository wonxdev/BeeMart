using BeeMart.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeMart.Api.Data;

public class BeeMartDbContext : DbContext
{
    public BeeMartDbContext(DbContextOptions<BeeMartDbContext> options)
        : base(options)
    {
    }

    public DbSet<MsUser> Users => Set<MsUser>();

    public DbSet<LtPaymentMethod> PaymentMethods => Set<LtPaymentMethod>();

    public DbSet<MsProduct> Products => Set<MsProduct>();

    public DbSet<TrTransaction> Transactions => Set<TrTransaction>();

    public DbSet<TrTransactionDetail> TransactionDetails => Set<TrTransactionDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LtPaymentMethod>(entity =>
        {
            entity.ToTable("LtPaymentMethod");

            entity.HasKey(e => e.IdPaymentMethod)
                .HasName("PK_LtPaymentMethod");

            entity.Property(e => e.IdPaymentMethod)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.MethodName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Stsrc)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateIn)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.UserIn)
                .HasMaxLength(36)
                .HasDefaultValue("SYSTEM");

            entity.Property(e => e.Dateup)
                .HasColumnType("datetime2");

            entity.Property(e => e.Userup)
                .HasMaxLength(36);

            entity.HasIndex(e => e.MethodName, "UQ_LtPaymentMethod_MethodName")
                .IsUnique();
        });

        modelBuilder.Entity<MsUser>(entity =>
        {
            entity.ToTable("MsUser");

            entity.HasKey(e => e.IdUser)
                .HasName("PK_MsUser");

            entity.Property(e => e.IdUser)
                .HasMaxLength(5)
                .ValueGeneratedNever();

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Stsrc)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateIn)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.UserIn)
                .HasMaxLength(36)
                .HasDefaultValue("SYSTEM");

            entity.Property(e => e.Dateup)
                .HasColumnType("datetime2");

            entity.Property(e => e.Userup)
                .HasMaxLength(36);

            entity.HasIndex(e => e.Username, "UQ_MsUser_Username")
                .IsUnique();
        });

        modelBuilder.Entity<MsProduct>(entity =>
        {
            entity.ToTable("MsProduct");

            entity.HasKey(e => e.IdProduct)
                .HasName("PK_MsProduct");

            entity.Property(e => e.IdProduct)
                .HasMaxLength(36)
                .HasDefaultValueSql("(CONVERT(NVARCHAR(36), NEWID()))");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Weight)
                .HasPrecision(10, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Stock)
                .HasDefaultValue(0);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Discount)
                .HasPrecision(5, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Stsrc)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateIn)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.UserIn)
                .HasMaxLength(36)
                .HasDefaultValue("SYSTEM");

            entity.Property(e => e.Dateup)
                .HasColumnType("datetime2");

            entity.Property(e => e.Userup)
                .HasMaxLength(36);

            entity.HasIndex(e => e.Name, "IX_MsProduct_Name");
        });

        modelBuilder.Entity<TrTransaction>(entity =>
        {
            entity.ToTable("TrTransaction");

            entity.HasKey(e => e.IdTransaction)
                .HasName("PK_TrTransaction");

            entity.Property(e => e.IdTransaction)
                .HasMaxLength(36)
                .HasDefaultValueSql("(CONVERT(NVARCHAR(36), NEWID()))");

            entity.Property(e => e.InvoiceNo)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.TransactionDate)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.IdUser)
                .IsRequired()
                .HasMaxLength(5);

            entity.Property(e => e.IdPaymentMethod)
                .IsRequired();

            entity.Ignore(e => e.TotalItem);

            entity.Property(e => e.TotalPrice)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Stsrc)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateIn)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.UserIn)
                .HasMaxLength(36);

            entity.Property(e => e.Dateup)
                .HasColumnType("datetime2");

            entity.Property(e => e.Userup)
                .HasMaxLength(36);

            entity.HasIndex(e => e.InvoiceNo, "UQ_TrTransaction_InvoiceNo")
                .IsUnique();

            entity.HasIndex(e => new { e.TransactionDate, e.IdUser }, "IX_TrTransaction_TransactionDate_IdUser");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TrTransaction_MsUser");

            entity.HasOne(d => d.PaymentMethod)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.IdPaymentMethod)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TrTransaction_LtPaymentMethod");
        });

        modelBuilder.Entity<TrTransactionDetail>(entity =>
        {
            entity.ToTable("TrTransactionDetail");

            entity.HasKey(e => e.IdTransactionDetail)
                .HasName("PK_TrTransactionDetail");

            entity.Property(e => e.IdTransactionDetail)
                .HasMaxLength(36)
                .HasDefaultValueSql("(CONVERT(NVARCHAR(36), NEWID()))");

            entity.Property(e => e.IdTransaction)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.IdProduct)
                .IsRequired()
                .HasMaxLength(36);

            entity.Property(e => e.Qty)
                .HasDefaultValue(1);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Discount)
                .HasPrecision(5, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.SubTotal)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Stsrc)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.DateIn)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(SYSDATETIME())");

            entity.Property(e => e.UserIn)
                .HasMaxLength(36);

            entity.Property(e => e.Dateup)
                .HasColumnType("datetime2");

            entity.Property(e => e.Userup)
                .HasMaxLength(36);

            entity.HasOne(d => d.Transaction)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.IdTransaction)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TrTransactionDetail_TrTransaction");

            entity.HasOne(d => d.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TrTransactionDetail_MsProduct");
        });
    }
}
