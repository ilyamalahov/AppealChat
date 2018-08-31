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
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК № " + expertKey;


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
                
                // Send message to user clients
                await Clients.User(Context.UserIdentifier).Receive(chatMessage);
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

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК № " + expertKey;

                
                // Add connection ID to collection
                connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType, expertKey);

                // Expert welcome message
                if (senderType == ContextType.Expert)
                {
                    // Get expert messages (not status)
                    var messagesCount = chatRepository.GetExpertMessagesCount(appealId, nickName);

                    // If count of expert messages is null
                    if (messagesCount == 0)
                    {
                        // Send expert welcome message
                        await SendExpertWelcomeMessage(appealId, nickName);
                    }
                    else
                    {
                        await SendJoinMessage(appealId, expertKey, nickName);
                    }
                }
                else
                {
                    await SendJoinMessage(appealId, expertKey, nickName);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }
        private async Task SendJoinMessage(Guid appealId, string expertKey, string nickName)
        {
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

            // Get online expert key
            var onlineExpertKey = connectionService.GetOnlineExpertKey(appealId, expertKey) ?? expertKey;
            
            // Send "Join" message to specified user clients
            await Clients.User(Context.UserIdentifier).Join(chatMessage, isAppealOnline, onlineExpertKey);
        }

        private async Task SendExpertWelcomeMessage(Guid appealId, string nickName)
        {
            // Chat message
            var message = new ChatMessage
            {
                AppealId = appealId,
                CreateDate = DateTime.Now,
                NickName = nickName,
                ChatMessageTypeId = ChatMessageTypes.FirstExpert
            };

            // Write message to database
            if (chatRepository.WriteChatMessage(message) == 0) return;

            // Send message to user clients
            await Clients.User(Context.UserIdentifier).FirstJoinExpert(nickName);
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

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Аппелянт" : "Член КК № " + expertKey;


                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, senderType, expertKey);

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
                
                // Get online expert key
                var onlineExpertKey = connectionService.GetOnlineExpertKey(appealId, expertKey);

                // Send "Leave" message to specified user clients 
                await Clients.User(Context.UserIdentifier).Leave(chatMessage, onlineExpertKey);
            }
            catch (Exception throwedException)
            {
                Console.WriteLine(throwedException.Message);
                return;
            }
        }

        public async Task ChangeExpert()
        {
            // Appeal ID from JWT token
            Guid appealId;

            if (!Guid.TryParse(Context.User.Identity.Name, out appealId)) return;

            // Expert key from JWT token
            var expertKey = Context.User.FindFirstValue("expertkey");

            // Nick name
            var nickName = "Член КК № " + expertKey;

            var messageString = "Произведена замена члена КК № " + expertKey;

            // Chat message
            var message = new ChatMessage
            {
                AppealId = appealId,
                CreateDate = DateTime.Now,
                NickName = nickName,
                MessageString = messageString,
                ChatMessageTypeId = ChatMessageTypes.ChangeExpert
            };

            // Write message to database
            if (chatRepository.WriteChatMessage(message) == 0) return;

            // Send message to user clients
            await Clients.User(Context.UserIdentifier).ChangeExpert(nickName);
        }
    }
}
