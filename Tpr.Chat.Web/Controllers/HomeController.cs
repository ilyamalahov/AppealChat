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
                return BadRequest("Сессии по данному ID аппелянта не существует");
            }
            
            // 
            var model = new IndexViewModel
            {
                ExpertKey = key,
                ChatSession = chatSession
            };

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return View("Early", model);
            }

            // 
            var connectionType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                model.IsFinished = DateTime.Now > chatSession.FinishTime;

                model.Messages = chatRepository.GetChatMessages(appealId);

                model.QuickReplies = chatRepository.GetQuickReplies();

                return View("Expert", model);
            }
            else if(connectionType == ContextType.Appeal)
            {
                // Check if current date more than chat finish time
                if (DateTime.Now > chatSession.FinishTime)
                {
                    return View("Complete", model);
                }

                // 
                var connectionId = connectionService.GetConnectionId(appealId);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    return BadRequest("Сессия все еще запущена на другом устройстве");
                }

                model.OnlineExpertKeys = connectionService.GetExpertKeys(appealId);

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

        //[Authorize]
        [Produces("application/json")]
        [HttpPost("update")]
        public IActionResult Update(Guid appealId)
        {
            // Current Time
            var currentDate = DateTime.Now;

            var chatSession = chatRepository.GetChatSession(appealId);

            if(chatSession == null)
            {
                return BadRequest("Error");
            }

            // Begin Time
            //long beginTimestamp;

            //if(!long.TryParse(HttpContext.User.FindFirstValue("nbf"), out beginTimestamp))
            //{
            //    return BadRequest(beginTimestamp);
            //}

            //var beginDate = DateTimeOffset.FromUnixTimeSeconds(beginTimestamp);

            var beginTime = chatSession.StartTime.Subtract(currentDate);

            // Remaining Time
            //long finishTimestamp;

            //if (!long.TryParse(HttpContext.User.FindFirstValue("exp"), out finishTimestamp))
            //{
            //    return BadRequest(beginTimestamp);
            //}

            //var finishDate = DateTimeOffset.FromUnixTimeSeconds(finishTimestamp);

            var remainingTime = chatSession.FinishTime.Subtract(currentDate);

            // 
            var isAlarm = remainingTime.TotalMinutes < 5;

            // JSON Response
            var response = new
            {
                moscowDate = currentDate,
                beginTime = beginTime.TotalMilliseconds,
                remainingTime = remainingTime.TotalMilliseconds,
                isAlarm
            };

            return Ok(response);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
