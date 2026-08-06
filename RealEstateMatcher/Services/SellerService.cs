using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Models;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher.Services;

public sealed class SellerService(AppDbContextFactory factory)
{

    public async Task<IReadOnlyList<SellerListItem>> GetAllAsync()
    {
        await using RealEstateDbContext db = factory.CreateDbContext();

        List<SellerListing> sellers = await db.SellerListings
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

        return sellers.Select(Map).ToList();
    }

    public async Task<int> SaveAsync(SellerInput input)
    {
        Validate(input);

        await using RealEstateDbContext db = factory.CreateDbContext();
        SellerListing entity;

        if (input.Id.HasValue)
        {
            entity = await db.SellerListings
                .Include(x => x.HouseTypes)
                .Include(x => x.Layouts)
                .Include(x => x.Options)
                .SingleOrDefaultAsync(x => x.Id == input.Id.Value)
                ?? throw new InvalidOperationException("找不到要修改的賣方房源。");
        }
        else
        {
            entity = new SellerListing();
            db.SellerListings.Add(entity);
        }

        entity.Name = input.Name.Trim();
        entity.Mobile = input.Mobile.Trim();
        entity.LineId = input.LineId.Trim();
        entity.Price = input.Price;
        ApplyCriteria(entity, input.Criteria);
        entity.Address = input.Address.Trim();
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
        SellerListing? entity = await db.SellerListings.SingleOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return;
        }

        db.SellerListings.Remove(entity);
        await db.SaveChangesAsync();
    }

    private static SellerListItem Map(SellerListing entity)
    {
        return new SellerListItem
        {
            Id = entity.Id,
            Name = entity.Name,
            Mobile = entity.Mobile,
            LineId = entity.LineId,
            Price = entity.Price,
            Address = entity.Address,
            Note = entity.Note,
            HouseTypes = entity.HouseTypes.Select(x => new LookupItem(x.Id, x.Name)).OrderBy(x => x.Name).ToArray(),
            Layouts = entity.Layouts.Select(x => new LookupItem(x.Id, x.Name)).OrderBy(x => x.Name).ToArray()
            ,Criteria = MapCriteria(entity),
            Options = entity.Options.Select(x => new CategorizedOptionItem(x.Id, x.Category, x.Name)).ToArray()
        };
    }

    private static void Validate(SellerInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidOperationException("賣方姓名不可空白。");
        }

        if (input.Price < 0)
        {
            throw new InvalidOperationException("房源開價不可小於 0。");
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

    private static void ApplyCriteria(SellerListing e, CriteriaValues c)
    {
        e.Bedrooms=c.MinBedrooms; e.LivingRooms=c.MinLivingRooms; e.Bathrooms=c.MinBathrooms; e.MainArea=c.MinMainArea; e.DeedArea=c.MinDeedArea;
        e.Floor=c.MinFloor; e.TotalFloors=c.MinTotalFloors; e.BuildingAge=c.MinBuildingAge; e.ParkingCount=c.MinParkingCount;
        e.PetsAllowed=(AvailabilityStatus)(int)c.PetRequirement; e.HasElevator=(AvailabilityStatus)(int)c.ElevatorRequirement; e.ExclusionNotes=c.ExclusionNotes;
    }

    private static CriteriaValues MapCriteria(SellerListing e) => new()
    {
        MinBedrooms=e.Bedrooms, MinLivingRooms=e.LivingRooms, MinBathrooms=e.Bathrooms, MinMainArea=e.MainArea, MinDeedArea=e.DeedArea,
        MinFloor=e.Floor, MinTotalFloors=e.TotalFloors, MinBuildingAge=e.BuildingAge, MinParkingCount=e.ParkingCount,
        PetRequirement=(RequirementLevel)(int)e.PetsAllowed, ElevatorRequirement=(RequirementLevel)(int)e.HasElevator, ExclusionNotes=e.ExclusionNotes
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
