using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class ConnectionService : IConnectionService
    {
        private Dictionary<Guid, ChatConnection> connections = new Dictionary<Guid, ChatConnection>();

        public string GetConnectionId(Guid appealId, bool isAppeal)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return null;
            }

            return isAppeal ? connection.AppealConnectionId : connection.ExpertConnectionId;
        }

        public void AddConnectionId(Guid appealId, string connectionId, bool isAppeal)
        {
            ChatConnection connection;

            if(!connections.TryGetValue(appealId, out connection))
            {
                lock (connections)
                {
                    connection = new ChatConnection();

                    if (!connections.TryAdd(appealId, connection)) return;
                }
            }

            if(isAppeal)
            {
                connection.AppealConnectionId = connectionId;
            }
            else
            {
                connection.ExpertConnectionId = connectionId;
            }
        }

        public void RemoveConnectionId(Guid appealId, bool isAppeal)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return;
            }

            if (isAppeal)
            {
                connection.AppealConnectionId = null;
            }
            else
            {
                connection.ExpertConnectionId = null;
            }
        }

        public bool isOnline(Guid appealId, bool isAppeal)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return false;
            }

            var connectionId = isAppeal ? connection.AppealConnectionId : connection.ExpertConnectionId;

            return !string.IsNullOrEmpty(connectionId);
        }
    }
}
