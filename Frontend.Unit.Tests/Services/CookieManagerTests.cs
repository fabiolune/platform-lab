using System.Linq;
using FluentAssertions;
using Frontend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using NUnit.Framework;

namespace Frontend.Unit.Tests.Services
{
    public class CookieManagerTests
    {
        private CookieManager _sut;
        private Mock<IHttpContextAccessor> _mockAccessor;

        [SetUp]
        public void SetUp()
        {
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _sut = new CookieManager(_mockAccessor.Object);
        }

        [Test]
        public void GetCookie_WhenCookieDoesNotExist_ShouldReturnNone()
        {
            var context = new DefaultHttpContext();
            _mockAccessor.Setup(_ => _.HttpContext).Returns(context);

            var result = _sut.GetCookie();

            result
                .IsNone
                .Should()
                .BeTrue();
        }

        [Test]
        public void GetCookie_WhenCookieExists_ShouldReturnSome()
        {
            var context = new DefaultHttpContext();

            var requestFeature = new HttpRequestFeature
            {
                Headers = new HeaderDictionary { { HeaderNames.Cookie, new StringValues("appsid=value") } }
            };

            var featureCollection = new FeatureCollection();
            featureCollection.Set<IHttpRequestFeature>(requestFeature);

            var cookiesFeature = new RequestCookiesFeature(featureCollection);

            context.Request.Cookies = cookiesFeature.Cookies;


            _mockAccessor.Setup(_ => _.HttpContext).Returns(context);

            var result = _sut.GetCookie();

            result
                .IsSome
                .Should()
                .BeTrue();

            result
                .IfSome(_ => _.Should().Be("value"));
        }

        [Test]
        public void SetCookie_ShouldPopulateResponseCookie()
        {
            var context = new DefaultHttpContext();
            _mockAccessor.Setup(_ => _.HttpContext).Returns(context);

            _sut.SetCookie("some-value");
            var cookieHeader = context
                .Response
                .Headers
                .Values
                .FirstOrDefault(_ => _.ToString().StartsWith("appsid"))
                .ToString();

            cookieHeader.Should().NotBeNullOrEmpty();
            cookieHeader.Should().StartWith("appsid=some-value");
        }
    }
}