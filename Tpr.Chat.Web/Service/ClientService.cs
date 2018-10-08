﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public enum ContextType
    {
        Appeal,
        Expert
    }

    public class Client
    {
        public Client()
        {
            ExpertClients = new ConcurrentDictionary<int, int>();
        }

        public string AppealClientId { get; set; }

        public ConcurrentDictionary<int, int> ExpertClients { get; set; }

        public bool AddExpertClient(int expertKey)
        {
            lock (ExpertClients)
            {
                var clientCount = ExpertClients.AddOrUpdate(expertKey, 0, (key, count) => count++);

                return clientCount > 0;
            }
        }

        public bool RemoveExpertClient(int expertKey)
        {
            lock (ExpertClients)
            {
                var getResult = ExpertClients.TryGetValue(expertKey, out var clientCount);

                // 
                if (!getResult) return false;

                if (clientCount-- <= 0)
                {
                    return ExpertClients.TryRemove(expertKey, out var count);
                }

                return ExpertClients.TryUpdate(expertKey, clientCount--, clientCount);
            }
        }
    }

    public class ChatClient
    {
        public List<Guid> AppealClientIds { get; set; }
        public Dictionary<string, List<Guid>> ExpertClientIds { get; set; }
    }

    public class ClientService : IClientService
    {
        private readonly ConcurrentDictionary<Guid, ChatClient> clients = new ConcurrentDictionary<Guid, ChatClient>();

        private readonly ILogger<ClientService> logger;

        public ClientService(ILogger<ClientService> logger)
        {
            this.logger = logger;
        }

        private ConcurrentDictionary<Guid, Client> concurrentClients = new ConcurrentDictionary<Guid, Client>();

        public ChatClient Get(Guid appealId, string expertKey = null)
        {
            if(expertKey == null)
            {
                try
                {
                    if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                    return clients.GetOrAdd(appealId, appeal => new ChatClient());
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, exception.Message);

                    return null;
                }
            }
            return null;
        }

        public Client GetClient(Guid appealId)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                return concurrentClients.GetOrAdd(appealId, appeal => new Client());
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return null;
            }
        }

        public bool AddExpert(Guid appealId, int expertKey)
        {
            var client = GetClient(appealId);

            if (client == null) return false;

            try
            {
                return client.AddExpertClient(expertKey);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }
        public bool AddAppeal(Guid appealId, Guid clientId)
        {
            var client = GetClient(appealId);

            if (client == null) return false;

            client.AppealClientId = clientId.ToString();

            return true;
        }

        public bool RemoveExpert(Guid appealId, int expertKey)
        {
            var client = GetClient(appealId);

            if (client == null) return false;

            try
            {
                return client.RemoveExpertClient(expertKey);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }
        public bool RemoveAppeal(Guid appealId)
        {
            var client = GetClient(appealId);

            if (client == null) return false;

            client.AppealClientId = null;

            return true;
        }
    }
}
