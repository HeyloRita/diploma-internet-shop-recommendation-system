using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminSettingsViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private int    _lowStockThreshold   = 5;
    [ObservableProperty] private int    _recommendationCount = 10;
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool   _isSuccess;

    public async Task LoadAsync()
    {
        IsBusy        = true;
        StatusMessage = string.Empty;
        try
        {
            var s = await api.GetSettingsAsync();
            if (s is null) return;
            LowStockThreshold   = s.LowStockThreshold;
            RecommendationCount = s.RecommendationCount;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy        = true;
        StatusMessage = string.Empty;
        try
        {
            var result = await api.UpdateSettingsAsync(LowStockThreshold, RecommendationCount);
            if (result is null)
            {
                IsSuccess     = false;
                StatusMessage = "Ошибка при сохранении настроек.";
            }
            else
            {
                IsSuccess     = true;
                StatusMessage = "Настройки успешно сохранены.";
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RetrainAsync()
    {
        IsBusy        = true;
        StatusMessage = string.Empty;
        try
        {
            await api.RetrainAsync();
            IsSuccess     = true;
            StatusMessage = "Модель рекомендаций поставлена на переобучение.";
        }
        finally { IsBusy = false; }
    }
}
