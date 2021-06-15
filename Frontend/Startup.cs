using System;
using System.Diagnostics.CodeAnalysis;
using Common.Extensions;
using Common.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Frontend
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
            services.AddSingleton<IVideoService, VideoService>();
            services.AddSingleton<ICookieManager, CookieManager>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRazorPages();

            Configuration
                .GetSection(nameof(VideoApiClientConfiguration))
                .Get<VideoApiClientConfiguration>()
                .Do(_ => services.AddHttpClient(Common.SharedParameters.VideoClientName, client =>
                {
                    client.BaseAddress = new Uri(_.BaseUrl);
                    client.DefaultRequestHeaders.Add(Common.SharedParameters.AuthHeaderName, _.ApiKey);
                }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}