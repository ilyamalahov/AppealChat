using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Services;

namespace Tpr.Chat.Web.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub<IChat>
    {
        private readonly IChatRepository chatRepository;
        private readonly IClientService clientService;
        private readonly ILogger<ChatHub> logger;

        public ChatHub(IChatRepository chatRepository, IClientService clientService, ILogger<ChatHub> logger)
        {
            this.chatRepository = chatRepository;
            this.clientService = clientService;
            this.logger = logger;
        }

        public async Task Join()
        {
            try
            {
                // Appeal identifier from JWT token
                var tokenAppealId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(tokenAppealId, out var appealId)) return;

                // 
                var tokenClientId = Context.User.FindFirstValue("clientid");

                if (!Guid.TryParse(tokenClientId, out var clientId)) return;

                // Expert key from JWT token
                var expertKey = int.TryParse(Context.User.FindFirstValue("expertkey"), out var parseExpertKey) ? parseExpertKey : default(int?);

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Get client object from service
                var client = clientService.Get(appealId);
                
                if (senderType == ContextType.Expert)
                {
                    // Get chat session from database
                    var session = await chatRepository.GetChatSession(appealId);

                    // Check if this expert is current expert
                    if (expertKey != session.CurrentExpertKey) return;

                    // Get welcome message of this expert
                    var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);

                    if (welcomeMessage == null)
                    {
                        // Check if expert is still online
                        if (client.IsOnline(senderType)) return;

                        // Add unique identifier of client
                        if (!client.TryAdd(clientId, senderType)) return;

                        // Expert nickname
                        var expertNickname = "Член КК № " + expertKey;

                        // 
                        await chatRepository.WriteChatMessage(appealId, expertNickname, null, ChatMessageTypes.FirstExpert);

                        // 
                        var isFirstAppealOnline = client.IsOnline(ContextType.Appeal);

                        // 
                        await Clients.User(Context.UserIdentifier).FirstJoinExpert(expertKey, isFirstAppealOnline, true);

                        return;
                    }
                }

                // Check if expert is still online
                if (client.IsOnline(senderType)) return;

                // Add unique identifier of client
                if (!client.TryAdd(clientId, senderType)) return;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                // Write message to database
                await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

                // 
                var isAppealOnline = client.IsOnline(ContextType.Appeal);
                
                // 
                var isExpertOnline = client.IsOnline(ContextType.Expert);

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, isAppealOnline, isExpertOnline);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }

        //
        public async Task SendMessage(string message)
        {
            try
            {
                // Appeal ID from JWT token
                var tokenAppealId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(tokenAppealId, out var appealId))
                {
                    return;
                }

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    CreateDate = DateTime.Now,
                    MessageString = HttpUtility.HtmlEncode(message),
                    NickName = nickName,
                    ChatMessageTypeId = ChatMessageTypes.Message
                };

                // Write message to database
                await chatRepository.WriteChatMessage(chatMessage);

                // Send message to user clients
                await Clients.User(Context.UserIdentifier).Receive(chatMessage);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);
            }
        }

        //public async Task<ChatMessage> SendWelcomeMessage(Guid appealId, string expertKey)
        //{
        //    //
        //    var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);

        //    if (welcomeMessage != null) { return null; }

        //    //
        //    var expertNickname = "Член КК № " + expertKey;

        //    // 
        //    await chatRepository.WriteChatMessage(appealId, expertNickname, null, ChatMessageTypes.FirstExpert);

        //    // 
        //    await Clients.User(Context.UserIdentifier).FirstJoinExpert(expertKey);

        //    return welcomeMessage;
        //}
    }
}
