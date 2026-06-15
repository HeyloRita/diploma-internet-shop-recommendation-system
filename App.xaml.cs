using System.Windows;
using InternetShop.WPF.Services;
using InternetShop.WPF.ViewModels;
using InternetShop.WPF.ViewModels.Admin;
using InternetShop.WPF.Views.Admin;
using InternetShop.WPF.ViewModels.Customer;
using InternetShop.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace InternetShop.WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var sc = new ServiceCollection();

        sc.AddHttpClient<ApiService>();

        sc.AddSingleton<AuthState>();
        sc.AddSingleton<NavigationService>();

        sc.AddTransient<LoginViewModel>();
        sc.AddTransient<MainViewModel>();
        sc.AddTransient<CatalogViewModel>();
        sc.AddTransient<ProductDetailViewModel>();
        sc.AddTransient<RecommendationsViewModel>();
        sc.AddTransient<OrderHistoryViewModel>();

        sc.AddTransient<AdminLogsViewModel>();
        sc.AddTransient<AdminRestockViewModel>();
        sc.AddTransient<AdminSettingsViewModel>();
        sc.AddTransient<AdminAnalyticsViewModel>();
        sc.AddTransient<AdminRecommendationsViewModel>();
        sc.AddTransient<AdminProductsViewModel>();

        Services = sc.BuildServiceProvider();

        var loginWindow = new LoginWindow
        {
            DataContext = Services.GetRequiredService<LoginViewModel>()
        };
        loginWindow.Show();
    }
}
