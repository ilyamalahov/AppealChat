using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Controllers
{
    [Route("ajax")]
    public class AjaxController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly ICommonService commonService;
        private readonly IHubContext<ChatHub, IChat> chatContext;

        public AjaxController(IChatRepository chatRepository, ICommonService commonService, IHubContext<ChatHub, IChat> chatContext)
        {
            this.chatRepository = chatRepository;
            this.commonService = commonService;
            this.chatContext = chatContext;
        }

        [HttpPost("expert/change")]
        public async Task<IActionResult> ChangeExpert(Guid appealId)
        {
            // Member replacement
            var replacement = await chatRepository.GetMemberReplacement(appealId);

            if (replacement != null)
            {
                return BadRequest("Замена консультанта уже была произведена");
            }

            // Chat session
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            // Current expert
            if (chatSession.CurrentExpertKey == null)
            {
                return BadRequest("Консультант еще не подключился к чату");
            }

            // 
            var oldExpertKey = chatSession.CurrentExpertKey.Value;

            // Insert new member replacement
            var isInserted = await chatRepository.AddMemberReplacement(appealId, oldExpertKey);

            if (!isInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            //
            chatSession.CurrentExpertKey = null;

            await chatRepository.UpdateSession(chatSession);

            // 
            var nickname = "Член КК № " + oldExpertKey;

            var messageText = "Произведена замена члена КК № " + oldExpertKey;

            var isMessageInserted = await chatRepository.WriteChatMessage(appealId, nickname, messageText, ChatMessageTypes.ChangeExpert);

            if(!isMessageInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            await chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

            // Send change expert request to external system

            //var identity = commonService.GetIdentity(appealId);

            //var accessToken = commonService.CreateToken(identity, chatSession.StartTime, chatSession.FinishTime);

            //var response = new { accessToken };

            // Return Success result to client
            return Ok();
        }

        [HttpPost("chat/complete")]
        public async Task<IActionResult> CompleteChat(Guid appealId)
        {
            // Chat session
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            //
            chatSession.IsEarlyCompleted = true;

            chatSession.EarlyCompleteTime = DateTime.Now;

            // Update session record
            var sessionResult = await chatRepository.UpdateSession(chatSession);

            if(!sessionResult)
            {
                return BadRequest("Серверная ошибка:<br/>Не удалось добавить запись таблицы 'Сессия чата'");
            }

            // Add message
            var nickname = "Аппелянт";

            bool messageResult = await chatRepository.WriteChatMessage(appealId, nickname, "", ChatMessageTypes.EarlyComplete);

            if (!sessionResult)
            {
                return BadRequest("Серверная ошибка:<br/>Не удалось добавить запись в таблицу 'Сообщения'");
            }

            await chatContext.Clients.User(appealId.ToString()).CompleteChat();

            // Return Success result to client
            return Ok();
        }
    }
}