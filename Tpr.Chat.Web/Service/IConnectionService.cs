using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IConnectionService
    {
        string GetConnectionId(Guid appealId, ContextType connectionType);
        void AddConnectionId(Guid appealId, string connectionId, ContextType connectionType);
        void RemoveConnectionId(Guid appealId, ContextType senderType);

        bool isOnline(Guid appealId, ContextType connectionType);
    }
}
