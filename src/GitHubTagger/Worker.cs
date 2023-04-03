using GitHubTagger.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace GitHubTagger;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly Timer _timer;
    private readonly GitHubTaggerConfiguration _configuration;
    private readonly SynchronizePullRequestsUseCase _useCase;
    private DateTime _lastRun;

    public Worker(ILogger<Worker> logger, GitHubTaggerConfiguration configuration, SynchronizePullRequestsUseCase useCase)
    {
        _configuration = configuration;
        _logger = logger;
        _useCase = useCase;
        _timer = new Timer(configuration.Interval);
        _timer.Elapsed += async (_, _) => await TimerElapsedAsync();
        _timer.Enabled = false;
        _timer.AutoReset = true;
    }

    private async Task TimerElapsedAsync()
    {
        await _useCase.ExecuteAsync(_lastRun);
        _lastRun = DateTime.UtcNow;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();

        _logger.LogInformation("Started timer with interval {Interval}", _timer.Interval);

        if (_configuration.RunAtStartup)
        {
            _logger.LogInformation("Starting pull request update at startup");

            Task.Run(TimerElapsedAsync, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();

        _logger.LogInformation("Stopped timer");

        return Task.CompletedTask;
    }
}