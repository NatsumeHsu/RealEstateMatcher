using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using RealEstateMatcher.Services;

namespace RealEstateMatcher.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly BuyerService _buyerService;
    private readonly SellerService _sellerService;
    private readonly LayoutService _layoutService;
    private readonly HouseTypeService _houseTypeService;
    private readonly MatchingService _matchingService;

    private BuyerListItem? _selectedBuyer;
    private SellerListItem? _selectedSeller;
    private BuyerListItem? _selectedMatchBuyer;
    private SellerListItem? _selectedMatchSeller;
    private string _statusMessage = "就緒";
    private MatchCriterionOption? _firstCriterion;
    private MatchCriterionOption? _secondCriterion;
    private MatchCriterionOption? _thirdCriterion;
    private MatchCriterionOption? _fourthCriterion;
    private bool _normalizingCriteria;
    private LayoutListItem? _selectedLayout;
    private string _layoutBedrooms = "3";
    private string _layoutLivingRooms = "2";
    private string _layoutBathrooms = "2";
    private HouseTypeListItem? _selectedHouseType;
    private string _houseTypeName = string.Empty;

    public MainViewModel(
        BuyerService buyerService,
        SellerService sellerService,
        LayoutService layoutService,
        HouseTypeService houseTypeService,
        MatchingService matchingService,
        string databasePath)
    {
        _buyerService = buyerService;
        _sellerService = sellerService;
        _layoutService = layoutService;
        _houseTypeService = houseTypeService;
        _matchingService = matchingService;
        DatabasePath = databasePath;

        _firstCriterion = MatchCriterionOptions[1];
        _secondCriterion = MatchCriterionOptions[2];
        _thirdCriterion = MatchCriterionOptions[3];
        _fourthCriterion = MatchCriterionOptions[4];

        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        NewBuyerCommand = new RelayCommand(NewBuyer);
        SaveBuyerCommand = new AsyncRelayCommand(SaveBuyerAsync);
        DeleteBuyerCommand = new AsyncRelayCommand(DeleteBuyerAsync);
        NewSellerCommand = new RelayCommand(NewSeller);
        SaveSellerCommand = new AsyncRelayCommand(SaveSellerAsync);
        DeleteSellerCommand = new AsyncRelayCommand(DeleteSellerAsync);
        MatchSellersCommand = new AsyncRelayCommand(MatchSellersAsync);
        MatchBuyersCommand = new AsyncRelayCommand(MatchBuyersAsync);
        NewLayoutCommand = new RelayCommand(NewLayout);
        SaveLayoutCommand = new AsyncRelayCommand(SaveLayoutAsync);
        DeleteLayoutCommand = new AsyncRelayCommand(DeleteLayoutAsync);
        NewHouseTypeCommand = new RelayCommand(NewHouseType);
        SaveHouseTypeCommand = new AsyncRelayCommand(SaveHouseTypeAsync);
        DeleteHouseTypeCommand = new AsyncRelayCommand(DeleteHouseTypeAsync);
    }

    public string DatabasePath { get; }

    public ObservableCollection<BuyerListItem> Buyers { get; } = [];
    public ObservableCollection<SellerListItem> Sellers { get; } = [];
    public ObservableCollection<MatchResultItem> SellerMatches { get; } = [];
    public ObservableCollection<MatchResultItem> BuyerMatches { get; } = [];
    public ObservableCollection<LayoutListItem> LayoutItems { get; } = [];
    public ObservableCollection<SelectableLayout> BuyerLayoutOptions { get; } = [];
    public ObservableCollection<SelectableLayout> SellerLayoutOptions { get; } = [];
    public ObservableCollection<HouseTypeListItem> HouseTypeItems { get; } = [];
    public ObservableCollection<SelectableHouseType> BuyerHouseTypeOptions { get; } = [];
    public ObservableCollection<SelectableHouseType> SellerHouseTypeOptions { get; } = [];

    public PartyFormViewModel BuyerForm { get; } = new();
    public PartyFormViewModel SellerForm { get; } = new();

    public AsyncRelayCommand RefreshCommand { get; }
    public RelayCommand NewBuyerCommand { get; }
    public AsyncRelayCommand SaveBuyerCommand { get; }
    public AsyncRelayCommand DeleteBuyerCommand { get; }
    public RelayCommand NewSellerCommand { get; }
    public AsyncRelayCommand SaveSellerCommand { get; }
    public AsyncRelayCommand DeleteSellerCommand { get; }
    public AsyncRelayCommand MatchSellersCommand { get; }
    public AsyncRelayCommand MatchBuyersCommand { get; }
    public RelayCommand NewLayoutCommand { get; }
    public AsyncRelayCommand SaveLayoutCommand { get; }
    public AsyncRelayCommand DeleteLayoutCommand { get; }
    public RelayCommand NewHouseTypeCommand { get; }
    public AsyncRelayCommand SaveHouseTypeCommand { get; }
    public AsyncRelayCommand DeleteHouseTypeCommand { get; }

    public HouseTypeListItem? SelectedHouseType
    {
        get => _selectedHouseType;
        set { if (SetProperty(ref _selectedHouseType, value) && value is not null) HouseTypeName = value.Name; }
    }
    public string HouseTypeName { get => _houseTypeName; set => SetProperty(ref _houseTypeName, value); }

    public LayoutListItem? SelectedLayout
    {
        get => _selectedLayout;
        set
        {
            if (SetProperty(ref _selectedLayout, value) && value is not null)
            {
                LayoutParts p = LayoutParser.Parse(value.Name);
                LayoutBedrooms = p.Bedrooms?.ToString() ?? "0";
                LayoutLivingRooms = p.LivingRooms?.ToString() ?? "0";
                LayoutBathrooms = p.Bathrooms?.ToString() ?? "0";
            }
        }
    }
    public string LayoutBedrooms { get => _layoutBedrooms; set => SetProperty(ref _layoutBedrooms, value); }
    public string LayoutLivingRooms { get => _layoutLivingRooms; set => SetProperty(ref _layoutLivingRooms, value); }
    public string LayoutBathrooms { get => _layoutBathrooms; set => SetProperty(ref _layoutBathrooms, value); }

    public BuyerListItem? SelectedBuyer
    {
        get => _selectedBuyer;
        set
        {
            if (SetProperty(ref _selectedBuyer, value) && value is not null)
            {
                BuyerForm.Load(value);
                SelectLayouts(BuyerLayoutOptions, value.Layouts);
                SelectHouseTypes(BuyerHouseTypeOptions, value.HouseTypes);
            }
        }
    }

    public SellerListItem? SelectedSeller
    {
        get => _selectedSeller;
        set
        {
            if (SetProperty(ref _selectedSeller, value) && value is not null)
            {
                SellerForm.Load(value);
                SelectLayouts(SellerLayoutOptions, value.Layouts);
                SelectHouseTypes(SellerHouseTypeOptions, value.HouseTypes);
            }
        }
    }

    public BuyerListItem? SelectedMatchBuyer
    {
        get => _selectedMatchBuyer;
        set => SetProperty(ref _selectedMatchBuyer, value);
    }

    public SellerListItem? SelectedMatchSeller
    {
        get => _selectedMatchSeller;
        set => SetProperty(ref _selectedMatchSeller, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public IReadOnlyList<MatchCriterionOption> MatchCriterionOptions { get; } =
    [
        new("不使用", null),
        new("價格／預算", MatchCriterion.Price),
        new("縣市／行政區", MatchCriterion.Location),
        new("地址", MatchCriterion.Address),
        new("房屋類型", MatchCriterion.HouseType),
        new("房／廳／衛", MatchCriterion.Layout),
        new("坪數", MatchCriterion.Area),
        new("樓層", MatchCriterion.Floor),
        new("屋齡", MatchCriterion.BuildingAge),
        new("車位", MatchCriterion.Parking),
        new("朝向", MatchCriterion.Orientation),
        new("裝潢", MatchCriterion.Decoration),
        new("寵物", MatchCriterion.Pet),
        new("電梯", MatchCriterion.Elevator),
        new("學區", MatchCriterion.School),
        new("交通", MatchCriterion.Transport)
    ];
    public IReadOnlyList<RequirementChoice> BuyerRequirementChoices { get; } =
    [new(0,"不限"), new(1,"必須"), new(2,"排除")];
    public IReadOnlyList<RequirementChoice> SellerAvailabilityChoices { get; } =
    [new(0,"未知"), new(1,"無／不允許"), new(2,"有／允許")];

    public IReadOnlyList<MatchCriterionOption> FirstCriterionOptions => MatchCriterionOptions;
    public IReadOnlyList<MatchCriterionOption> SecondCriterionOptions => ExcludeSelected(FirstCriterion);
    public IReadOnlyList<MatchCriterionOption> ThirdCriterionOptions => ExcludeSelected(FirstCriterion, SecondCriterion);
    public IReadOnlyList<MatchCriterionOption> FourthCriterionOptions => ExcludeSelected(FirstCriterion, SecondCriterion, ThirdCriterion);

    public MatchCriterionOption? FirstCriterion
    {
        get => _firstCriterion;
        set
        {
            if (SetProperty(ref _firstCriterion, value)) NormalizeCriterionOrder();
        }
    }

    public MatchCriterionOption? SecondCriterion
    {
        get => _secondCriterion;
        set
        {
            if (SetProperty(ref _secondCriterion, value)) NormalizeCriterionOrder();
        }
    }

    public MatchCriterionOption? ThirdCriterion
    {
        get => _thirdCriterion;
        set
        {
            if (SetProperty(ref _thirdCriterion, value)) NormalizeCriterionOrder();
        }
    }

    public MatchCriterionOption? FourthCriterion
    {
        get => _fourthCriterion;
        set
        {
            if (SetProperty(ref _fourthCriterion, value)) NormalizeCriterionOrder();
        }
    }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            int? selectedBuyerId = SelectedBuyer?.Id;
            int? selectedSellerId = SelectedSeller?.Id;
            int? matchBuyerId = SelectedMatchBuyer?.Id;
            int? matchSellerId = SelectedMatchSeller?.Id;

            IReadOnlyList<BuyerListItem> buyers = await _buyerService.GetAllAsync();
            IReadOnlyList<SellerListItem> sellers = await _sellerService.GetAllAsync();
            IReadOnlyList<LayoutListItem> layouts = await _layoutService.GetAllAsync();
            IReadOnlyList<HouseTypeListItem> houseTypes = await _houseTypeService.GetAllAsync();

            ReplaceCollection(Buyers, buyers);
            ReplaceCollection(Sellers, sellers);
            ReplaceCollection(LayoutItems, layouts);
            ReplaceLayoutOptions(BuyerLayoutOptions, layouts);
            ReplaceLayoutOptions(SellerLayoutOptions, layouts);
            ReplaceCollection(HouseTypeItems, houseTypes);
            ReplaceHouseTypeOptions(BuyerHouseTypeOptions, houseTypes);
            ReplaceHouseTypeOptions(SellerHouseTypeOptions, houseTypes);

            SelectedBuyer = Buyers.FirstOrDefault(x => x.Id == selectedBuyerId);
            SelectedSeller = Sellers.FirstOrDefault(x => x.Id == selectedSellerId);
            SelectedMatchBuyer = Buyers.FirstOrDefault(x => x.Id == matchBuyerId) ?? Buyers.FirstOrDefault();
            SelectedMatchSeller = Sellers.FirstOrDefault(x => x.Id == matchSellerId) ?? Sellers.FirstOrDefault();

            StatusMessage = $"已載入 {Buyers.Count} 位買方、{Sellers.Count} 筆賣方房源。";
        });
    }

    private void NewBuyer()
    {
        SelectedBuyer = null;
        BuyerForm.Clear();
        StatusMessage = "已清空買方表單。";
    }

    private void NewSeller()
    {
        SelectedSeller = null;
        SellerForm.Clear();
        StatusMessage = "已清空賣方表單。";
    }

    private async Task SaveBuyerAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            BuyerInput input = new()
            {
                Id = BuyerForm.Id,
                Name = BuyerForm.Name,
                Mobile = BuyerForm.Mobile,
                LineId = BuyerForm.LineId,
                MaxPrice = ParseNullablePrice(BuyerForm.PriceText),
                Address = BuyerForm.Address,
                Note = BuyerForm.Note,
                HouseTypes = BuyerHouseTypeOptions.Where(x => x.IsSelected).Select(x => x.Name).ToArray(),
                Layouts = BuyerLayoutOptions.Where(x => x.IsSelected).Select(x => x.Name).ToArray(),
                Criteria = BuildCriteria(BuyerForm, true),
                Options = BuildOptions(BuyerForm)
            };

            int id = await _buyerService.SaveAsync(input);
            await RefreshAsync();
            SelectedBuyer = Buyers.FirstOrDefault(x => x.Id == id);
            SelectedMatchBuyer = Buyers.FirstOrDefault(x => x.Id == id);
            StatusMessage = "買方資料已儲存。";
        });
    }

    private async Task SaveSellerAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            SellerInput input = new()
            {
                Id = SellerForm.Id,
                Name = SellerForm.Name,
                Mobile = SellerForm.Mobile,
                LineId = SellerForm.LineId,
                Price = ParseNullablePrice(SellerForm.PriceText),
                Address = SellerForm.Address,
                Note = SellerForm.Note,
                HouseTypes = SellerHouseTypeOptions.Where(x => x.IsSelected).Select(x => x.Name).ToArray(),
                Layouts = SellerLayoutOptions.Where(x => x.IsSelected).Select(x => x.Name).ToArray(),
                Criteria = BuildCriteria(SellerForm, false),
                Options = BuildOptions(SellerForm)
            };

            int id = await _sellerService.SaveAsync(input);
            await RefreshAsync();
            SelectedSeller = Sellers.FirstOrDefault(x => x.Id == id);
            SelectedMatchSeller = Sellers.FirstOrDefault(x => x.Id == id);
            StatusMessage = "賣方房源已儲存。";
        });
    }

    private async Task DeleteBuyerAsync()
    {
        if (BuyerForm.Id is null)
        {
            StatusMessage = "請先選擇要刪除的買方。";
            return;
        }

        if (MessageBox.Show(
                $"確定刪除買方「{BuyerForm.Name}」？",
                "刪除確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            await _buyerService.DeleteAsync(BuyerForm.Id.Value);
            BuyerForm.Clear();
            SellerMatches.Clear();
            await RefreshAsync();
            StatusMessage = "買方資料已刪除。";
        });
    }

    private async Task DeleteSellerAsync()
    {
        if (SellerForm.Id is null)
        {
            StatusMessage = "請先選擇要刪除的賣方房源。";
            return;
        }

        if (MessageBox.Show(
                $"確定刪除賣方房源「{SellerForm.Name}」？",
                "刪除確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            await _sellerService.DeleteAsync(SellerForm.Id.Value);
            SellerForm.Clear();
            BuyerMatches.Clear();
            await RefreshAsync();
            StatusMessage = "賣方房源已刪除。";
        });
    }

    private async Task MatchSellersAsync()
    {
        if (SelectedMatchBuyer is null)
        {
            StatusMessage = "請先選擇買方。";
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            MatchCriteria criteria = GetMatchCriteria();
            IReadOnlyList<MatchResultItem> results =
                await _matchingService.FindSellersForBuyerAsync(
                    SelectedMatchBuyer.Id,
                    criteria);

            ReplaceCollection(SellerMatches, results);
            StatusMessage = $"已依所選條件排序 {results.Count} 筆賣方房源。";
        });
    }

    private async Task MatchBuyersAsync()
    {
        if (SelectedMatchSeller is null)
        {
            StatusMessage = "請先選擇賣方房源。";
            return;
        }

        await ExecuteSafeAsync(async () =>
        {
            MatchCriteria criteria = GetMatchCriteria();
            IReadOnlyList<MatchResultItem> results =
                await _matchingService.FindBuyersForSellerAsync(
                    SelectedMatchSeller.Id,
                    criteria);

            ReplaceCollection(BuyerMatches, results);
            StatusMessage = $"已依所選條件排序 {results.Count} 位買方。";
        });
    }

    private void NewLayout()
    {
        SelectedLayout = null;
        LayoutBedrooms = "3";
        LayoutLivingRooms = "2";
        LayoutBathrooms = "2";
    }

    private async Task SaveLayoutAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            int bedrooms = ParseRequiredCount(LayoutBedrooms, "房");
            int livingRooms = ParseRequiredCount(LayoutLivingRooms, "廳");
            int bathrooms = ParseRequiredCount(LayoutBathrooms, "衛");
            int id = await _layoutService.SaveAsync(SelectedLayout?.Id, bedrooms, livingRooms, bathrooms);
            await RefreshAsync();
            SelectedLayout = LayoutItems.FirstOrDefault(x => x.Id == id);
            StatusMessage = "格局項目已儲存。";
        });
    }

    private async Task DeleteLayoutAsync()
    {
        if (SelectedLayout is null) { StatusMessage = "請先選擇格局項目。"; return; }
        await ExecuteSafeAsync(async () =>
        {
            await _layoutService.DeleteAsync(SelectedLayout.Id);
            NewLayout();
            await RefreshAsync();
            StatusMessage = "格局項目已移除。";
        });
    }

    private void NewHouseType()
    {
        SelectedHouseType = null;
        HouseTypeName = string.Empty;
    }

    private async Task SaveHouseTypeAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            int id = await _houseTypeService.SaveAsync(SelectedHouseType?.Id, HouseTypeName);
            await RefreshAsync();
            SelectedHouseType = HouseTypeItems.FirstOrDefault(x => x.Id == id);
            StatusMessage = "房屋類型已儲存。";
        });
    }

    private async Task DeleteHouseTypeAsync()
    {
        if (SelectedHouseType is null) { StatusMessage = "請先選擇房屋類型。"; return; }
        await ExecuteSafeAsync(async () =>
        {
            await _houseTypeService.DeleteAsync(SelectedHouseType.Id);
            NewHouseType();
            await RefreshAsync();
            StatusMessage = "房屋類型已移除。";
        });
    }

    private MatchCriteria GetMatchCriteria()
    {
        MatchCriterion[] order =
        [
            .. new[] { FirstCriterion, SecondCriterion, ThirdCriterion, FourthCriterion }
                .Where(x => x?.Criterion is not null)
                .Select(x => x!.Criterion!.Value)
        ];

        if (order.Length == 0)
        {
            throw new InvalidOperationException("請至少選擇一個比對條件。");
        }

        if (order.Distinct().Count() != order.Length)
        {
            throw new InvalidOperationException("比對順位不可選擇重複條件。");
        }

        return new MatchCriteria(order);
    }

    private void NormalizeCriterionOrder()
    {
        if (_normalizingCriteria)
        {
            return;
        }

        _normalizingCriteria = true;
        try
        {
            var used = new HashSet<MatchCriterion>();
            MatchCriterionOption none = MatchCriterionOptions[0];

            RemoveDuplicate(ref _firstCriterion, nameof(FirstCriterion));
            RemoveDuplicate(ref _secondCriterion, nameof(SecondCriterion));
            RemoveDuplicate(ref _thirdCriterion, nameof(ThirdCriterion));
            RemoveDuplicate(ref _fourthCriterion, nameof(FourthCriterion));

            void RemoveDuplicate(ref MatchCriterionOption? option, string propertyName)
            {
                if (option?.Criterion is not MatchCriterion criterion)
                {
                    return;
                }

                if (used.Add(criterion))
                {
                    return;
                }

                option = none;
                OnPropertyChanged(propertyName);
            }
        }
        finally
        {
            _normalizingCriteria = false;
        }

        OnPropertyChanged(nameof(SecondCriterionOptions));
        OnPropertyChanged(nameof(ThirdCriterionOptions));
        OnPropertyChanged(nameof(FourthCriterionOptions));
    }

    private IReadOnlyList<MatchCriterionOption> ExcludeSelected(
        params MatchCriterionOption?[] higherPriorities)
    {
        HashSet<MatchCriterion> excluded = higherPriorities
            .Where(x => x?.Criterion is not null)
            .Select(x => x!.Criterion!.Value)
            .ToHashSet();

        return MatchCriterionOptions
            .Where(x => x.Criterion is null || !excluded.Contains(x.Criterion.Value))
            .ToArray();
    }

    private async Task ExecuteSafeAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            MessageBox.Show(ex.Message, "操作失敗", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static decimal? ParseNullablePrice(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("，", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (!decimal.TryParse(
                normalized,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal result))
        {
            throw new InvalidOperationException("價格格式錯誤，請輸入數字或留白。例：12000000");
        }

        return result;
    }

    private static CriteriaValues BuildCriteria(PartyFormViewModel f, bool buyer)
    {
        return new CriteriaValues
        {
            MinPrice = buyer ? ParseNullableDecimal(f.MinPriceText, "價格下限") : null,
            MinBedrooms=ParseNullableInt(f.MinBedroomsText,"房數"), MaxBedrooms=buyer?ParseNullableInt(f.MaxBedroomsText,"房數上限"):null,
            MinLivingRooms=ParseNullableInt(f.MinLivingRoomsText,"廳數"), MaxLivingRooms=buyer?ParseNullableInt(f.MaxLivingRoomsText,"廳數上限"):null,
            MinBathrooms=ParseNullableInt(f.MinBathroomsText,"衛浴數"), MaxBathrooms=buyer?ParseNullableInt(f.MaxBathroomsText,"衛浴上限"):null,
            MinMainArea=ParseNullableDecimal(f.MinMainAreaText,"主建物坪數"), MaxMainArea=buyer?ParseNullableDecimal(f.MaxMainAreaText,"主建物坪數上限"):null,
            MinDeedArea=ParseNullableDecimal(f.MinDeedAreaText,"權狀坪數"), MaxDeedArea=buyer?ParseNullableDecimal(f.MaxDeedAreaText,"權狀坪數上限"):null,
            MinFloor=ParseNullableInt(f.MinFloorText,"樓層"), MaxFloor=buyer?ParseNullableInt(f.MaxFloorText,"樓層上限"):null,
            MinTotalFloors=ParseNullableInt(f.MinTotalFloorsText,"總樓層"), MaxTotalFloors=buyer?ParseNullableInt(f.MaxTotalFloorsText,"總樓層上限"):null,
            MinBuildingAge=ParseNullableDecimal(f.MinBuildingAgeText,"屋齡"), MaxBuildingAge=buyer?ParseNullableDecimal(f.MaxBuildingAgeText,"屋齡上限"):null,
            MinParkingCount=ParseNullableInt(f.MinParkingCountText,"車位數量"), PetRequirement=(Models.RequirementLevel)f.PetRequirement,
            ElevatorRequirement=(Models.RequirementLevel)f.ElevatorRequirement, ExclusionNotes=f.ExclusionNotes
        };
    }

    private static IReadOnlyDictionary<Models.OptionCategory, IReadOnlyList<string>> BuildOptions(PartyFormViewModel f) =>
        new Dictionary<Models.OptionCategory, IReadOnlyList<string>>
        {
            [Models.OptionCategory.City]=TextListParser.Parse(f.CitiesText), [Models.OptionCategory.District]=TextListParser.Parse(f.DistrictsText),
            [Models.OptionCategory.ParkingType]=TextListParser.Parse(f.ParkingTypesText), [Models.OptionCategory.Orientation]=TextListParser.Parse(f.OrientationsText),
            [Models.OptionCategory.Decoration]=TextListParser.Parse(f.DecorationStatusesText), [Models.OptionCategory.School]=TextListParser.Parse(f.SchoolsText),
            [Models.OptionCategory.Transport]=TextListParser.Parse(f.TransportText)
        };

    private static int? ParseNullableInt(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!int.TryParse(value, out int result) || result < 0) throw new InvalidOperationException($"{label}必須是 0 以上整數或留白。");
        return result;
    }

    private static decimal? ParseNullableDecimal(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!decimal.TryParse(value, out decimal result) || result < 0) throw new InvalidOperationException($"{label}必須是 0 以上數字或留白。");
        return result;
    }

    private static int ParseRequiredCount(string value, string label)
    {
        if (!int.TryParse(value, out int count) || count < 0)
            throw new InvalidOperationException($"{label}數必須是 0 以上整數。");
        return count;
    }

    private static void ReplaceLayoutOptions(ObservableCollection<SelectableLayout> target, IEnumerable<LayoutListItem> layouts)
    {
        target.Clear();
        foreach (LayoutListItem item in layouts) target.Add(new SelectableLayout(item.Id, item.Name));
    }

    private static void SelectLayouts(ObservableCollection<SelectableLayout> options, IEnumerable<LookupItem> selected)
    {
        HashSet<int> ids = selected.Select(x => x.Id).ToHashSet();
        foreach (SelectableLayout option in options) option.IsSelected = ids.Contains(option.Id);
    }

    private static void ReplaceHouseTypeOptions(ObservableCollection<SelectableHouseType> target, IEnumerable<HouseTypeListItem> items)
    {
        target.Clear();
        foreach (HouseTypeListItem item in items) target.Add(new SelectableHouseType(item.Id, item.Name));
    }

    private static void SelectHouseTypes(ObservableCollection<SelectableHouseType> options, IEnumerable<LookupItem> selected)
    {
        HashSet<int> ids = selected.Select(x => x.Id).ToHashSet();
        foreach (SelectableHouseType option in options) option.IsSelected = ids.Contains(option.Id);
    }

    private static void ReplaceCollection<T>(
        ObservableCollection<T> target,
        IEnumerable<T> source)
    {
        target.Clear();
        foreach (T item in source)
        {
            target.Add(item);
        }
    }
}

public sealed record MatchCriterionOption(string Name, MatchCriterion? Criterion);
public sealed record RequirementChoice(int Value, string Name);
