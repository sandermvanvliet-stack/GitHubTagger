using System.Diagnostics;
using System.Text.RegularExpressions;
using GitHubTagger.Models;
using GitHubTagger.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace GitHubTagger;

public class Worker : IHostedService
{
    private readonly IGitHubApi _gitHubApi;
    private IJiraApi _jiraApi;
    private readonly ILogger<Worker> _logger;
    private readonly Timer _timer;

    public Worker(IGitHubApi gitHubApi, IJiraApi jiraApi, ILogger<Worker> logger, GitHubTaggerConfiguration configuration)
    {
        _gitHubApi = gitHubApi;
        _jiraApi = jiraApi;
        _logger = logger;
        _timer = new Timer(configuration.Interval);
        _timer.Elapsed += async (sender, args) => await ProcessPullRequestsAsync();
        _timer.Enabled = false;
        _timer.AutoReset = true;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Debugger.IsAttached)
        {
            Task.Run(ProcessPullRequestsAsync);
        }
        else
        {
            _timer.Start();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        return Task.CompletedTask;
    }

    private async Task ProcessPullRequestsAsync()
    {
        var user = await _gitHubApi.GetCurrentUserAsync();

        _logger.LogInformation("Checking pull requests for {User}", user.UserName);

        var pullRequests = await _gitHubApi.GetPullRequests(user.UserName);

        _logger.LogInformation("Found {Count} open pull requests", pullRequests.Length);

        foreach (var pr in pullRequests)
        {
            _logger.LogInformation("Processing PR {Number}", pr.Number);

            var jiraTicketId = GetJiraTicketIdFromTitle(pr);

            if (!string.IsNullOrWhiteSpace(jiraTicketId))
            {
                _logger.LogInformation("Retrieving Jira ticket {JiraTicketId}", jiraTicketId);

                var jiraTicket = await _jiraApi.GetTicketByIdAsync(jiraTicketId);

                if (jiraTicket != null)
                {
                    await UpdatePullRequestAsync(pr, jiraTicket);
                }
                else
                {
                    _logger.LogWarning("Jira ticket {JiraTicketId} was not found", jiraTicketId);
                }
            }
        }
    }

    private async Task UpdatePullRequestAsync(PullRequest pullRequest, JiraTicket jiraTicket)
    {
        var labelsToAdd = new List<string>();
        var reviewersToAdd = new List<string>();

        if (jiraTicket.Labels.Contains("community-growth") &&
            !pullRequest.Labels.Contains("team:teams-community-growth-pod"))
        {
            labelsToAdd.Add("team:teams-community-growth-pod");
        }

        if (jiraTicket.Labels.Contains("enterprise") &&
            !pullRequest.Labels.Contains("enterprise"))
        {
            labelsToAdd.Add("enterprise");
        }

        if (jiraTicket.Labels.Contains("community-growth") &&
            !pullRequest.Reviewers.Contains("stackeng/community-growth-pod"))
        {
            reviewersToAdd.Add("stackeng/community-growth-pod");
        }

        if (jiraTicket.Labels.Contains("enterprise") &&
            !pullRequest.Reviewers.Contains("iprefer-pi"))
        {
            reviewersToAdd.Add("iprefer-pi");
        }

        if (labelsToAdd.Any() || reviewersToAdd.Any())
        {
            _logger.LogInformation(
                "Updating PR {Number}, adding labels: {Labels}, adding reviewers: {Reviewers}",
                pullRequest.Number,
                string.Join(", ", labelsToAdd),
                string.Join(", ", reviewersToAdd));

            await _gitHubApi.UpdatePullRequestAsync(pullRequest, labelsToAdd, reviewersToAdd);
        }
    }

    private static string? GetJiraTicketIdFromTitle(PullRequest pr)
    {
        var regex = new Regex(@"^\[([A-Za-z]*-[0-9]+)\].*$");

        var match = regex.Match(pr.Title);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
}