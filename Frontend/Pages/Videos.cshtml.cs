using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Models;
using Frontend.Services;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages
{
    public class VideosModel : PageModel
    {
        private readonly IVideoService _service;

        [BindProperty]
        public IEnumerable<Video> Videos { get; set; }

        [BindProperty]
        public Video NewVideo { get; set; }

        [BindProperty]
        public bool Error { get; set; }

        public VideosModel(IVideoService service)
        {
            _service = service;
        }

        public async Task OnGetAsync()
            => Videos = await _service
                .GetAll()
                .Match(_ => _, Array.Empty<Video>);

        public async Task OnPostAsync()
        {
            await _service
                .Add(NewVideo)
                .IfNone(() => Unit.Default.Tee(unit => Error = true));
            
            Videos = await _service
                .GetAll()
                .Match(_ => _, Array.Empty<Video>);
        }
    }
}