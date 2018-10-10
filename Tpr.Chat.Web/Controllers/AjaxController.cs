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

        //[HttpPost("change")]
        [HttpPost]
        public async Task<IActionResult> Change(Guid appealId)
        {
            // Member replacement
            var replacement = await chatRepository.GetReplacement(appealId);

            if (replacement?.OldMember != null)
            {
                return BadRequest("Замена консультанта уже была произведена");
            }

            // Chat session
            var chatSession = await chatRepository.GetChatSession(appealId);
            
            if (chatSession?.CurrentExpertKey == null)
            {
                return BadRequest("Консультант еще не подключился к чату");
            }

            // 
            var oldExpertKey = chatSession.CurrentExpertKey.Value;
            
            // Add member replacement
            if (!await chatRepository.AddReplacement(appealId, oldExpertKey))
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            //
            chatSession.CurrentExpertKey = null;

            if(!await chatRepository.UpdateSession(chatSession))
            {
                return BadRequest("Не удалось обновить таблицу");
            }

            // 
            var nickname = "Член КК № " + oldExpertKey;

            var messageText = "Произведена замена члена КК № " + oldExpertKey;

            // 
            var messageResult = await chatRepository.WriteChatMessage(appealId, nickname, messageText, ChatMessageTypes.ChangeExpert);

            if (!messageResult)
            {
                return BadRequest("Не удалось вставить запись в таблицу");
            }

            await chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

            // Return success response
            return Ok();
        }

        //[HttpPost("complete")]
        [HttpPost]
        public async Task<IActionResult> Complete(Guid appealId)
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
            if (!await chatRepository.UpdateSession(chatSession))
            {
                return BadRequest("Не удалось добавить запись в таблицу");
            }

            // Add message
            var nickname = "Аппелянт";

            // 
            bool messageResult = await chatRepository.WriteChatMessage(appealId, nickname, null, ChatMessageTypes.EarlyComplete);

            if (!messageResult)
            {
                return BadRequest("Не удалось добавить запись в таблицу");
            }

            await chatContext.Clients.User(appealId.ToString()).CompleteChat();

            // Return success response
            return Ok();
        }

        [HttpGet]
        public IActionResult Client(Guid appealId, int? expertKey, Guid? clientId)
        {
            // 
            if (clientId == null)
            {
                clientId = Guid.NewGuid();
            }

            // 
            var client = clientService.Get(appealId);

            // 
            var isExistsExcept = client.IsExistsExcept(clientId, ContextType.Appeal);

            if (expertKey == null && isExistsExcept)
            {
                return BadRequest("Пользователь все еще онлайн");
            }

            return Ok(clientId);
        }
        
        [HttpGet]
        public async Task<IActionResult> Token(Guid appealId, int? expertKey, Guid clientId)
        {
            // 
            if (clientId == null)
            {
                return BadRequest("Отсутствует уникальный идентификатор клиента");
            }

            //
            var chatSession = await chatRepository.GetChatSession(appealId);

            // Is current expert?
            var isCurrentExpert = expertKey != null && expertKey == chatSession?.CurrentExpertKey;

            // 
            if (expertKey == null || isCurrentExpert)
            {
                // 
                var tokenSource = taskService.GetTokenSource(clientId);

                tokenSource?.Cancel();

                taskService.Add(clientId, async canceltoken => await LeaveFromChat(appealId, expertKey, clientId, canceltoken));
            }

            // JWT token configuration
            var jwtConfiguration = configuration.GetSection("JWT");

            // Token identifier
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

            // Return success response with access token
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
                
                if (!client.TryRemove(clientId, clientType)) return;

                if (!client.IsOnline(clientType))
                {
                    logger.LogInformation("Appeal {appealId} left chat", appealId);

                    await SendDisconnectMessage(appealId, expertKey);
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Client {clientId} already online", clientId);
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