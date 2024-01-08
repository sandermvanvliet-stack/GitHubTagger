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
            _client = Jira.CreateRestClient(configuration.JiraUrl, configuration.JiraUserName, configuration.JiraApiKey);
        }

        public async Task<JiraTicket?> GetTicketByIdAsync(string id)
        {
            var project = id.Split("-")[0];
            
            var issue = _client.Issues.Queryable.SingleOrDefault(issue => issue.Project == project && issue.Key == id);

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
