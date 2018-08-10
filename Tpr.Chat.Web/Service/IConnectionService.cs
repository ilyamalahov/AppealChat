using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IConnectionService
    {
        ChatConnection Get(Guid appealId);
        bool Add(Guid appealId, ChatConnection connection);
        bool Remove(Guid appealId);
    }
}
