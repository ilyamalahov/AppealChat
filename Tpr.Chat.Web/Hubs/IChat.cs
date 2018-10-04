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
        Task Join(DateTime messageDate, string nickname, bool isAppealOnline, bool isExpertOnline);
        Task Leave(DateTime messageDate, string nickname);

        // Expert send methods
        Task InitializeChange(string messageText);
        Task CompleteChange(int expertKey);

        Task FirstJoinExpert(string expertKey);
        Task CompleteChat();

        Task OnlineStatus(bool isOnline);
    }
}
