namespace RealEstateMatcher.ViewModels;

public sealed class PartyFormViewModel : ObservableObject
{
    private int? _id;
    private string _name = string.Empty;
    private string _mobile = string.Empty;
    private string _lineId = string.Empty;
    private string _priceText = string.Empty;
    private string _address = string.Empty;
    private string _houseTypesText = string.Empty;
    private string _layoutsText = string.Empty;
    private string _bedroomsText = string.Empty;
    private string _livingRoomsText = string.Empty;
    private string _bathroomsText = string.Empty;
    private string _note = string.Empty;
    private string _minPriceText = string.Empty;
    private string _minBedroomsText = string.Empty;
    private string _maxBedroomsText = string.Empty;
    private string _minLivingRoomsText = string.Empty;
    private string _maxLivingRoomsText = string.Empty;
    private string _minBathroomsText = string.Empty;
    private string _maxBathroomsText = string.Empty;
    private string _minMainAreaText = string.Empty;
    private string _maxMainAreaText = string.Empty;
    private string _minDeedAreaText = string.Empty;
    private string _maxDeedAreaText = string.Empty;
    private string _minFloorText = string.Empty;
    private string _maxFloorText = string.Empty;
    private string _minTotalFloorsText = string.Empty;
    private string _maxTotalFloorsText = string.Empty;
    private string _minBuildingAgeText = string.Empty;
    private string _maxBuildingAgeText = string.Empty;
    private string _minParkingCountText = string.Empty;
    private string _citiesText = string.Empty;
    private string _districtsText = string.Empty;
    private string _parkingTypesText = string.Empty;
    private string _orientationsText = string.Empty;
    private string _decorationStatusesText = string.Empty;
    private string _schoolsText = string.Empty;
    private string _transportText = string.Empty;
    private string _exclusionNotes = string.Empty;
    private int _petRequirement;
    private int _elevatorRequirement;

    public int? Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Mobile
    {
        get => _mobile;
        set => SetProperty(ref _mobile, value);
    }

    public string LineId
    {
        get => _lineId;
        set => SetProperty(ref _lineId, value);
    }

