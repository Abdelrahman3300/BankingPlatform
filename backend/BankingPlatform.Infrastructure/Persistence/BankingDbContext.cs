
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
    }
}
