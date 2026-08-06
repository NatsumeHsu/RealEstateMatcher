using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Models;

namespace RealEstateMatcher.Data;

public static class DatabaseInitializer
{
    private static readonly string[] DefaultHouseTypes =
    [
        "公寓", "社區", "大樓", "電梯大樓", "華廈", "透天厝", "別墅", "套房", "店面", "辦公室"
    ];

    public static async Task InitializeAsync(AppDbContextFactory factory)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();
        if (await db.Database.CanConnectAsync())
        {
            await using var versionCommand = db.Database.GetDbConnection().CreateCommand();
            versionCommand.CommandText = "PRAGMA user_version;";
            if (versionCommand.Connection!.State != System.Data.ConnectionState.Open) await versionCommand.Connection.OpenAsync();
            int version = Convert.ToInt32(await versionCommand.ExecuteScalarAsync());
            await versionCommand.Connection.CloseAsync();
            if (version < 2) await db.Database.EnsureDeletedAsync();
        }
        await db.Database.EnsureCreatedAsync();

        HashSet<string> existingHouseTypes = (await db.HouseTypes.Select(x => x.Name).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        db.HouseTypes.AddRange(DefaultHouseTypes
            .Where(x => !existingHouseTypes.Contains(x))
            .Select(x => new HouseType { Name = x }));

        HashSet<string> existingLayouts = (await db.Layouts.Select(x => x.Name).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        await db.SaveChangesAsync();

        if (!await db.Buyers.AnyAsync() && !await db.SellerListings.AnyAsync())
        {
            HouseType apartment = await db.HouseTypes.FirstAsync(x => x.Name == "電梯大樓");
            Layout layout = await db.Layouts.FirstAsync(x => x.Name == "3房2廳2衛");
            var city = new RequirementOption { Category = OptionCategory.City, Name = "新北市" };
            var district = new RequirementOption { Category = OptionCategory.District, Name = "板橋區" };
            var parking = new RequirementOption { Category = OptionCategory.ParkingType, Name = "坡道平面" };
            var orientation = new RequirementOption { Category = OptionCategory.Orientation, Name = "坐北朝南" };
            var decoration = new RequirementOption { Category = OptionCategory.Decoration, Name = "簡易裝潢" };
            var transport = new RequirementOption { Category = OptionCategory.Transport, Name = "板橋捷運站" };
            db.Buyers.Add(new Buyer
            {
                Name="範例買方王小姐", Mobile="0912-000-001", MinPrice=12000000, MaxPrice=18000000,
                DesiredAddress="新北市板橋區", MinBedrooms=3, MaxBedrooms=4, MinLivingRooms=2, MinBathrooms=2,
                MinMainArea=20, MinDeedArea=30, MaxBuildingAge=20, MinParkingCount=1,
                PetRequirement=RequirementLevel.Required, ElevatorRequirement=RequirementLevel.Required,
                HouseTypes=[apartment], Layouts=[layout], Options=[city,district,parking,orientation,decoration,transport], Note="近捷運優先"
            });
            db.SellerListings.Add(new SellerListing
            {
                Name="範例賣方陳先生", Mobile="0912-000-002", Price=16800000, Address="新北市板橋區文化路一段",
                Bedrooms=3, LivingRooms=2, Bathrooms=2, MainArea=22.5m, DeedArea=35.8m, Floor=8, TotalFloors=15,
                BuildingAge=12, ParkingCount=1, PetsAllowed=AvailabilityStatus.Yes, HasElevator=AvailabilityStatus.Yes,
                HouseTypes=[apartment], Layouts=[layout], Options=[city,district,parking,orientation,decoration,transport], Note="含平面車位"
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Buyers.AnyAsync(x => x.Name == "範例買方林先生"))
        {
            HouseType apartment = await db.HouseTypes.FirstAsync(x => x.Name == "公寓");
            Layout layout = await db.Layouts.FirstAsync(x => x.Name == "2房1廳1衛");
            RequirementOption city = await GetOrCreateOptionAsync(OptionCategory.City, "台北市");
            RequirementOption district = await GetOrCreateOptionAsync(OptionCategory.District, "中山區");
            RequirementOption parking = await GetOrCreateOptionAsync(OptionCategory.ParkingType, "機械車位");
            RequirementOption orientation = await GetOrCreateOptionAsync(OptionCategory.Orientation, "坐東朝西");
            RequirementOption decoration = await GetOrCreateOptionAsync(OptionCategory.Decoration, "全新裝潢");
            RequirementOption school = await GetOrCreateOptionAsync(OptionCategory.School, "中山國小");
            RequirementOption transport = await GetOrCreateOptionAsync(OptionCategory.Transport, "行天宮捷運站");

            db.Buyers.Add(new Buyer
            {
                Name="範例買方林先生", Mobile="0922-000-003", LineId="lin.demo", MinPrice=9000000, MaxPrice=14000000,
                DesiredAddress="台北市中山區", MinBedrooms=2, MaxBedrooms=3, MinLivingRooms=1, MaxLivingRooms=2,
                MinBathrooms=1, MaxBathrooms=2, MinMainArea=15, MinDeedArea=24, MaxBuildingAge=30, MinParkingCount=0,
                PetRequirement=RequirementLevel.NoPreference, ElevatorRequirement=RequirementLevel.NoPreference,
                HouseTypes=[apartment], Layouts=[layout], Options=[city,district,parking,orientation,decoration,school,transport],
                ExclusionNotes="排除頂樓加蓋", Note="自住，近捷運優先"
            });
            db.SellerListings.Add(new SellerListing
            {
                Name="範例賣方張小姐", Mobile="0933-000-004", LineId="chang.demo", Price=12800000,
                Address="台北市中山區民權東路二段", Bedrooms=2, LivingRooms=1, Bathrooms=1,
                MainArea=17.2m, DeedArea=26.6m, Floor=6, TotalFloors=10, BuildingAge=18, ParkingCount=1,
                PetsAllowed=AvailabilityStatus.Yes, HasElevator=AvailabilityStatus.Yes,
                HouseTypes=[apartment], Layouts=[layout], Options=[city,district,parking,orientation,decoration,school,transport],
                Note="近行天宮捷運站，含機械車位"
            });
            await db.SaveChangesAsync();
        }

        await db.Database.ExecuteSqlRawAsync("PRAGMA user_version = 2;");

        async Task<RequirementOption> GetOrCreateOptionAsync(OptionCategory category, string name)
        {
            return await db.RequirementOptions.FirstOrDefaultAsync(x => x.Category == category && x.Name == name)
                ?? new RequirementOption { Category = category, Name = name };
        }
    }
}
