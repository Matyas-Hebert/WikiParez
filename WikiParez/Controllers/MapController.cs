using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WikiParez.Models;

namespace WikiParez.Controllers
{
    public class MapController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WikiService _wikiService;

        public MapController(ILogger<HomeController> logger, WikiService wikiService)
        {
            _logger = logger;
            _wikiService = wikiService;
        }
        public IActionResult Elevators()
        {
            return View("Elevator");
        }
    }
}