using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Customer;

public partial class OrderHistoryViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<OrderDto> _orders = new();
    [ObservableProperty] private OrderDto? _selectedOrder;
    [ObservableProperty] private bool      _isBusy;
    [ObservableProperty] private string    _statusText = string.Empty;

    public async Task LoadAsync()
    {
        IsBusy     = true;
        StatusText = string.Empty;
        try
        {
            var orders = await api.GetMyOrdersAsync();
            if (orders is null || orders.Count == 0)
            {
                StatusText = "У вас пока нет заказов.";
                Orders     = new();
                return;
            }
            Orders        = new ObservableCollection<OrderDto>(orders);
            SelectedOrder = Orders.FirstOrDefault();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private void SelectOrder(OrderDto order)
        => SelectedOrder = order;
}
