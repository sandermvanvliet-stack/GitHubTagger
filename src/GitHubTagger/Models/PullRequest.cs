namespace GitHubTagger.Models;

public class PullRequest
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string[] Labels { get; set; }
    public string[] Reviewers { get; set; }
    public int Number { get; set; }
    public string Owner { get; set; }
    public string Name { get; set; }
}