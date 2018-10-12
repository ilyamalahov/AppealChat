using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;
using Tpr.Chat.Web.Services;

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
            var chatSession = await chatRepository.GetChatSession(appealId);

            var remainingTimespan = chatSession.FinishTime.Subtract(currentDate);

            var remainingTime = remainingTimespan.TotalMilliseconds;

            // Is alarm
            var isAlarm = remainingTimespan.TotalMinutes < 5;

            // Is finished
            var isFinished = remainingTimespan.TotalMilliseconds <= 0;

            // Send information response to caller
            await Clients.Caller.SendAsync("ReceiveInfo", currentDate, remainingTime, isAlarm, isFinished);
        }

        public async Task EarlyUpdate(Guid appealId)
        {
            // Current Time
            var currentDate = DateTime.Now;

            // Remaining Time
            var chatSession = await chatRepository.GetChatSession(appealId);

            var remainingTimespan = chatSession.StartTime.Subtract(currentDate);

            var remainingTime = remainingTimespan.TotalMilliseconds;

            // Is finished
            var isStarted = remainingTimespan.TotalMilliseconds <= 0;

            // Send information response to caller
            await Clients.Caller.SendAsync("ReceiveInfo", currentDate, remainingTime, isStarted);
        }

        public async Task CompleteUpdate()
        {
            // Current Time
            var currentDate = DateTime.Now;

            // Send information response to caller
            await Clients.Caller.SendAsync("ReceiveInfo", currentDate);
        }
        public ChannelReader<int> Counter(int count, int delay)
        {
            var channel = Channel.CreateUnbounded<int>();

            // We don't want to await WriteItems, otherwise we'd end up waiting 
            // for all the items to be written before returning the channel back to
            // the client.
            _ = WriteItems(channel.Writer, count, delay);

            return channel.Reader;
        }

        private async Task WriteItems(ChannelWriter<int> writer, int count, int delay)
        {
            for (var i = 0; i < count; i++)
            {
                await writer.WriteAsync(i);
                await Task.Delay(delay);
            }

            writer.TryComplete();
        }

        //public ChannelReader<dynamic> Counter(Guid appealId, int count, int delay)
        //{
        //    var channel = Channel.CreateUnbounded<dynamic>();

        //    _ = WriteItems(channel.Writer, appealId, count, delay);

        //    return channel.Reader;
        //}

        //private async Task WriteItems(ChannelWriter<dynamic> writer, Guid appealId, int count, int delay)
        //{
        //    // Remaining Time
        //    var chatSession = chatRepository.GetChatSession(appealId);

        //    for (var i = 0; i < count; i++)
        //    {
        //        var currentDate = DateTime.Now;

        //        var remainingTimespan = chatSession.StartTime.Subtract(currentDate);

        //        var data = new
        //        {
        //            currentDate,
        //            appealId
        //        };

        //        await writer.WriteAsync(data);
        //        await Task.Delay(delay);
        //    }

        //    writer.TryComplete();
        //}
    }
}
