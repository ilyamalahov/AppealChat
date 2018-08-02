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

namespace Tpr.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly IConfiguration configuration;

        public HomeController(IChatRepository chatRepository, IConfiguration configuration)
        {
            this.chatRepository = chatRepository;
            this.configuration = configuration;
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

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return View("Early");
            }

            // Check if current date more than chat finish time
            if (DateTime.Now > chatSession.FinishTime)
            {
                return View("Complete");
            }

            // Expert checkings
            if (key > 0)
            {
                // Secret checkings, etc...
            }

            // View model
            var viewModel = new IndexViewModel
            {
                AppealId = appealId,
                ExpertKey = key,
                Messages = chatRepository.GetChatMessages(appealId)
            };

            return View(viewModel);
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

            var identity = GetIdentity(appealId, key, secretKey);

            if (identity == null)
            {
                return Error();
            }

            var accessToken = CreateToken(identity, chatSession.FinishTime);

            // JSON Response
            var response = new
            {
                accessToken
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPost("/update")]
        public IActionResult Update()
        {
            // Appeal Identifier
            Guid appealId = Guid.Empty;

            if (!Guid.TryParse(HttpContext.User.Identity.Name, out appealId))
            {
                return Error();
            }

            // Chat session
            var chatSession = chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return Error();
            }

            // Current Date
            var currentDate = DateTimeOffset.Now;

            // Remaining Time
            var remainingTime = chatSession.FinishTime.Subtract(currentDate.TimeOfDay);

            // Begin Time
            var beginTime = chatSession.StartTime.Subtract(currentDate.TimeOfDay);

            // JSON Response
            var response = new
            {
                currentDate = currentDate.ToUnixTimeMilliseconds(),
                remainingTime = remainingTime.ToUnixTimeMilliseconds(),
                beginTime = beginTime.ToUnixTimeMilliseconds()
            };

            return Ok(response);
        }

        public ClaimsIdentity GetIdentity(Guid appealId, int key, string secretKey)
        {
            try
            {
                // Nick name
                string nickname = key > 0 ? "Член конфликтной комиссии №" + key : "Аппелянт";

                // Identity claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString()),
                    new Claim("Nickname", nickname),
                };

                // Expert
                if (key > 0)
                {
                    // Secret checkings, etc...

                    claims.Add(new Claim("ExpertKey", key.ToString()));
                }

                return new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CreateToken(ClaimsIdentity identity, DateTimeOffset expiresDate)
        {
            try
            {
                var jwtConfiguration = configuration.GetSection("JWT");

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

                var token = new JwtSecurityToken(
                    issuer: jwtConfiguration["Issuer"],
                    audience: jwtConfiguration["Audience"],
                    claims: identity.Claims,
                    expires: expiresDate.DateTime,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
