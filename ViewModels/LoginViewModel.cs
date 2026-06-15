using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Services;
using InternetShop.WPF.Views;

namespace InternetShop.WPF.ViewModels;

public partial class LoginViewModel(ApiService api, AuthState auth) : ObservableObject
{
    [ObservableProperty] private string _loginEmail    = string.Empty;
    [ObservableProperty] private string _loginPassword = string.Empty;

    [ObservableProperty] private string _regName     = string.Empty;
    [ObservableProperty] private string _regEmail    = string.Empty;
    [ObservableProperty] private string _regPassword = string.Empty;

    [ObservableProperty] private string  _errorMessage  = string.Empty;
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private bool    _showRegister;

    [RelayCommand]
    private void ToggleForm()
    {
        ShowRegister = !ShowRegister;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task LoginAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            ErrorMessage = "Введите email и пароль.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var resp = await api.LoginAsync(LoginEmail, LoginPassword);
            if (resp is null) { ErrorMessage = "Неверный email или пароль."; return; }

            auth.Login(resp);
            OpenMainWindow(window);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RegisterAsync(Window window)
    {
        if (string.IsNullOrWhiteSpace(RegName) ||
            string.IsNullOrWhiteSpace(RegEmail) ||
            string.IsNullOrWhiteSpace(RegPassword))
        {
            ErrorMessage = "Заполните все поля.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var resp = await api.RegisterAsync(RegName, RegEmail, RegPassword);
            if (resp is null) { ErrorMessage = "Email уже используется."; return; }

            auth.Login(resp);
            OpenMainWindow(window);
        }
        finally { IsBusy = false; }
    }

    private static void OpenMainWindow(Window current)
    {
        var main = new MainWindow
        {
            DataContext = App.Services.GetService(typeof(MainViewModel))
        };
        main.Show();
        current.Close();
    }
}
