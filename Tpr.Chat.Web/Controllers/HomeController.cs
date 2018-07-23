using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Jwt;
using Tpr.Chat.Web.Models;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;

        public HomeController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public IActionResult Index(Guid appealId)
        {
            //var chatSession = chatRepository.GetChatSession(appealId);

            //if (chatSession == null)
            //{
            //    ModelState.AddModelError("", "Chat session is not found");

            //    return BadRequest(ModelState);
            //}

            //var claims = new List<Claim>
            //{
            //    new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString())
            //};

            //var identity = new ClaimsIdentity(claims, "Token");

            return View();
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
}
