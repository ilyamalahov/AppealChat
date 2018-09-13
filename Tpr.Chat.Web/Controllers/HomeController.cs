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
            var isBefore = DateTime.Now < chatSession.StartTime;

            //
            var isAfter = DateTime.Now > chatSession.FinishTime;

            // 
            var model = new IndexViewModel
            {
                ChatSession = chatSession,
                IsAfter = isAfter,
                IsBefore = isBefore
            };

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
                model.ExpertKey = key;

                model.QuickReplies = chatRepository.GetQuickReplies();

                // Member replacement check
                var replacement = chatRepository.GetMemberReplacement(appealId);

                if (replacement == null)
                {
                    chatSession.CurrentExpertKey = key;

                    var isSessionUpdated = chatRepository.UpdateSession(chatSession);

                    if (!isSessionUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }
                }
                else if (replacement.OldMember == key)
                {
                    // Block ability to send messages
                    model.IsReadOnly = true;

                    model.Messages = chatRepository.GetChatMessages(appealId);
                }
                else if (!replacement.NewMember.HasValue)
                {
                    replacement.ReplaceTime = DateTime.Now;
                    replacement.NewMember = key;

                    var isReplacementUpdated = chatRepository.UpdateMemberReplacement(replacement);

                    if (!isReplacementUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }

                    chatSession.FinishTime = replacement.ReplaceTime.Value + (replacement.RequestTime - chatSession.StartTime);
                    chatSession.CurrentExpertKey = key;

                    var isSessionUpdated = chatRepository.UpdateSession(chatSession);

                    if (!isSessionUpdated)
                    {
                        return BadRequest("Не удалось обновить запись бд");
                    }
                }
                else
                {
                    model.Messages = chatRepository.GetChatMessages(appealId);
                }

                return View("Expert", model);
            }
            else if (connectionType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (isAfter)
                {
                    return View("After", model);
                }

                // Member replacement check
                var replacement = chatRepository.GetMemberReplacement(appealId);

                model.IsReadOnly = replacement != null && replacement.NewMember == null;

                // 
                //var connectionId = connectionService.GetConnectionId(appealId);

                //if (!string.IsNullOrEmpty(connectionId))
                //{
                //    return BadRequest("Сессия все еще запущена на другом устройстве");
                //}

                model.Messages = chatRepository.GetChatMessages(appealId);

                return View("Appeal", model);
            }

            return BadRequest();
        }

        private IActionResult FirstExpertConnection(IndexViewModel model, int expertKey)
        {
            model.ChatSession.CurrentExpertKey = expertKey;

            var isUpdated = chatRepository.UpdateSession(model.ChatSession);

            if (!isUpdated)
            {
                return BadRequest("Не удалось обновить сессию");
            }

            return View("Expert", model);
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
        public IActionResult ChangeExpert(Guid appealId)
        {
            // Member replacement
            var checkReplacement = chatRepository.GetMemberReplacement(appealId);

            if (checkReplacement != null)
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

            // Insert new member replacement
            var newReplacement = new MemberReplacement
            {
                AppealId = appealId,
                RequestTime = DateTime.Now,
                OldMember = chatSession.CurrentExpertKey.Value
            };

            var isInserted = chatRepository.AddMemberReplacement(newReplacement);

            if (!isInserted)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            // Send change expert request to external system

            // Return Success result to client
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
