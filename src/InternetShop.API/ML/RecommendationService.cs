using InternetShop.API.Data;
using InternetShop.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace InternetShop.API.ML;

public class RecommendationService(AppDbContext db, ILogger<RecommendationService> logger)
{
    private readonly MLContext _mlContext = new(seed: 42);

    public async Task TrainAndSaveAsync()
    {
        logger.LogInformation("Запуск обучения рекомендательной модели...");

        var events = await db.UserEvents.ToListAsync();
        if (events.Count < 5)
        {
            logger.LogWarning("Недостаточно данных для обучения (нужно минимум 5 событий).");
            return;
        }

        var products = await db.Products.Where(p => p.IsActive).Select(p => p.Id).ToListAsync();
        var users    = await db.Users.Where(u => u.Role == UserRole.Customer).Select(u => u.Id).ToListAsync();
        var settings = await db.AppSettings.FirstAsync();

        var ratings = events
            .GroupBy(e => (e.UserId, e.ProductId))
            .Select(g => new ProductRating
            {
                UserId    = (uint)g.Key.UserId,
                ProductId = (uint)g.Key.ProductId,
                Label     = g.Sum(e => e.Type == EventType.Purchase ? 2f : 1f),
            })
            .ToList();

        var dataView  = _mlContext.Data.LoadFromEnumerable(ratings);
        var options   = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(ProductRating.ProductId),
            MatrixRowIndexColumnName    = nameof(ProductRating.UserId),
            LabelColumnName             = nameof(ProductRating.Label),
            NumberOfIterations          = 20,
            ApproximationRank           = 10,
        };
        var trainer   = _mlContext.Recommendation().Trainers.MatrixFactorization(options);
        var pipeline  = _mlContext.Transforms
            .Conversion.MapValueToKey(nameof(ProductRating.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(ProductRating.ProductId)))
            .Append(trainer);

        var model = pipeline.Fit(dataView);

        var newRecs = new List<Recommendation>();
        int count   = settings.RecommendationCount;

        foreach (var userId in users)
        {
            var purchased = events
                .Where(e => e.UserId == userId && e.Type == EventType.Purchase)
                .Select(e => e.ProductId)
                .ToHashSet();

            var toScore = products
                .Where(pid => !purchased.Contains(pid))
                .Select(pid => new ProductRating { UserId = (uint)userId, ProductId = (uint)pid })
                .ToList();

            if (toScore.Count == 0) continue;

            var scoreData = _mlContext.Data.LoadFromEnumerable(toScore);
            var predictions = model.Transform(scoreData);

            var scores = _mlContext.Data
                .CreateEnumerable<ScoredRating>(predictions, reuseRowObject: false)
                .Select(r => r.Score)
                .ToList();

            var ranked = toScore
                .Zip(scores, (orig, score) => (orig.ProductId, Score: score))
                .OrderByDescending(r => r.Score)
                .Take(count);

            newRecs.AddRange(ranked.Select(r => new Recommendation
            {
                UserId    = userId,
                ProductId = (int)r.ProductId,
                Score     = r.Score,
            }));
        }

        var oldRecs = db.Recommendations.Where(r => users.Contains(r.UserId));
        db.Recommendations.RemoveRange(oldRecs);
        db.Recommendations.AddRange(newRecs);
        await db.SaveChangesAsync();

        logger.LogInformation("Обучение завершено. Сохранено {Count} рекомендаций.", newRecs.Count);
    }

    private class ProductRating
    {
        public uint UserId    { get; set; }
        public uint ProductId { get; set; }
        public float Label    { get; set; }
    }

    private class ScoredRating
    {
        public float Score { get; set; }
    }
}
