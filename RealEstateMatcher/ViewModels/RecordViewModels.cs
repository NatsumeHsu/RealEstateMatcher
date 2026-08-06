namespace RealEstateMatcher.ViewModels;

public sealed record LookupItem(int Id, string Name);
public sealed record LayoutListItem(int Id, string Name);
public sealed record HouseTypeListItem(int Id, string Name);
public sealed record CategorizedOptionItem(int Id, Models.OptionCategory Category, string Name);

public sealed class SelectableLayout : ObservableObject
{
    private bool _isSelected;
    public SelectableLayout(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
}

public sealed class SelectableHouseType : ObservableObject
{
    private bool _isSelected;
    public SelectableHouseType(int id, string name) { Id = id; Name = name; }
    public int Id { get; }
    public string Name { get; }
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
}

public sealed class BuyerListItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string LineId { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public IReadOnlyList<LookupItem> HouseTypes { get; init; } = Array.Empty<LookupItem>();
    public IReadOnlyList<LookupItem> Layouts { get; init; } = Array.Empty<LookupItem>();
    public CriteriaValues Criteria { get; init; } = new();
    public IReadOnlyList<CategorizedOptionItem> Options { get; init; } = [];

    public string PriceText => Price?.ToString("N0") ?? "不限";
    public string HouseTypesText => HouseTypes.Count == 0 ? "不限" : string.Join("、", HouseTypes.Select(x => x.Name));
    public string LayoutsText => Layouts.Count == 0 ? "不限" : string.Join("、", Layouts.Select(x => x.Name));
    public string DisplayName => $"{Name}｜預算：{PriceText}｜{Address}";
}

public sealed class SellerListItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string LineId { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public IReadOnlyList<LookupItem> HouseTypes { get; init; } = Array.Empty<LookupItem>();
    public IReadOnlyList<LookupItem> Layouts { get; init; } = Array.Empty<LookupItem>();
    public CriteriaValues Criteria { get; init; } = new();
    public IReadOnlyList<CategorizedOptionItem> Options { get; init; } = [];

    public string PriceText => Price?.ToString("N0") ?? "不限";
    public string HouseTypesText => HouseTypes.Count == 0 ? "不限" : string.Join("、", HouseTypes.Select(x => x.Name));
    public string LayoutsText => Layouts.Count == 0 ? "不限" : string.Join("、", Layouts.Select(x => x.Name));
    public string DisplayName => $"{Name}｜開價：{PriceText}｜{Address}";
}

public sealed class BuyerInput
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string LineId { get; init; } = string.Empty;
    public decimal? MaxPrice { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public IReadOnlyList<string> HouseTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Layouts { get; init; } = Array.Empty<string>();
    public CriteriaValues Criteria { get; init; } = new();
    public IReadOnlyDictionary<Models.OptionCategory, IReadOnlyList<string>> Options { get; init; } = new Dictionary<Models.OptionCategory, IReadOnlyList<string>>();
}

public sealed class SellerInput
{
    public int? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string LineId { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public string Address { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public IReadOnlyList<string> HouseTypes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Layouts { get; init; } = Array.Empty<string>();
    public CriteriaValues Criteria { get; init; } = new();
    public IReadOnlyDictionary<Models.OptionCategory, IReadOnlyList<string>> Options { get; init; } = new Dictionary<Models.OptionCategory, IReadOnlyList<string>>();
}

public sealed class CriteriaValues
{
    public decimal? MinPrice { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MaxBedrooms { get; init; }
    public int? MinLivingRooms { get; init; }
    public int? MaxLivingRooms { get; init; }
    public int? MinBathrooms { get; init; }
    public int? MaxBathrooms { get; init; }
    public decimal? MinMainArea { get; init; }
    public decimal? MaxMainArea { get; init; }
    public decimal? MinDeedArea { get; init; }
    public decimal? MaxDeedArea { get; init; }
    public int? MinFloor { get; init; }
    public int? MaxFloor { get; init; }
    public int? MinTotalFloors { get; init; }
    public int? MaxTotalFloors { get; init; }
    public decimal? MinBuildingAge { get; init; }
    public decimal? MaxBuildingAge { get; init; }
    public int? MinParkingCount { get; init; }
    public Models.RequirementLevel PetRequirement { get; init; }
    public Models.RequirementLevel ElevatorRequirement { get; init; }
    public string ExclusionNotes { get; init; } = string.Empty;
}

public sealed class MatchResultItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public string LineId { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public string Address { get; init; } = string.Empty;
    public string HouseTypesText { get; init; } = string.Empty;
    public string LayoutsText { get; init; } = string.Empty;
    public double AddressSimilarity { get; init; }
    public double PriceScore { get; init; }
    public double HouseTypeScore { get; init; }
    public double LayoutScore { get; init; }
    public double BedroomScore { get; init; }
    public double LivingRoomScore { get; init; }
    public double BathroomScore { get; init; }
    public double TotalScore { get; init; }
    public IReadOnlyDictionary<Services.MatchCriterion, double> Scores { get; init; } = new Dictionary<Services.MatchCriterion, double>();

    public string PriceText => Price?.ToString("N0") ?? "不限";
    public string AddressSimilarityText => AddressSimilarity.ToString("P0");
    public string PriceScoreText => PriceScore.ToString("P0");
    public string HouseTypeScoreText => HouseTypeScore.ToString("P0");
    public string LayoutScoreText => LayoutScore.ToString("P0");
    public string BedroomScoreText => BedroomScore.ToString("P0");
    public string LivingRoomScoreText => LivingRoomScore.ToString("P0");
    public string BathroomScoreText => BathroomScore.ToString("P0");
    public string TotalScoreText => TotalScore.ToString("P0");
    public string ContactText => $"手機：{Mobile}  LINE：{LineId}";
}
