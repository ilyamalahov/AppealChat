using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;
using Tpr.Chat.Web.Services;

namespace Tpr.Chat.Web.Controllers
{
    [Route("ajax")]
    public class AjaxController : Controller
    {
        private readonly IChatRepository chatRepository;
        private readonly IHubContext<ChatHub, IChat> chatContext;
        private readonly IClientService clientService;
        private readonly ITaskService taskService;
        private readonly ILogger<AjaxController> logger;
        private readonly IConfiguration configuration;

        public AjaxController(
            IConfiguration configuration,
            IChatRepository chatRepository,
            IHubContext<ChatHub, IChat> chatContext,
            IClientService clientService,
            ITaskService taskService,
            ILogger<AjaxController> logger)
        {
            this.chatRepository = chatRepository;
            this.chatContext = chatContext;
            this.clientService = clientService;
            this.taskService = taskService;
            this.logger = logger;
            this.configuration = configuration;
        }

        [HttpPost("change/expert")]
        public async Task<IActionResult> ChangeExpert(Guid appealId)
        {
            // Member replacement
            var replacement = await chatRepository.GetReplacement(appealId);

            if (replacement.OldMember != null)
            {
                return BadRequest("Замена консультанта уже была произведена");
            }

            // Chat session
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            // Current expert
            if (chatSession.CurrentExpertKey == null)
            {
                return BadRequest("Консультант еще не подключился к чату");
            }

            // 
            var oldExpertKey = chatSession.CurrentExpertKey.Value;

            replacement.OldMember = oldExpertKey;
            replacement.RequestTime = DateTime.Now;

            // Update member replacement
            var replacementResult = await chatRepository.UpdateReplacement(replacement);

            if (!replacementResult)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            //
            chatSession.CurrentExpertKey = null;

            var sessionResult = await chatRepository.UpdateSession(chatSession);



            // 
            var nickname = "Член КК № " + oldExpertKey;

            var messageText = "Произведена замена члена КК № " + oldExpertKey;

            var messageResult = await chatRepository.WriteChatMessage(appealId, nickname, messageText, ChatMessageTypes.ChangeExpert);

            if (!messageResult)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            await chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

            // Return Success result to client
            return Ok();
        }

        [HttpPost("chat/complete")]
        public async Task<IActionResult> CompleteChat(Guid appealId)
        {
            // Chat session
            var chatSession = await chatRepository.GetChatSession(appealId);

            if (chatSession == null)
            {
                return BadRequest("Сессия для данного апеллянта не найдена");
            }

            //
            chatSession.IsEarlyCompleted = true;

            chatSession.EarlyCompleteTime = DateTime.Now;

            // Update session record
            var sessionResult = await chatRepository.UpdateSession(chatSession);

            if (!sessionResult)
            {
                return BadRequest("Не удалось добавить запись таблицы 'Сессия'");
            }

            // Add message
            var nickname = "Аппелянт";

            bool messageResult = await chatRepository.WriteChatMessage(appealId, nickname, "", ChatMessageTypes.EarlyComplete);

            if (!sessionResult)
            {
                return BadRequest("Не удалось добавить запись в таблицу 'Сообщения'");
            }

            await chatContext.Clients.User(appealId.ToString()).CompleteChat();

            // Return Success result to client
            return Ok();
        }

        [Produces("application/json")]
        [HttpGet("token")]
        public async Task<IActionResult> Token(Guid appealId, int? expertKey, Guid clientId)
        {
            // 
            if (clientId == null) return BadRequest("Отсутствует уникальный идентификатор клиента");

            //
            var session = await chatRepository.GetChatSession(appealId);

            if (session == null) return BadRequest("Сессия не найдена");

            // Client type
            var clientType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

            // 
            var isCurrentExpert = (clientType == ContextType.Expert && expertKey == session.CurrentExpertKey);

            // 
            if (clientType == ContextType.Appeal || isCurrentExpert)
            {
                // 
                var tokenSource = taskService.GetTokenSource(clientId);

                tokenSource?.Cancel();

                taskService.Add(clientId, async canceltoken => await LeaveFromChat(appealId, expertKey, clientId, canceltoken));
            }

            // JWT token configuration
            var jwtConfiguration = configuration.GetSection("JWT");

            // Token identifier (Appeal Id)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appealId.ToString()),
                new Claim("clientid", clientId.ToString())
            };

            // Expert key
            if (expertKey != null)
            {
                claims.Add(new Claim("expertkey", expertKey.ToString()));
            }

            // Expiration time
            var expires = jwtConfiguration.GetValue<TimeSpan>("Expires");

            // Credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration["SecretKey"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Token object
            var token = new JwtSecurityToken(
                jwtConfiguration["Issuer"],
                jwtConfiguration["Audience"],
                claims,
                expires: DateTime.UtcNow.Add(expires),
                signingCredentials: credentials
            );

            // Token string
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Response
            return Ok(accessToken);
        }

        private async Task LeaveFromChat(Guid appealId, int? expertKey, Guid clientId, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), token);

                // Client type
                var clientType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // 
                var client = clientService.Get(appealId);

                if (client == null) return;

                if (!client.TryRemove(clientId, clientType)) return;

                if (client.IsOnline(clientType)) return;

                //logger.LogInformation("Appeal {appealId} left", appealId);

                await SendDisconnectMessage(appealId, expertKey);
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Client {clientId} already online", clientId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        private async Task SendDisconnectMessage(Guid appealId, int? expertKey)
        {
            // Nick name
            var nickName = expertKey == null ? "Апеллянт" : "Член КК № " + expertKey;

            // Write message to database
            await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

            // 
            await chatContext.Clients.User(appealId.ToString()).Leave(DateTime.Now, nickName);
        }
    }
}