using GitHubTagger.Models;
using GitHubTagger.Ports;

namespace GitHubTagger.Tests.Unit;

public class StubGitHubApi : IGitHubApi
{
    public Task<PullRequest[]> GetPullRequests(string userName)
    {
        return Task.FromResult(new PullRequest[] { PullRequest });
    }

    public Task<GitHubUser> GetCurrentUserAsync()
    {
        return Task.FromResult(new GitHubUser { UserName = "testuser" });
    }

    public Task UpdatePullRequestAsync(PullRequest pullRequest, List<string> labelsToAdd, List<string> reviewersToAdd)
    {
        PullRequestUpdates.Add((labelsToAdd, reviewersToAdd));
        return Task.CompletedTask;
    }

    public List<(List<string> LabelsToAdd, List<string> ReviewersToAdd)> PullRequestUpdates { get; } = new();
    public PullRequest PullRequest { get; set; } = new()
    {
        Id = 1,
        Number = 2,
        Title = "[JIRA-123] Pull-request title",
        Labels = Array.Empty<string>(),
        Reviewers = Array.Empty<string>()
    };
}