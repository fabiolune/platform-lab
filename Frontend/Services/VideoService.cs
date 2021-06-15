using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Models;
using LanguageExt;
using static Common.SharedParameters;

namespace Frontend.Services
{
    public class VideoService : IVideoService
    {
        private readonly IHttpClientFactory _factory;
        private readonly ICookieManager _cookieManager;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public VideoService(IHttpClientFactory factory, ICookieManager cookieManager)
        {
            _factory = factory;
            _cookieManager = cookieManager;
        }

        public OptionAsync<IEnumerable<Video>> GetAll() =>
            _cookieManager
                .GetCookie()
                .Bind<string>(_ => $"{_}/{VideoListPath}")
                .BindAsync<string, HttpResponseMessage>(_ => _factory.CreateClient(VideoClientName).GetAsync(_))
                .Bind(_ => _.IsSuccessStatusCode ? _ : OptionAsync<HttpResponseMessage>.None)
                .Bind<HttpContent>(_ => _.Content)
                .Bind<string>(_ => _.ReadAsStringAsync())
                .Map(_ => JsonSerializer.Deserialize<IEnumerable<Video>>(_, JsonOptions));

        public OptionAsync<Unit> Add(Video video) =>
            _cookieManager
                .GetCookie()
                .Bind<string>(_ => $"{_}/{VideoPath}")
                .BindAsync<string, HttpResponseMessage>(_ => _factory.CreateClient(VideoClientName).PostAsync(_, JsonContent.Create(video)))
                .Bind(_ => _.StatusCode == HttpStatusCode.Accepted ? _ : OptionAsync<HttpResponseMessage>.None)
                .Map(_ => Unit.Default);
    }
}