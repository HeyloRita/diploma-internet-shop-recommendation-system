using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InternetShop.WPF.Models;

namespace InternetShop.WPF.Services;

public class ApiService(HttpClient http, AuthState auth)
{
    private const string Base = "http://localhost:5000/api";


    public async Task<AuthResponse?> RegisterAsync(string name, string email, string password)
        => await PostAsync<AuthResponse>("/auth/register", new { name, email, password });

    public async Task<AuthResponse?> LoginAsync(string email, string password)
        => await PostAsync<AuthResponse>("/auth/login", new { email, password });


    public async Task<ProductListResponse?> GetProductsAsync(
        string? search = null, int? categoryId = null,
        decimal? minPrice = null, decimal? maxPrice = null,
        int page = 1, int pageSize = 12)
    {
        var q = $"/products?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) q += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId.HasValue)  q += $"&categoryId={categoryId}";
        if (minPrice.HasValue)    q += $"&minPrice={minPrice}";
        if (maxPrice.HasValue)    q += $"&maxPrice={maxPrice}";
        return await GetAsync<ProductListResponse>(q);
    }

    public async Task<ProductDto?> GetProductAsync(int id)
        => await GetAsync<ProductDto>($"/products/{id}");

    public async Task<List<CategoryDto>?> GetCategoriesAsync()
        => await GetAsync<List<CategoryDto>>("/products/categories");


    public async Task<List<OrderDto>?> GetMyOrdersAsync()
        => await GetAuthAsync<List<OrderDto>>("/orders");

    public async Task<OrderDto?> CreateOrderAsync(List<OrderItemRequest> items)
        => await PostAuthAsync<OrderDto>("/orders", new CreateOrderRequest(items));


    public async Task RecordViewAsync(int productId)
    {
        if (!auth.IsLoggedIn) return;
        await PostAuthAsync<object>($"/events/view/{productId}", null);
    }


    public async Task<List<ProductDto>?> GetRecommendationsAsync()
        => await GetAuthAsync<List<ProductDto>>("/recommendations");


    public async Task<(List<SystemLogDto>? Items, int Total)> GetLogsAsync(
        string? level = null, int page = 1, int pageSize = 50)
    {
        SetBearer();
        var q = $"/admin/logs?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(level)) q += $"&level={level}";
        var resp = await http.GetAsync(Base + q);
        if (!resp.IsSuccessStatusCode) return (null, 0);
        var data = await resp.Content.ReadFromJsonAsync<LogsResponse>();
        return (data?.Items, data?.Total ?? 0);
    }

    public async Task<List<RestockTaskDto>?> GetRestockTasksAsync()
        => await GetAuthAsync<List<RestockTaskDto>>("/admin/restock");

    public async Task<AppSettingsDto?> GetSettingsAsync()
        => await GetAuthAsync<AppSettingsDto>("/admin/settings");

    public async Task<AppSettingsDto?> UpdateSettingsAsync(int lowStock, int recCount)
    {
        SetBearer();
        var resp = await http.PutAsJsonAsync(Base + "/admin/settings",
            new { lowStockThreshold = lowStock, recommendationCount = recCount });
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<AppSettingsDto>();
    }

    public async Task<AnalyticsDto?> GetAnalyticsAsync()
        => await GetAuthAsync<AnalyticsDto>("/admin/analytics");

    public async Task<ModelInfoDto?> GetModelInfoAsync()
        => await GetAuthAsync<ModelInfoDto>("/admin/recommendations/info");

    public async Task RetrainAsync()
    {
        SetBearer();
        await http.PostAsync(Base + "/admin/recommendations/retrain", null);
    }

    public async Task<List<ProductDto>?> GetAllProductsAdminAsync()
        => await GetAuthAsync<List<ProductDto>>("/admin/products");

    public async Task<ProductDto?> CreateProductAsync(AdminCreateProductRequest req)
        => await PostAuthAsync<ProductDto>("/admin/products", req);

    public async Task<ProductDto?> UpdateProductAsync(int id, AdminUpdateProductRequest req)
    {
        SetBearer();
        var resp = await http.PutAsJsonAsync(Base + $"/admin/products/{id}", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        SetBearer();
        var resp = await http.DeleteAsync(Base + $"/admin/products/{id}");
        return resp.IsSuccessStatusCode;
    }


    private record LogsResponse(List<SystemLogDto> Items, int Total);

    private async Task<T?> GetAsync<T>(string path)
    {
        var resp = await http.GetAsync(Base + path);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> GetAuthAsync<T>(string path)
    {
        SetBearer();
        return await GetAsync<T>(path);
    }

    private async Task<T?> PostAsync<T>(string path, object? body)
    {
        var resp = await http.PostAsJsonAsync(Base + path, body);
        if (!resp.IsSuccessStatusCode) return default;
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> PostAuthAsync<T>(string path, object? body)
    {
        SetBearer();
        return await PostAsync<T>(path, body);
    }

    private void SetBearer()
    {
        http.DefaultRequestHeaders.Authorization =
            auth.Token is null ? null : new AuthenticationHeaderValue("Bearer", auth.Token);
    }
}
