using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;

namespace Tpr.Chat.Web.Controllers
{
    [Route("ajax")]
    public class AjaxController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly IHubContext<ChatHub, IChat> chatContext;

        public AjaxController(IChatRepository chatRepository, IHubContext<ChatHub, IChat> chatContext)
        {
            this.chatRepository = chatRepository;
            this.chatContext = chatContext;
        }

        //[Route("complete")]
        public IActionResult CompleteChat()
        {
            return PartialView("_CompleteModal");
        }

        [HttpPost("expert/change")]
        public IActionResult ChangeExpert(Guid appealId)
        {
            // Member replacement
            var replacement = chatRepository.GetMemberReplacement(appealId);

            if (replacement != null)
            {
                return BadRequest("Замена консультанта уже была произведена");
            }

            // Chat session
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            // Current expert
            if (!chatSession.CurrentExpertKey.HasValue)
            {
                return BadRequest("Консультант еще не подключился к чату");
            }

            // 
            var oldExpertKey = chatSession.CurrentExpertKey.Value;

            // Insert new member replacement
            var isInserted = chatRepository.AddMemberReplacement(appealId, oldExpertKey);

            if (!isInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            // 
            var nickname = "Член КК № " + oldExpertKey;

            var messageText = "Произведена замена члена КК № " + oldExpertKey;

            var isMessageInserted = chatRepository.WriteChatMessage(appealId, nickname, messageText, ChatMessageTypes.ChangeExpert);

            if(!isMessageInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

            // Send change expert request to external system

            // Return Success result to client
            return Ok();
        }

        [HttpPost("chat/complete")]
        public IActionResult CompleteChat(Guid appealId)
        {
            // Chat session
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            //
            chatSession.IsEarlyCompleted = true;

            //
            chatSession.EarlyCompleteTime = DateTime.Now;

            var updateResult = chatRepository.UpdateSession(chatSession);

            if(!updateResult)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            chatContext.Clients.User(appealId.ToString()).CompleteChat();

            // Return Success result to client
            return Ok();
        }
    }
}