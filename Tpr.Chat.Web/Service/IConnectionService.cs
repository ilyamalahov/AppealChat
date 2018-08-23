using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IConnectionService
    {
        string GetConnectionId(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null);
        void AddConnectionId(Guid appealId, string connectionId, ContextType connectionType = ContextType.Appeal, string expertKey = null);
        void RemoveConnectionId(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null);

        bool isOnline(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null);

        IEnumerable<string> GetExpertKeys(Guid appealId);

        string GetOnlineExpertKey(Guid appealId, string expertKey);
    }
}
