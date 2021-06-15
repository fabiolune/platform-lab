using System;
using Api.Models;
using Api.Services;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFlakiness(this IServiceCollection services, FlakinessConfiguration configuration) =>
            configuration
                .ToOption(_ => _.Percentage <= 0)
                .Bind<int>(_ => _.Percentage)
                .Bind<int>(_ => _.ToOption(__ => __ > 100).IfNone(() => 100))
                .IfSome(_ =>
                {
                    services.AddSingleton<Func<bool>>(() => new Random().Next(0, 100) < _);
                    services.Decorate<IVideoRepositoryService, FlakyVideoRepositoryService>();
                });
    }

}