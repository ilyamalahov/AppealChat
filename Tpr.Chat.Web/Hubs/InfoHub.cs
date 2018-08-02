using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class InfoHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.User(Context.UserIdentifier).SendAsync("Connect");
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
