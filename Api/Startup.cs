using System.Diagnostics.CodeAnalysis;
using Api.Extensions;
using Api.Filters;
using Api.Models;
using Api.Services;
using Common.Extensions;
using Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            Configuration
                .GetSection("RedisConnectionString")
                .Value
                .Map(_ => new RedisConfiguration
                {
                    ConnectionString = _
                })
                .Do(_ => services.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(_));

            Configuration
                .GetSection(nameof(VideoApiConfiguration))
                .Get<VideoApiConfiguration>()
                .Do(_ => services.AddSingleton(_));

            services.AddScoped<IApiKeyFilter, ApiKeyFilter>();
            services.AddSingleton<IVideoRepositoryService, VideoRepositoryService>();
            Configuration
                .GetSection(nameof(FlakinessConfiguration))
                .Get<FlakinessConfiguration>()
                .Do(services.AddFlakiness);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(ep => ep.MapControllers());
        }
    }
}