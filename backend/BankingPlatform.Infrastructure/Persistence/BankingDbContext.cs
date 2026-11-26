
using BankingPlatform.Domain.Entities;
using BankingPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BankingPlatform.Infrastructure.Persistence;

public class BankingDbContext : DbContext
{
    public BankingDbContext(
        DbContextOptions<BankingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("banking");

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers", "identity");

            entity.HasKey(customer => customer.Id);

            entity.Property(customer => customer.Id)
                .ValueGeneratedNever();

            entity.Property(customer => customer.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(customer => customer.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(customer => customer.Email)
                .HasConversion(
                    email => email.Value,
                    value => EmailAddress.Create(value))
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();

            entity.HasIndex(customer => customer.Email)
                .IsUnique()
                .HasDatabaseName("ix_customers_email");

            entity.Property(customer => customer.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(customer => customer.CreatedAtUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(customer => customer.UpdatedAtUtc)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts", "banking");

            entity.HasKey(account => account.Id);

            entity.Property(account => account.Id)
                .ValueGeneratedNever();

            entity.Property(account => account.CustomerId)
                .IsRequired();

            entity.Property(account => account.AccountNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(account => account.AccountNumber)
                .IsUnique()
                .HasDatabaseName("ix_accounts_account_number");

            entity.HasIndex(account => account.CustomerId)
                .HasDatabaseName("ix_accounts_customer_id");

            entity.Property(account => account.Currency)
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.Create(code))
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(account => account.Status)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(account => account.CreatedAtUtc)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(account => account.UpdatedAtUtc)
                .HasColumnType("timestamp with time zone");

            entity.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(account => account.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_accounts_customers_customer_id");
        });
    }
}
