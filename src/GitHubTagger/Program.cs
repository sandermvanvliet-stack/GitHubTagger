using GitHubTagger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configurationBuilder) => configurationBuilder.AddEnvironmentVariables())
    .ConfigureServices((hostContext, services) => { new Startup().ConfigureServices(services, hostContext.Configuration); })
    .Build();

host.Run();