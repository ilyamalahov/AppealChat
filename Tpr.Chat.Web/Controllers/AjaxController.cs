using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
        private readonly IAuthService authService;
        private readonly IHubContext<ChatHub, IChat> chatContext;

        public IBackgroundTaskQueue BackgroundQueue { get; }

        public AjaxController(
            IChatRepository chatRepository,
            IAuthService authService,
            IHubContext<ChatHub, IChat> chatContext,
            IBackgroundTaskQueue backgroundQueue
            )
        {
            this.chatRepository = chatRepository;
            this.authService = authService;
            this.chatContext = chatContext;

            BackgroundQueue = backgroundQueue;
        }

        [HttpPost("change/expert")]
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

            if (!isMessageInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            await chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

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

            if (!sessionResult)
            {
                return BadRequest("Не удалось добавить запись таблицы 'Сессия'");
            }

            // Add message
            var nickname = "Аппелянт";

            bool messageResult = await chatRepository.WriteChatMessage(appealId, nickname, "", ChatMessageTypes.EarlyComplete);

            if (!sessionResult)
            {
                return BadRequest("Не удалось добавить запись в таблицу 'Сообщения'");
            }

            await chatContext.Clients.User(appealId.ToString()).CompleteChat();

            // Return Success result to client
            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("token")]
        public async Task<IActionResult> Token(Guid appealId, string expertKey = null)
        {
            // 
            var chatSession = await chatRepository.GetChatSession(appealId);

            // Check if chat session is exists
            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            // 
            var identity = authService.GetIdentity(appealId, expertKey);

            // 
            var accessToken = authService.CreateToken(identity, TimeSpan.FromSeconds(30));
            
            // 
            return Ok(accessToken);
        }

        [HttpPost("chat/disconnect")]
        public void DisconnectChat(Guid appealId, string expertKey = null)
        {
            BackgroundQueue.QueueBackgroundWorkItem(async token => {
                await Task.Delay(TimeSpan.FromSeconds(10), token);

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                // Write message to database
                await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

                // Send "Leave" message to specified user clients 
                await chatContext.Clients.User(appealId.ToString()).Leave(DateTime.Now, nickName);
            });
        }

        public async Task WaitDisconnect(CancellationToken token)
        {
            //// Sender type
            //var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

            //// Nick name
            //var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

            //// Remove caller's connection ID
            //connectionService.RemoveConnectionId(appealId, senderType);

            //// Write message to database
            //await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

            //// Send "Leave" message to specified user clients 
            //await Clients.User(Context.UserIdentifier).Leave(DateTime.Now, nickName);
        }
    }
}