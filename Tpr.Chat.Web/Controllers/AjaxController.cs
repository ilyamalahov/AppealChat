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
        private readonly IClientService clientService;
        private readonly ITaskService taskService;
        private readonly IHubContext<ChatHub, IChat> chatContext;
        private readonly IConfiguration configuration;
        private readonly ILogger<AjaxController> logger;

        public AjaxController(
            IChatRepository chatRepository,
            IClientService clientService,
            ITaskService taskService,
            IHubContext<ChatHub, IChat> chatContext,
            IConfiguration configuration,
            ILogger<AjaxController> logger)
        {
            this.chatRepository = chatRepository;
            this.clientService = clientService;
            this.taskService = taskService;
            this.chatContext = chatContext;
            this.configuration = configuration;
            this.logger = logger;
        }
        
        [HttpPost]
        public async Task<IActionResult> Change(Guid appealId)
        {
            // Get member replacement from database
            var replacement = await chatRepository.GetReplacement(appealId);

            // Check if member replacement or old member is null
            if (replacement?.OldMember != null)
            {
                return BadRequest("Замена Члена КК уже была произведена");
            }

            // Get chat session from database
            var chatSession = await chatRepository.GetChatSession(appealId);
            
            // Check if chat session or current expert is null
            if (chatSession?.CurrentExpertKey == null)
            {
                return BadRequest("Член КК еще не подключился к онлайн-чату");
            }

            // Key of old expert
            var oldExpertKey = chatSession.CurrentExpertKey.Value;

            // Add member replacement to database and check if this member replacement has been added
            if (!await chatRepository.AddReplacement(appealId, oldExpertKey))
            {
                return BadRequest("Не удалось добавить запись в таблицу");
            }

            // Set value of current expert of chat session to null
            chatSession.CurrentExpertKey = null;

            // Update chat session and check if this chat session has been updated
            if(!await chatRepository.UpdateSession(chatSession))
            {
                return BadRequest("Не удалось обновить таблицу");
            }

            // Expert nickname of message
            var nickname = "Член КК № " + oldExpertKey;

            // Text of message
            var messageText = "Произведена замена члена КК № " + oldExpertKey;

            // Add "Replace expert" message to database
            var messageResult = await chatRepository.WriteChatMessage(appealId, nickname, messageText, ChatMessageTypes.ChangeExpert);

            // Check if message has been added to database
            if (!messageResult)
            {
                return BadRequest("Не удалось добавить запись в таблицу");
            }

            // Send "Initialize expert replacement" message to all connected clients of this appeal
            await chatContext.Clients.User(appealId.ToString()).InitializeChange(messageText);

            // Return success response
            return Ok();
        }
        
        [HttpPost]
        public async Task<IActionResult> Complete(Guid appealId)
        {
            // Get chat session from database
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
                var url = string.Format("{0}://{1}/{2}", HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.Query["appealId"]);

                var messageText = string.Format("<a href=\"{0}\">{0}</a> открыта на другом устройстве или вкладке.</br>Для продолжения работы в новой вкладке, закройте старую вкладку и повторите повторный переход через 30 секунд.", url);

                return BadRequest(messageText);
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