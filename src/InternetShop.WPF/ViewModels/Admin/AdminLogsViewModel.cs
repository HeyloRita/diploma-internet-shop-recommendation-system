using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminLogsViewModel(ApiService api) : ObservableObject
{
    private static readonly string[] LevelOptions = ["Все", "Info", "Warning", "Error"];

    [ObservableProperty] private ObservableCollection<SystemLogDto> _logs = new();
    [ObservableProperty] private ObservableCollection<string> _levels = new(LevelOptions);
    [ObservableProperty] private string  _selectedLevel = "Все";
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private string  _statusText    = string.Empty;
    [ObservableProperty] private int     _currentPage   = 1;
    [ObservableProperty] private int     _totalCount;
    [ObservableProperty] private int     _pageSize      = 50;

    public int  TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
    public bool HasPrev    => CurrentPage > 1;
    public bool HasNext    => CurrentPage < TotalPages;
    public string PageInfo => $"Страница {CurrentPage} из {TotalPages}  ·  Всего: {TotalCount}";

    public async Task LoadAsync()
    {
        IsBusy     = true;
        StatusText = string.Empty;
        try
        {
            string? level = SelectedLevel == "Все" ? null : SelectedLevel;
            var (items, total) = await api.GetLogsAsync(level, CurrentPage, PageSize);

            TotalCount = total;
            Logs       = items is null
                ? new()
                : new ObservableCollection<SystemLogDto>(items);

            if (total == 0) StatusText = "Журнал пуст.";

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasPrev));
            OnPropertyChanged(nameof(HasNext));
            OnPropertyChanged(nameof(PageInfo));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private async Task FilterAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand] private async Task PrevPageAsync()
    {
        if (!HasPrev) return;
        CurrentPage--;
        await LoadAsync();
    }

    [RelayCommand] private async Task NextPageAsync()
    {
        if (!HasNext) return;
        CurrentPage++;
        await LoadAsync();
    }

    [RelayCommand] private async Task RefreshAsync() => await LoadAsync();
}
