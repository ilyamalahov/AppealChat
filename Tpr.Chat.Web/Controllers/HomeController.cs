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
using Tpr.Chat.Web.ViewModels;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;

        public HomeController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Index(Guid id, int key = 0, string secretKey = null)
        {
            var chatSession = chatRepository.GetChatSession(id);

            if (chatSession == null)
            {
                ModelState.AddModelError("", "Chat session is not found");

                return BadRequest(ModelState);
            }

            if(DateTime.Now < chatSession.StartTime)
            {
                ModelState.AddModelError("", "Consultation has not yet begun");

                return BadRequest(ModelState);
            }

            if(DateTime.Now > chatSession.FinishTime)
            {
                ModelState.AddModelError("", "The consultation has already been completed");

                return BadRequest(ModelState);
            }

            //var moscowTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Russian Standard Time");

            //var claims = new List<Claim>
            //{
            //    new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString())
            //};

            //var identity = new ClaimsIdentity(claims, "Token");

            IndexViewModel indexViewModel = new IndexViewModel
            {
                IsExpert = key > 0
            };
            
            return View(indexViewModel);
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
