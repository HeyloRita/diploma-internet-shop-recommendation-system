using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Customer;

public partial class RecommendationsViewModel(ApiService api) : ObservableObject
{
    public event Action<int>? NavigateToDetail;

    [ObservableProperty] private ObservableCollection<ProductDto> _items = new();
    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _statusText = string.Empty;

    public async Task LoadAsync()
    {
        IsBusy     = true;
        StatusText = string.Empty;
        try
        {
            var recs = await api.GetRecommendationsAsync();
            if (recs is null || recs.Count == 0)
            {
                StatusText = "Рекомендации появятся после первых покупок.";
                Items      = new();
                return;
            }
            Items = new ObservableCollection<ProductDto>(recs);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private void OpenProduct(ProductDto product)
        => NavigateToDetail?.Invoke(product.Id);
}
