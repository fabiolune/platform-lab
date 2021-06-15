using System;
using System.Collections.Generic;
using Common.Models;
using FluentAssertions;
using Frontend.Pages;
using Frontend.Services;
using LanguageExt;
using Moq;
using NUnit.Framework;

namespace Frontend.Unit.Tests.Pages
{
    public class VideosTests
    {
        private Mock<IVideoService> _mockService;
        private VideosModel _sut;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IVideoService>();
            _sut = new VideosModel(_mockService.Object);
        }

        private static IEnumerable<object[]> VideoServiceGetScenarios()
            => new[]
            {
                new object[]
                {
                    OptionAsync<IEnumerable<Video>>.None, Array.Empty<Video>()
                },
                new object[]
                {
                    OptionAsync<IEnumerable<Video>>.Some(Array.Empty<Video>()), Array.Empty<Video>()
                },
                new object[]
                {
                    OptionAsync<IEnumerable<Video>>.Some(new List<Video>
                    {
                        new Video
                        {
                            Title = "title",
                            Url = "http://url"
                        }
                    }), new List<Video>
                    {
                        new Video
                        {
                            Title = "title",
                            Url = "http://url"
                        }
                    }
                }
            };

        [TestCaseSource(nameof(VideoServiceGetScenarios))]
        public void OnGet_PopulatesVideosAccordingToService(OptionAsync<IEnumerable<Video>> serviceResult, IEnumerable<Video> expected)
        {
            _mockService
                .Setup(_ => _.GetAll())
                .Returns(serviceResult);

            _sut.OnGetAsync().Wait();

            _sut.Videos.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<object[]> VideoServicePostScenarios() =>
            new List<object[]>
            {
                new object[]{ OptionAsync<LanguageExt.Unit>.None, true},
                new object[]{ OptionAsync<LanguageExt.Unit>.Some(LanguageExt.Unit.Default), false},
            };

        [TestCaseSource(nameof(VideoServicePostScenarios))]
        public void OnPostAsync_WhenServiceReturnsSome_ShouldSetErrorToTrue(OptionAsync<LanguageExt.Unit> serviceResult, bool expectedError)
        {
            _mockService
                .Setup(_ => _.Add(It.IsAny<Video>()))
                .Returns(serviceResult);

            _sut.OnPostAsync().Wait();

            _sut.Error.Should().Be(expectedError);
        }
    }
}