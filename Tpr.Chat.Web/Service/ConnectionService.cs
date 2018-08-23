using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class ConnectionService : IConnectionService
    {
        private Dictionary<Guid, ChatConnection> connections = new Dictionary<Guid, ChatConnection>();

        public void AddConnectionId(Guid appealId, string connectionId, ContextType connectionType = ContextType.Appeal, string expertKey = null)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
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
                    connection.ExpertConnections.Add(expertKey, connectionId);
                    break;
            }
        }

        public string GetConnectionId(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null)
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
                    return connection.ExpertConnections[expertKey];
            }

            return null;
        }

        public void RemoveConnectionId(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null)
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
                    connection.ExpertConnections.Remove(expertKey);
                    break;
            }
        }

        public bool isOnline(Guid appealId, ContextType connectionType = ContextType.Appeal, string expertKey = null)
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
                    connectionId = connection.ExpertConnections[expertKey];
                    break;
                default:
                    connectionId = null;
                    break;
            }

            return !string.IsNullOrEmpty(connectionId);
        }

        public IEnumerable<string> GetExpertKeys(Guid appealId)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return null;
            }

            return connection.ExpertConnections.Keys;
        }

        public string GetOnlineExpertKey(Guid appealId, string expertKey)
        {
            ChatConnection connection;

            if (!connections.TryGetValue(appealId, out connection))
            {
                return null;
            }

            return connection.ExpertConnections.Select(ec => ec.Key).FirstOrDefault(key => key != expertKey);
        }
    }
}
