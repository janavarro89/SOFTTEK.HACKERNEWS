using SOFTTEK.HACKERNEWS.STORIES.Services;

namespace SOFTTEK.HACKERNEWS.STORIES
{
    public static class AddCustomServices
    {
        #region Custom Dependency Injection}
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IHackerNewsClient, HackerNewsClient>();
            services.AddSingleton<IBestStoriesService, BestStoriesService>();

            return services;
        }
        #endregion
    }
}
