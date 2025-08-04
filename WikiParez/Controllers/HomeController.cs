using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        ViewBag.FinishedPages = _wikiService.getFinishedPages();
        ViewBag.TotalPages = _wikiService.getTotalPages();
        var splashtexts = _wikiService.getSplashTexts();
        var random = new Random();
        ViewBag.splashtext = splashtexts[random.Next(splashtexts.Count)];
        var percentagedone = ((double)_wikiService.getFinishedPages() / (double)_wikiService.getTotalPages());
        var b = percentagedone * 360.0f;
        var c = Math.Round(percentagedone * 100, 2);
        var timesincefirstpage = (DateTime.Today - new DateTime(2025, 6, 6)).TotalDays;
        var pagesperday = (double)_wikiService.getFinishedPages() / (double)timesincefirstpage;
        var pagesremaining = _wikiService.getTotalPages() - _wikiService.getFinishedPages();
        var expecteddone = DateTime.Today.AddDays(Math.Round(pagesremaining / pagesperday)).ToString("dd/MM/yyyy");
        ViewBag.date = expecteddone;
        ViewBag.degrees = b.ToString() + "deg";
        ViewBag.percentage = c.ToString() + "%";
        return View(_wikiService.Last10pages());
    }

    public IActionResult New(){
        return View("New");
    }

    [Route("Top/Parezle")]
    public IActionResult TopParezle()
    {
        return View("Leaderboard", _wikiService.GetTopParezleRooms());
    }

    [Route("Top/Menu")]
    public IActionResult TopMenu()
    {
        return View("TopMenu");
    }

    [Route("List/Rooms")]
    public IActionResult Listrooms()
    {
        var list = new List<string>();
        var pages = _wikiService.GetParezlePages();
        foreach (var page in pages.Keys)
        {
            list.Add(pages[page].Title);
        }
        list.Sort();
        return View("List", list);
    }

    [Route("List/Blocks")]
    public IActionResult Listblocks()
    {
        var list = new List<string>();
        var pages = _wikiService.GetPages();
        foreach (var page in pages.Keys)
        {
            if (pages[page].Type == "blok") list.Add(pages[page].Title);
        }
        list.Sort();
        return View("List", list);
    }

    [Route("List/Okrseks")]
    public IActionResult Listokrseks()
    {
        var list = new List<string>();
        var pages = _wikiService.GetPages();
        foreach (var page in pages.Keys)
        {
            if (pages[page].Type == "okrsek") list.Add(pages[page].Title);
        }
        list.Sort();
        return View("List", list);
    }

    [Route("List/Ctvrts")]
    public IActionResult Listctvrts()
    {
        var list = new List<string>();
        var pages = _wikiService.GetPages();
        foreach (var page in pages.Keys)
        {
            if (pages[page].Type == "čtvrť") list.Add(pages[page].Title);
        }
        list.Sort();
        return View("List", list);
    }

    public IActionResult Ideas()
    {
        var a = new List<string>();
        a.AddRange(["ŽIVÁ VODA", "TÁBOR", "ZÁMEK", "KAMENNÁ POUŠŤ", "ČEKÁRNA", "KNIHOVNA", "HÁJE", "LÁVKA", "STAN", "HALA", "PAVILON", "PARK", "KOŠUMBERK", "BLATA", "FAKULTA", "BROD", "MENZA", "STAVBA", "FONTÁNA", "ŠŤASTNÝ ASFALT", "KANCELÁŘ", "BAZAR", "HOUBY", "BAMBULE", "PUMPA", "VÝJEZD", "POŽÁR", "SLUNEČNÍK", "HABR", "HYDRANT", "ČLUN", "SERVIS", "OKOLÍ", "JETEL", "BISTRO", "ROZTOKY", "ZNAČKA", "ZÁVORA", "GUMA", "ČERVENÝ TRAKTŮREK", "KROUŽEK", "OBJEVITEL", "ZBÍJEČKA", "ŠIŠKA", "POTRUBÍ", "AKORDEON", "CENA", "KONTEXT", "VARIANTA", "CENA", "ŽEBRÁK", "ZÁVIST", "PATNÍK", "TLAČÍTKO", "HADICE", "BRIKETA", "LEKLA", "ZÁKAZ", "DÁVKOVAČ", "FOBIE", "ÚZKOST"]);
        return View("Ideas", a);
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
