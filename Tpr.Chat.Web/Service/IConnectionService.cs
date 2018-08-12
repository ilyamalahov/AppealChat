using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IConnectionService
    {
        string GetConnectionId(Guid appealId, bool isAppeal);
        void AddConnectionId(Guid appealId, string connectionId, bool isAppeal);
        void RemoveConnectionId(Guid appealId, bool isAppeal);

        bool isOnline(Guid appealId, bool isAppeal);
    }
}
