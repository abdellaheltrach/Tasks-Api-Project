using LoginApp.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    private readonly TimeSpan _cleanupInterval;
    public RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var hoursString = config["CleanupSettings:RefreshTokenCleanupHours"] ?? "6"; // default 6 hours
        _cleanupInterval = TimeSpan.FromHours(Convert.ToInt32( hoursString));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RefreshTokenCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var refreshTokenRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

                // perform cleanup
                var deletedCount = await refreshTokenRepo.DeleteInactiveTokensAsync();

                if (deletedCount > 0)
                    _logger.LogInformation("Cleaned up {count} inactive refresh tokens at {time}.", deletedCount, DateTime.UtcNow);
                else
                    _logger.LogInformation("No inactive refresh tokens found for cleanup at {time}.", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up refresh tokens.");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("RefreshTokenCleanupService stopped.");
    }
}
