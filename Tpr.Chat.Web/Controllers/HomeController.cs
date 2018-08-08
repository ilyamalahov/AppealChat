using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Constants;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Models;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly ICommonService commonService;

        public HomeController(IChatRepository chatRepository, ICommonService commonService)
        {
            this.chatRepository = chatRepository;
            this.commonService = commonService;
        }

        [HttpGet("/{appealId}")]
        public IActionResult Index(Guid appealId, int key = 0, string secretKey = null)
        {
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
            if (key > 0)
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

            //identity.AddClaim(new Claim("BeginDate", ""));

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
            var moscowDate = DateTime.Now;

            // Begin Time
            long beginTimestamp;

            if(!long.TryParse(HttpContext.User.FindFirstValue("nbf"), out beginTimestamp))
            {
                return BadRequest(beginTimestamp);
            }

            var beginDate = DateTimeOffset.FromUnixTimeSeconds(beginTimestamp);

            // Remaining Time
            long finishTimestamp;

            if (!long.TryParse(HttpContext.User.FindFirstValue("exp"), out finishTimestamp))
            {
                return BadRequest(beginTimestamp);
            }

            var finishDate = DateTimeOffset.FromUnixTimeSeconds(finishTimestamp);

            var beginTime = beginDate.Subtract(moscowDate);

            var remainingTime = finishDate.Subtract(moscowDate);

            var response = new
            {
                moscowDate,
                beginTime,
                remainingTime
            };

            return Ok(response);
        }
    }
}
