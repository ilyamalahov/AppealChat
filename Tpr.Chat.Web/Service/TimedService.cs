using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Hubs;

namespace Tpr.Chat.Web.Service
{
    public interface ITimedService
    {
        bool Add(Guid appealId, string expertKey = null);
        bool ResetState(Guid appealId);
        bool Remove(Guid appealId);
    }

    public class TimedService : ITimedService
    {
        private readonly ConcurrentDictionary<Guid, Timer> timers = new ConcurrentDictionary<Guid, Timer>();
        private readonly ConcurrentDictionary<Guid, bool> states = new ConcurrentDictionary<Guid, bool>();

        private readonly IChatRepository chatRepository;
        private readonly IHubContext<ChatHub, IChat> chatContext;
        private readonly ILogger<TimedService> logger;

        public TimedService(IChatRepository chatRepository, IHubContext<ChatHub, IChat> chatContext, ILogger<TimedService> logger)
        {
            this.chatRepository = chatRepository;
            this.chatContext = chatContext;
            this.logger = logger;
        }

        public bool Add(Guid appealId, string expertKey = null)
        {
            // 
            logger.LogDebug("Start timer for appeal \"{0}\"", appealId);
         
            //
            var timer = new Timer(async state => await CheckState(appealId, expertKey), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            //
            return timers.TryAdd(appealId, timer);
        }

        public bool ResetState(Guid appealId)
        {
            logger.LogDebug("Reset state for appeal \"{0}\"", appealId);

            return UpdateState(appealId, false);
        }

        public bool Remove(Guid appealId)
        {
            if (!states.TryRemove(appealId, out var removedState)) return false;

            logger.LogDebug("State removed");

            if (!timers.TryRemove(appealId, out var timer)) return false;

            logger.LogDebug("Timer removed");

            // 
            timer.Dispose();

            return true;
        }

        private async Task CheckState(Guid appealId, string expertKey)
        {
            //if (!states.TryGetValue(appealId, out var state)) return;

            var state = states.GetOrAdd(appealId, false);

            if (!state)
            {
                logger.LogDebug("Revert state for appeal \"{0}\"", appealId);

                UpdateState(appealId, true);

                return;
            }

            logger.LogDebug("Appeal \"{0}\" left", appealId);

            if (!Remove(appealId)) return;

            // Sender type
            var senderType = expertKey == null ? ContextType.Appeal : ContextType.Expert;

            // Nick name
            var nickName = senderType == ContextType.Appeal ? "Апеллянт" : "Член КК № " + expertKey;

            // Write message to database
            await chatRepository.WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);

            // 
            await chatContext.Clients.User(appealId.ToString()).Leave(DateTime.Now, nickName);

            // 
            await chatContext.Clients.User(appealId.ToString()).OnlineStatus(false);
        }

        private bool UpdateState(Guid appealId, bool newstate)
        {
            // 
            if (!states.TryGetValue(appealId, out var oldstate)) return false;

            // 
            var result = states.TryUpdate(appealId, newstate, oldstate);

            // 
            if (!states.TryGetValue(appealId, out var checkstate)) return false;

            logger.LogDebug("New state: {0}", checkstate);

            return result;
        }
    }
}
