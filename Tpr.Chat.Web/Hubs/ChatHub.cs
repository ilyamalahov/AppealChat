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
            try
            {
                var appealId = Guid.Parse(Context.User.Identity.Name);
                var nickName = Context.User.FindFirstValue("Nickname");

                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Message,
                    CreateDate = DateTime.Now,
                    NickName = nickName,
                    MessageString = message
                };
                
                if (chatRepository.WriteChatMessage(chatMessage) > 0)
                {
                    var isAppeal = Context.User.FindFirstValue("ExpertKey") == null;

                    await Clients.User(Context.UserIdentifier).SendAsync("Receive", chatMessage, isAppeal);
                }

                //await Clients.User(Context.UserIdentifier).SendAsync("Receive", chatMessage);
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
                var appealId = Guid.Parse(Context.User.Identity.Name);
                var nickName = Context.User.FindFirstValue("Nickname");

                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Joined,
                    CreateDate = DateTime.Now,
                    NickName = nickName
                };

                if (chatRepository.WriteChatMessage(chatMessage) > 0)
                {
                    var expertKey = Context.User.FindFirstValue("ExpertKey");

                    var isAppeal = expertKey == null;

                    await Clients.User(Context.UserIdentifier).SendAsync("Join", chatMessage, isAppeal);
                }

                //await Clients.User(Context.UserIdentifier).SendAsync("Join", chatMessage);
            }
            catch (Exception)
            {
                return;
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var appealId = Guid.Parse(Context.User.Identity.Name);
                var nickName = Context.User.FindFirstValue("Nickname");

                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Leave,
                    CreateDate = DateTime.Now,
                    NickName = nickName
                };

                if (chatRepository.WriteChatMessage(chatMessage) > 0)
                {
                    var isAppeal = Context.User.FindFirstValue("ExpertKey") == null;

                    await Clients.User(Context.UserIdentifier).SendAsync("Leave", chatMessage, isAppeal);
                }

                //await Clients.User(Context.UserIdentifier).SendAsync("Leave", chatMessage);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
