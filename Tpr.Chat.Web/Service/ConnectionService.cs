using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class ConnectionService : IConnectionService
    {
        private Dictionary<Guid, ChatConnection> connections = new Dictionary<Guid, ChatConnection>();

        public string GetConnectionId(Guid appealId, ContextType connectionType)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return null;
            }

            switch (connectionType)
            {
                case ContextType.Appeal:
                    return connection.AppealConnectionId;
                case ContextType.Expert:
                    return connection.ExpertConnectionId;
                default:
                    return null;
            }
        }

        public void AddConnectionId(Guid appealId, string connectionId, ContextType connectionType)
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
            
            switch (connectionType)
            {
                case ContextType.Appeal:
                    connection.AppealConnectionId = connectionId;
                    break;
                case ContextType.Expert:
                    connection.ExpertConnectionId = connectionId;
                    break;
            }
        }

        public void RemoveConnectionId(Guid appealId, ContextType connectionType)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return;
            }

            switch (connectionType)
            {
                case ContextType.Appeal:
                    connection.AppealConnectionId = null;
                    break;
                case ContextType.Expert:
                    connection.ExpertConnectionId = null;
                    break;
            }
        }

        public bool isOnline(Guid appealId, ContextType connectionType)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return false;
            }

            string connectionId;

            switch (connectionType)
            {
                case ContextType.Appeal:
                    connectionId = connection.AppealConnectionId;
                    break;
                case ContextType.Expert:
                    connectionId = connection.ExpertConnectionId;
                    break;
                default:
                    connectionId = null;
                    break;
            }

            return !string.IsNullOrEmpty(connectionId);
        }
    }
}
