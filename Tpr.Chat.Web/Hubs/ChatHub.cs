using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub<IChat>
    {
        private readonly IChatRepository chatRepository;

        public ChatHub(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        //
        //public Task SendDisconnectMessage(Guid appealId, string expertKey)
        //{
        // Sender type
        //var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

        // Nick name
        //var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

        // 
        //if (senderType == ContextType.Expert)
        //{
        //    // 
        //    var chatSession = await chatRepository.GetChatSession(appealId);

        //    if (chatSession.CurrentExpertKey == null)
        //    {
        //        return;
        //    }

        //    // 
        //    if (expertKey != chatSession.CurrentExpertKey.ToString())
        //    {
        //        return;
        //    }
        //}

        // Remove caller's connection ID
        //connectionService.RemoveConnectionId(appealId, senderType);

        // Write message to database
        //await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

        // Send "Leave" message to specified user clients 
        //await Clients.User(Context.UserIdentifier).Leave(DateTime.Now, nickName);
        //}

        //
        //public async Task SendConnectMessage(Guid appealId, string expertKey)
        //{
        // Sender type
        //var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

        // Nick name
        //var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

        // Expert welcome message
        //if (senderType == ContextType.Expert)
        //{
        // 
        //var chatSession = await chatRepository.GetChatSession(appealId);

        //if (chatSession.CurrentExpertKey == null)
        //{
        //    return;
        //}

        //// 
        //if (expertKey != chatSession.CurrentExpertKey.ToString())
        //{
        //    return;
        //}

        //var welcomeResult = await SendWelcomeMessage(appealId, expertKey);

        //if (welcomeResult != null) return;
        //}

        // Add connection ID to collection
        //connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType);

        // Write message to database
        //await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

        // Check if appeal is online
        //var isAppealOnline = connectionService.isOnline(appealId);

        // Check if current expert is online
        //var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert);

        // Send "Join" message to specified user clients
        //await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, isAppealOnline, isExpertOnline);
        //}

        private async Task Join(Guid appealId, string expertKey = null)
        {
            // Sender type
            var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

            // Nick name
            var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

            // Write message to database
            await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

            // Send "Join" message to specified user clients
            await Clients.User(appealId.ToString()).Join(DateTime.Now, nickName, false, false);
        }

        //
        public async Task SendMessage(Guid appealId, string message, string expertKey = null)
        {
            try
            {
                //// Appeal ID from JWT token
                //var appealId = Guid.Parse(Context.User.FindFirstValue(ClaimTypes.NameIdentifier));

                //// Expert key from JWT token
                //var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                // 
                //if (senderType == ContextType.Expert)
                //{
                //    // 
                //    var chatSession = await chatRepository.GetChatSession(appealId);

                //    if (chatSession.CurrentExpertKey == null)
                //    {
                //        return;
                //    }

                //    //
                //    if (expertKey != chatSession.CurrentExpertKey.ToString())
                //    {
                //        return;
                //    }
                //}

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
                Console.WriteLine(exception.Message);

                return;
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
