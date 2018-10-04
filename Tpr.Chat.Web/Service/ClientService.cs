using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IClientService
    {
        bool Add(Guid appealId, string clientId);
        bool Remove(Guid appealId);
        string Get(Guid appealId);
    }

    public class ClientService : IClientService
    {
        private readonly Dictionary<Guid, string> clients = new Dictionary<Guid, string>();

        private readonly ILogger<ClientService> logger;

        public ClientService(ILogger<ClientService> logger)
        {
            this.logger = logger;
        }

        public bool Add(Guid appealId, string clientId)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                var client = Get(appealId);

                if (client != null) throw new Exception("Client already exists");

                lock (clients)
                {
                    return clients.TryAdd(appealId, clientId);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }

        public string Get(Guid appealId)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                var clientResult = clients.TryGetValue(appealId, out var clientId);

                if (!clientResult) throw new Exception("Client does not exists");

                return clientId;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return null;
            }
        }

        public bool Remove(Guid appealId)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                var client = Get(appealId);

                if(client == null) throw new Exception("Client does not exists");

                lock (clients)
                {
                    return clients.Remove(appealId);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }
    }
}
