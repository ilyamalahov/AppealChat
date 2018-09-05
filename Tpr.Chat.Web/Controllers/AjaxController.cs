using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Controllers
{
    [Route("ajax")]
    public class AjaxController : Controller
    {
        private readonly IChatRepository chatRepository;

        public AjaxController(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        //[Route("complete")]
        public IActionResult CompleteChat()
        {
            return PartialView("_CompleteModal");
        }

        //[Route("changeexpert")]
        public IActionResult SwitchExpert()
        {
            return PartialView("_ChangeExpertModal");
        }

        [HttpGet("appealinfo")]
        public IActionResult AppealInfo(Guid appealId)
        {
            // Check if chat session is exists
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID аппелянта не существует");
            }

            return PartialView("_AppealInfoModal", chatSession);
        }
    }
}