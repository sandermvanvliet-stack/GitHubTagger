using GitHubTagger.Models;

namespace GitHubTagger.Ports;

public interface IGitHubApi
{
    Task<PullRequest[]> GetPullRequests(string userName);
    Task<GitHubUser> GetCurrentUserAsync();
    Task UpdatePullRequestAsync(PullRequest pullRequest, List<string> labelsToAdd, List<string> reviewersToAdd);
}