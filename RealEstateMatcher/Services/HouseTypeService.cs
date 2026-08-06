using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Models;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher.Services;

public sealed class HouseTypeService(AppDbContextFactory factory)
{
    public async Task<IReadOnlyList<HouseTypeListItem>> GetAllAsync()
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        return await db.HouseTypes.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new HouseTypeListItem(x.Id, x.Name)).ToListAsync();
    }

    public async Task<int> SaveAsync(int? id, string name)
    {
        name = name.Trim();
        if (name.Length == 0) throw new InvalidOperationException("類型名稱不可空白。");
        await using RealEstateDbContext db = factory.CreateDbContext();
        if (await db.HouseTypes.AnyAsync(x => x.Name == name && (!id.HasValue || x.Id != id.Value)))
            throw new InvalidOperationException($"類型「{name}」已存在。");
        HouseType entity = id.HasValue ? await db.HouseTypes.SingleAsync(x => x.Id == id.Value) : new HouseType();
        if (!id.HasValue) db.HouseTypes.Add(entity);
        entity.Name = name;
        await db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteAsync(int id)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        HouseType? entity = await db.HouseTypes.SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return;
        db.HouseTypes.Remove(entity);
        await db.SaveChangesAsync();
    }
}
