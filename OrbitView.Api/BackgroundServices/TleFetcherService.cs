using OrbitView.Api.Services;

namespace OrbitView.Api.BackgroundServices;

public class TleFetcherService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TleFetcherService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public TleFetcherService(IServiceProvider services,
        ILogger<TleFetcherService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TLE Fetcher background service started.");

        // Run immediately on startup
        await RunFetchAsync();

        // Then repeat every hour
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunFetchAsync();
        }
    }

    private async Task RunFetchAsync()
    {
        try
        {
            // Create a new scope because TleService is Scoped
            // but BackgroundService is Singleton
            using var scope = _services.CreateScope();
            var tleService = scope.ServiceProvider.GetRequiredService<ITleService>();
            await tleService.FetchAndStoreAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in TLE fetch cycle");
        }
    }
}