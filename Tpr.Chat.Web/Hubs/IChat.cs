using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Hubs
{
    public interface IChat
    {
        Task Receive(ChatMessage message, string sender);
        Task Join(ChatMessage message, string sender, bool isAppealOnline, bool isExpertOnline);
        Task Leave(ChatMessage message, string sender);

        Task ChangeStatus(bool isOnline);
    }
}
