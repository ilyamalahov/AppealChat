using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Models;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly ICommonService commonService;
        private readonly IConnectionService connectionService;

        public HomeController(
            IChatRepository chatRepository,
            ICommonService commonService,
            IConnectionService connectionService)
        {
            this.chatRepository = chatRepository;
            this.commonService = commonService;
            this.connectionService = connectionService;
        }

        [HttpGet("/{appealId}")]
        public IActionResult Index(Guid appealId, int key = 0, string secretKey = null)
        {
            // Check if chat session is exists
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID апеллянта не существует");
            }

            //
            var isEarly = DateTime.Now < chatSession.StartTime;

            //
            var isCompleted = DateTime.Now > chatSession.FinishTime;

            // 
            var model = new IndexViewModel
            {
                ExpertKey = key,
                ChatSession = chatSession,
                IsInRange = !isEarly && !isCompleted
            };

            // Check if current date less than chat start time
            if (isEarly)
            {
                return View("Before", model);
            }

            // 
            var connectionType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                model.Messages = chatRepository.GetChatMessages(appealId);

                model.QuickReplies = chatRepository.GetQuickReplies();

                return View("Expert", model);
            }
            else if (connectionType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (isCompleted)
                {
                    return View("After", model);
                }

                // 
                var connectionId = connectionService.GetConnectionId(appealId);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    return BadRequest("Сессия все еще запущена на другом устройстве");
                }

                model.Messages = chatRepository.GetChatMessages(appealId);

                return View("Appeal", model);
            }

            return BadRequest();
        }

        [Produces("application/json")]
        [HttpPost("token")]
        public IActionResult Token(Guid appealId, int key = 0, string secretKey = null)
        {
            var chatSession = chatRepository.GetChatSession(appealId);

            // Check if chat session is exists
            if (chatSession == null)
            {
                return Error();
            }

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return Error();
            }

            // Check if current date more than chat finish time
            if (DateTime.Now > chatSession.FinishTime)
            {
                return Error();
            }

            var identity = commonService.GetIdentity(appealId, key, secretKey);

            if (identity == null)
            {
                return Error();
            }

            var accessToken = commonService.CreateToken(identity, chatSession.StartTime, chatSession.FinishTime);

            // JSON Response
            var response = new { accessToken };

            return Ok(response);
        }

        [HttpPost("expert/change")]
        public async Task<IActionResult> ChangeExpert(Guid appealId)
        {
            await Task.Delay(5000);

            var chatSession = chatRepository.GetChatSession(appealId);

            if(chatSession.IsExpertChanged)
            {
                return BadRequest("Консультант уже был заменен");
            }

            chatSession.IsExpertChanged = true;

            var isUpdated = chatRepository.UpdateSession(chatSession);

            if (!isUpdated)
            {
                return BadRequest("Ошибка обновления сессии чата");
            }

            var expertKey = new Random().Next(100, 10000);

            var response = new { expertKey };

            return Ok(response);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
