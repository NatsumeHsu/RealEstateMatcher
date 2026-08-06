using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Models;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher.Services;

public sealed class BuyerService(AppDbContextFactory factory)
{
    public async Task<IReadOnlyList<BuyerListItem>> GetAllAsync()
    {
        await using RealEstateDbContext db = factory.CreateDbContext();

        List<Buyer> buyers = await db.Buyers
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        return [.. buyers.Select(Map)];
    }

    public async Task<int> SaveAsync(BuyerInput input)
    {
        Validate(input);

        await using RealEstateDbContext db = factory.CreateDbContext();
        Buyer entity;

        if (input.Id.HasValue)
        {
            entity = await db.Buyers
                .Include(x => x.HouseTypes)
                .Include(x => x.Layouts)
                .Include(x => x.Options)
                .SingleOrDefaultAsync(x => x.Id == input.Id.Value)
                ?? throw new InvalidOperationException("找不到要修改的買方。");
        }
        else
        {
            entity = new Buyer();
            db.Buyers.Add(entity);
        }

        entity.Name = input.Name.Trim();
        entity.Mobile = input.Mobile.Trim();
        entity.LineId = input.LineId.Trim();
        entity.MaxPrice = input.MaxPrice;
        ApplyCriteria(entity, input.Criteria);
        entity.DesiredAddress = input.Address.Trim();
        entity.Note = input.Note.Trim();
        entity.UpdatedAt = DateTime.Now;

        await ReplaceHouseTypesAsync(db, entity.HouseTypes, input.HouseTypes);
        await ReplaceLayoutsAsync(db, entity.Layouts, input.Layouts);
        await ReplaceOptionsAsync(db, entity.Options, input.Options);

        await db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteAsync(int id)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        Buyer? entity = await db.Buyers.SingleOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return;
        }

        db.Buyers.Remove(entity);
        await db.SaveChangesAsync();
    }

    private static BuyerListItem Map(Buyer entity)
    {
        return new BuyerListItem
        {
            Id = entity.Id,
            Name = entity.Name,
            Mobile = entity.Mobile,
            LineId = entity.LineId,
            Price = entity.MaxPrice,
            Address = entity.DesiredAddress,
            Note = entity.Note,
            HouseTypes = entity.HouseTypes.Select(x => new LookupItem(x.Id, x.Name)).OrderBy(x => x.Name).ToArray(),
            Layouts = entity.Layouts.Select(x => new LookupItem(x.Id, x.Name)).OrderBy(x => x.Name).ToArray()
            ,Criteria = MapCriteria(entity),
            Options = entity.Options.Select(x => new CategorizedOptionItem(x.Id, x.Category, x.Name)).ToArray()
        };
    }

    private static void Validate(BuyerInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidOperationException("買方姓名不可空白。");
        }

        if (input.MaxPrice < 0)
        {
            throw new InvalidOperationException("最高預算不可小於 0。");
        }
    }

    private static async Task ReplaceHouseTypesAsync(
        RealEstateDbContext db,
        ICollection<HouseType> target,
        IReadOnlyList<string> names)
    {
        target.Clear();
        if (names.Count == 0)
        {
            return;
        }

        List<HouseType> existing = await db.HouseTypes
            .Where(x => names.Contains(x.Name))
            .ToListAsync();

        foreach (string name in names)
        {
            HouseType? item = existing.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            item ??= new HouseType { Name = name };
            target.Add(item);
        }
    }

    private static async Task ReplaceLayoutsAsync(RealEstateDbContext db, ICollection<Layout> target, IReadOnlyList<string> names)
    {
        target.Clear();
        List<Layout> existing = await db.Layouts.Where(x => names.Contains(x.Name)).ToListAsync();
        foreach (string name in names)
        {
            Layout? item = existing.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            item ??= new Layout { Name = name };
            target.Add(item);
        }
    }

    private static void ApplyCriteria(Buyer e, CriteriaValues c)
    {
        e.MinPrice=c.MinPrice; e.MinBedrooms=c.MinBedrooms; e.MaxBedrooms=c.MaxBedrooms; e.MinLivingRooms=c.MinLivingRooms; e.MaxLivingRooms=c.MaxLivingRooms;
        e.MinBathrooms=c.MinBathrooms; e.MaxBathrooms=c.MaxBathrooms; e.MinMainArea=c.MinMainArea; e.MaxMainArea=c.MaxMainArea;
        e.MinDeedArea=c.MinDeedArea; e.MaxDeedArea=c.MaxDeedArea; e.MinFloor=c.MinFloor; e.MaxFloor=c.MaxFloor;
        e.MinTotalFloors=c.MinTotalFloors; e.MaxTotalFloors=c.MaxTotalFloors; e.MinBuildingAge=c.MinBuildingAge; e.MaxBuildingAge=c.MaxBuildingAge;
        e.MinParkingCount=c.MinParkingCount; e.PetRequirement=c.PetRequirement; e.ElevatorRequirement=c.ElevatorRequirement; e.ExclusionNotes=c.ExclusionNotes;
    }

    private static CriteriaValues MapCriteria(Buyer e) => new()
    {
        MinPrice=e.MinPrice, MinBedrooms=e.MinBedrooms, MaxBedrooms=e.MaxBedrooms, MinLivingRooms=e.MinLivingRooms, MaxLivingRooms=e.MaxLivingRooms,
        MinBathrooms=e.MinBathrooms, MaxBathrooms=e.MaxBathrooms, MinMainArea=e.MinMainArea, MaxMainArea=e.MaxMainArea, MinDeedArea=e.MinDeedArea,
        MaxDeedArea=e.MaxDeedArea, MinFloor=e.MinFloor, MaxFloor=e.MaxFloor, MinTotalFloors=e.MinTotalFloors, MaxTotalFloors=e.MaxTotalFloors,
        MinBuildingAge=e.MinBuildingAge, MaxBuildingAge=e.MaxBuildingAge, MinParkingCount=e.MinParkingCount, PetRequirement=e.PetRequirement,
        ElevatorRequirement=e.ElevatorRequirement, ExclusionNotes=e.ExclusionNotes
    };

    private static async Task ReplaceOptionsAsync(RealEstateDbContext db, ICollection<RequirementOption> target, IReadOnlyDictionary<OptionCategory, IReadOnlyList<string>> groups)
    {
        target.Clear();
        foreach ((OptionCategory category, IReadOnlyList<string> names) in groups)
            foreach (string name in names)
            {
                RequirementOption? option = await db.RequirementOptions.FirstOrDefaultAsync(x => x.Category == category && x.Name == name);
                target.Add(option ?? new RequirementOption { Category = category, Name = name });
            }
    }
}
