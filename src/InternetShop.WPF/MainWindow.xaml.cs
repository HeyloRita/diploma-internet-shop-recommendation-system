using System.Windows;
using InternetShop.WPF.Services;
using InternetShop.WPF.ViewModels;

namespace InternetShop.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var nav = App.Services.GetService(typeof(NavigationService)) as NavigationService;
        nav!.Initialize(ContentArea);

        if (DataContext is MainViewModel vm)
            vm.Initialize();
    }
}
