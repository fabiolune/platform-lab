using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Models;
using LanguageExt;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Api.Services
{
    public class VideoRepositoryService : IVideoRepositoryService
    {
        private readonly IRedisCacheClient _cacheClient;

        public VideoRepositoryService(IRedisCacheClient cacheClient)
        {
            _cacheClient = cacheClient;
        }

        public Task<Option<IEnumerable<Video>>> GetVideosAsync(string session) =>
            _cacheClient
                .GetDbFromConfiguration()
                .GetAsync<IEnumerable<Video>>(session)
                .ToOptionAsync(_ => !_.Any())
                .IfNoneAsync(Array.Empty<Video>)
                .ToOptionAsync();

        public Task<Option<Unit>> Add(string session, IEnumerable<Video> videos) =>
            _cacheClient
                .GetDbFromConfiguration()
                .AddAsync(session, videos)
                .ToOptionAsync(_ => Unit.Default, _ => !_);
    }
}