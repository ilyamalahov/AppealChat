using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Models;
using Tpr.Chat.Web.Service;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly IAuthService authService;
        private readonly IClientService clientService;
        private readonly ITaskService taskService;
        private readonly IHubContext<ChatHub, IChat> chatContext;
        private readonly ILogger<HomeController> logger;

        public HomeController(
            IChatRepository chatRepository,
            IAuthService authService,
            IClientService clientService,
            ITaskService taskService,
            IHubContext<ChatHub, IChat> chatContext,
            ILogger<HomeController> logger)
        {
            this.chatRepository = chatRepository;
            this.authService = authService;
            this.clientService = clientService;
            this.taskService = taskService;
            this.chatContext = chatContext;
            this.logger = logger;
        }

        public async Task<IActionResult> Index(Guid appealId, int key = 0, string secretKey = null)
        {
            // Check if chat session is exists
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID апеллянта не существует");
            }

            //
            var isBefore = DateTime.Now < chatSession.StartTime;

            //
            var isAfter = DateTime.Now > chatSession.FinishTime;
            
            // 
            var sessionModel = new SessionViewModel
            {
                AppealId = appealId,
                Session = chatSession,
                IsAfter = isAfter,
                IsBefore = isBefore
            };

            // 
            sessionModel.IsActive = !isBefore && !isAfter && !chatSession.IsEarlyCompleted;

            // Check if current date less than chat start time
            if (isBefore)
            {
                return View("Before", sessionModel);
            }

            // 
            var clientType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // Expert checkings
            if (clientType == ContextType.Expert)
            {
                // Member replacement check
                var replacement = await chatRepository.GetReplacement(appealId);

                if (replacement.OldMember == null && chatSession.CurrentExpertKey == null)
                {
                    chatSession.CurrentExpertKey = key;

                    var sessionResult = await chatRepository.UpdateSession(chatSession);

                    if (!sessionResult)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }
                }

                if (replacement.OldMember != null && replacement.NewMember == null)
                {
                    // 
                    replacement.NewMember = key;
                    replacement.ReplaceTime = DateTime.Now;

                    var replacementResult = await chatRepository.UpdateReplacement(replacement);

                    if (!replacementResult)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    var waitTime = replacement.ReplaceTime.Value.Subtract(replacement.RequestTime.Value);

                    chatSession.FinishTime = chatSession.FinishTime.Add(waitTime);
                    chatSession.CurrentExpertKey = key;

                    var sessionResult = await chatRepository.UpdateSession(chatSession);

                    if (!sessionResult)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    await chatContext.Clients.User(appealId.ToString()).CompleteChange(key);
                }

                if (key != chatSession.CurrentExpertKey)
                {
                    sessionModel.IsActive = false;
                }

                var expertModel = new ExpertViewModel
                {
                    SessionModel = sessionModel,
                    ExpertKey = key,
                    Messages = await chatRepository.GetChatMessages(appealId),
                    QuickReplies = await chatRepository.GetQuickReplies()
                };

                // 
                //await chatContext.Clients.User(appealId.ToString()).OnlineStatus(true);

                //
                return View("Expert", expertModel);
            }
            else if (clientType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (isAfter || chatSession.IsEarlyCompleted)
                {
                    return View("After", sessionModel);
                }

                // 
                //var client = clientService.GetClient(appealId);

                //if (client.AppealClientId != null) return BadRequest("Пользователь уже существует");

                //clientService.AddAppeal(appealId, Guid.NewGuid());

                //
                //await SendConnectMessage(appealId);

                // Member replacement check
                var replacement = await chatRepository.GetReplacement(appealId);

                var isWaiting = replacement.OldMember != null && replacement.NewMember == null;

                var appealModel = new AppealViewModel
                {
                    SessionModel = sessionModel,
                    Messages = await chatRepository.GetChatMessages(appealId),
                    IsWaiting = isWaiting
                };

                return View("Appeal", appealModel);
            }

            return BadRequest();
        }

        private async Task SendConnectMessage(Guid appealId, string expertKey = null)
        {
            // Sender type
            var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

            // Nick name
            var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

            // Write message to database
            await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

            // Send "Join" message to specified user clients
            await chatContext.Clients.User(appealId.ToString()).Join(DateTime.Now, nickName, false, false);
        }

        public async Task<ChatMessage> SendWelcomeMessage(Guid appealId, string expertKey)
        {
            //
            var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);

            if (welcomeMessage != null) { return null; }

            //
            var expertNickname = "Член КК № " + expertKey;

            // 
            await chatRepository.WriteChatMessage(appealId, expertNickname, null, ChatMessageTypes.FirstExpert);

            // 
            await chatContext.Clients.User(appealId.ToString()).FirstJoinExpert(expertKey);

            return welcomeMessage;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
