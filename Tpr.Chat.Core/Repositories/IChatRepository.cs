using System;
using System.Collections.Generic;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public interface IChatRepository
    {
        ChatSession GetChatSession(Guid appealId);
        IList<Message> GetChatMessages(Guid appealId);
        long WriteMessage(Guid appealId, string nickName, string messageString);
        long WriteJoined(Guid appealId, string nickName);
        long WriteLeave(Guid appealId, string nickName);
    }
}