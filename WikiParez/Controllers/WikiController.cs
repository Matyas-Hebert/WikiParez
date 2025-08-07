using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WikiParez.Controllers
{
    public class WikiController : Controller
    {
        private readonly WikiService _wikiService;
        private static readonly Random rnd = new Random();

        public WikiController(WikiService wikiService)
        {
            _wikiService = wikiService;
        }

        private WikiPage eastereggs(WikiPage page)
        {
            foreach (var section in page.Sections)
            {
                section.Content = Regex.Replace(section.Content, @"<<(.+?)>>::([0-9.]+)", match =>
                {
                    string message = match.Groups[1].Value;
                    double treshhold = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                    double chance = rnd.NextDouble();
                    if (chance <= treshhold)
                    {
                        return message;
                    }
                    return "";
                });
            }
            return page;
        }

        private WikiPage ClonePage(WikiPage page)
        {
            return new WikiPage
            {
                Title = page.Title,
                Type = page.Type,
                Images = page.Images,
                Image_titles = page.Image_titles,
                Bordering_rooms = page.Bordering_rooms,
                Alternate_names = page.Alternate_names,
                Metadata = page.Metadata,
                Sections = page.Sections.Select(s => new Section
                {
                    Title = s.Title,
                    Content = s.Content
                }).ToList(),
                image_id = page.image_id,
                area = page.area,
                Empty = page.Empty,
                numberOfRooms = page.numberOfRooms,
                redirect = page.redirect
            };
        }

        [Route("{slug}")]
        public IActionResult Page(string slug)
        {
            var page = _wikiService.GetPageBySlug(slug);
            var newpage = eastereggs(ClonePage(page));

            if (page == null)
                return NotFound();
            if (page.redirect != null && page.redirect != string.Empty)
            {
                return Page(page.redirect);
            }
            ViewBag.Slug = slug;
            return View("Index", newpage);
        }

        public IActionResult Random()
        {
            var slug = _wikiService.GetRandomSlug(false);
            var page = _wikiService.GetPageBySlug(slug);
            var newpage = eastereggs(ClonePage(page));
            if (page == null)
                return NotFound();
            ViewBag.Slug = slug;
            return Redirect("/" + slug);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var slug = _wikiService.FindBestMatch(query, 1);
            ViewBag.Slug = slug;
            var page = _wikiService.GetPageBySlug(slug);
            var newpage = eastereggs(ClonePage(page));
            return Redirect("/" + slug);
        }

        public IActionResult Other()
        {
            return View("Other");
        }

        public IActionResult Games()
        {
            return View("Games");
        }

        [HttpPost]
        public IActionResult SendEmail(string message, string slug)
        {
            SendEmailToMe(message, slug);
            ViewBag.Slug = slug;
            var page = _wikiService.GetPageBySlug(slug);
            var newpage = eastereggs(ClonePage(page));
            return Redirect("/" + slug);
        }

        private void SendEmailToMe(string message, string slug)
        {
            var mail = new MailMessage();
            mail.To.Add("hebertmatyas@gmail.com");
            mail.From = new MailAddress("Parez@Wiki.com");
            mail.Subject = "Error on " + slug + " page.";
            mail.Body = message;
            var smtp = new SmtpClient("smtp.gmail.com", 587);
            string emailUser = Environment.GetEnvironmentVariable("emailUser");
            string emailPass = Environment.GetEnvironmentVariable("emailPass");
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(emailUser, emailPass);
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}
