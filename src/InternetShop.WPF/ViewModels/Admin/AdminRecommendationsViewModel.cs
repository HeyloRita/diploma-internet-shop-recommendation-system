using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminRecommendationsViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private bool     _isBusy;
    [ObservableProperty] private string   _statusText = string.Empty;

    [ObservableProperty] private string _lastTrainedAt    = "—";
    [ObservableProperty] private int    _totalRecs;
    [ObservableProperty] private int    _usersWithRecs;
    [ObservableProperty] private int    _trainingEvents;

    [ObservableProperty] private string _algorithm        = "—";
    [ObservableProperty] private int    _modelIterations;
    [ObservableProperty] private int    _modelRank;

    [ObservableProperty] private ObservableCollection<TopRecommendedDto> _topRecommended = new();

    [ObservableProperty] private string _retrainStatus    = string.Empty;
    [ObservableProperty] private bool   _retrainSuccess;

    public async Task LoadAsync()
    {
        IsBusy      = true;
        StatusText  = string.Empty;
        RetrainStatus = string.Empty;
        try
        {
            var info = await api.GetModelInfoAsync();
            if (info is null)
            {
                StatusText = "Не удалось загрузить данные модели.";
                return;
            }

            LastTrainedAt  = info.LastTrainedAt.HasValue
                ? info.LastTrainedAt.Value.ToString("dd.MM.yyyy HH:mm:ss")
                : "Модель ещё не обучена";
            TotalRecs      = info.TotalRecommendations;
            UsersWithRecs  = info.UsersWithRecs;
            TrainingEvents = info.TrainingEventsCount;
            Algorithm      = info.Algorithm;
            ModelIterations = info.ModelIterations;
            ModelRank      = info.ModelRank;
            TopRecommended = new ObservableCollection<TopRecommendedDto>(info.TopRecommended);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    [RelayCommand]
    private async Task Retrain()
    {
        IsBusy        = true;
        RetrainStatus = string.Empty;
        try
        {
            await api.RetrainAsync();
            await LoadAsync();
            RetrainStatus  = "✅ Модель успешно переобучена.";
            RetrainSuccess = true;
        }
        catch
        {
            RetrainStatus  = "❌ Ошибка при переобучении.";
            RetrainSuccess = false;
        }
        finally { IsBusy = false; }
    }
}
