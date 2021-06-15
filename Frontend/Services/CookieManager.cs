using System;
using Common;
using Common.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Http;

namespace Frontend.Services
{
    public class CookieManager : ICookieManager
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private const string CookieName = "appsid";

        public CookieManager(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Option<string> GetCookie() =>
            _contextAccessor
                .HttpContext
                .Request
                .Cookies[CookieName]
                .ToOption(string.IsNullOrWhiteSpace);

        public Unit SetCookie(string value) =>
            Unit
                .Default
                .Tee(_ => _contextAccessor
                    .HttpContext
                    .Response
                    .Cookies
                    .Append(CookieName, value, new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow + SharedParameters.SessionDuration
                    }));
    }
}