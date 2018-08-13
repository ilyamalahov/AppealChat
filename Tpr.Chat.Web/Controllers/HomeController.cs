using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Index(Guid appealId, int key = 0, string secretKey = null)
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
            if(connectionType == ContextType.Expert)
            {
                var expertIsInSession = chatRepository.IsExists(key, appealId);

                if (!expertIsInSession)
                {
                    return BadRequest("");
                }
            }

            // 
            var model = new IndexViewModel { ChatSession = chatSession };

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return View("Early", model);
            }

            // 
            model.Messages = chatRepository.GetChatMessages(appealId);

            // Expert checkings
            if (connectionType == ContextType.Expert)
            {
                //if (key != chatSession.CurrentExpert)
                //{
                //    return BadRequest("");
                //}
                // Secret checkings, etc...

                return View("Expert", model);
            }

            // Check if current date more than chat finish time
            if (DateTime.Now > chatSession.FinishTime)
            {
                return View("Complete");
            }

            //if (currentExpert == null)
            //{
            //    return BadRequest("Вы не можете зайти в консультацию, пока к ней не привязан консультант");
            //}

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

        [Authorize]
        [Produces("application/json")]
        [HttpPost("/update")]
        public IActionResult Update()
        {
            // Current Time
            var moscowDate = DateTimeOffset.Now;

            // Begin Time
            long beginTimestamp;

            if(!long.TryParse(HttpContext.User.FindFirstValue("nbf"), out beginTimestamp))
            {
                return BadRequest(beginTimestamp);
            }

            var beginDate = DateTimeOffset.FromUnixTimeSeconds(beginTimestamp);

            var beginTime = beginDate.Subtract(moscowDate).TotalMilliseconds;

            // Remaining Time
            long finishTimestamp;

            if (!long.TryParse(HttpContext.User.FindFirstValue("exp"), out finishTimestamp))
            {
                return BadRequest(beginTimestamp);
            }

            var finishDate = DateTimeOffset.FromUnixTimeSeconds(finishTimestamp);

            var remainingTime = finishDate.Subtract(moscowDate).TotalMilliseconds;

            // JSON Response
            var response = new
            {
                moscowDate = moscowDate.ToUnixTimeMilliseconds(),
                beginTime,
                remainingTime
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
