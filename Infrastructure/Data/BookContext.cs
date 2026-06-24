using Core.Models;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BookContext: IdentityDbContext<AppUser>
{
    public BookContext(DbContextOptions<BookContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<StripeWebhookEventLog> StripeWebhookEventLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order configuration
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(o => o.OwnerUserId)
            .HasMaxLength(450);

        // Payment configuration
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stripe webhook event log configuration
        modelBuilder.Entity<StripeWebhookEventLog>()
            .HasIndex(e => e.StripeEventId)
            .IsUnique();

        modelBuilder.Entity<StripeWebhookEventLog>()
            .Property(e => e.StripeEventId)
            .HasMaxLength(255)
            .IsRequired();

        modelBuilder.Entity<StripeWebhookEventLog>()
            .Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired();
    }
}