    public string PriceText
    {
        get => _priceText;
        set => SetProperty(ref _priceText, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string HouseTypesText
    {
        get => _houseTypesText;
        set => SetProperty(ref _houseTypesText, value);
    }

    public string LayoutsText
    {
        get => _layoutsText;
        set => SetProperty(ref _layoutsText, value);
    }

    public string BedroomsText { get => _bedroomsText; set => SetProperty(ref _bedroomsText, value); }
    public string LivingRoomsText { get => _livingRoomsText; set => SetProperty(ref _livingRoomsText, value); }
    public string BathroomsText { get => _bathroomsText; set => SetProperty(ref _bathroomsText, value); }
    public string MinPriceText { get => _minPriceText; set => SetProperty(ref _minPriceText, value); }
    public string MinBedroomsText { get => _minBedroomsText; set => SetProperty(ref _minBedroomsText, value); }
    public string MaxBedroomsText { get => _maxBedroomsText; set => SetProperty(ref _maxBedroomsText, value); }
    public string MinLivingRoomsText { get => _minLivingRoomsText; set => SetProperty(ref _minLivingRoomsText, value); }
    public string MaxLivingRoomsText { get => _maxLivingRoomsText; set => SetProperty(ref _maxLivingRoomsText, value); }
    public string MinBathroomsText { get => _minBathroomsText; set => SetProperty(ref _minBathroomsText, value); }
    public string MaxBathroomsText { get => _maxBathroomsText; set => SetProperty(ref _maxBathroomsText, value); }
    public string MinMainAreaText { get => _minMainAreaText; set => SetProperty(ref _minMainAreaText, value); }
    public string MaxMainAreaText { get => _maxMainAreaText; set => SetProperty(ref _maxMainAreaText, value); }
    public string MinDeedAreaText { get => _minDeedAreaText; set => SetProperty(ref _minDeedAreaText, value); }
    public string MaxDeedAreaText { get => _maxDeedAreaText; set => SetProperty(ref _maxDeedAreaText, value); }
    public string MinFloorText { get => _minFloorText; set => SetProperty(ref _minFloorText, value); }
    public string MaxFloorText { get => _maxFloorText; set => SetProperty(ref _maxFloorText, value); }
    public string MinTotalFloorsText { get => _minTotalFloorsText; set => SetProperty(ref _minTotalFloorsText, value); }
    public string MaxTotalFloorsText { get => _maxTotalFloorsText; set => SetProperty(ref _maxTotalFloorsText, value); }
    public string MinBuildingAgeText { get => _minBuildingAgeText; set => SetProperty(ref _minBuildingAgeText, value); }
    public string MaxBuildingAgeText { get => _maxBuildingAgeText; set => SetProperty(ref _maxBuildingAgeText, value); }
    public string MinParkingCountText { get => _minParkingCountText; set => SetProperty(ref _minParkingCountText, value); }
    public string CitiesText { get => _citiesText; set => SetProperty(ref _citiesText, value); }
    public string DistrictsText { get => _districtsText; set => SetProperty(ref _districtsText, value); }
    public string ParkingTypesText { get => _parkingTypesText; set => SetProperty(ref _parkingTypesText, value); }
    public string OrientationsText { get => _orientationsText; set => SetProperty(ref _orientationsText, value); }
    public string DecorationStatusesText { get => _decorationStatusesText; set => SetProperty(ref _decorationStatusesText, value); }
    public string SchoolsText { get => _schoolsText; set => SetProperty(ref _schoolsText, value); }
    public string TransportText { get => _transportText; set => SetProperty(ref _transportText, value); }
    public string ExclusionNotes { get => _exclusionNotes; set => SetProperty(ref _exclusionNotes, value); }
    public int PetRequirement { get => _petRequirement; set => SetProperty(ref _petRequirement, value); }
    public int ElevatorRequirement { get => _elevatorRequirement; set => SetProperty(ref _elevatorRequirement, value); }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public void Clear()
    {
        Id = null;
        Name = string.Empty;
        Mobile = string.Empty;
        LineId = string.Empty;
        PriceText = string.Empty;
        Address = string.Empty;
        HouseTypesText = string.Empty;
        LayoutsText = string.Empty;
        BedroomsText = string.Empty;
        LivingRoomsText = string.Empty;
        BathroomsText = string.Empty;
        LoadCriteria(new CriteriaValues());
        LoadOptions([]);
        Note = string.Empty;
    }

    public void Load(BuyerListItem item)
    {
        Id = item.Id;
        Name = item.Name;
        Mobile = item.Mobile;
        LineId = item.LineId;
        PriceText = item.Price?.ToString("0.##") ?? string.Empty;
        Address = item.Address;
        HouseTypesText = string.Join("、", item.HouseTypes.Select(x => x.Name));
        LayoutsText = string.Join("、", item.Layouts.Select(x => x.Name));
        LoadLayoutParts(LayoutsText);
        LoadCriteria(item.Criteria);
        LoadOptions(item.Options);
        Note = item.Note;
    }

    public void Load(SellerListItem item)
    {
        Id = item.Id;
        Name = item.Name;
        Mobile = item.Mobile;
        LineId = item.LineId;
        PriceText = item.Price?.ToString("0.##") ?? string.Empty;
        Address = item.Address;
        HouseTypesText = string.Join("、", item.HouseTypes.Select(x => x.Name));
        LayoutsText = string.Join("、", item.Layouts.Select(x => x.Name));
        LoadLayoutParts(LayoutsText);
        LoadCriteria(item.Criteria);
        LoadOptions(item.Options);
        Note = item.Note;
    }

    private void LoadLayoutParts(string value)
    {
        Services.LayoutParts parts = Services.LayoutParser.Parse(value);
        BedroomsText = parts.Bedrooms?.ToString() ?? string.Empty;
        LivingRoomsText = parts.LivingRooms?.ToString() ?? string.Empty;
        BathroomsText = parts.Bathrooms?.ToString() ?? string.Empty;
    }

    private void LoadCriteria(CriteriaValues c)
    {
        MinPriceText = Number(c.MinPrice); MinBedroomsText = Number(c.MinBedrooms); MaxBedroomsText = Number(c.MaxBedrooms);
        MinLivingRoomsText = Number(c.MinLivingRooms); MaxLivingRoomsText = Number(c.MaxLivingRooms);
        MinBathroomsText = Number(c.MinBathrooms); MaxBathroomsText = Number(c.MaxBathrooms);
        MinMainAreaText = Number(c.MinMainArea); MaxMainAreaText = Number(c.MaxMainArea);
        MinDeedAreaText = Number(c.MinDeedArea); MaxDeedAreaText = Number(c.MaxDeedArea);
        MinFloorText = Number(c.MinFloor); MaxFloorText = Number(c.MaxFloor);
        MinTotalFloorsText = Number(c.MinTotalFloors); MaxTotalFloorsText = Number(c.MaxTotalFloors);
        MinBuildingAgeText = Number(c.MinBuildingAge); MaxBuildingAgeText = Number(c.MaxBuildingAge);
        MinParkingCountText = Number(c.MinParkingCount); PetRequirement = (int)c.PetRequirement;
        ElevatorRequirement = (int)c.ElevatorRequirement; ExclusionNotes = c.ExclusionNotes;
    }

    private void LoadOptions(IEnumerable<CategorizedOptionItem> options)
    {
        CitiesText = Join(Models.OptionCategory.City); DistrictsText = Join(Models.OptionCategory.District);
        ParkingTypesText = Join(Models.OptionCategory.ParkingType); OrientationsText = Join(Models.OptionCategory.Orientation);
        DecorationStatusesText = Join(Models.OptionCategory.Decoration); SchoolsText = Join(Models.OptionCategory.School);
        TransportText = Join(Models.OptionCategory.Transport);
        string Join(Models.OptionCategory category) => string.Join("、", options.Where(x => x.Category == category).Select(x => x.Name));
    }

    private static string Number<T>(T? value) where T : struct => value?.ToString() ?? string.Empty;
}
