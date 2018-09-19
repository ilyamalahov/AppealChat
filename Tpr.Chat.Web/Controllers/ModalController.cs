using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Controllers
{
    public class ModalController : Controller
    {
        private readonly IChatRepository chatRepository;

        public ModalController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }
        
        public IActionResult ChangeExpert()
        {
            return PartialView("ChangeExpert");
        }

        public IActionResult WaitChange(Guid appealId)
        {
            return PartialView("WaitChange");
        }

        public IActionResult Contacts()
        {
            return PartialView("Contacts");
        }

        public IActionResult AppealInfo(Guid appealId)
        {
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID апеллянта не существует");
            }

            return PartialView("AppealInfo", chatSession);
        }

    }
}