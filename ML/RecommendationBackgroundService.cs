namespace InternetShop.API.ML;

public class RecommendationBackgroundService(IServiceProvider services, ILogger<RecommendationBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), ct);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<RecommendationService>();
                    await svc.TrainAndSaveAsync();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Ошибка при обучении модели рекомендаций.");
                }

                await Task.Delay(TimeSpan.FromHours(24), ct);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
