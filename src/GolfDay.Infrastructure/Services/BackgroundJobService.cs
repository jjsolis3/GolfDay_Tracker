using GolfDay.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GolfDay.Infrastructure.Services;

/// <summary>
/// Background service that runs periodic maintenance tasks:
/// - Mark overdue league matches
/// - Recalculate stats nightly
/// </summary>
public class BackgroundJobService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(IServiceProvider serviceProvider, ILogger<BackgroundJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background job service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var leagueScheduler = scope.ServiceProvider.GetRequiredService<ILeagueSchedulerService>();
                await leagueScheduler.MarkOverdueMatchesAsync(stoppingToken);
                _logger.LogDebug("Overdue match check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background job execution.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
