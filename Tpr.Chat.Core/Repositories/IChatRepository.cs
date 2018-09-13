﻿using System;
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
        int GetExpertMessagesCount(Guid appealId, string nickName);

        long WriteMessage(Guid appealId, string nickName, string messageString);
        long WriteJoined(Guid appealId, string nickName);
        long WriteLeave(Guid appealId, string nickName);

        long WriteChatMessage(ChatMessage message);

        // Experts
        IEnumerable<QuickReply> GetQuickReplies();

        // Member replacements
        bool AddMemberReplacement(MemberReplacement replacement);
        bool UpdateMemberReplacement(MemberReplacement replacement);
        MemberReplacement GetMemberReplacement(Guid appealId);
        MemberReplacement GetMemberReplacement(Guid appealId, string expertKey);
    }
}