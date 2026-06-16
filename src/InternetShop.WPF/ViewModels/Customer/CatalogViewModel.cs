using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Customer;

public partial class CatalogViewModel(ApiService api) : ObservableObject
{
    public event Action<int>? NavigateToDetail;

    [ObservableProperty] private ObservableCollection<ProductDto> _products = new();
    [ObservableProperty] private ObservableCollection<CategoryDto> _categories = new();
    [ObservableProperty] private string  _searchText    = string.Empty;
    [ObservableProperty] private CategoryDto? _selectedCategory;
    [ObservableProperty] private string  _minPrice      = string.Empty;
    [ObservableProperty] private string  _maxPrice      = string.Empty;
    [ObservableProperty] private bool    _isBusy;
    [ObservableProperty] private string  _statusText    = string.Empty;
    [ObservableProperty] private int     _currentPage   = 1;
    [ObservableProperty] private int     _totalCount;
    [ObservableProperty] private int     _pageSize      = 12;

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrev   => CurrentPage > 1;
    public bool HasNext   => CurrentPage < TotalPages;

    public async Task LoadAsync()
    {
        IsBusy = true;
        StatusText = string.Empty;
        try
        {
            if (!Categories.Any())
            {
                var cats = await api.GetCategoriesAsync();
                Categories = new ObservableCollection<CategoryDto>(
                    cats?.Prepend(new CategoryDto(0, "Все категории")) ?? []);
                SelectedCategory = Categories.FirstOrDefault();
            }

            decimal? min = decimal.TryParse(MinPrice, out var mn) ? mn : null;
            decimal? max = decimal.TryParse(MaxPrice, out var mx) ? mx : null;
            int? catId   = SelectedCategory?.Id == 0 ? null : SelectedCategory?.Id;

            var result = await api.GetProductsAsync(
                string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                catId, min, max, CurrentPage, PageSize);

            if (result is null) { StatusText = "Не удалось загрузить товары."; return; }

            Products   = new ObservableCollection<ProductDto>(result.Items);
            TotalCount = result.TotalCount;
            StatusText = TotalCount == 0 ? "Товары не найдены." : string.Empty;

            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(HasPrev));
            OnPropertyChanged(nameof(HasNext));
        }
        finally { IsBusy = false; }
    }

    [RelayCommand] private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand] private async Task ResetFiltersAsync()
    {
        SearchText       = string.Empty;
        MinPrice         = string.Empty;
        MaxPrice         = string.Empty;
        SelectedCategory = Categories.FirstOrDefault();
        CurrentPage      = 1;
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

    [RelayCommand] private void OpenProduct(ProductDto product)
        => NavigateToDetail?.Invoke(product.Id);
}
