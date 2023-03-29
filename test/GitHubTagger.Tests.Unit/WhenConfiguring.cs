using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubTagger.Tests.Unit
{
    public class WhenConfiguring
    {
        [Fact]
        public void GivenGitHubPatNotConfigured_ExceptionIsThrown()
        {
            var configuration = GivenConfiguration(new Dictionary<string, string?>());
            var serviceCollection = new ServiceCollection();

            var action = () => new Startup().ConfigureServices(serviceCollection, configuration);

            action
                .Should()
                .Throw<InvalidConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("The value 'GitHubPat' was not configured");
        }

        [Fact]
        public void GivenJiraApiKeyNotConfigured_ExceptionIsThrown()
        {
            var configuration = GivenConfiguration(new Dictionary<string, string?> { { "GitHubTagger:GitHubPat", "DEADBEEF"} });
            var serviceCollection = new ServiceCollection();

            var action = () => new Startup().ConfigureServices(serviceCollection, configuration);

            action
                .Should()
                .Throw<InvalidConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("The value 'JiraApiKey' was not configured");
        }

        [Fact]
        public void GivenIntervalIsLessThanFiveMinutes_ExceptionIsThrown()
        {
            var configuration = GivenConfiguration(new Dictionary<string, string?>
            {
                { "GitHubTagger:GitHubPat", "DEADBEEF" },
                { "GitHubTagger:JiraApiKey", "DEADBEEF" },
                { "GitHubTagger:JiraUrl", "https://example.com" },
                { "GitHubTagger:Interval", "00:01:00" }
            });
            var serviceCollection = new ServiceCollection();

            var action = () => new Startup().ConfigureServices(serviceCollection, configuration);

            action
                .Should()
                .Throw<InvalidConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("'Interval' must be 5 minutes or more");
        }

        [Fact]
        public void GivenAllSettingsPresent_ConfigurationIsValid()
        {
            var configuration = GivenConfiguration(new Dictionary<string, string?>
            {
                { "GitHubTagger:GitHubPat", "DEADBEEF" },
                { "GitHubTagger:JiraApiKey", "DEADBEEF" },
                { "GitHubTagger:JiraUrl", "https://example.com" },
                { "GitHubTagger:Interval", "00:10:00" }
            });
            var serviceCollection = new ServiceCollection();

            new Startup().ConfigureServices(serviceCollection, configuration);

            var gitHubTaggerConfiguration = serviceCollection.BuildServiceProvider().GetRequiredService<GitHubTaggerConfiguration>();

            gitHubTaggerConfiguration.GitHubPat.Should().Be("DEADBEEF");
            gitHubTaggerConfiguration.JiraApiKey.Should().Be("DEADBEEF");
        }

        private IConfiguration GivenConfiguration(IEnumerable<KeyValuePair<string, string?>> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}
