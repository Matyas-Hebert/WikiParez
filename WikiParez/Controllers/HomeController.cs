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
        ViewBag.FinishedPages = _wikiService.getFinishedPages();
        ViewBag.TotalPages = _wikiService.getTotalPages();
        return View(_wikiService.Last10pages());
    }

    public IActionResult New(){
        return View("New");
    }

    public IActionResult Ideas(){
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
