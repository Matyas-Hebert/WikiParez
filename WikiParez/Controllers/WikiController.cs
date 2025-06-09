using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;

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

        public IActionResult Random()
        {
            var slug = _wikiService.GetRandomSlug();
            var page = _wikiService.GetPageBySlug(slug);
            if (page == null)
                return NotFound();
            return View("Index", page);
        }

        public IActionResult HigherLower()
        {
            var pageslug1 = _wikiService.GetRandomRoomSlug();
            var pageslug2 = _wikiService.GetRandomRoomSlug();
            while (pageslug2 == pageslug1){
                pageslug2 = _wikiService.GetRandomRoomSlug();
            }
            var page1 = _wikiService.GetPageBySlug(pageslug1);
            var page2 = _wikiService.GetPageBySlug(pageslug2);

            var pages = new List<WikiPage> { page1, page2 };
            ViewBag.Slug1 = pageslug1;
            ViewBag.Slug2 = pageslug2;
            ViewBag.Score = 0;
            ViewBag.HighScore = 0;
            ViewBag.RoomToChange = 0;
            ViewBag.Area1 = page1.area.ToString() + " blok²";
            ViewBag.Area2 = "";
            return View("HigherLower", pages);
        }

        [HttpPost]
        public IActionResult Choose(string slug1, string slug2, int highScore, int score, int roomToChange, int action){
            var area1 = _wikiService.GetPageBySlug(slug1).area;
            var area2 = _wikiService.GetPageBySlug(slug2).area;
            if ((area1 >= area2 && action == 0) || (area2 >= area1 && action == 1)){
                score++;
            }
            else{
                if (score > highScore){
                    highScore = score;
                }
                score = 0;
            }

            if (roomToChange == 0){
                slug1 = _wikiService.GetRandomRoomSlug();
                while(slug1 == slug2){
                    slug1 = _wikiService.GetRandomRoomSlug();
                }
                ViewBag.Area1 = "";
                ViewBag.Area2 = area2.ToString() + " blok²";
            }

            if (roomToChange == 1){
                slug2 = _wikiService.GetRandomRoomSlug();
                while(slug1 == slug2){
                    slug2 = _wikiService.GetRandomRoomSlug();
                }
                ViewBag.Area1 = area1.ToString() + " blok²";
                ViewBag.Area2 = "";
            }

            ViewBag.Slug1 = slug1;
            ViewBag.Slug2 = slug2;
            ViewBag.Score = score;
            ViewBag.HighScore = highScore;
            if (roomToChange == 0) ViewBag.RoomToChange = 1;
            else ViewBag.RoomToChange = 0;

            var page1 = _wikiService.GetPageBySlug(slug1);
            var page2 = _wikiService.GetPageBySlug(slug2);

            var pages = new List<WikiPage> { page1, page2 };

            return View("HigherLower", pages);
        }
    }
}
