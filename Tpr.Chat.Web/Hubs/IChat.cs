using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Hubs
{
    public interface IChat
    {
        // Common send methods
        Task Receive(ChatMessage message);
        Task Join(ChatMessage message, bool isAppealOnline, string expertKey);
        Task Leave(ChatMessage message, string onlineExpertKey);

        // Expert send methods
        Task ChangeExpert(string nickname);
        Task FirstJoinExpert(string nickname);
        Task ToggleChat(bool isBlocked);
    }
}
