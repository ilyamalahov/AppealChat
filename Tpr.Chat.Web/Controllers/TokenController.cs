using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Controllers
{
    public class TokenController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IChatRepository chatRepository;

        public TokenController(IChatRepository chatRepository, IConfiguration configuration)
        {
            this.chatRepository = chatRepository;
            this.configuration = configuration;
        }

        [Produces("application/json")]
        [HttpPost("/token")]
        public IActionResult Token(Guid appealId)
        {
            var chatSession = chatRepository.GetChatSession(appealId);

            // Check if chat session is exists
            if (chatSession == null)
            {
                return NotFound(appealId);
            }

            // Check if current date less than chat start time
            if (DateTime.Now < chatSession.StartTime)
            {
                return BadRequest(chatSession);
            }

            // Check if current date more than chat finish time
            if (DateTime.Now > chatSession.FinishTime)
            {
                return BadRequest(chatSession);
            }

            // Jwt Token Process

            var jwtConfiguration = configuration.GetSection("JWT");

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, appealId.ToString())
            };

            //var identity = new ClaimsIdentity(claims, "Token");
            
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SigningKey"]));

            var token = new JwtSecurityToken(
                issuer: jwtConfiguration["Issuer"],
                audience: jwtConfiguration["Audience"],
                claims: claims,
                expires: chatSession.FinishTime,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            // JSON Response
            var response = new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return Ok(response);
        }
    }
}