using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Models;
using Tpr.Chat.Web.Services;

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

        public async Task<IActionResult> Index(Guid appealId, Guid? clientId, int? key, string secretKey)
        {
            // Check if chat session is exists
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному идентификатору апеллянта не существует");
            }

            //
            var isBefore = DateTime.Now < chatSession.StartTime;

            //
            var isAfter = DateTime.Now > chatSession.FinishTime;

            // Member replacement check
            var replacement = await chatRepository.GetReplacement(appealId);

            // 
            var sessionModel = new SessionViewModel
            {
                AppealId = appealId,
                Session = chatSession,
                IsAfter = isAfter,
                IsBefore = isBefore,
                IsActive = !isBefore && !isAfter && !chatSession.IsEarlyCompleted,
                IsReplaced = replacement?.OldMember != null
            };

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
                if (replacement?.OldMember == null)
                {
                    // Before expert replacement
                    if (chatSession.CurrentExpertKey == null)
                    {
                        chatSession.CurrentExpertKey = key;

                        if (!await chatRepository.UpdateSession(chatSession))
                        {
                            return BadRequest("Не удалось обновить запись бд");
                        }
                    }
                }
                else if (replacement.OldMember != key && replacement.ReplaceTime == null)
                {
                    // During expert replacement

                    if (replacement.NewMember == null)
                    {
                        replacement.NewMember = key;
                    }

                    // 
                    replacement.ReplaceTime = DateTime.Now;
                    
                    // Update member replacement
                    if (!await chatRepository.UpdateReplacement(replacement))
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    var waitingTime = replacement.ReplaceTime.Value.Subtract(replacement.RequestTime.Value);

                    chatSession.FinishTime = chatSession.FinishTime.Add(waitingTime);
                    chatSession.CurrentExpertKey = key;
                    
                    // Update session
                    if (!await chatRepository.UpdateSession(chatSession))
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
                    ClientId = clientId,
                    Messages = await chatRepository.GetChatMessages(appealId),
                    QuickReplies = await chatRepository.GetQuickReplies()
                };

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
                
                var isWaiting = replacement?.OldMember != null && replacement?.NewMember == null;

                var appealModel = new AppealViewModel
                {
                    SessionModel = sessionModel,
                    Messages = await chatRepository.GetChatMessages(appealId),
                    IsWaiting = isWaiting,
                    ClientId = clientId
                };

                return View("Appeal", appealModel);
            }

            return BadRequest();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
