using GitHubTagger.Models;

namespace GitHubTagger.Ports;

public interface IJiraApi
{
    Task<JiraTicket?> GetTicketByIdAsync(string id);
}