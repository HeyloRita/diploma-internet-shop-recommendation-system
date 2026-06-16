using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Services;
using InternetShop.WPF.ViewModels.Admin;
using InternetShop.WPF.ViewModels.Customer;
using InternetShop.WPF.Views;
using InternetShop.WPF.Views.Admin;
using InternetShop.WPF.Views.Customer;

namespace InternetShop.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly NavigationService _nav;
    private readonly AuthState _auth;

    public string UserName => _auth.Name ?? string.Empty;
    public bool   IsAdmin  => _auth.IsAdmin;

    public MainViewModel(NavigationService nav, AuthState auth)
    {
        _nav  = nav;
        _auth = auth;
    }

    public void Initialize()
    {
        if (_auth.IsAdmin)
            NavigateAnalytics();
        else
            NavigateCatalog();
    }

    [RelayCommand] private void NavigateCatalog()
    {
        var vm   = App.Services.GetService(typeof(CatalogViewModel)) as CatalogViewModel;
        var view = new CatalogView { DataContext = vm };
        vm!.NavigateToDetail += NavigateToDetail;
        _nav.Navigate(view);
        _ = vm.LoadAsync();
    }

    [RelayCommand] private void NavigateRecommendations()
    {
        var vm   = App.Services.GetService(typeof(RecommendationsViewModel)) as RecommendationsViewModel;
        var view = new RecommendationsView { DataContext = vm };
        vm!.NavigateToDetail += NavigateToDetail;
        _nav.Navigate(view);
        _ = vm.LoadAsync();
    }

    [RelayCommand] private void NavigateOrders()
    {
        var vm   = App.Services.GetService(typeof(OrderHistoryViewModel)) as OrderHistoryViewModel;
        var view = new OrderHistoryView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    private void NavigateToDetail(int productId)
    {
        var vm   = App.Services.GetService(typeof(ProductDetailViewModel)) as ProductDetailViewModel;
        var view = new ProductDetailView { DataContext = vm };
        vm!.NavigateToDetail += NavigateToDetail;
        vm.Back += NavigateCatalog;
        _nav.Navigate(view);
        _ = vm.LoadAsync(productId);
    }

    [RelayCommand] private void NavigateLogs()
    {
        var vm   = App.Services.GetService(typeof(AdminLogsViewModel)) as AdminLogsViewModel;
        var view = new AdminLogsView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand] private void NavigateRestock()
    {
        var vm   = App.Services.GetService(typeof(AdminRestockViewModel)) as AdminRestockViewModel;
        var view = new AdminRestockView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand] private void NavigateSettings()
    {
        var vm   = App.Services.GetService(typeof(AdminSettingsViewModel)) as AdminSettingsViewModel;
        var view = new AdminSettingsView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand] private void NavigateAnalytics()
    {
        var vm   = App.Services.GetService(typeof(AdminAnalyticsViewModel)) as AdminAnalyticsViewModel;
        var view = new AdminAnalyticsView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand] private void NavigateProducts()
    {
        var vm   = App.Services.GetService(typeof(AdminProductsViewModel)) as AdminProductsViewModel;
        var view = new AdminProductsView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand] private void NavigateRecommendationsInfo()
    {
        var vm   = App.Services.GetService(typeof(AdminRecommendationsViewModel)) as AdminRecommendationsViewModel;
        var view = new AdminRecommendationsView { DataContext = vm };
        _nav.Navigate(view);
        _ = vm!.LoadAsync();
    }

    [RelayCommand]
    private void Logout(Window window)
    {
        _auth.Logout();
        var login = new LoginWindow
        {
            DataContext = App.Services.GetService(typeof(LoginViewModel))
        };
        login.Show();
        window.Close();
    }
}
