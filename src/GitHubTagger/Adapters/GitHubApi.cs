using GitHubTagger.Models;
using GitHubTagger.Ports;
using Octokit;
using Octokit.Internal;
using PullRequest = GitHubTagger.Models.PullRequest;

namespace GitHubTagger.Adapters
{
    internal class GitHubApi : IGitHubApi
    {
        private readonly GitHubClient _client;

        public GitHubApi(GitHubTaggerConfiguration configuration)
        {
            _client = new GitHubClient(
                new ProductHeaderValue("GitHubTagger", ApplicationInfo.Version), 
                new InMemoryCredentialStore(new Credentials(configuration.GitHubPat)));
        }

        public async Task<PullRequest[]> GetPullRequests(string userName)
        {
            var searchRequest = new SearchIssuesRequest
            {
                Type = IssueTypeQualifier.PullRequest,
                Author = userName,
                State = ItemState.Open,
                Repos = new RepositoryCollection { "StackEng/StackOverflow" }
            };

            var results = await _client.Search.SearchIssues(searchRequest);

            return results
                .Items
                .Select(ToPullRequest)
                .ToArray();
        }

        private static PullRequest ToPullRequest(Issue item)
        {
            var url = new Uri(item.Url);
            var pathParts = url.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var ownerName = pathParts[1];
            var repositoryName = pathParts[2];

            var pullRequest = new PullRequest
            {
                Id = item.Id,
                Number = item.Number,
                Owner = ownerName,
                Name = repositoryName,
                Title = item.Title,
                Labels = item.Labels.Select(l => l.Name.ToLower()).ToArray(),
                Reviewers = item.PullRequest.RequestedReviewers?.Select(r => r.Name.ToLower()).ToArray() ?? Array.Empty<string>()
            };

            return pullRequest;
        }

        public async Task<GitHubUser> GetCurrentUserAsync()
        {
            var user = await _client.User.Current();

            if (user != null)
            {
                return new GitHubUser
                {
                    Id = user.Id,
                    UserName = user.Login
                };
            }

            throw new Exception("Current user not found");
        }

        public async Task UpdatePullRequestAsync(PullRequest pullRequest, List<string> labelsToAdd, List<string> reviewersToAdd)
        {
            if(labelsToAdd.Any())
            {
                await _client.Issue.Labels.AddToIssue(
                    pullRequest.Owner, 
                    pullRequest.Name, 
                    pullRequest.Number,
                    labelsToAdd.ToArray());
            }

            if (reviewersToAdd.Any())
            {
                var reviewUsers = reviewersToAdd.Where(r => !r.Contains("/")).ToArray();
                var reviewTeams = reviewersToAdd.Where(r => r.Contains("/")).Select(r => r.Split('/')[1]).ToArray();

                await _client.PullRequest.ReviewRequest.Create(
                    pullRequest.Owner,
                    pullRequest.Name,
                    pullRequest.Number,
                    new PullRequestReviewRequest(reviewUsers, reviewTeams));
            }
        }
    }
}
