using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Hubs
{
    public interface IChat
    {
        Task Receive(ChatMessage message, bool isAppeal);
        Task Join(ChatMessage message, bool isAppeal);
        Task Leave(ChatMessage message, bool isAppeal);

        Task ChangeStatus(bool status);
    }
}
