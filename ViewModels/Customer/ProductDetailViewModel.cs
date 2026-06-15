using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Customer;

public partial class ProductDetailViewModel(ApiService api, AuthState auth) : ObservableObject
{
    public event Action<int>? NavigateToDetail;
    public event Action?      Back;

    [ObservableProperty] private ProductDto?    _product;
    [ObservableProperty] private ObservableCollection<ProductDto> _recommendations = new();
    [ObservableProperty] private int            _quantity = 1;
    [ObservableProperty] private bool           _isBusy;
    [ObservableProperty] private string         _statusMessage = string.Empty;
    [ObservableProperty] private bool           _isSuccess;
    [ObservableProperty] private bool           _isLoggedIn;

    public async Task LoadAsync(int productId)
    {
        IsBusy        = true;
        StatusMessage = string.Empty;
        Quantity      = 1;
        IsLoggedIn    = auth.IsLoggedIn;

        try
        {
            Product = await api.GetProductAsync(productId);
            if (Product is null) return;

            await api.RecordViewAsync(productId);

            if (auth.IsLoggedIn)
            {
                var recs = await api.GetRecommendationsAsync();
                var filtered = recs?
                    .Where(r => r.Id != productId)
                    .Take(4)
                    .ToList() ?? [];
                Recommendations = new ObservableCollection<ProductDto>(filtered);
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private void IncreaseQty()
    {
        if (Product is not null && Quantity < Product.Stock)
            Quantity++;
    }
    [RelayCommand] private void DecreaseQty()
    {
        if (Quantity > 1) Quantity--;
    }

    [RelayCommand]
    private async Task BuyAsync()
    {
        if (!auth.IsLoggedIn) { StatusMessage = "Войдите, чтобы оформить заказ."; return; }
        if (Product is null)  return;

        IsBusy = true;
        try
        {
            var order = await api.CreateOrderAsync(
                [new OrderItemRequest(Product.Id, Quantity)]);

            if (order is null)
            {
                IsSuccess     = false;
                StatusMessage = "Ошибка при оформлении заказа.";
            }
            else
            {
                IsSuccess     = true;
                StatusMessage = $"Заказ #{order.Id} оформлен! Итого: {order.TotalAmount:C}";
                Product = Product with { Stock = Product.Stock - Quantity };
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private void GoBack() => Back?.Invoke();

    [RelayCommand] private void OpenRecommendation(ProductDto product)
        => NavigateToDetail?.Invoke(product.Id);
}
