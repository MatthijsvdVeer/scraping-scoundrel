using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ScrapingScoundrel.Functions.Startup))]
namespace ScrapingScoundrel.Functions
{
    using Azure.Data.Tables;
    using Azure.Identity;
    using Microsoft.Azure.Functions.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var tableEndpoint = builder.GetContext().Configuration["TableEndpoint"];
            var tableName = builder.GetContext().Configuration["TableName"];
            _ = builder.Services.AddSingleton(new TableClient(
                new Uri($"{tableEndpoint}/{tableName}"),
                tableName,
                new DefaultAzureCredential()));
            _ = builder.Services.AddHttpClient<ScrapeFunction>();
        }
    }
}
