using GitHubTagger.Adapters;
using GitHubTagger.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubTagger;

public class Startup
{
    public void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var gitHubTaggerConfiguration = new GitHubTaggerConfiguration();

        configuration.GetSection("GitHubTagger").Bind(gitHubTaggerConfiguration);

        gitHubTaggerConfiguration.ThrowIfInvalid();

        serviceCollection.AddSingleton(gitHubTaggerConfiguration);

        serviceCollection.AddSingleton<IGitHubApi, GitHubApi>();
        serviceCollection.AddSingleton<IJiraApi, JiraApi>();

        serviceCollection.AddHostedService<Worker>();
    }
}