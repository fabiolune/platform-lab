using System.Collections.Generic;
using Common.Models;
using LanguageExt;

namespace Frontend.Services
{
    public interface IVideoService
    {
        OptionAsync<IEnumerable<Video>> GetAll();
        OptionAsync<Unit> Add(Video video);
    }
}