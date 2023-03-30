using GitHubTagger.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace GitHubTagger;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly Timer _timer;

    public Worker(ILogger<Worker> logger, GitHubTaggerConfiguration configuration, SynchronizePullRequestsUseCase useCase)
    {
        _logger = logger;
        _timer = new Timer(configuration.Interval);
        _timer.Elapsed += async (sender, args) => await useCase.ExecuteAsync();
        _timer.Enabled = false;
        _timer.AutoReset = true;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();

        _logger.LogInformation("Started timer with interval {Interval}", _timer.Interval);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();

        _logger.LogInformation("Stopped timer");

        return Task.CompletedTask;
    }
}