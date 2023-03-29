using Atlassian.Jira;
using GitHubTagger.Models;
using GitHubTagger.Ports;

namespace GitHubTagger.Adapters
{
    internal class JiraApi : IJiraApi
    {
        private readonly Jira _client;

        public JiraApi(GitHubTaggerConfiguration configuration)
        {
            _client = Jira.CreateRestClient(configuration.JiraUrl, "svanvliet@stackoverflow.com", configuration.JiraApiKey);
        }

        public async Task<JiraTicket?> GetTicketByIdAsync(string id)
        {
            var issue = _client.Issues.Queryable.SingleOrDefault(issue => issue.Project == "TEAMS" && issue.Key == id);

            if (issue != null)
            {
                return new JiraTicket
                {
                    Id = issue.JiraIdentifier,
                    Labels = issue.Labels.Select(l => l.ToLower()).ToArray()
                };
            }

            return null;
        }
    }
}
