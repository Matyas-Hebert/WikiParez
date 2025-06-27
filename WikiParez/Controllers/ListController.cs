using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Net;
using System.Net.Mail;

namespace WikiParez.Controllers
{
    public class ListController : Controller
    {
        private readonly WikiService _wikiService;

        public ListController(WikiService wikiService)
        {
            _wikiService = wikiService;
        }

        public IActionResult List()
        {
            var dict = _wikiService.GetSimplifiedDict();
            return View("Index", dict);
        }
    }
}