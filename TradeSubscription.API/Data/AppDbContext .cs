using Microsoft.EntityFrameworkCore;
using TradeSubscriptionAPI.Models;

namespace TradeSubscriptionAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Incoterm> Incoterms => Set<Incoterm>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<CompanySubscription> CompanySubscriptions => Set<CompanySubscription>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── AppUser ──────────────────────────────────────────────────────
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasQueryFilter(u => !u.IsDeleted);
        });

        // ── Company ──────────────────────────────────────────────────────
        modelBuilder.Entity<Company>(e =>
        {
            e.HasIndex(c => c.Name);
            e.HasIndex(c => c.RegistrationNumber);
            e.HasQueryFilter(c => !c.IsDeleted);
        });

        // ── Incoterm ─────────────────────────────────────────────────────
        modelBuilder.Entity<Incoterm>(e =>
        {
            e.HasIndex(i => i.Code).IsUnique();
            e.HasQueryFilter(i => !i.IsDeleted);
        });

        // ── Trade ────────────────────────────────────────────────────────
        modelBuilder.Entity<Trade>(e =>
        {
            e.HasIndex(t => t.TradeNumber).IsUnique();
            e.HasIndex(t => t.TradeDate);
            e.HasIndex(t => t.Status);
            e.HasQueryFilter(t => !t.IsDeleted);

            e.HasOne(t => t.Company)
             .WithMany(c => c.Trades)
             .HasForeignKey(t => t.CompanyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Incoterm)
             .WithMany(i => i.Trades)
             .HasForeignKey(t => t.IncotermId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── SubscriptionPlan ─────────────────────────────────────────────
        modelBuilder.Entity<SubscriptionPlan>(e =>
        {
            e.HasIndex(s => s.Name).IsUnique();
            e.HasQueryFilter(s => !s.IsDeleted);
        });

        // ── CompanySubscription ──────────────────────────────────────────
        modelBuilder.Entity<CompanySubscription>(e =>
        {
            e.HasIndex(cs => new { cs.CompanyId, cs.Status });
            e.HasQueryFilter(cs => !cs.IsDeleted);

            e.HasOne(cs => cs.Company)
             .WithMany(c => c.Subscriptions)
             .HasForeignKey(cs => cs.CompanyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(cs => cs.SubscriptionPlan)
             .WithMany(sp => sp.Subscriptions)
             .HasForeignKey(cs => cs.SubscriptionPlanId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Invoice ──────────────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.HasIndex(i => i.InvoiceDate);
            e.HasIndex(i => i.Status);
            e.HasQueryFilter(i => !i.IsDeleted);

            e.HasOne(i => i.Company)
             .WithMany(c => c.Invoices)
             .HasForeignKey(i => i.CompanyId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Trade)
             .WithMany(t => t.Invoices)
             .HasForeignKey(i => i.TradeId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.CompanySubscription)
             .WithMany(cs => cs.Invoices)
             .HasForeignKey(i => i.CompanySubscriptionId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed Data ────────────────────────────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Incoterms
        modelBuilder.Entity<Incoterm>().HasData(
            new Incoterm { Id = 1, Code = "EXW", Name = "Ex Works", Description = "Seller makes goods available at their premises. Buyer takes all risk and cost.", TransportMode = TransportMode.Any, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 2, Code = "FCA", Name = "Free Carrier", Description = "Seller delivers goods to carrier at named place. Risk passes on delivery to carrier.", TransportMode = TransportMode.Any, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 3, Code = "FOB", Name = "Free on Board", Description = "Seller loads goods on vessel at named port. Risk passes when goods are on board.", TransportMode = TransportMode.SeaAndInlandWaterway, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 4, Code = "CIF", Name = "Cost, Insurance and Freight", Description = "Seller pays cost, insurance and freight to destination port. Risk passes when goods are on board.", TransportMode = TransportMode.SeaAndInlandWaterway, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 5, Code = "CFR", Name = "Cost and Freight", Description = "Seller pays cost and freight to destination port. Buyer arranges insurance.", TransportMode = TransportMode.SeaAndInlandWaterway, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 6, Code = "DAP", Name = "Delivered at Place", Description = "Seller delivers at named destination, not unloaded. Buyer handles import customs and unloading.", TransportMode = TransportMode.Any, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 7, Code = "DDP", Name = "Delivered Duty Paid", Description = "Seller bears all costs including import duties to named destination. Maximum obligation for seller.", TransportMode = TransportMode.Any, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Incoterm { Id = 8, Code = "CPT", Name = "Carriage Paid To", Description = "Seller pays freight to named destination. Risk transfers on delivery to first carrier.", TransportMode = TransportMode.Any, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // Seed Subscription Plans
        modelBuilder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan { Id = 1, Name = "Free", Description = "Get started with basic trade management", MonthlyPrice = 0, AnnualPrice = 0, Currency = "USD", MaxUsers = 1, MaxTrades = 10, HasApiAccess = false, HasReporting = false, HasAdvancedAnalytics = false, Tier = PlanTier.Free, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new SubscriptionPlan { Id = 2, Name = "Basic", Description = "For small businesses managing regular trades", MonthlyPrice = 29.99m, AnnualPrice = 299.99m, Currency = "USD", MaxUsers = 5, MaxTrades = 100, HasApiAccess = false, HasReporting = true, HasAdvancedAnalytics = false, Tier = PlanTier.Basic, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new SubscriptionPlan { Id = 3, Name = "Professional", Description = "For growing businesses with heavy trade volume", MonthlyPrice = 79.99m, AnnualPrice = 799.99m, Currency = "USD", MaxUsers = 20, MaxTrades = 1000, HasApiAccess = true, HasReporting = true, HasAdvancedAnalytics = false, Tier = PlanTier.Professional, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
            new SubscriptionPlan { Id = 4, Name = "Enterprise", Description = "Unlimited trades, users, and full analytics suite", MonthlyPrice = 199.99m, AnnualPrice = 1999.99m, Currency = "USD", MaxUsers = -1, MaxTrades = -1, HasApiAccess = true, HasReporting = true, HasAdvancedAnalytics = true, Tier = PlanTier.Enterprise, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // Seed Admin User (password: Admin@123)
        modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = 1,
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@tradesub.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );
    }
}