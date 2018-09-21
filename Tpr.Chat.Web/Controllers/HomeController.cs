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
using Tpr.Chat.Core.Models;
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
        private readonly IHubContext<ChatHub, IChat> chatContext;

        public HomeController(
            IChatRepository chatRepository,
            ICommonService commonService,
            IConnectionService connectionService, 
            IHubContext<ChatHub, IChat> chatContext)
        {
            this.chatRepository = chatRepository;
            this.commonService = commonService;
            this.connectionService = connectionService;
            this.chatContext = chatContext;
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
            var isBefore = DateTime.Now < chatSession.StartTime;

            //
            var isAfter = DateTime.Now > chatSession.FinishTime;

            // 
            var sessionModel = new IndexViewModel
            {
                AppealId = appealId,
                Session = chatSession,
                IsAfter = isAfter,
                IsBefore = isBefore
            };

            // Check if current date less than chat start time
            if (isBefore)
            {
                return View("Before", sessionModel);
            }

            // 
            var connectionType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                sessionModel.ExpertKey = key;

                sessionModel.QuickReplies = chatRepository.GetQuickReplies();

                // Member replacement check
                var replacement = chatRepository.GetMemberReplacement(appealId);

                if (replacement == null)
                {
                    // 
                    chatSession.CurrentExpertKey = key;

                    var isSessionUpdated = chatRepository.UpdateSession(chatSession);

                    if (!isSessionUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    //var nickname = "Член КК № " + key;

                    //var isInserted = chatRepository.WriteChatMessage(appealId, nickname, null, ChatMessageTypes.FirstExpert);

                    //if(!isInserted)
                    //{
                    //    return BadRequest("Не удалось добавить запись бд");
                    //}

                    // 
                    //chatContext.Clients.User(appealId.ToString()).FirstJoinExpert(key);
                }
                else if (replacement.OldMember == key)
                {
                    // Block ability to send messages
                    sessionModel.IsExpertChanged = true;
                }
                else if (!replacement.NewMember.HasValue)
                {
                    // 
                    replacement.ReplaceTime = DateTime.Now;
                    replacement.NewMember = key;

                    var isReplacementUpdated = chatRepository.UpdateMemberReplacement(replacement);

                    if (!isReplacementUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    chatSession.FinishTime = replacement.ReplaceTime.Value + (replacement.RequestTime - chatSession.StartTime);
                    chatSession.CurrentExpertKey = key;

                    var isSessionUpdated = chatRepository.UpdateSession(chatSession);

                    if (!isSessionUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    // 
                    //var connectionId = connectionService.GetConnectionId(appealId);

                    chatContext.Clients.User(appealId.ToString()).CompleteChange(key);

                    // 
                    //var nickname = "Член КК № " + key;

                    //var isInserted = chatRepository.WriteChatMessage(appealId, nickname, null, ChatMessageTypes.FirstExpert);

                    //if (!isInserted)
                    //{
                    //    return BadRequest("Не удалось добавить запись бд");
                    //}

                    //// 
                    //chatContext.Clients.User(appealId.ToString()).FirstJoinExpert(key);
                }
                else if(replacement.NewMember != key)
                {
                    sessionModel.IsExpertChanged = true;
                }

                sessionModel.Messages = chatRepository.GetChatMessages(appealId);

                return View("Expert", sessionModel);
            }
            else if (connectionType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (isAfter)
                {
                    return View("After", sessionModel);
                }

                sessionModel.Messages = chatRepository.GetChatMessages(appealId);

                // Member replacement check
                var replacement = chatRepository.GetMemberReplacement(appealId);

                if (replacement != null)
                {
                    sessionModel.IsWaiting = replacement.NewMember == null;
                    sessionModel.IsExpertChanged = replacement.OldMember != 0;
                }

                // 
                //var connectionId = connectionService.GetConnectionId(appealId);

                //if (!string.IsNullOrEmpty(connectionId))
                //{
                //    return BadRequest("Сессия все еще запущена на другом устройстве");
                //}

                return View("Appeal", sessionModel);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
