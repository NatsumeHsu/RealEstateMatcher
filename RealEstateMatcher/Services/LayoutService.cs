using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Models;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher.Services;

public sealed class LayoutService(AppDbContextFactory factory)
{
    public async Task<IReadOnlyList<LayoutListItem>> GetAllAsync()
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        return await db.Layouts.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new LayoutListItem(x.Id, x.Name)).ToListAsync();
    }

    public async Task<int> SaveAsync(int? id, int bedrooms, int livingRooms, int bathrooms)
    {
        if (bedrooms < 0 || livingRooms < 0 || bathrooms < 0) throw new InvalidOperationException("房、廳、衛不可小於 0。");
        string name = LayoutParser.Format(bedrooms, livingRooms, bathrooms);
        await using RealEstateDbContext db = factory.CreateDbContext();
        if (await db.Layouts.AnyAsync(x => x.Name == name && (!id.HasValue || x.Id != id.Value)))
            throw new InvalidOperationException($"格局「{name}」已存在。");
        Layout entity = id.HasValue
            ? await db.Layouts.SingleAsync(x => x.Id == id.Value)
            : new Layout();
        if (!id.HasValue) db.Layouts.Add(entity);
        entity.Name = name;
        await db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteAsync(int id)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        Layout? entity = await db.Layouts.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;
        db.Layouts.Remove(entity);
        await db.SaveChangesAsync();
    }
}
