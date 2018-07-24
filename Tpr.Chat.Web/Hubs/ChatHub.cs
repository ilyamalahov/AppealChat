using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatRepository chatRepository;

        public ChatHub(ChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        //[AllowAnonymous]
        public async Task SendMessage(Guid appealId, string message, string username)
        {
            chatRepository.WriteMessage(appealId, username, message);

            await Clients.All.SendAsync("ReceiveMessage", message, username);
        }
    }
}
