using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminProductsViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ProductDto>  _products   = new();
    [ObservableProperty] private ObservableCollection<CategoryDto> _categories = new();

    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private bool   _isEditing;
    [ObservableProperty] private bool   _isNew;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool   _isSuccess;

    [ObservableProperty] private int         _editId;
    [ObservableProperty] private string      _editName        = string.Empty;
    [ObservableProperty] private string      _editDescription = string.Empty;
    [ObservableProperty] private decimal     _editPrice;
    [ObservableProperty] private int         _editStock;
    [ObservableProperty] private string      _editImageUrl    = string.Empty;
    [ObservableProperty] private CategoryDto? _editCategory;
    [ObservableProperty] private bool        _editIsActive    = true;

    public string FormTitle => IsNew ? "Новый товар" : "Редактировать товар";

    partial void OnIsNewChanged(bool value) => OnPropertyChanged(nameof(FormTitle));

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var cats = await api.GetCategoriesAsync();
            if (cats is not null)
                Categories = new ObservableCollection<CategoryDto>(cats);

            var products = await api.GetAllProductsAdminAsync();
            if (products is not null)
                Products = new ObservableCollection<ProductDto>(products);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void StartAdd()
    {
        IsNew            = true;
        EditId           = 0;
        EditName         = string.Empty;
        EditDescription  = string.Empty;
        EditPrice        = 0;
        EditStock        = 0;
        EditImageUrl     = string.Empty;
        EditCategory     = Categories.FirstOrDefault();
        EditIsActive     = true;
        StatusMessage    = string.Empty;
        IsEditing        = true;
    }

    [RelayCommand]
    private void StartEdit(ProductDto p)
    {
        IsNew            = false;
        EditId           = p.Id;
        EditName         = p.Name;
        EditDescription  = p.Description;
        EditPrice        = p.Price;
        EditStock        = p.Stock;
        EditImageUrl     = p.ImageUrl ?? string.Empty;
        EditCategory     = Categories.FirstOrDefault(c => c.Id == p.CategoryId);
        EditIsActive     = p.IsActive;
        StatusMessage    = string.Empty;
        IsEditing        = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        IsEditing     = false;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            IsSuccess     = false;
            StatusMessage = "Название товара обязательно.";
            return;
        }
        if (EditPrice <= 0)
        {
            IsSuccess     = false;
            StatusMessage = "Цена должна быть больше нуля.";
            return;
        }
        if (EditCategory is null)
        {
            IsSuccess     = false;
            StatusMessage = "Выберите категорию.";
            return;
        }

        IsBusy        = true;
        StatusMessage = string.Empty;
        try
        {
            string? imageUrl = string.IsNullOrWhiteSpace(EditImageUrl) ? null : EditImageUrl;

            if (IsNew)
            {
                var req    = new AdminCreateProductRequest(EditName, EditDescription,
                    EditPrice, EditStock, imageUrl, EditCategory.Id);
                var result = await api.CreateProductAsync(req);
                if (result is null)
                {
                    IsSuccess     = false;
                    StatusMessage = "Ошибка при создании товара.";
                    return;
                }
                Products.Add(result);
            }
            else
            {
                var req    = new AdminUpdateProductRequest(EditName, EditDescription,
                    EditPrice, EditStock, imageUrl, EditCategory.Id, EditIsActive);
                var result = await api.UpdateProductAsync(EditId, req);
                if (result is null)
                {
                    IsSuccess     = false;
                    StatusMessage = "Ошибка при сохранении товара.";
                    return;
                }
                var idx = Products.IndexOf(Products.First(p => p.Id == EditId));
                Products[idx] = result;
            }

            IsSuccess     = true;
            StatusMessage = IsNew ? "Товар создан." : "Товар сохранён.";
            IsEditing     = false;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductDto p)
    {
        IsBusy = true;
        try
        {
            bool ok = await api.DeleteProductAsync(p.Id);
            if (ok)
            {
                var updated = p with { IsActive = false };
                var idx     = Products.IndexOf(p);
                Products[idx] = updated;
            }
        }
        finally { IsBusy = false; }
    }
}
