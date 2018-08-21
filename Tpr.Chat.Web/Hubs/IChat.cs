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
        Task Join(ChatMessage message, string sender, bool isAppealOnline, string expertKey);
        Task Leave(ChatMessage message, string sender, string expertKey);

        Task ChangeStatus(bool isOnline);
    }
}
