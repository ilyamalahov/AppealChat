using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Models;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IChatRepository chatRepository;

        public HomeController(IUserRepository userRepository, IChatRepository chatRepository)
        {
            this.userRepository = userRepository;
            this.chatRepository = chatRepository;
        }

        public async Task<IActionResult> Index(Guid appealId)
        {
            //var user = await userRepository.Get(appealId);

            //if (user == null)
            //{
            //    ModelState.AddModelError("", "User is not found");

            //    return BadRequest(ModelState);
            //}

            var chatSession = chatRepository.GetChatSession(appealId);

            if(chatSession == null)
            {
                ModelState.AddModelError("", "Chat session is not found");

                return BadRequest(ModelState);
            }

            //chatSession.FinishTime.Subtract(chatSession.StartTime);

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
