using System.Collections.Generic;
using Api.Services;
using Common.Models;
using FluentAssertions;
using LanguageExt;
using Moq;
using NUnit.Framework;

namespace Api.Unit.Tests.Services
{
    public class FlakyVideoRepositoryServiceTests
    {
        private Mock<IVideoRepositoryService> _mockService;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IVideoRepositoryService>();
        }

        [Test]
        public void GetVideosAsync_WhenFlakyIsTrue_ShouldReturnNone()
        {
            var sut = new FlakyVideoRepositoryService(() => true, _mockService.Object);

            var result = sut.GetVideosAsync("session").Result;

            result
                .IsNone
                .Should()
                .BeTrue();

            _mockService.Verify(_ => _.GetVideosAsync("session"), Times.Never);
        }

        [Test]
        public void GetVideosAsync_WhenFlakyIsFalseAndServiceReturnsSome_ShouldReturnSome()
        {
            var sut = new FlakyVideoRepositoryService(() => false, _mockService.Object);
            var videos = new[]
            {
                new Video
                {
                    Title = "title 1"
                },
                new Video
                {
                    Title = "title 2"
                }
            };
            _mockService.Setup(_ => _.GetVideosAsync("session")).ReturnsAsync(videos);

            var result = sut.GetVideosAsync("session").Result;

            result
                .IsSome
                .Should()
                .BeTrue();

            result.IfSome(_ => _.Should().BeSameAs(videos));
        }

        [Test]
        public void GetVideosAsync_WhenFlakyIsFalseAndServiceReturnsNone_ShouldReturnNone()
        {
            var sut = new FlakyVideoRepositoryService(() => false, _mockService.Object);
            _mockService.Setup(_ => _.GetVideosAsync("session")).ReturnsAsync(Option<IEnumerable<Video>>.None);

            var result = sut.GetVideosAsync("session").Result;

            result
                .IsNone
                .Should()
                .BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Add_WhenServiceReturnsNone_ShouldReturnNone(bool isFlaky)
        {
            var sut = new FlakyVideoRepositoryService(() => isFlaky, _mockService.Object);

            _mockService.Setup(_ => _.Add("session", It.IsAny<IEnumerable<Video>>())).ReturnsAsync(Option<LanguageExt.Unit>.None);

            var videos = new[]
            {
                new Video
                {
                    Title = "title 1"
                },
                new Video
                {
                    Title = "title 2"
                }
            };

            var result = sut.Add("session", videos).Result;

            result
                .IsNone
                .Should()
                .BeTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Add_WhenServiceReturnsSome_ShouldReturnSome(bool isFlaky)
        {
            var sut = new FlakyVideoRepositoryService(() => isFlaky, _mockService.Object);

            _mockService.Setup(_ => _.Add("session", It.IsAny<IEnumerable<Video>>())).ReturnsAsync(LanguageExt.Unit.Default);

            var videos = new[]
            {
                new Video
                {
                    Title = "title 1"
                },
                new Video
                {
                    Title = "title 2"
                }
            };

            var result = sut.Add("session", videos).Result;

            result
                .IsSome
                .Should()
                .BeTrue();
        }
    }
}
