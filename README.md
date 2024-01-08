# GitHub Tagger

This application is a utility I built to automatically add the relevant labels and reviewers to pull requests based on their Jira ticket.

It works like this:

- Get open pull requests for the configured account from GitHub
- For each pull request:
  - Check if the title contains a Jira ticket like `[TEAMS-13245]`
  - Look up the ticket in Jira
  - For each label on the Jira ticket:
    - Map it to a GitHub label (using the `GitHubTaggerConfiguration.JiraToGitHubLabelMappings` values) and add them to the pull request if they are not present yet
    - Map it to a GitHub reviewer (using the `GitHubTaggerConfiguration.JiraLabelToGitHubReviewerMappings` values) and add them to the pull request if they are not present yet

## Usage

You wil need:

- A GitHub personal access token
- A Jira API token
- A list of Jira labels you want to map and:
  - the relevant GitHub labels they map to
  - the relevant GitHub reviewers they map to

The application uses the Microsoft `IConfiguration` approach so you can either configure these through environment variables (useful when running via Docker) or by dropping an `appsettings.json` into the working directory.

The `appsettings.json` would look like this:

```json
{
    "GitHubTagger": {
        "GitHubPat": "super secret",
        "JiraApiKey": "super secret",
        "JiraUrl": "https://your-company.atlassian.net",
        "Interval": "00:05:00",
        "JiraToGitHubLabelMappings": {
            "label-1": [ "gh-label-A", "gh-label-B" ],
            "label-2": [ "gh-label-C" ]
        },
        "JiraLabelToGitHubReviewerMappings": {
            "label-1": [ "gh-review-group-A", "gh-review-group-B" ],
            "label-5": [ "gh-review-group-C" ]
        }
    }
}
```

`Interval` is `HH:mm:ss`, don't set this too short because GitHub will rate-limit you.

Using environment variables, you can do that like this (the mappings get a bit clunky):

```.env
GitHubTagger:GitHubPat=super secret
GitHubTagger:JiraApiKey=super secret
GitHubTagger:JiraUrl=https://your-company.atlassian.net
GitHubTagger:Interval=00:05:00
GitHubTagger:JiraToGitHubLabelMappings:label-1:0=gh-label-A
GitHubTagger:JiraToGitHubLabelMappings:label-1:1=gh-label-B
GitHubTagger:JiraToGitHubLabelMappings:label-2:0=gh-label-C
GitHubTagger:JiraLabelToGitHubReviewerMappings:label-1:0=gh-review-group-A
GitHubTagger:JiraLabelToGitHubReviewerMappings:label-1:1=gh-review-group-B
GitHubTagger:JiraLabelToGitHubReviewerMappings:label-5:0=gh-review-group-C
```

Example [`.env`](/src/GitHubTagger/.env) file.

The easiest way is to run using Docker:

```bash
user@host> docker build . -t githubtagger/latest
user@host> docker run -u app --rm --env-file .env --name githubtagger githubtagger/latest
```