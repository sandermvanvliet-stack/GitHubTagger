using System.Text.RegularExpressions;
using GitHubTagger.Models;
using GitHubTagger.Ports;
using Microsoft.Extensions.Logging;

namespace GitHubTagger.UseCases
{
    public class SynchronizePullRequestsUseCase
    {
        private readonly ILogger<SynchronizePullRequestsUseCase> _logger;
        private readonly IGitHubApi _gitHubApi;
        private readonly IJiraApi _jiraApi;
        private readonly GitHubTaggerConfiguration _configuration;
        private static readonly Regex JiraTicketNumberRegex = new(@"^\[([A-Za-z]*-[0-9]+)\].*$", RegexOptions.Compiled);

        public SynchronizePullRequestsUseCase(
            ILogger<SynchronizePullRequestsUseCase> logger,
            IGitHubApi gitHubApi, 
            IJiraApi jiraApi,
            GitHubTaggerConfiguration configuration)
        {
            _logger = logger;
            _gitHubApi = gitHubApi;
            _jiraApi = jiraApi;
            _configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            var user = await _gitHubApi.GetCurrentUserAsync();

            _logger.LogInformation("Checking pull requests for {User}", user.UserName);

            var pullRequests = await _gitHubApi.GetPullRequests(user.UserName);

            _logger.LogInformation("Found {Count} open pull requests", pullRequests.Length);

            foreach (var pullRequest in pullRequests)
            {
                _logger.LogInformation("Processing PR {Number}", pullRequest.Number);

                var jiraTicketId = GetJiraTicketIdFromTitle(pullRequest);

                if (string.IsNullOrWhiteSpace(jiraTicketId))
                {
                    _logger.LogDebug("Pull Request title didn't have a Jira-like ticket id");
                    continue;
                }

                _logger.LogInformation("Retrieving Jira ticket {JiraTicketId}", jiraTicketId);

                var jiraTicket = await _jiraApi.GetTicketByIdAsync(jiraTicketId);

                if (jiraTicket != null)
                {
                    await UpdatePullRequestAsync(pullRequest, jiraTicket);
                }
                else
                {
                    _logger.LogWarning("Jira ticket {JiraTicketId} was not found", jiraTicketId);
                }
            }
        }

        private async Task UpdatePullRequestAsync(PullRequest pullRequest, JiraTicket jiraTicket)
        {
            var labelsToAdd = new List<string>();
            var reviewersToAdd = new List<string>();

            foreach (var label in jiraTicket.Labels)
            {
                if (_configuration.JiraToGitHubLabelMappings.ContainsKey(label))
                {
                    var githubLabels = _configuration.JiraToGitHubLabelMappings[label];

                    foreach (var githubLabel in githubLabels)
                    {
                        if (!pullRequest.Labels.Contains(githubLabel))
                        {
                            labelsToAdd.Add(githubLabel);
                        }
                    }
                }

                if (_configuration.JiraLabelToGitHubReviewerMappings.ContainsKey(label))
                {
                    var githubReviewers = _configuration.JiraLabelToGitHubReviewerMappings[label];

                    foreach (var githubReviewer in githubReviewers)
                    {
                        if (!pullRequest.Reviewers.Contains(githubReviewer))
                        {
                            reviewersToAdd.Add(githubReviewer);
                        }
                    }
                }
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
            var match = JiraTicketNumberRegex.Match(pr.Title);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }
    }
}
