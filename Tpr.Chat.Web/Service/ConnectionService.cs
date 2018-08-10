using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class ConnectionService : IConnectionService
    {
        private Dictionary<Guid, ChatConnection> connections;

        public ConnectionService()
        {
            connections = new Dictionary<Guid, ChatConnection>();
        }

        public ChatConnection Get(Guid appealId)
        {
            if (appealId == null) return null;

            ChatConnection connection;

            if(!connections.TryGetValue(appealId, out connection))
            {
                connection = new ChatConnection();

                var result = Add(appealId, connection);

                if (!result) return null;

                return connection;
            }

            return connection;
        }

        public bool Add(Guid appealId, ChatConnection connection)
        {
            if (appealId == null) return false;
            
            lock (connections)
            {
                connections.Add(appealId, connection);
            }

            return true;
        }

        public bool Remove(Guid appealId)
        {
            if (appealId == null) return false;

            lock (connections)
            {
                connections.Remove(appealId);
            }

            return true;
        }
    }
}
