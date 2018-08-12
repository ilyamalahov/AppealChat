using System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
            var isExpert = key > 0;

            var connectionId = connectionService.GetConnectionId(appealId, !isExpert);

            if(!string.IsNullOrEmpty(connectionId))
            {
                return View("InvalidSession");
            }

            var chatSession = chatRepository.GetChatSession(appealId);

            // Check if chat session is exists
            if (chatSession == null)
            {
                return Error();
            }

            var model = new IndexViewModel
            {
                AppealId = appealId,
                ExpertKey = key
            };

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return View("Early", model);
            }

            // Expert checkings
            if (isExpert)
            {
                // Secret checkings, etc...

                model.Messages = chatRepository.GetChatMessages(appealId);

                return View("Expert", model);
            }

            // Check if current date more than chat finish time
            if (DateTime.Now > chatSession.FinishTime)
            {
                return View("Complete");
            }

            model.Messages = chatRepository.GetChatMessages(appealId);

            return View("Appeal", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
    }
}
