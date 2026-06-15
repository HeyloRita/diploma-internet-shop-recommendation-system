using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InternetShop.WPF.Models;
using InternetShop.WPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace InternetShop.WPF.ViewModels.Admin;

public partial class AdminAnalyticsViewModel(ApiService api) : ObservableObject
{
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int     _totalOrders;
    [ObservableProperty] private int     _totalUsers;

    [ObservableProperty]
    private ObservableCollection<TopProductDto> _topProducts = new();

    [ObservableProperty] private ISeries[]  _salesSeries  = [];
    [ObservableProperty] private Axis[]     _xAxes        = [];
    [ObservableProperty] private Axis[]     _yAxes        = [];

    [ObservableProperty] private ISeries[]  _pieSeries    = [];

    [ObservableProperty] private bool   _isBusy;
    [ObservableProperty] private string _statusText = string.Empty;

    public async Task LoadAsync()
    {
        IsBusy     = true;
        StatusText = string.Empty;
        try
        {
            var data = await api.GetAnalyticsAsync();
            if (data is null) { StatusText = "Не удалось загрузить данные."; return; }

            TotalRevenue = data.TotalRevenue;
            TotalOrders  = data.TotalOrders;
            TotalUsers   = data.TotalUsers;
            TopProducts  = new ObservableCollection<TopProductDto>(data.TopProducts);

            BuildSalesChart(data.DailySales);
            BuildPieChart(data.TopProducts);
        }
        finally { IsBusy = false; }
    }

    private void BuildSalesChart(List<DailySalesDto> daily)
    {
        if (daily.Count == 0) return;

        var revenueValues = daily.Select(d => (double)d.Revenue).ToArray();
        var labels        = daily.Select(d => d.Date.ToString("dd.MM")).ToArray();

        SalesSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values        = revenueValues,
                Name          = "Выручка (₽)",
                Stroke        = new SolidColorPaint(SKColor.Parse("#2E86AB")) { StrokeThickness = 3 },
                Fill          = new SolidColorPaint(SKColor.Parse("#2E86AB").WithAlpha(40)),
                GeometrySize  = 8,
                GeometryStroke= new SolidColorPaint(SKColor.Parse("#2E86AB")) { StrokeThickness = 2 },
                GeometryFill  = new SolidColorPaint(SKColors.White),
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels    = labels,
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#888888")),
                TicksPaint  = new SolidColorPaint(SKColor.Parse("#DDE3EA")),
            }
        };

        YAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColor.Parse("#888888")),
                Labeler     = v => $"{v:N0} ₽",
            }
        };
    }

    private void BuildPieChart(List<TopProductDto> top)
    {
        if (top.Count == 0) return;

        var colors = new[]
        {
            "#2E86AB", "#E84855", "#F4A261", "#2EC4B6", "#8338EC"
        };

        PieSeries = top.Select((p, i) => (ISeries)new PieSeries<double>
        {
            Values    = new[] { (double)p.SoldCount },
            Name      = p.ProductName,
            Fill      = new SolidColorPaint(SKColor.Parse(colors[i % colors.Length])),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize  = 13,
        }).ToArray();
    }

    [RelayCommand] private async Task RefreshAsync() => await LoadAsync();
}
