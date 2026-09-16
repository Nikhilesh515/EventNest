using EventNest.AuthService.Application.Interfaces;

namespace EventNest.AuthService.API.BackgroundServices;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RefreshTokenCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalHours = int.TryParse(
            _configuration["Jwt:RefreshTokenCleanupIntervalHours"], out var configured)
            ? Math.Max(1, configured)
            : 12;

        using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var removed = await repository.DeleteExpiredAsync(DateTime.UtcNow, cancellationToken);
            _logger.LogInformation("Refresh token cleanup removed {RemovedCount} expired tokens.", removed);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Refresh token cleanup failed.");
        }
    }
}
