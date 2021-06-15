using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Models;
using LanguageExt;

namespace Api.Services
{
    public interface IVideoRepositoryService
    {
        Task<Option<IEnumerable<Video>>> GetVideosAsync(string session);
        Task<Option<Unit>> Add(string session, IEnumerable<Video> videos);
    }
}
