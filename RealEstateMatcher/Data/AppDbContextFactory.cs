using Microsoft.EntityFrameworkCore;

namespace RealEstateMatcher.Data;

public sealed class AppDbContextFactory(DbContextOptions<RealEstateDbContext> options)
{

    public RealEstateDbContext CreateDbContext()
    {
        return new RealEstateDbContext(options);
    }
}
