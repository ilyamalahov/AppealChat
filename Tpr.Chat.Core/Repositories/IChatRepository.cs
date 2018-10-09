using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public interface IChatRepository
    {
        // Session
        Task<ChatSession> GetChatSession(Guid appealId);
        Task<bool> UpdateSession(ChatSession chatSession);

        // Messages
        Task<IEnumerable<ChatMessage>> GetChatMessages(Guid appealId);
        Task<ChatMessage> GetWelcomeMessage(Guid appealId, int? expertKey);
        
        Task<bool> WriteChatMessage(Guid appealId, string nickName, string messageString, ChatMessageTypes messageType);
        Task<bool> WriteChatMessage(ChatMessage message);

        // Experts
        Task<IEnumerable<QuickReply>> GetQuickReplies();

        // Member replacements
        Task<bool> AddReplacement(MemberReplacement replacement);
        Task<bool> UpdateReplacement(MemberReplacement replacement);
        Task<MemberReplacement> GetReplacement(Guid appealId);
    }
}