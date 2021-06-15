using System.Collections.Generic;
using System.Linq;
using System.Net;
using Api.Controllers;
using Api.Services;
using Common.Models;
using FluentAssertions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Api.Unit.Tests.Controllers
{
    public class VideoControllerTests
    {
        private Mock<IVideoRepositoryService> _mockRepo;
        private VideoController _sut;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IVideoRepositoryService>();
            _sut = new VideoController(_mockRepo.Object);
        }

        [Test]
        public void GetVideosAsync_WhenDatabaseReturnsSomeData_ShouldReturnOkWithData()
        {
            var videos = new[]
            {
                new Video
                {
                    Title = "one",
                    Url = "url://one"
                },
                new Video
                {
                    Title = "two",
                    Url = "url://two"
                }
            };
            _mockRepo
                .Setup(_ => _.GetVideosAsync("session"))
                .ReturnsAsync(videos);

            var result = _sut.GetVideosAsync("session").Result;

            result.Should().BeOfType<JsonResult>();

            ((JsonResult)result)
                .Value
                .Should()
                .BeEquivalentTo(videos);
        }

        [Test]
        public void GetVideosAsync_WhenDatabaseReturnsNone_ShouldReturn500()
        {
            _mockRepo
                .Setup(_ => _.GetVideosAsync("session"))
                .ReturnsAsync(Option<IEnumerable<Video>>.None);

            var result = _sut.GetVideosAsync("session").Result;

            result.Should().BeOfType<StatusCodeResult>();

            ((StatusCodeResult) result)
                .StatusCode
                .Should()
                .Be((int) HttpStatusCode.InternalServerError);
        }

        [Test]
        public void AddVideoAsync_WhenRepositoryGetReturnsNone_ShouldReturn500()
        {
            _mockRepo
                .Setup(_ => _.GetVideosAsync("session"))
                .ReturnsAsync(Option<IEnumerable<Video>>.None);

            var video = new Video
            {
                Title = "some_title"
            };

            var result = _sut.AddVideoAsync("session", video).Result;

            result.Should().BeOfType<StatusCodeResult>();

            ((StatusCodeResult)result)
                .StatusCode
                .Should()
                .Be((int)HttpStatusCode.InternalServerError);

            _mockRepo
                .Verify(_ => _.Add("session", It.IsAny<IEnumerable<Video>>()), Times.Never);
        }

        [Test]
        public void AddVideoAsync_WhenDatabaseReturnsFalse_ShouldReturnInternalServerError()
        {
            var videos = new []
            {
                new Video
                {
                    Title = "one",
                    Url = "url://one"
                },
                new Video
                {
                    Title = "two",
                    Url = "url://two"
                }
            };

            var video = new Video
            {
                Title = "three",
                Url = "url://three"
            };

            _mockRepo
                .Setup(_ => _.GetVideosAsync("session"))
                .ReturnsAsync(videos);

            var allVideos = videos.Concat(new[]
            {
                video
            });

            _mockRepo
                .Setup(_ => _.Add("session", allVideos))
                .ReturnsAsync(Option<LanguageExt.Unit>.None);

            var result = _sut.AddVideoAsync("session", video).Result;

            result
                .Should()
                .BeEquivalentTo(new StatusCodeResult((int)HttpStatusCode.InternalServerError));

            _mockRepo
                .Verify(_ => _.Add("session", allVideos), Times.Once);

        }

        [Test]
        public void AddVideoAsync_WhenDatabaseAddReturnsTrue_ShouldReturnAccepted()
        {
            var videos = new[]
            {
                new Video
                {
                    Title = "one",
                    Url = "url://one"
                },
                new Video
                {
                    Title = "two",
                    Url = "url://two"
                }
            };

            var video = new Video
            {
                Title = "three",
                Url = "url://three"
            };

            _mockRepo
                .Setup(_ => _.GetVideosAsync("session"))
                .ReturnsAsync(videos);

            var allVideos = videos.Concat(new[]
            {
                video
            });

            _mockRepo
                .Setup(_ => _.Add("session", allVideos))
                .ReturnsAsync(LanguageExt.Unit.Default);

            var result = _sut.AddVideoAsync("session", video).Result;

            result
                .Should()
                .BeEquivalentTo(new AcceptedResult());

            _mockRepo
                .Verify(_ => _.Add("session", allVideos), Times.Once);

        }
    }
}