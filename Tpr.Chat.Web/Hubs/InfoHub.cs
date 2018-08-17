using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Service;

namespace Tpr.Chat.Web.Hubs
{
    public class InfoHub : Hub
    {
        private readonly IChatRepository chatRepository;
        private readonly IBackgroundTaskQueue queue;
        private readonly ILogger logger;

        public InfoHub(IChatRepository chatRepository, IBackgroundTaskQueue queue, ILoggerFactory loggerFactory)
        {
            this.chatRepository = chatRepository;
            this.queue = queue;
            logger = loggerFactory.CreateLogger<InfoHub>();
        }

        public override Task OnConnectedAsync()
        {
            queue.QueueBackgroundWorkItem(Update);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        private async Task Update(CancellationToken token)
        {
            var taskId = Guid.NewGuid();

            logger.LogInformation("Task: {0} is started", taskId);

            while (!token.IsCancellationRequested)
            {
                logger.LogDebug("Task: {0}, Current date: {1}", taskId, DateTime.Now);

                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }

            logger.LogInformation("Task: {0} is completed", taskId);
        }

        private void UpdateInfo(object state)
        {
            logger.LogDebug("Current date: {0}", DateTime.Now);
        }

        //[Authorize(AuthenticationSchemes = "Bearer")]
        public async Task MainUpdate(Guid appealId)
        {
            // Current Time
            var currentDate = DateTime.Now;

            // Remaining Time
            var chatSession = chatRepository.GetChatSession(appealId);

            var remainingTimespan = chatSession.FinishTime.Subtract(currentDate);

            var remainingTime = remainingTimespan.TotalMilliseconds;

            // Is alarm
            var isAlarm = remainingTimespan.TotalMinutes < 5;

            // Send
            await Clients.All.SendAsync("ReceiveInfo", currentDate, remainingTime, isAlarm);
        }

        public async Task UpdateInfo()
        {
            // Appeal Identifier
            Guid appealId = Guid.Empty;

            if (!Guid.TryParse(Context.User.Identity.Name, out appealId))
            {
                return;
            }

            // Current Time
            var currentOffset = DateTimeOffset.Now;

            // Begin Time
            long beginTimestamp = 0;

            var beginOffset = DateTimeOffset.FromUnixTimeMilliseconds(beginTimestamp);

            var beginTime = beginOffset.Subtract(currentOffset.TimeOfDay);

            // Remaining Time
            long finishTimestamp = 0;

            var remainingOffset = DateTimeOffset.FromUnixTimeMilliseconds(finishTimestamp);

            var remainingTime = remainingOffset.Subtract(currentOffset.TimeOfDay);

            var info = new
            {
                currentTime = currentOffset.ToUnixTimeMilliseconds(),
                beginTime = beginTime.ToUnixTimeMilliseconds(),
                remainingTime = remainingTime.ToUnixTimeMilliseconds()
            };

            await Clients.User(Context.UserIdentifier).SendAsync("UpdateInfo", info);
        }
    }
}
