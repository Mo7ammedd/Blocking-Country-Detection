namespace BD.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using BD.Core.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TemporalBlocksCleanupService : BackgroundService
{
    private readonly ILogger<TemporalBlocksCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public TemporalBlocksCleanupService(
        ILogger<TemporalBlocksCleanupService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temporal Blocks Cleanup Service is starting.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredBlocksAsync();
        }
    }

    private async Task CleanupExpiredBlocksAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var countryRepository = scope.ServiceProvider.GetRequiredService<ICountryRepository>();
            
            var blocksRemoved = countryRepository.RemoveExpiredTemporalBlocks();
            
            if (blocksRemoved)
            {
                _logger.LogInformation("Removed expired temporal blocks at {time}", DateTimeOffset.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cleaning up expired temporal blocks at {time}", DateTimeOffset.UtcNow);
        }
    }
}