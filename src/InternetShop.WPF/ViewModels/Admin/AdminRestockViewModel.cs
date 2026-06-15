using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminRestockViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<RestockTaskDto> _tasks = new();
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _statusText = string.Empty;

    public async Task LoadAsync()
    {
        IsBusy     = true;
        StatusText = string.Empty;
        try
        {
            var tasks = await api.GetRestockTasksAsync();
            if (tasks is null || tasks.Count == 0)
            {
                StatusText = "Заданий на дозаказ нет — все остатки в норме.";
                Tasks      = new();
                return;
            }
            Tasks = new ObservableCollection<RestockTaskDto>(tasks);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private async Task RefreshAsync() => await LoadAsync();
}
