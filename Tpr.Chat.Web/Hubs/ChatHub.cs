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

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Appeal ID from JWT token
                var appealId = Guid.Parse(Context.User.Identity.Name);

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                //
                var messageType = ChatMessageTypes.Joined;

                // Add connection ID to collection
                connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType);

                // Expert welcome message
                if (senderType == ContextType.Expert)
                {
                    //
                    var welcomeMessage = chatRepository.GetWelcomeMessage(appealId, expertKey);
                    
                    //
                    messageType = welcomeMessage == null ? ChatMessageTypes.FirstExpert : ChatMessageTypes.Joined;
                }

                // Write message to database
                var isInserted = chatRepository.WriteChatMessage(appealId, nickName, null, messageType);

                if (!isInserted)
                {
                    return;
                }

                // Check if appeal is online
                var isAppealOnline = connectionService.isOnline(appealId, ContextType.Appeal);

                // Check if current expert is online
                var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert);

                // Is first join of expert?
                var isFirstJoined = messageType == ChatMessageTypes.FirstExpert;

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, isFirstJoined, isAppealOnline, isExpertOnline);

                //await SendJoinMessage(appealId, expertKey, nickName);
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

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;


                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, senderType);

                // Write message to database
                if (!chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave))
                {
                    return;
                }

                // Chat message
                var chatMessage = new ChatMessage
                {
                    AppealId = appealId,
                    ChatMessageTypeId = ChatMessageTypes.Leave,
                    CreateDate = DateTime.Now,
                    NickName = nickName
                };

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

        public async Task SendMessage(string message)
        {
            try
            {
                // Appeal ID from JWT token
                Guid appealId;

                if (!Guid.TryParse(Context.User.Identity.Name, out appealId))
                {
                    return;
                }

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;


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
                if (!chatRepository.WriteChatMessage(chatMessage)) return;

                // Send message to user clients
                await Clients.User(Context.UserIdentifier).Receive(chatMessage);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
        }

        //private async Task SendJoinMessage(Guid appealId, string expertKey, string nickName)
        //{
        //    //
        //    var isFirstJoined = welcomeMessage == null;

        //    //
        //    var messageType = isFirstJoined ? ChatMessageTypes.FirstExpert : ChatMessageTypes.Joined;

        //    // Write message to database
        //    var isInserted = chatRepository.WriteChatMessage(appealId, nickName, null, messageType);
            
        //    if (!isInserted)
        //    {
        //        return;
        //    }

        //    // Check if appeal is online
        //    var isAppealOnline = connectionService.isOnline(appealId, ContextType.Appeal);

        //    // Check if current expert is online
        //    var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert, expertKey);

        //    // Send "Join" message to specified user clients
        //    await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, isFirstJoined, isAppealOnline, isExpertOnline);
        //}

        //private async Task SendWelcomeMessage(Guid appealId, string nickName)
        //{
        //    // Chat message
        //    var message = new ChatMessage
        //    {
        //        AppealId = appealId,
        //        CreateDate = DateTime.Now,
        //        NickName = nickName,
        //        ChatMessageTypeId = ChatMessageTypes.FirstExpert
        //    };

        //    // Write message to database
        //    if (!chatRepository.WriteChatMessage(message)) return;

        //    // Send message to user clients
        //    //await Clients.User(Context.UserIdentifier).FirstJoinExpert(ex);
        //}

        //public async Task ChangeExpert()
        //{
        //    // Appeal ID from JWT token
        //    Guid appealId;

        //    if (!Guid.TryParse(Context.User.Identity.Name, out appealId)) return;

        //    // Expert key from JWT token
        //    var expertKey = Context.User.FindFirstValue("expertkey");

        //    // Nick name
        //    var nickName = "Член КК № " + expertKey;

        //    var messageString = "Произведена замена члена КК № " + expertKey;

        //    // Chat message
        //    var message = new ChatMessage
        //    {
        //        AppealId = appealId,
        //        CreateDate = DateTime.Now,
        //        NickName = nickName,
        //        MessageString = messageString,
        //        ChatMessageTypeId = ChatMessageTypes.ChangeExpert
        //    };

        //    // Write message to database
        //    if (!chatRepository.WriteChatMessage(message)) return;

        //    // Send message to user clients
        //    //await Clients.User(Context.UserIdentifier).ChangeExpert(expertKey);
        //}
    }
}
