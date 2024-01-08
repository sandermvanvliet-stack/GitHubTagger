using GitHubTagger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configurationBuilder) => 
        configurationBuilder
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json", true))
    .ConfigureServices((hostContext, services) => { new Startup().ConfigureServices(services, hostContext.Configuration); })
    .Build();

host.Run();