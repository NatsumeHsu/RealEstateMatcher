using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Models;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher.Services;

public sealed class MatchingService(
    AppDbContextFactory factory,
    AddressSimilarityService addressSimilarityService)
{

    public async Task<IReadOnlyList<MatchResultItem>> FindSellersForBuyerAsync(
        int buyerId,
        MatchCriteria criteria)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();

        Buyer buyer = await db.Buyers
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == buyerId)
            ?? throw new InvalidOperationException("找不到指定買方。");

        List<SellerListing> sellers = await db.SellerListings
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .ToListAsync();

        var results = new List<MatchResultItem>();

        foreach (SellerListing seller in sellers)
        {
            double addressScore = addressSimilarityService.Calculate(
                buyer.DesiredAddress,
                seller.Address);

            double priceScore = CalculateRangeScore(buyer.MinPrice, buyer.MaxPrice, seller.Price);
            double houseTypeScore = CalculateSetScore(
                buyer.HouseTypes.Select(x => x.Id),
                seller.HouseTypes.Select(x => x.Id));
            LayoutMatchScores layoutScores = new(
                CalculateRangeScore(buyer.MinBedrooms, buyer.MaxBedrooms, seller.Bedrooms),
                CalculateRangeScore(buyer.MinLivingRooms, buyer.MaxLivingRooms, seller.LivingRooms),
                CalculateRangeScore(buyer.MinBathrooms, buyer.MaxBathrooms, seller.Bathrooms));
            double layoutScore = layoutScores.Total;
            Dictionary<MatchCriterion,double> scores = BuildScores(buyer, seller, addressScore, priceScore, houseTypeScore, layoutScore);
            double totalScore = CalculateSelectedScore(criteria, scores);

            results.Add(new MatchResultItem
            {
                Id = seller.Id,
                Name = seller.Name,
                Mobile = seller.Mobile,
                LineId = seller.LineId,
                Price = seller.Price,
                Address = seller.Address,
                HouseTypesText = TextListParser.Join(seller.HouseTypes.Select(x => x.Name)),
                LayoutsText = TextListParser.Join(seller.Layouts.Select(x => x.Name)),
                AddressSimilarity = addressScore,
                PriceScore = priceScore,
                HouseTypeScore = houseTypeScore,
                LayoutScore = layoutScore,
                BedroomScore = layoutScores.Bedrooms,
                LivingRoomScore = layoutScores.LivingRooms,
                BathroomScore = layoutScores.Bathrooms,
                TotalScore = totalScore,
                Scores = scores
            });
        }

        return SortByCriteria(results, criteria);
    }

    public async Task<IReadOnlyList<MatchResultItem>> FindBuyersForSellerAsync(
        int sellerId,
        MatchCriteria criteria)
    {
        await using RealEstateDbContext db = factory.CreateDbContext();

        SellerListing seller = await db.SellerListings
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .SingleOrDefaultAsync(x => x.Id == sellerId)
            ?? throw new InvalidOperationException("找不到指定賣方房源。");

        List<Buyer> buyers = await db.Buyers
            .AsNoTracking()
            .Include(x => x.HouseTypes)
            .Include(x => x.Layouts)
            .Include(x => x.Options)
            .ToListAsync();

        var results = new List<MatchResultItem>();

        foreach (Buyer buyer in buyers)
        {
            double addressScore = addressSimilarityService.Calculate(
                buyer.DesiredAddress,
                seller.Address);

            double priceScore = CalculateRangeScore(buyer.MinPrice, buyer.MaxPrice, seller.Price);
            double houseTypeScore = CalculateSetScore(
                buyer.HouseTypes.Select(x => x.Id),
                seller.HouseTypes.Select(x => x.Id));
            LayoutMatchScores layoutScores = new(
                CalculateRangeScore(buyer.MinBedrooms, buyer.MaxBedrooms, seller.Bedrooms),
                CalculateRangeScore(buyer.MinLivingRooms, buyer.MaxLivingRooms, seller.LivingRooms),
                CalculateRangeScore(buyer.MinBathrooms, buyer.MaxBathrooms, seller.Bathrooms));
            double layoutScore = layoutScores.Total;
            Dictionary<MatchCriterion,double> scores = BuildScores(buyer, seller, addressScore, priceScore, houseTypeScore, layoutScore);
            double totalScore = CalculateSelectedScore(criteria, scores);

            results.Add(new MatchResultItem
            {
                Id = buyer.Id,
                Name = buyer.Name,
                Mobile = buyer.Mobile,
                LineId = buyer.LineId,
                Price = buyer.MaxPrice,
                Address = buyer.DesiredAddress,
                HouseTypesText = TextListParser.Join(buyer.HouseTypes.Select(x => x.Name)),
                LayoutsText = TextListParser.Join(buyer.Layouts.Select(x => x.Name)),
                AddressSimilarity = addressScore,
                PriceScore = priceScore,
                HouseTypeScore = houseTypeScore,
                LayoutScore = layoutScore,
                BedroomScore = layoutScores.Bedrooms,
                LivingRoomScore = layoutScores.LivingRooms,
                BathroomScore = layoutScores.Bathrooms,
                TotalScore = totalScore,
                Scores = scores
            });
        }

        return SortByCriteria(results, criteria);
    }

    private static double CalculateSelectedScore(MatchCriteria criteria, IReadOnlyDictionary<MatchCriterion,double> scores)
    {
        double weightedScore = 0;
        double selectedWeight = 0;

        foreach (MatchCriterion criterion in criteria.Order) Add(true, scores[criterion], 1.0);

        return selectedWeight == 0 ? 0 : weightedScore / selectedWeight;

        void Add(bool selected, double score, double weight)
        {
            if (!selected) return;
            weightedScore += score * weight;
            selectedWeight += weight;
        }
    }

    private static IReadOnlyList<MatchResultItem> SortByCriteria(
        IEnumerable<MatchResultItem> results,
        MatchCriteria criteria)
    {
        IOrderedEnumerable<MatchResultItem>? ordered = null;
        foreach (MatchCriterion criterion in criteria.Order)
        {
            Func<MatchResultItem, double> scoreSelector = x => x.Scores.TryGetValue(criterion, out double score) ? score : 0;

            ordered = ordered is null
                ? results.OrderByDescending(scoreSelector)
                : ordered.ThenByDescending(scoreSelector);
        }

        return (ordered ?? results.OrderByDescending(x => x.TotalScore))
            .ThenByDescending(x => x.TotalScore)
            .ToList();
    }

    private static double CalculateSetScore(IEnumerable<int> leftValues, IEnumerable<int> rightValues)
    {
        HashSet<int> left = leftValues.ToHashSet();
        HashSet<int> right = rightValues.ToHashSet();

        if (left.Count == 0 || right.Count == 0)
        {
            return 1.0;
        }

        int intersection = left.Intersect(right).Count();
        int union = left.Union(right).Count();
        return union == 0 ? 1.0 : (double)intersection / union;
    }

    private static LayoutMatchScores CalculateLayoutScores(
        IEnumerable<string> buyerLayouts,
        IEnumerable<string> sellerLayouts)
    {
        LayoutParts[] buyers = buyerLayouts.Select(LayoutParser.Parse).ToArray();
        LayoutParts[] sellers = sellerLayouts.Select(LayoutParser.Parse).ToArray();
        if (buyers.Length == 0 || sellers.Length == 0) return new(1, 1, 1);

        return (from buyer in buyers
                from seller in sellers
                let scores = new LayoutMatchScores(
                    CountScore(buyer.Bedrooms, seller.Bedrooms),
                    CountScore(buyer.LivingRooms, seller.LivingRooms),
                    CountScore(buyer.Bathrooms, seller.Bathrooms))
                orderby scores.Total descending
                select scores).First();
    }

    private static double CountScore(int? desired, int? actual)
    {
        if (!desired.HasValue || !actual.HasValue) return 1.0;
        if (desired.Value == actual.Value) return 1.0;
        int maximum = Math.Max(desired.Value, actual.Value);
        return maximum == 0 ? 0 : (double)Math.Min(desired.Value, actual.Value) / maximum;
    }

    private static Dictionary<MatchCriterion,double> BuildScores(
        Buyer buyer, SellerListing seller, double address, double price, double houseType, double layout)
    {
        return new Dictionary<MatchCriterion, double>
        {
            [MatchCriterion.Price]=price, [MatchCriterion.Location]=Average(CategoryScore(buyer,seller,OptionCategory.City),CategoryScore(buyer,seller,OptionCategory.District)),
            [MatchCriterion.Address]=address, [MatchCriterion.HouseType]=houseType, [MatchCriterion.Layout]=layout,
            [MatchCriterion.Area]=Average(CalculateRangeScore(buyer.MinMainArea,buyer.MaxMainArea,seller.MainArea),CalculateRangeScore(buyer.MinDeedArea,buyer.MaxDeedArea,seller.DeedArea)),
            [MatchCriterion.Floor]=Average(CalculateRangeScore(buyer.MinFloor,buyer.MaxFloor,seller.Floor),CalculateRangeScore(buyer.MinTotalFloors,buyer.MaxTotalFloors,seller.TotalFloors)),
            [MatchCriterion.BuildingAge]=CalculateRangeScore(buyer.MinBuildingAge,buyer.MaxBuildingAge,seller.BuildingAge),
            [MatchCriterion.Parking]=Average(CategoryScore(buyer,seller,OptionCategory.ParkingType),MinimumScore(buyer.MinParkingCount,seller.ParkingCount)),
            [MatchCriterion.Orientation]=CategoryScore(buyer,seller,OptionCategory.Orientation),
            [MatchCriterion.Decoration]=CategoryScore(buyer,seller,OptionCategory.Decoration),
            [MatchCriterion.Pet]=RequirementScore(buyer.PetRequirement,seller.PetsAllowed),
            [MatchCriterion.Elevator]=RequirementScore(buyer.ElevatorRequirement,seller.HasElevator),
            [MatchCriterion.School]=CategoryScore(buyer,seller,OptionCategory.School),
            [MatchCriterion.Transport]=CategoryScore(buyer,seller,OptionCategory.Transport)
        };
    }

    private static double CategoryScore(Buyer buyer, SellerListing seller, OptionCategory category) => CalculateSetScore(
        buyer.Options.Where(x=>x.Category==category).Select(x=>x.Id), seller.Options.Where(x=>x.Category==category).Select(x=>x.Id));

    private static double CalculateRangeScore(decimal? minimum, decimal? maximum, decimal? actual)
    {
        if (!actual.HasValue || (!minimum.HasValue && !maximum.HasValue)) return 1;
        if (minimum.HasValue && actual < minimum) return Math.Clamp((double)(actual.Value / minimum.Value),0,1);
        if (maximum.HasValue && actual > maximum) return Math.Clamp((double)(maximum.Value / actual.Value),0,1);
        return 1;
    }

    private static double MinimumScore(int? minimum, int? actual)
    {
        if (!minimum.HasValue || !actual.HasValue || actual >= minimum) return 1;
        return minimum == 0 ? 1 : (double)actual.Value / minimum.Value;
    }

    private static double RequirementScore(RequirementLevel requirement, AvailabilityStatus actual) => requirement switch
    {
        RequirementLevel.NoPreference => 1,
        RequirementLevel.Required => actual == AvailabilityStatus.Yes ? 1 : actual == AvailabilityStatus.Unknown ? 0.5 : 0,
        RequirementLevel.Excluded => actual == AvailabilityStatus.No ? 1 : actual == AvailabilityStatus.Unknown ? 0.5 : 0,
        _ => 0
    };

    private static double Average(params double[] values) => values.Average();

    private static double CalculatePriceScore(decimal? buyerMaxPrice, decimal? sellerPrice)
    {
        if (!buyerMaxPrice.HasValue || !sellerPrice.HasValue || buyerMaxPrice.Value == 0)
        {
            return 1.0;
        }

        if (sellerPrice.Value <= buyerMaxPrice.Value)
        {
            return 1.0;
        }

        return Math.Clamp((double)(buyerMaxPrice.Value / sellerPrice.Value), 0.0, 1.0);
    }
}

public enum MatchCriterion
{
    Price, Location, Address, HouseType, Layout, Area, Floor, BuildingAge,
    Parking, Orientation, Decoration, Pet, Elevator, School, Transport
}

public sealed record MatchCriteria(IReadOnlyList<MatchCriterion> Order);

internal sealed record LayoutMatchScores(double Bedrooms, double LivingRooms, double Bathrooms)
{
    public double Total => (Bedrooms + LivingRooms + Bathrooms) / 3.0;
}
