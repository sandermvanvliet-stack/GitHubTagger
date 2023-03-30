using GitHubTagger.Models;
using GitHubTagger.Ports;

namespace GitHubTagger.Tests.Unit;

public class StubJiraApi : IJiraApi
{
    public Task<JiraTicket?> GetTicketByIdAsync(string id)
    {
        if (Tickets.ContainsKey(id))
        {
            return Task.FromResult(Tickets[id]);
        }

        return Task.FromResult<JiraTicket?>(null);
    }

    public Dictionary<string, JiraTicket?> Tickets { get; set; } = new();
}