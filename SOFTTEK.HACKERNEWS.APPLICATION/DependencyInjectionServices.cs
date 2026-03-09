using Microsoft.Extensions.DependencyInjection;
using SOFTTEK.HACKERNEWS.APPLICATION.Cases;
using SOFTTEK.HACKERNEWS.APPLICATION.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOFTTEK.HACKERNEWS.APPLICATION
{
    public static class DependencyInjectionServices
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IGetBestStoriesHandler, GetBestStoriesHandler>();
            return services;
        }
    }
}
