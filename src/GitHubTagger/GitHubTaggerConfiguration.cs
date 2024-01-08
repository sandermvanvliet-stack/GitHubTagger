namespace GitHubTagger;

public class GitHubTaggerConfiguration
{
    public string GitHubPat { get; set; }
    public string JiraApiKey { get; set; }
    public string JiraUrl { get; set; }
    public TimeSpan Interval { get; set; }
    public bool RunAtStartup { get; set; } = true;

    public Dictionary<string, List<string>> JiraToGitHubLabelMappings = new()
    {
        { "content", new List<string> { "team:teams-content-pod" } },
        { "community-growth", new List<string> { "team:teams-community-growth-pod" } },
        { "enterprise", new List<string> { "enterprise" } }
    };

    public Dictionary<string, List<string>> JiraLabelToGitHubReviewerMappings = new()
    {
        { "content", new List<string> { "stackeng/content-pod" } },
        { "community-growth", new List<string> { "stackeng/community-growth-pod" } },
        { "enterprise", new List<string> { "iprefer-pi" } }
    };

    public void ThrowIfInvalid()
    {
        if (string.IsNullOrWhiteSpace(GitHubPat))
        {
            throw new InvalidConfigurationException($"The value '{nameof(GitHubPat)}' was not configured");
        }

        if (string.IsNullOrWhiteSpace(JiraApiKey))
        {
            throw new InvalidConfigurationException($"The value '{nameof(JiraApiKey)}' was not configured");
        }

        if (string.IsNullOrWhiteSpace(JiraUrl))
        {
            throw new InvalidConfigurationException($"The value '{nameof(JiraUrl)}' was not configured");
        }

        if (Interval == TimeSpan.Zero)
        {
            throw new InvalidConfigurationException($"The value '{nameof(Interval)}' was not configured");
        }

        if (Interval < TimeSpan.FromMinutes(5))
        {
            throw new InvalidConfigurationException($"'{nameof(Interval)}' must be 5 minutes or more");
        }
    }
}