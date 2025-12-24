using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.AspNetCore.Components.Endpoints;
using System.Numerics;
using System.Drawing;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Linq;

public class DictionaryShuffler
{
    private static Random rng = new Random();

    public static Dictionary<TKey, TValue> ShuffleDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        var keys = dictionary.Keys.ToList();
        var values = dictionary.Values.ToList();

        // Shuffle the values
        values = values.OrderBy(x => rng.Next()).ToList();

        // Create a new dictionary with the shuffled values
        var shuffledDictionary = keys.Zip(values, (k, v) => new { Key = k, Value = v })
                                     .ToDictionary(x => x.Key, x => x.Value);

        return shuffledDictionary;
    }
}

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

        public IActionResult PatternleMenu()
        {
            return View("Patternle/Menu");
        }

        [HttpGet]
        [Route("Game/Patternle")]
        public IActionResult Patternle(int numberofcoords)
        {
            var coords = _wikiService.GetNCoordinates(numberofcoords);
            var center = new Coordinates();
            var colorpalette = new List<string>();
            colorpalette.AddRange(["rgb(255,0,0)", "rgb(255,255,0)", "rgb(0,0,255)", "rgb(0,128,0)", "rgb(255,165,0)", "rgb(128,0,128)", "rgb(0,255,255)", "rgb(255,0,255)", "rgb(0,255,0)", "rgb(255,192,203)", "rgb(165,42,42)", "rgb(0,128,128)"]);
            double cameradistance = 0.0f;
            foreach (var coord in coords.Values)
            {
                center.x += coord.x;
                center.y += coord.y;
                center.z += coord.z;
            }
            center.x /= numberofcoords;
            center.y /= numberofcoords;
            center.z /= numberofcoords;

            foreach (var coord in coords.Values)
            {
                var distance = center.distanceFrom(coord);
                if (distance > cameradistance)
                {
                    cameradistance = distance;
                }
            }

            ViewBag.sphereradius = cameradistance;
            cameradistance *= 3;
            
            ViewBag.cameradistance = cameradistance;
            ViewBag.center = center;
            ViewBag.count = numberofcoords;
            var quaternion = _wikiService.RandomQuaternion();
            ViewBag.quaternion = new
            {
                x = quaternion.X,
                y = quaternion.Y,
                z = quaternion.Z,
                w = quaternion.W
            };

            var colors = new Dictionary<string, string>();
            //Random rnd = new Random();
            var i = 0;
            foreach (var key in coords.Keys)
            {
                //var color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                colors[key] = colorpalette[i];
                i++;
            }

            ViewBag.colors = colors;
            var sortedKeys = coords.Keys.ToList();
            sortedKeys.Sort();
            ViewBag.sortedKeys = sortedKeys;

            var pitch = Math.Asin(2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X));
            ViewBag.pitch = pitch;
            var yaw = Math.Atan2(2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y), 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z));
            ViewBag.yaw = yaw;

            return View("Patternle/Patternle", coords);
        }

        [HttpGet]
        [Route("Game/Patternle/4")]
        public IActionResult Patternle4()
        {
            return Patternle(4);
        }

        [HttpGet]
        [Route("Game/Vlakle")]
        public IActionResult Vlakle()
        {
            return View("Vlakle/Vlakle", _wikiService.GetVlaklePages());
        }

        [HttpGet]
        [Route("Game/Patternle/6")]
        public IActionResult Patternle6()
        {
            return Patternle(6);
        }

        [HttpGet]
        [Route("Game/Patternle/8")]
        public IActionResult Patternle8()
        {
            return Patternle(8);
        }

        [HttpGet]
        [Route("Game/Patternle/10")]
        public IActionResult Patternle10()
        {
            return Patternle(10);
        }

        [HttpGet]
        [Route("Game/Patternle/12")]
        public IActionResult Patternle12()
        {
            return Patternle(12);
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

            var pages = _wikiService.GetParezlePages();
            //var shuffled = DictionaryShuffler.ShuffleDictionary(pages);

            return View("Parezle/Parezle", shuffled);
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

        [HttpGet]
        [Route("Game/Parezle")]
        public IActionResult Parezle(string start, string end)
        {
            ViewBag.Start = start;
            ViewBag.End = end;
            ViewBag.ShortestPath = _wikiService.FindPath(start, end);
            ViewBag.Text = "CUSTOM PAŘEZLE";
            return View("Parezle/Parezle", _wikiService.GetParezlePages());
        }

        [HttpGet]
        [Route("Game/Parezle/Custom")]
        public IActionResult CustomParezle()
        {
            return View("Parezle/Custom", _wikiService.GetParezlePages());
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