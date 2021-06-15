using System.Threading.Tasks;
using Common.Extensions;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static Common.SharedParameters;

namespace Api.Filters
{
    public class ApiKeyFilter : IApiKeyFilter
    {
        private readonly VideoApiConfiguration _apiConfiguration;
        private static readonly UnauthorizedResult Unauthorized = new UnauthorizedResult();

        public ApiKeyFilter(VideoApiConfiguration apiConfiguration)
        {
            _apiConfiguration = apiConfiguration;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) =>
            context
                .HttpContext
                .Request
                .Headers
                .ToOption(_ => !_.ContainsKey(AuthHeaderName) || _[AuthHeaderName] != _apiConfiguration.ApiKey)
                .Match(_ => next(), () =>
                {
                    //context.
                    context.Result = Unauthorized;
                    return Task.CompletedTask;
                });
    }
}
