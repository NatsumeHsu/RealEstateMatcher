using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Models;

namespace RealEstateMatcher.Data;

public sealed class RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : DbContext(options)
{
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<SellerListing> SellerListings => Set<SellerListing>();
    public DbSet<HouseType> HouseTypes => Set<HouseType>();
    public DbSet<Layout> Layouts => Set<Layout>();
    public DbSet<RequirementOption> RequirementOptions => Set<RequirementOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Buyer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Mobile).HasMaxLength(30);
            entity.Property(x => x.LineId).HasMaxLength(100);
            entity.Property(x => x.DesiredAddress).HasMaxLength(300);
            entity.Property(x => x.MaxPrice).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(2000);

            entity.HasMany(x => x.HouseTypes)
                .WithMany(x => x.Buyers)
                .UsingEntity("BuyerHouseTypes");

            entity.HasMany(x => x.Layouts).WithMany(x => x.Buyers).UsingEntity("BuyerLayouts");
            entity.HasMany(x => x.Options).WithMany(x => x.Buyers).UsingEntity("BuyerRequirementOptions");

        });

        modelBuilder.Entity<SellerListing>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Mobile).HasMaxLength(30);
            entity.Property(x => x.LineId).HasMaxLength(100);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(2000);

            entity.HasMany(x => x.HouseTypes)
                .WithMany(x => x.SellerListings)
                .UsingEntity("SellerHouseTypes");

            entity.HasMany(x => x.Layouts).WithMany(x => x.SellerListings).UsingEntity("SellerLayouts");
            entity.HasMany(x => x.Options).WithMany(x => x.SellerListings).UsingEntity("SellerRequirementOptions");

        });

        modelBuilder.Entity<HouseType>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(80)
                .UseCollation("NOCASE")
                .IsRequired();

            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Layout>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(80).UseCollation("NOCASE").IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<RequirementOption>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.Category, x.Name }).IsUnique();
        });

    }
}
