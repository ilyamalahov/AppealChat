using System;
using System.Collections.Generic;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public interface IChatRepository
    {
        // Session
        ChatSession GetChatSession(Guid appealId);
        bool UpdateSession(ChatSession chatSession);

        // Messages
        IList<ChatMessage> GetChatMessages(Guid appealId);

        bool WriteMessage(Guid appealId, string nickName, string messageString);
        bool WriteJoined(Guid appealId, string nickName);
        bool WriteLeave(Guid appealId, string nickName);

        bool WriteChatMessage(Guid appealId, string nickName, string messageString, ChatMessageTypes messageType);
        bool WriteChatMessage(ChatMessage message);

        // Experts
        IEnumerable<QuickReply> GetQuickReplies();

        // Member replacements
        bool AddMemberReplacement(MemberReplacement replacement);
        bool AddMemberReplacement(Guid appealId, int expertKey);

        bool UpdateMemberReplacement(MemberReplacement replacement);

        MemberReplacement GetMemberReplacement(Guid appealId);
        MemberReplacement GetMemberReplacement(Guid appealId, string expertKey);
    }
}