using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace WikiParez.Controllers
{
    public class WikiController : Controller
    {
        private readonly WikiService _wikiService;

        public WikiController(WikiService wikiService)
        {
            _wikiService = wikiService;
        }

        [Route("{slug}")]
        public IActionResult Page(string slug)
        {
            var page = _wikiService.GetPageBySlug(slug);
            if (page == null)
                return NotFound();
            if (page.redirect != null && page.redirect != string.Empty)
            {
                return Page(page.redirect);
            }
            return View("Index", page);
        }

        public IActionResult Random()
        {
            var slug = _wikiService.GetRandomSlug();
            var page = _wikiService.GetPageBySlug(slug);
            if (page == null)
                return NotFound();
            return View("Index", page);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var slug = _wikiService.FindBestMatch(query, 1);
            return View("Index", _wikiService.GetPageBySlug(slug));
        }
    }
}
