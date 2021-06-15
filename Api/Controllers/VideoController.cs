using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.Filters;
using Api.Services;
using Common.Models;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ServiceFilter(typeof(IApiKeyFilter))]
    public class VideoController : ControllerBase
    {
        private static readonly IActionResult FailureResult =
            new StatusCodeResult((int) HttpStatusCode.InternalServerError);

        private readonly IVideoRepositoryService _repositoryService;

        public VideoController(IVideoRepositoryService repositoryService)
        {
            _repositoryService = repositoryService;
        }

        [HttpGet("{session}/videos")]
        public Task<IActionResult> GetVideosAsync(string session) =>
            _repositoryService
                .GetVideosAsync(session)
                .Match(_ => new JsonResult(_), () => FailureResult);

        [HttpPost("{session}/video")]
        public Task<IActionResult> AddVideoAsync(string session, [FromBody] Video video) =>
            _repositoryService
                .GetVideosAsync(session)
                .MapAsync(_ => _.Concat(new[] {video}))
                .MatchAsync(_ => _repositoryService.Add(session, _), () => Option<Unit>.None)
                .Match(_ => Accepted(), () => FailureResult);

    }
}