using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Constants;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub<IChat>
    {
        private readonly IChatRepository chatRepository;
        private readonly IConnectionService connectionService;

        public ChatHub(IChatRepository chatRepository, IConnectionService connectionService)
        {
            this.chatRepository = chatRepository;
            this.connectionService = connectionService;
        }

        public async Task SendMessage(string message)
        {
            try
            {
                // Appeal ID from JWT token
                Guid appealId;

                if (!Guid.TryParse(Context.User.Identity.Name, out appealId)) return;

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК №" + expertKey;

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    CreateDate = DateTime.Now,
                    MessageString = message,
                    NickName = nickName,
                    ChatMessageTypeId = ChatMessageTypes.Message
                };

                // Write message to database
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // Sender
                string sender = senderType == ContextType.Appeal ? "appeal" : "expert";

                // Send message to user clients
                await Clients.User(Context.UserIdentifier).Receive(chatMessage, sender);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Appeal ID from JWT token
                Guid appealId;

                if (!Guid.TryParse(Context.User.Identity.Name, out appealId)) return;

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Add connection ID to collection
                connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType, expertKey);

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК №" + expertKey;

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Joined,
                    CreateDate = DateTime.Now,
                    NickName = nickName
                };

                // Write message to database
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // Check if appeal is online
                var isAppealOnline = connectionService.isOnline(appealId, ContextType.Appeal);

                // Check if expert is online
                var isExpertOnline = false;
                //var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert, expertKey);

                // Sender
                string sender = senderType == ContextType.Appeal ? "appeal" : "expert";

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(chatMessage, sender, isAppealOnline, isExpertOnline);

                // Expert welcome message
                if(senderType == ContextType.Expert)
                {
                    // Send expert welcome message
                    await SendExpertWelcomeMessage(appealId, nickName);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }

        private async Task SendExpertWelcomeMessage(Guid appealId, string nickName)
        {
            // Get expert messages (not information)
            IEnumerable<ChatMessage> messages = chatRepository.GetExpertMessages(appealId, nickName, ChatMessageTypes.Message);

            // If count of expert messages equals null
            if (messages.Count() == 0)
            {
                // Message string
                var messageString = "Здравствуйте! Готовы проконсультировать Вас по вопросам оценивания Вашей экзаменационной работы";

                // Chat message
                var welcomeMessage = new ChatMessage
                {
                    AppealId = appealId,
                    CreateDate = DateTime.Now,
                    MessageString = messageString,
                    NickName = nickName,
                    ChatMessageTypeId = ChatMessageTypes.Message
                };

                // Write message to database
                if (chatRepository.WriteChatMessage(welcomeMessage) == 0) return;

                // Send message to user clients
                await Clients.User(Context.UserIdentifier).Receive(welcomeMessage, "expert");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                // Appeal ID from JWT token
                Guid appealId;

                if (!Guid.TryParse(Context.User.Identity.Name, out appealId)) return;
                
                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, senderType, expertKey);

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК №" + expertKey;

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Leave,
                    CreateDate = DateTime.Now,
                    NickName = nickName
                };

                // Write message to database
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // Sender
                string sender = senderType == ContextType.Appeal ? "appeal" : "expert";

                // Send "Leave" message to specified user clients 
                await Clients.User(Context.UserIdentifier).Leave(chatMessage, sender);
            }
            catch (Exception throwedException)
            {
                Console.WriteLine(throwedException.Message);
                return;
            }
        }
    }
}
