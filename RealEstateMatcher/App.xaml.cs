using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RealEstateMatcher.Data;
using RealEstateMatcher.Services;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Directory.CreateDirectory("data");
            string databasePath = Path.Combine("data", "realestate.db");

            var factory = new AppDbContextFactory(
                new DbContextOptionsBuilder<RealEstateDbContext>()
                    .UseSqlite($"Data Source={databasePath}")
                    .Options);

            await DatabaseInitializer.InitializeAsync(factory);

            var addressSimilarityService = new AddressSimilarityService();
            var buyerService = new BuyerService(factory);
            var sellerService = new SellerService(factory);
            var layoutService = new LayoutService(factory);
            var houseTypeService = new HouseTypeService(factory);
            var matchingService = new MatchingService(factory, addressSimilarityService);

            var viewModel = new MainViewModel(
                buyerService,
                sellerService,
                layoutService,
                houseTypeService,
                matchingService,
                databasePath);

            await viewModel.InitializeAsync();

            var window = new MainWindow(viewModel);
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"程式啟動失敗：\n{ex.Message}",
                "啟動錯誤",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }
}
