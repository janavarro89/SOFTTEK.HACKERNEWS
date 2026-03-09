using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOFTTEK.HACKERNEWS.APPLICATION.Cases;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Clients;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Configuration;
using SOFTTEK.HACKERNEWS.INFRASTRUCTURE.Gateways;

namespace SOFTTEK.HACKERNEWS.INFRASTRUCTURE
{
    public static class DependencyInjectionServices
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<HackerNewsOptions>()
                .Bind(configuration.GetSection(HackerNewsOptions.SectionName))
                .ValidateDataAnnotations()
                .Validate(
                    options => options.MaximumCandidates >= options.CandidateMultiplier,
                    "MaximumCandidates must be greater than or equal to CandidateMultiplier.")
                .ValidateOnStart();

            services.AddMemoryCache(options => options.SizeLimit = 10_000);

            services
                .AddHttpClient<IHackerNewsApiClient, HackerNewsApiClient>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<HackerNewsOptions>>().Value;
                    client.BaseAddress = new Uri(options.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(10);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Santander-HackerNews-Assessment/1.0");
                })
                .AddStandardResilienceHandler();

            services.AddScoped<RemoteHackerNewsGateway>();
            services.AddScoped<IHackerNewsGateway>(serviceProvider =>
            {
                var cache = serviceProvider.GetRequiredService<IMemoryCache>();
                var options = serviceProvider.GetRequiredService<IOptions<HackerNewsOptions>>();
                var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedHackerNewsGateway>>();
                var inner = serviceProvider.GetRequiredService<RemoteHackerNewsGateway>();

                return new CachedHackerNewsGateway(cache, inner, options, logger);
            });

            return services;
        }
    }
}
