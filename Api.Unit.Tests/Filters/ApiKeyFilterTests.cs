using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Filters;
using Common.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using RouteData = Microsoft.AspNetCore.Routing.RouteData;

namespace Api.Unit.Tests.Filters
{
    public class ApiKeyFilterTests
    {
        private ApiKeyFilter _sut;
        private const string ApiKey = "some-api-key";

        [SetUp]
        public void SetUp()
        {
            var config = new VideoApiConfiguration
            {
                ApiKey = ApiKey
            };

            _sut = new ApiKeyFilter(config);
        }

        [Test]
        public void OnActionExecutionAsync_WhenHeaderIsPresentAndValid_ShouldExecuteNext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("x-video-apikey", new StringValues(ApiKey));

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor(),
            };
            var metadata = new List<IFilterMetadata>();
            var controller = Mock.Of<Controller>();

            var context = new ActionExecutingContext(
                actionContext,
                metadata,
                new Dictionary<string, object>(),
                controller);
            
            var executed = false;
            _sut.OnActionExecutionAsync(context, () => {
                var ctx = new ActionExecutedContext(actionContext, metadata, controller);
                executed = true;
                return Task.FromResult(ctx);
            }).Wait();

            executed.Should().BeTrue();
        }

        [Test]
        public void OnActionExecutionAsync_WhenHeaderIsPresentButInvalid_ShouldReturn401()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("x-video-apikey", new StringValues("some wrong value"));

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor(),
            };
            var metadata = new List<IFilterMetadata>();
            var controller = Mock.Of<Controller>();

            var context = new ActionExecutingContext(
                actionContext,
                metadata,
                new Dictionary<string, object>(),
                controller);

            var executed = false;
            _sut.OnActionExecutionAsync(context, () => {
                var ctx = new ActionExecutedContext(actionContext, metadata, controller);
                executed = true;
                return Task.FromResult(ctx);
            }).Wait();

            executed.Should().BeFalse();
            context.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Test]
        public void OnActionExecutionAsync_WhenHeaderIsNotPresent_ShouldReturn401()
        {
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor(),
            };
            var metadata = new List<IFilterMetadata>();
            var controller = Mock.Of<Controller>();

            var context = new ActionExecutingContext(
                actionContext,
                metadata,
                new Dictionary<string, object>(),
                controller);

            var executed = false;
            _sut.OnActionExecutionAsync(context, () => {
                var ctx = new ActionExecutedContext(actionContext, metadata, controller);
                executed = true;
                return Task.FromResult(ctx);
            }).Wait();

            executed.Should().BeFalse();
            context.Result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
