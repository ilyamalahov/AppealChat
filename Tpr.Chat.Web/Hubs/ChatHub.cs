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

                // Expert welcome message
                if (senderType == ContextType.Expert)
                {
                    // 
                    var chatSession = await chatRepository.GetChatSession(appealId);

                    if (chatSession.CurrentExpertKey == null)
                    {
                        return;
                    }

                    // 
                    if(expertKey != chatSession.CurrentExpertKey.ToString())
                    {
                        return;
                    }

                    //
                    var welcomeMessage = await chatRepository.GetWelcomeMessage(appealId, expertKey);
                    
                    if(welcomeMessage == null)
                    {
                        await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.FirstExpert);

                        await Clients.User(Context.UserIdentifier).FirstJoinExpert(expertKey);

                        return;
                    }
                }

                // Add connection ID to collection
                connectionService.AddConnectionId(appealId, Context.ConnectionId, senderType);

                // Write message to database
                await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);

                // Check if appeal is online
                var isAppealOnline = connectionService.isOnline(appealId, ContextType.Appeal);

                // Check if current expert is online
                var isExpertOnline = connectionService.isOnline(appealId, ContextType.Expert);

                // Send "Join" message to specified user clients
                await Clients.User(Context.UserIdentifier).Join(DateTime.Now, nickName, isAppealOnline, isExpertOnline);

                // 
                await base.OnConnectedAsync();
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
                var appealId = Guid.Parse(Context.User.Identity.Name);

                // Expert key from JWT token
                var expertKey = Context.User.FindFirstValue("expertkey");

                // Sender type
                var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

                // Nick name
                var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

                // 
                if(senderType == ContextType.Expert)
                {
                    // 
                    var chatSession = await chatRepository.GetChatSession(appealId);

                    if (chatSession.CurrentExpertKey == null)
                    {
                        return;
                    }

                    // 
                    if (expertKey != chatSession.CurrentExpertKey.ToString())
                    {
                        return;
                    }
                }

                // Remove caller's connection ID
                connectionService.RemoveConnectionId(appealId, senderType);

                // Write message to database
                await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

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

                // 
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception innerException)
            {
                Console.WriteLine(innerException.Message);

                return;
            }
        }

        public async Task SendMessage(string message)
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
                if (senderType == ContextType.Expert)
                {
                    // 
                    var chatSession = await chatRepository.GetChatSession(appealId);

                    if (chatSession.CurrentExpertKey == null)
                    {
                        return;
                    }

                    //
                    if (expertKey != chatSession.CurrentExpertKey.ToString())
                    {
                        return;
                    }
                }

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
    }
}
