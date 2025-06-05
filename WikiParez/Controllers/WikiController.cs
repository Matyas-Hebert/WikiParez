using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;

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
            return View("Index", page);
        }
    }
}
