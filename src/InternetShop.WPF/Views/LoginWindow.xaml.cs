using System.Windows;
using InternetShop.WPF.ViewModels;

namespace InternetShop.WPF.Views;

public partial class LoginWindow : Window
{
    public LoginWindow() => InitializeComponent();

    private void OnLoginClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.LoginPassword = LoginPwd.Password;
            vm.LoginCommand.Execute(this);
        }
    }

    private void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.RegPassword = RegPwd.Password;
            vm.RegisterCommand.Execute(this);
        }
    }
}
