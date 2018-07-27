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

namespace Tpr.Chat.Web.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChatHub : Hub
    {
        private readonly IChatRepository chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }
        
        public async Task SendMessage(string message)
        {

            Guid appealId = Guid.Empty;

            if(Guid.TryParse(Context.User.Identity.Name, out appealId))
            {
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Message,
                    CreateDate = DateTime.Now,
                    NickName = GetNickname(),
                    MessageString = message
                };

                if (chatRepository.WriteChatMessage(chatMessage) > 0)
                {
                    await Clients.All.SendAsync("Receive", chatMessage);
                }
                //await Clients.All.SendAsync("Receive", chatMessage);
            }
        }

        public override async Task OnConnectedAsync()
        {
            Guid appealId = Guid.Empty;

            if(Guid.TryParse(Context.User.Identity.Name, out appealId))
            {
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Joined,
                    CreateDate = DateTime.Now,
                    NickName = GetNickname()
                };

                //if (chatRepository.WriteChatMessage(chatMessage) > 0)
                //{
                //    await Clients.All.SendAsync("Join", chatMessage);
                //}
                await Clients.All.SendAsync("Join", chatMessage);

                //string nickname = GetNickname();

                //chatRepository.WriteJoined(appealId, nickname);

                //await Clients.All.SendAsync("Join", nickname);
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Guid appealId = Guid.Empty;

            if (Guid.TryParse(Context.User.Identity.Name, out appealId))
            {
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Leave,
                    CreateDate = DateTime.Now,
                    NickName = GetNickname()
                };

                //if (chatRepository.WriteChatMessage(chatMessage) > 0)
                //{
                //    await Clients.All.SendAsync("Leave", chatMessage);
                //}
                await Clients.All.SendAsync("Leave", chatMessage);
            }
        }

        private string GetNickname()
        {
            try
            {
                var roleClaim = Context.User.Claims.Single(c => c.Type == ClaimTypes.Role);

                var role = Enum.Parse<ChatRole>(roleClaim?.Value);

                return role == ChatRole.Expert ? "Консультант" : "Аппелянт";
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
