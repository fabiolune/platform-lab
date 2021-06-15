using System;
using System.Collections.Generic;
using Api.Services;
using Common.Models;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Api.Unit.Tests.Services
{
    public class VideoRepositoryServiceTests
    {
        private Mock<IRedisDatabase> _mockDatabase;

        private VideoRepositoryService _sut;

        [SetUp]
        public void SetUp()
        {
            _mockDatabase = new Mock<IRedisDatabase>();
            var mockClient = new Mock<IRedisCacheClient>();
            mockClient.Setup(_ => _.GetDbFromConfiguration()).Returns(_mockDatabase.Object);
            _sut = new VideoRepositoryService(mockClient.Object);
        }

        [Test]
        public void GetVideos_WhenRedisReturnsItems_ShouldReturnThem()
        {
            var videos = new List<Video>
            {
                new Video
                {
                    Title = "title1"
                },
                new Video
                {
                    Title = "title2"
                }
            };

            _mockDatabase
                .Setup(_ => _.GetAsync<IEnumerable<Video>>("session", CommandFlags.None))
                .ReturnsAsync(videos);

            var result = _sut.GetVideosAsync("session").Result;

            result
                .IsSome
                .Should()
                .BeTrue();

            result
                .IfSome(_ => _.Should().BeEquivalentTo(videos));
        }

        [Test]
        public void GetVideos_WhenRedisReturnsNull_ShouldReturnThem()
        {
            _mockDatabase
                .Setup(_ => _.GetAsync<IEnumerable<Video>>("session", CommandFlags.None))
                .ReturnsAsync((IEnumerable<Video>) null);

            var result = _sut.GetVideosAsync("session").Result;

            result
                .IsSome
                .Should()
                .BeTrue();

            result
                .IfSome(_ => _.Should().BeSameAs(Array.Empty<Video>()));
        }

        [Test]
        public void GetVideos_WhenRedisReturnsEmpty_ShouldReturnThem()
        {
            _mockDatabase
                .Setup(_ => _.GetAsync<IEnumerable<Video>>("session", CommandFlags.None))
                .ReturnsAsync(Array.Empty<Video>());

            var result = _sut.GetVideosAsync("session").Result;

            result
                .IsSome
                .Should()
                .BeTrue();

            result
                .IfSome(_ => _.Should().BeSameAs(Array.Empty<Video>()));
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void AddVideosAsync_ShouldReturnRedisDatabaseResult(bool dbResult, bool expectedSome)
        {
            IEnumerable<Video> videos = new List<Video>
            {
                new Video
                {
                    Title = "title1"
                },
                new Video
                {
                    Title = "title2"
                }
            };

            _mockDatabase
                .Setup(_ => _.AddAsync("session", videos, When.Always, CommandFlags.None, null))
                .ReturnsAsync(dbResult);

            var result = _sut.Add("session", videos).Result;

            result
                .IsSome
                .Should()
                .Be(expectedSome);
        }
    }
}