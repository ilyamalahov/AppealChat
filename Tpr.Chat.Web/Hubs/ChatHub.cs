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

                // 
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

                // 
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // 
                string sender = senderType == ContextType.Appeal ? "appeal" : "expert";

                // 
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

                // 
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // 
                connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType);

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

                // 
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // 
                var isAppealOnline = connectionService.isOnline(appealId, ContextType.Appeal);

                // 
                var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert);

                // 
                string sender = senderType == ContextType.Appeal ? "appeal" : "expert";

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(chatMessage, sender, isAppealOnline, isExpertOnline);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
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

                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, senderType);

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

                // 
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // 
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
