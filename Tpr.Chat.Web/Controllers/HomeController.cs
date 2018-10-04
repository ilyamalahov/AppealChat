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
        private readonly IHubContext<ChatHub, IChat> chatContext;

        private readonly ILogger<HomeController> logger;

        public HomeController(
            IChatRepository chatRepository,
            IAuthService authService,
            IClientService clientService,
            IHubContext<ChatHub, IChat> chatContext, 
            ILogger<HomeController> logger)
        {
            this.chatRepository = chatRepository;
            this.authService = authService;
            this.clientService = clientService;
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
            var model = new IndexViewModel
            {
                AppealId = appealId,
                Session = chatSession,
                IsAfter = isAfter,
                IsBefore = isBefore
            };

            // 
            ViewBag.IsNotActive = isBefore || isAfter || chatSession.IsEarlyCompleted;

            // Check if current date less than chat start time
            if (isBefore)
            {
                return View("Before", model);
            }

            // 
            var connectionType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                // Member replacement check
                var replacement = await chatRepository.GetMemberReplacement(appealId);

                if (replacement == null)
                {
                    // 
                    chatSession.CurrentExpertKey = key;

                    var isSessionUpdated = await chatRepository.UpdateSession(chatSession);

                    if (!isSessionUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }
                }
                else if (replacement.OldMember == key)
                {
                    // Block ability to send messages
                    ViewBag.IsNotActive = true;
                }
                else if (replacement.NewMember == null || replacement.NewMember == key)
                {
                    // 
                    replacement.ReplaceTime = DateTime.Now;
                    replacement.NewMember = key;

                    var replacementResult = await chatRepository.UpdateMemberReplacement(replacement);

                    if (!replacementResult)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    var waitTime = replacement.ReplaceTime.Value.Subtract(replacement.RequestTime);

                    chatSession.FinishTime = chatSession.FinishTime.Add(waitTime);
                    
                    //
                    chatSession.CurrentExpertKey = key;

                    var sessionResult = await chatRepository.UpdateSession(chatSession);

                    if (!sessionResult)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    await chatContext.Clients.User(appealId.ToString()).CompleteChange(key);
                }
                else if (replacement.NewMember != key)
                {
                    ViewBag.IsNotActive = true;
                }

                // 
                model.ExpertKey = key;

                //
                model.Messages = await chatRepository.GetChatMessages(appealId);

                // 
                model.QuickReplies = await chatRepository.GetQuickReplies();

                //
                return View("Expert", model);
            }
            else if (connectionType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (chatSession.IsEarlyCompleted || isAfter)
                {
                    return View("After", model);
                }

                // 
                var clientId = clientService.Get(appealId);

                if (clientId != null) return BadRequest("Пользователь уже существует");

                clientService.Add(appealId, Guid.NewGuid().ToString());

                //
                await SendConnectMessage(appealId);

                // 
                model.Messages = await chatRepository.GetChatMessages(appealId);

                // Member replacement check
                var replacement = await chatRepository.GetMemberReplacement(appealId);

                if (replacement != null)
                {
                    model.IsWaiting = replacement.NewMember == null;
                    model.IsExpertChanged = replacement.OldMember != 0;
                }

                return View("Appeal", model);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
