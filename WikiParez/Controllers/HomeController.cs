using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WikiParez.Models;
using WikiParez.Services;

namespace WikiParez.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly WikiService _wikiService;

    public HomeController(ILogger<HomeController> logger, WikiService wikiService)
    {
        _logger = logger;
        _wikiService = wikiService;
    }

    public IActionResult Index()
    {
        return View(_wikiService.Last10pages());
    }

    public IActionResult New(){
        return View("New");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
