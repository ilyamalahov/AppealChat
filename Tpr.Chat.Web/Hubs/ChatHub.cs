using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
        public async Task SendMessage(Guid appealId, string message, string username)
        {
            //Context.User.Claims.First(u => u.Equals())
            chatRepository.WriteMessage(appealId, username, message);

            await Clients.All.SendAsync("ReceiveMessage", message, username);
        }
    }
}
