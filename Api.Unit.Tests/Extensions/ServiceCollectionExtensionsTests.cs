using System;
using System.Collections.Generic;
using System.Linq;
using Api.Extensions;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceCollectionExtensions = Api.Extensions.ServiceCollectionExtensions;

namespace Api.Unit.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        private ServiceCollection _services;

        [SetUp]
        public void SetUp()
        {
            _services = new ServiceCollection();
        }

        private static IEnumerable<FlakinessConfiguration> GetInvalidConfigurations() =>
            new[]
            {
                null,
                new FlakinessConfiguration(),
                new FlakinessConfiguration
                {
                    Percentage = -10
                },
                new FlakinessConfiguration
                {
                    Percentage = 0
                }
            };

        [TestCaseSource(nameof(GetInvalidConfigurations))]
        public void WhenFlakinessConfigurationIsNotValid_ShouldNotRegisterDecorator(FlakinessConfiguration configuration)
        {
            _services.AddSingleton<IVideoRepositoryService, IVideoRepositoryService>();
            _services.AddFlakiness(configuration);

            _services
                .Where(_ => _.ServiceType == typeof(IVideoRepositoryService))
                .Should()
                .HaveCount(1);

            var registered = _services
                .FirstOrDefault(_ => _.ServiceType == typeof(IVideoRepositoryService));
            var registeredFunc = _services
                .FirstOrDefault(_ => _.ServiceType == typeof(Func<bool>));

            registered.ImplementationType.Should().Be<IVideoRepositoryService>();
            registered.ImplementationFactory.Should().BeNull();
            registeredFunc
                .Should()
                .BeNull();
        }

        [TestCase(100)]
        [TestCase(101)]
        [TestCase(102)]
        [TestCase(1000)]
        public void WhenPercentageIsGreaterThan100_ShouldRegisterDecoratorWithMaxFlakinessPercentage(int percentage)
        {
            var configuration = new FlakinessConfiguration
            {
                Percentage = percentage
            };
            _services.AddSingleton<IVideoRepositoryService, IVideoRepositoryService>();
            _services.AddFlakiness(configuration);

            _services
                .Where(_ => _.ServiceType == typeof(IVideoRepositoryService))
                .Should()
                .HaveCount(1);

            var registered = _services
                .FirstOrDefault(_ => _.ServiceType == typeof(IVideoRepositoryService));
            var registeredFunc = _services
                .FirstOrDefault(_ => _.ServiceType == typeof(Func<bool>));

            registered
                .ImplementationType
                .Should()
                .BeNull();

            registered
                .ImplementationFactory
                .Should()
                .NotBeNull();

            registeredFunc
                .ImplementationInstance
                .Should()
                .BeOfType<Func<bool>>();
        }
    }
}