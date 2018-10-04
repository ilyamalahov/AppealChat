using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class TaskService
    {
        private readonly Dictionary<Guid, CancellationTokenSource> tokens = new Dictionary<Guid, CancellationTokenSource>();

        public IBackgroundTaskQueue TaskQueue { get; }

        public TaskService(IBackgroundTaskQueue taskQueue)
        {
            TaskQueue = taskQueue;
        }

        public void Add(Guid appealId, Task task)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            var disconnectTask = Disconnect(tokenSource.Token);

            TaskQueue.Queue(disconnectTask);

            lock (tokens)
            {
                tokens.Add(appealId, tokenSource);
            }
        }

        private Task Disconnect(CancellationToken token)
        {

        }

        public CancellationTokenSource GetToken(Guid appealId)
        {
            if(!tokens.TryGetValue(appealId, out var tokenSource))
            {
                return null;
            }

            return tokenSource;
        }
    }
}
