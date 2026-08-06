using System.Windows;
using RealEstateMatcher.ViewModels;

namespace RealEstateMatcher;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
