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
            // 
            var connectionType = key > 0 ? ContextType.Expert : ContextType.Appeal;

            // 
            var connectionId = connectionService.GetConnectionId(appealId, connectionType);

            if(!string.IsNullOrEmpty(connectionId))
            {
                return BadRequest("Сессия все еще запущена на другом устройстве");
            }

            // Check if chat session is exists
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессии по данному ID не существует");
            }

            // 
            //if(connectionType == ContextType.Expert)
            //{
            //    var experts = chatRepository.GetExperts(appealId);

            //    if (!experts.Contains(key))
            //    {
            //        return BadRequest(string.Format("Ключ эксперта ({0}) не прикреплен к сессии", key));
            //    }
            //}

            bool isFinished = DateTime.Now > chatSession.FinishTime;

            // 
            var model = new IndexViewModel
            {
                ExpertKey = key,
                ChatSession = chatSession,
                IsFinished = isFinished
            };

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return View("Early", model);
            }
            
            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                //if (DateTime.Now < chatSession.FinishTime && key != chatSession.CurrentExpertKey)
                //{
                //    return BadRequest("Ключ эксперта не используется для данной сессии");
                //}
                // Secret checkings, etc...

                model.Messages = chatRepository.GetChatMessages(appealId);

                model.QuickReplies = chatRepository.GetQuickReplies();

                return View("Expert", model);
            }

            // Check if current date more than chat finish time
            if (isFinished)
            {
                return View("Complete", model);
            }

            //if (currentExpert == null)
            //{
            //    return BadRequest("Вы не можете зайти в консультацию, пока к ней не привязан консультант");
            //}

            model.Messages = chatRepository.GetChatMessages(appealId);

            return View("Appeal", model);
        }

        [Produces("application/json")]
        [HttpPost("/token")]
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

            //var connection = connectionService.Get(appealId);

            //if(connection != null)
            //{
            //    // Send "Change status" to client-caller
            //    var connectionId = key > 0 ? connection.AppealConnectionId : connection.ExpertConnectionId;

            //    await hubContext.Clients.Clients(connectionId).ChangeStatus(true);
            //}

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
