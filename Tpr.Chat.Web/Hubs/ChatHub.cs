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

                // Nickname from JWT token
                var nickName = Context.User.FindFirstValue("nickname");

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Message,
                    CreateDate = DateTime.Now,
                    NickName = nickName,
                    MessageString = message
                };

                // 
                if (chatRepository.WriteChatMessage(chatMessage) == 0) return;

                // 
                var isAppeal = expertKey == null;

                await Clients.User(Context.UserIdentifier).Receive(chatMessage, isAppeal);
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

                // Nickname from JWT token
                var nickName = Context.User.FindFirstValue("nickname");

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                var isAppeal = expertKey == null;

                // Get chat connections by Appeal ID
                var connection = connectionService.Get(appealId);

                if (connection == null) return;
                
                // 
                if (isAppeal)
                {
                    connection.AppealConnectionId = Context.ConnectionId;
                }
                else
                {
                    connection.ExpertConnectionId = Context.ConnectionId;
                }

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

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(chatMessage, isAppeal);

                // Send "Change status" to client-caller
                //string callerConnectionId = connection.GetConnectionId(expertKey);
                var callerConnectionId = isAppeal ? connection.AppealConnectionId : connection.ExpertConnectionId;
                
                await Clients.Client(callerConnectionId).ChangeStatus(true);

                // Send "Change status" to client other than caller
                string otherConnectionId = isAppeal ? connection.AppealConnectionId : connection.ExpertConnectionId;

                await Clients.Client(otherConnectionId).ChangeStatus(true);
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
                
                // Nickname from JWT token
                var nickName = Context.User.FindFirstValue("nickname");
                
                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                var isAppeal = expertKey == null;

                // Get chat connections by Appeal ID
                var connection = connectionService.Get(appealId);

                if (connection == null) return;

                // 
                if (isAppeal)
                {
                    connection.AppealConnectionId = null;
                }
                else
                {
                    connection.ExpertConnectionId = null;
                }

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

                // Send "Leave" message to specified user clients 
                await Clients.User(Context.UserIdentifier).Leave(chatMessage, isAppeal);

                // Send "Change status" to client other than caller
                string otherConnectionId = isAppeal ? connection.ExpertConnectionId : connection.AppealConnectionId;

                await Clients.Client(otherConnectionId).ChangeStatus(false);
            }
            catch (Exception throwedException)
            {
                Console.WriteLine(throwedException.Message);
                return;
            }
        }
    }
}
