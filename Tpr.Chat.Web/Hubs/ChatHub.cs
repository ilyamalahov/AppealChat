using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        private readonly ILogger<ChatHub> logger;

        public ChatHub(IChatRepository chatRepository, ILogger<ChatHub> logger)
        {
            this.chatRepository = chatRepository;
            this.logger = logger;
        }

        public async Task Join()
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

                // 
                if (senderType == ContextType.Expert)
                {
                    // Get welcome message of certain expert
                    var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);

                    if (welcomeMessage == null)
                    {
                        // 
                        await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.FirstExpert);

                        // 
                        await Clients.User(Context.UserIdentifier).FirstJoinExpert(expertKey);

                        return;
                    }
                }

                // Write message to database
                await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, false, false);
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

        public async Task<ChatMessage> SendWelcomeMessage(Guid appealId, string expertKey)
        {
            //
            var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);

            if (welcomeMessage != null) { return null; }

            //
            var expertNickname = "Член КК № " + expertKey;

            // 
            await chatRepository.WriteChatMessage(appealId, expertNickname, null, ChatMessageTypes.FirstExpert);

            // 
            await Clients.User(Context.UserIdentifier).FirstJoinExpert(expertKey);

            return welcomeMessage;
        }
    }
}
