using Microsoft.AspNetCore.Mvc;
using WikiParez.Services;
using WikiParez.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Net;
using System.Net.Mail;

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
            if (page.redirect != null && page.redirect != string.Empty)
            {
                return Page(page.redirect);
            }
            ViewBag.Slug = slug;
            return View("Index", page);
        }

        public IActionResult Random()
        {
            var slug = _wikiService.GetRandomSlug(false);
            var page = _wikiService.GetPageBySlug(slug);
            if (page == null)
                return NotFound();
            ViewBag.Slug = slug;
            return View("Index", page);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var slug = _wikiService.FindBestMatch(query, 1);
            ViewBag.Slug = slug;
            return View("Index", _wikiService.GetPageBySlug(slug));
        }

        [HttpPost]
        public IActionResult SendEmail(string message, string slug)
        {
            SendEmailToMe(message, slug);
            ViewBag.Slug = slug;
            return View("Index", _wikiService.GetPageBySlug(slug));
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
