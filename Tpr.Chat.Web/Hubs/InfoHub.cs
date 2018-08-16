using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Web.Hubs
{
    public class InfoHub : Hub
    {
        private readonly IChatRepository chatRepository;

        public InfoHub(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
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
