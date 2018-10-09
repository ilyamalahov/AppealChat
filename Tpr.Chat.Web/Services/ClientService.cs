using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Services
{
    public enum ContextType
    {
        Appeal,
        Expert
    }
    
    public class ChatClient
    {
        public ChatClient(ILogger<ClientService> logger)
        {
            appealClientIds = new List<Guid>();
            expertClientIds = new List<Guid>();

            this.logger = logger;
        }

        private readonly List<Guid> appealClientIds;
        private readonly List<Guid> expertClientIds;

        private readonly ILogger<ClientService> logger;

        public bool TryAdd(Guid clientId, ContextType clientType)
        {
            try
            {
                if (clientId == null) throw new ArgumentNullException(nameof(clientId));

                switch (clientType)
                {
                    case ContextType.Appeal:
                        if (appealClientIds.Contains(clientId)) return false;

                        lock (appealClientIds)
                        {
                            appealClientIds.Add(clientId);
                        }
                        break;
                    case ContextType.Expert:
                        if (expertClientIds.Contains(clientId)) return false;

                        lock (expertClientIds)
                        {
                            expertClientIds.Add(clientId);
                        }
                        break;
                    default: return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }

        public bool TryRemove(Guid clientId, ContextType clientType)
        {
            try
            {
                if (clientId == null) throw new ArgumentNullException(nameof(clientId));

                switch (clientType)
                {
                    case ContextType.Appeal:
                        if (!appealClientIds.Contains(clientId)) return false;

                        lock (appealClientIds)
                        {
                            return appealClientIds.Remove(clientId);
                        }
                    case ContextType.Expert:
                        if (!expertClientIds.Contains(clientId)) return false;

                        lock (expertClientIds)
                        {
                            return expertClientIds.Remove(clientId);
                        }
                }

                return false;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }

        public bool IsOnline(ContextType clientType)
        {
            var clientCount = 0;

            switch (clientType)
            {
                case ContextType.Appeal:
                    clientCount = appealClientIds.Count;
                    break;
                case ContextType.Expert:
                    clientCount = expertClientIds.Count;
                    break;
            }

            return clientCount > 0;
        }

        //public bool IsExists(Guid clientId, ContextType clientType)
        //{
        //    try
        //    {
        //        if (clientId == null) throw new ArgumentNullException(nameof(clientId));

        //        switch (clientType)
        //        {
        //            case ContextType.Appeal:
        //                return appealClientIds.Contains(clientId);
        //            case ContextType.Expert:
        //                return expertClientIds.Contains(clientId);
        //        }

        //        return false;
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.LogError(exception, exception.Message);

        //        return false;
        //    }
        //}

        public bool IsExistsExcept(Guid clientId, ContextType clientType)
        {
            try
            {
                if (clientId == null) throw new ArgumentNullException(nameof(clientId));

                switch (clientType)
                {
                    case ContextType.Appeal:
                        return appealClientIds.Count(value => value != clientId) > 0;
                    case ContextType.Expert:
                        return expertClientIds.Count(value => value != clientId) > 0;
                }

                return false;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }
    }

    public class ClientService : IClientService
    {
        private readonly ConcurrentDictionary<Guid, ChatClient> clients = new ConcurrentDictionary<Guid, ChatClient>();

        private readonly ILogger<ClientService> logger;

        public ClientService(ILogger<ClientService> logger)
        {
            this.logger = logger;
        }

        public ChatClient Get(Guid appealId)
        {
            try
            {
                // 
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                // 
                return clients.GetOrAdd(appealId, key => new ChatClient(logger));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return null;
            }
        }

        public bool Remove(Guid appealId, int? expertKey)
        {
            // 
            var client = Get(appealId);

            if (client == null) return false;



            return true;
        }

        public bool RemoveItem(Guid appealId, int? expertKey, Guid clientId)
        {
            // 
            var client = Get(appealId);

            if (client == null) return false;

            ////
            //if (expertKey == null)
            //{
            //    lock (client.appealClientIds)
            //    {
            //        return client.appealClientIds.Remove(clientId);
            //    }
            //}
            //else
            //{
            //    lock (client.expertClientIds)
            //    {
            //        return client.expertClientIds.Remove(clientId);
            //    }
            //}

            return true;
        }

        public bool AddItem(Guid appealId, int? expertKey, Guid clientId)
        {
            // 
            var client = Get(appealId);

            if (client != null) return false;

            // 
            //if (expertKey == null)
            //{
            //    lock (client.appealClientIds)
            //    {
            //        client.appealClientIds.Add(clientId);
            //    }
            //}
            //else
            //{
            //    lock (client.expertClientIds)
            //    {
            //        client.expertClientIds.Add(clientId);
            //    }
            //}

            return true;
        }

        public IEnumerable<Guid> GetClients(Guid appealId, int? expertKey)
        {
            return null;
        }

        //public Client GetClient(Guid appealId)
        //{
        //    try
        //    {
        //        if (appealId == null) throw new ArgumentNullException(nameof(appealId));

        //        return concurrentClients.GetOrAdd(appealId, appeal => new Client());
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.LogError(exception, exception.Message);

        //        return null;
        //    }
        //}

        //public bool AddExpert(Guid appealId, int expertKey)
        //{
        //    var client = GetClient(appealId);

        //    if (client == null) return false;

        //    try
        //    {
        //        return client.AddExpertClient(expertKey);
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.LogError(exception, exception.Message);

        //        return false;
        //    }
        //}
        //public bool AddAppeal(Guid appealId, Guid clientId)
        //{
        //    var client = GetClient(appealId);

        //    if (client == null) return false;

        //    client.AppealClientId = clientId.ToString();

        //    return true;
        //}

        //public bool RemoveExpert(Guid appealId, int expertKey)
        //{
        //    var client = GetClient(appealId);

        //    if (client == null) return false;

        //    try
        //    {
        //        return client.RemoveExpertClient(expertKey);
        //    }
        //    catch (Exception exception)
        //    {
        //        logger.LogError(exception, exception.Message);

        //        return false;
        //    }
        //}
        //public bool RemoveAppeal(Guid appealId)
        //{
        //    var client = GetClient(appealId);

        //    if (client == null) return false;

        //    client.AppealClientId = null;

        //    return true;
        //}
    }
}
