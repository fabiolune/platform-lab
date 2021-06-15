using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Models;
using LanguageExt;

namespace Api.Services
{
    public class FlakyVideoRepositoryService : IVideoRepositoryService
    {
        private readonly Func<bool> _flakyWhen;
        private readonly IVideoRepositoryService _service;

        public FlakyVideoRepositoryService(Func<bool> flakyWhen, IVideoRepositoryService service)
        {
            _flakyWhen = flakyWhen;
            _service = service;
        }

        public Task<Option<IEnumerable<Video>>> GetVideosAsync(string session) =>
            _flakyWhen()
                .ToOption(_ => _)
                .Match(_ => _service.GetVideosAsync(session), Task.FromResult(Option<IEnumerable<Video>>.None));

        public Task<Option<Unit>> Add(string session, IEnumerable<Video> videos) =>
            _service.Add(session, videos);

    }
}