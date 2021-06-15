using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Models;
using FluentAssertions;
using Frontend.Services;
using LanguageExt;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Frontend.Unit.Tests.Services
{
    public class VideoServiceTests
    {
        private const string CookieValue = "cookie-value";
        private const string BaseUrl = "http://video-api";
        private VideoService _sut;
        private Mock<HttpMessageHandler> _mockHandler;
        private Mock<ICookieManager> _mockManager;

        [SetUp]
        public void Setup()
        {
            _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var mockFactory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(_mockHandler.Object)
            {
                BaseAddress = new Uri(BaseUrl)
            };
            mockFactory
                .Setup(_ => _.CreateClient(SharedParameters.VideoClientName))
                .Returns(client);
            _mockManager = new Mock<ICookieManager>();

            _sut = new VideoService(mockFactory.Object, _mockManager.Object);
        }

        [Test]
        public void GetAll_WhenClientReturnsData_ShouldReturnVideoList()
        {
            _mockManager.Setup(_ => _.GetCookie()).Returns(CookieValue);
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(_ => _.RequestUri == new Uri($"{BaseUrl}/{CookieValue}/{SharedParameters.VideoListPath}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[{\"url\":\"https://www.youtube.com/watch?v=16fgzklcF7Y\",\"title\":\"Istio & Service Mesh - simply explained in 15 mins\"},{\"url\":\"https://www.youtube.com/watch?v=fy8SHvNZGeE\",\"title\":\"What is Helm?\"}]"),
                })
                .Verifiable();

            var result = _sut.GetAll();

            result
                .IsSome
                .Result
                .Should()
                .BeTrue();

            result
                .IfSome(_ => _.Should().BeEquivalentTo(new Video
                {
                    Url = "https://www.youtube.com/watch?v=16fgzklcF7Y",
                    Title = "Istio & Service Mesh - simply explained in 15 mins"
                }, new Video
                {
                    Url = "https://www.youtube.com/watch?v=fy8SHvNZGeE",
                    Title = "What is Helm?"
                }));

        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public void GetAll_WhenClientFails_ShouldReturnNone(HttpStatusCode code)
        {
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(_ => _.Method == HttpMethod.Get && _.RequestUri == new Uri($"{BaseUrl}/{CookieValue}/{SharedParameters.VideoListPath}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = code
                })
                .Verifiable();

            var result = _sut.GetAll();

            result
                .IsNone
                .Result
                .Should()
                .BeTrue();
        }

        [Test]
        public void GetAll_WhenCookieManagerReturnsNone_ShouldReturnNone()
        {
            _mockManager.Setup(_ => _.GetCookie()).Returns(Option<string>.None);

            var result = _sut.GetAll();

            result
                .IsNone
                .Result
                .Should()
                .BeTrue();
        }

        [Test]
        public void Add_WhenCookieManagerReturnsNone_ShouldReturnNone()
        {
            _mockManager.Setup(_ => _.GetCookie()).Returns(Option<string>.None);

            var result = _sut.Add(new Video());

            result
                .IsNone
                .Result
                .Should()
                .BeTrue();
        }

        [Test]
        public void Add_WhenClientReturnsOk_ShouldReturnSome()
        {
            _mockManager.Setup(_ => _.GetCookie()).Returns(CookieValue);
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(_ => _.Method == HttpMethod.Post && _.RequestUri == new Uri($"{BaseUrl}/{CookieValue}/{SharedParameters.VideoPath}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted
                })
                .Verifiable();

            var result = _sut.Add(new Video
            {
                Title = "title",
                Url = "http://url"
            });

            result
                .IsSome
                .Result
                .Should()
                .BeTrue();
        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public void Add_WhenClientFails_ShouldReturnNone(HttpStatusCode code)
        {
            _mockManager.Setup(_ => _.GetCookie()).Returns(CookieValue);
            _mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(_ => _.Method == HttpMethod.Post && _.RequestUri == new Uri($"{BaseUrl}/{CookieValue}/{SharedParameters.VideoPath}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = code
                })
                .Verifiable();

            var result = _sut.Add(new Video
            {
                Title = "title",
                Url = "http://url"
            });

            result
                .IsNone
                .Result
                .Should()
                .BeTrue();
        }
    }
}