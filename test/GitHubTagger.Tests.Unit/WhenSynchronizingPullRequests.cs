using FluentAssertions;
using GitHubTagger.Models;
using GitHubTagger.UseCases;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GitHubTagger.Tests.Unit
{
    public class WhenSynchronizingPullRequests
    {
        [Fact]
        public async Task GivenPullRequestDoesNotHaveJiraTicketId_PullRequestIsIgnored()
        {
            _stubGitHubApi.PullRequest.Title = "TITLE WITHOUT TICKET ID";

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task GivenPullRequestHasNonExistentJiraTicketId_PullRequestIsIgnored()
        {
            _stubGitHubApi.PullRequest.Title = "[NOEXIST-123] BAD TICKET ID";

            _stubJiraApi.Tickets.Add("JIRA-123", new JiraTicket { Labels = Array.Empty<string>() });

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task GivenPullRequestHasValidJiraTicketIdAndJiraTicketHasNoLabels_PullRequestIsIgnored()
        {
            _stubGitHubApi.PullRequest.Labels = Array.Empty<string>();

            _stubJiraApi.Tickets.Add("JIRA-123", new JiraTicket { Labels = Array.Empty<string>() });

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Should()
                .BeEmpty();
        }

        [Fact]
        public async Task
            GivenPullRequestWithoutLabelsAndJiraTicketHasEnterpriseLabel_PullRequestIsUpdatedWithEnterpriseLabel()
        {
            _stubGitHubApi.PullRequest.Labels = Array.Empty<string>();

            _stubJiraApi.Tickets.Add("JIRA-123", new JiraTicket { Labels = new [] { "enterprise" }});

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Single()
                .LabelsToAdd
                .Should()
                .Contain("enterprise");
        }

        [Fact]
        public async Task
            GivenPullRequestWithoutReviewersAndJiraTicketHasEnterpriseLabel_PullRequestIsUpdatedWithEnterpriseReviewers()
        {
            _stubGitHubApi.PullRequest.Reviewers = Array.Empty<string>();

            _stubJiraApi.Tickets.Add("JIRA-123", new JiraTicket { Labels = new[] { "enterprise" } });

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Single()
                .ReviewersToAdd
                .Should()
                .Contain(_configuration.JiraLabelToGitHubReviewerMappings["enterprise"]);
        }

        [Fact]
        public async Task
            GivenPullRequestHasOneEnterpriseReviewerAndJiraTicketHasEnterpriseLabel_PullRequestIsUpdatedWithEnterpriseMissingReviewer()
        {
            _stubGitHubApi.PullRequest.Reviewers = new[] { "enterprise-review-1" };

            _stubJiraApi.Tickets.Add("JIRA-123", new JiraTicket { Labels = new[] { "enterprise" } });

            await SynchronizePullRequestsAsync();

            _stubGitHubApi
                .PullRequestUpdates
                .Single()
                .ReviewersToAdd
                .Should()
                .Contain("enterprise-review-2");
        }

        private readonly SynchronizePullRequestsUseCase _useCase;
        private readonly StubGitHubApi _stubGitHubApi;
        private readonly StubJiraApi _stubJiraApi;
        private readonly GitHubTaggerConfiguration _configuration;

        public WhenSynchronizingPullRequests()
        {
            _stubGitHubApi = new StubGitHubApi();
            _stubJiraApi = new StubJiraApi();
            _configuration = new GitHubTaggerConfiguration
            {
                JiraLabelToGitHubReviewerMappings =
                {
                    ["enterprise"] = new List<string> { "enterprise-review-1", "enterprise-review-2" }
                }
            };

            _useCase = new SynchronizePullRequestsUseCase(
                new Logger<SynchronizePullRequestsUseCase>(new NullLoggerFactory()),
                _stubGitHubApi,
                _stubJiraApi,
                _configuration);
        }

        private async Task SynchronizePullRequestsAsync()
        {
            await _useCase.ExecuteAsync(DateTime.UtcNow);
        }
    }
}
