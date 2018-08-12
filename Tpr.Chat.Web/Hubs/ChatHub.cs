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

                var isAppealSender = expertKey == null;

                // Nick name
                var nickName = isAppealSender ? "Аппелянт" : "Член КК №" + expertKey;

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
                await Clients.User(Context.UserIdentifier).Receive(chatMessage, isAppealSender);
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

                var isAppealSender = expertKey == null;

                // Nick name
                var nickName = isAppealSender ? "Аппелянт" : "Член КК №" + expertKey;

                connectionService.AddConnectionId(appealId, Context.ConnectionId, isAppealSender);

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
                var isAppealOnline = connectionService.isOnline(appealId, isAppealSender);

                // 
                var isExpertOnline = connectionService.isOnline(appealId, !isAppealSender);

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(chatMessage, isAppealSender, isAppealOnline, isExpertOnline);

                // Send "Change status" to client-caller
                //var callerConnectionId = connectionService.GetConnectionId(appealId, isAppeal);
                
                //await Clients.Client(callerConnectionId).ChangeStatus(true);

                //// Send "Change status" to client other than caller
                //string otherConnectionId = connectionService.GetConnectionId(appealId, !isAppeal);

                //await Clients.Client(otherConnectionId).ChangeStatus(true);
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

                var isAppealSender = expertKey == null;

                // Nick name
                var nickName = isAppealSender ? "Аппелянт" : "Член КК №" + expertKey;

                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, isAppealSender);

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
                await Clients.User(Context.UserIdentifier).Leave(chatMessage, isAppealSender);

                // Send "Change status" to client other than caller
                //string otherConnectionId = connectionService.GetConnectionId(appealId, !isAppeal);

                //await Clients.Client(otherConnectionId).ChangeStatus(false);
            }
            catch (Exception throwedException)
            {
                Console.WriteLine(throwedException.Message);
                return;
            }
        }
    }
}
