using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.AspNetCore.Components.Endpoints;

namespace WikiParez.Controllers
{
    public class GameController : Controller
    {
        private readonly WikiService _wikiService;

        public GameController(WikiService wikiService)
        {
            _wikiService = wikiService;
        }

        public IActionResult Bordering()
        {
            var slug = _wikiService.GetRandomRoomSlug(true);
            var page = _wikiService.GetPageBySlug(slug);
            var borderingRooms = new List<string>();

            foreach (var borderingRoom in page.Bordering_rooms){
                if (_wikiService.DoesSlugExist(borderingRoom)){
                    borderingRooms.Add(borderingRoom);
                }
            }

            var answer = new Random().Next(0,2);
            var pages = new List<WikiPage>();

            if (answer == 0){
                var slug2 = _wikiService.GetRandomRoomSlug(true);
                while(slug2 == slug || borderingRooms.Contains(slug2)){
                    slug2 = _wikiService.GetRandomRoomSlug(true);
                }
                pages = new List<WikiPage> {page, _wikiService.GetPageBySlug(slug2)};
            }
            else{
                var randomRoom = borderingRooms[new Random().Next(0, borderingRooms.Count)];
                var page2 = _wikiService.GetPageBySlug(randomRoom);

                pages = new List<WikiPage> {page, page2};
            }
            ViewBag.Answer = answer;
            return View("Bordering", pages);
        }

        public IActionResult ChooseBordering(string action){
            return Bordering();
        }

        public IActionResult HigherLower()
        {
            var pageslug1 = _wikiService.GetRandomRoomSlug(true);
            var pageslug2 = _wikiService.GetRandomRoomSlug(true);
            while (pageslug2 == pageslug1){
                pageslug2 = _wikiService.GetRandomRoomSlug(true);
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

        public IActionResult ParezleMenu()
        {
            return View("Parezle/Menu");
        }

        [HttpGet]
        [Route("Game/Parezle/Daily")]
        public IActionResult DailyParezle()
        {
            var seed = _wikiService.GetParezleSeed();
            Console.WriteLine(seed);

            var room1 = "";
            var room2 = "";
            if (seed == 741340)
            {
                room1 = "mi_kaplicka";
                room2 = "mi_ah_fuck";
            }
            else
            {
                var random = new Random(seed);

                room1 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
                room2 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
                while (room1 == room2 || _wikiService.DoesBorder(room1, room2))
                {
                    room2 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
                }
            }

            ViewBag.Start = room1;
            ViewBag.End = room2;
            ViewBag.ShortestPath = _wikiService.FindPath(room1, room2);
            ViewBag.Text = "DNEŠNÍ PAŘEZLE";

            return View("Parezle/Parezle", _wikiService.GetParezlePages());
        }

        [HttpGet]
        [Route("Game/Parezle/Random")]
        public IActionResult RandomParezle()
        {
            var random = new Random();

            var room1 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
            var room2 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
            while (room1 == room2 || _wikiService.DoesBorder(room1, room2))
            {
                room2 = _wikiService.GetRoomSlugByID(random.Next(0, _wikiService.GetNumberOfRooms() - 1));
            }

            ViewBag.Start = room1;
            ViewBag.End = room2;
            ViewBag.ShortestPath = _wikiService.FindPath(room1, room2);
            ViewBag.Text = "NÁHODNÝ PAŘEZLE";


            return View("Parezle/Parezle", _wikiService.GetParezlePages());
        }


        [HttpPost]
        public IActionResult ChooseHigherLower(string slug1, string slug2, int highScore, int score, int roomToChange, int action){
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
                slug1 = _wikiService.GetRandomRoomSlug(true);
                while(slug1 == slug2){
                    slug1 = _wikiService.GetRandomRoomSlug(true);
                }
                ViewBag.Area1 = "";
                ViewBag.Area2 = area2.ToString() + " blok²";
            }

            if (roomToChange == 1){
                slug2 = _wikiService.GetRandomRoomSlug(true);
                while(slug1 == slug2){
                    slug2 = _wikiService.GetRandomRoomSlug(true);
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