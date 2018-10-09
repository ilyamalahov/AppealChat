using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Services
{
    public class CancellationTask
    {
        public Guid ClientId { get; }

        public CancellationTokenSource TokenSource { get; }

        public Func<CancellationToken, Task> TaskMethod { get; }

        public CancellationTask(Guid clientId, Func<CancellationToken, Task> taskMethod, CancellationTokenSource tokenSource)
        {
            ClientId = clientId;

            TokenSource = tokenSource;

            TaskMethod = taskMethod;
        }

        //public void Dispose()
        //{
        //    TokenSource.Dispose();
        //}
    }

    public class TaskService : ITaskService
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> tokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private readonly ILogger<TaskService> logger;
        private readonly IBackgroundTaskQueue taskQueue;

        public TaskService(IBackgroundTaskQueue taskQueue, ILogger<TaskService> logger)
        {
            this.logger = logger;
            this.taskQueue = taskQueue;
        }

        public bool Add(Guid clientId, Func<CancellationToken, Task> method)
        {
            try
            {
                if (clientId == null) throw new ArgumentNullException(nameof(clientId));

                // 
                var tokenSource = new CancellationTokenSource();

                if(!tokenSources.TryAdd(clientId, tokenSource)) return false;

                //var tokenSource = tokenSources.AddOrUpdate(clientId, new CancellationTokenSource(), (key, current) => new CancellationTokenSource());

                //if (tokenSource == null) return false;

                // 
                taskQueue.Queue(new CancellationTask(clientId, method, tokenSource));

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }

        public CancellationTokenSource GetTokenSource(Guid clientId)
        {
            try
            {
                if (clientId == null) throw new ArgumentNullException(nameof(clientId));

                //return tokens.GetOrAdd(clientId, new CancellationTokenSource());
                if (!tokenSources.TryGetValue(clientId, out var tokenSource)) return null;

                return tokenSource;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return null;
            }
        }

        public bool Remove(Guid clientId)
        {
            var sourceResult = tokenSources.TryRemove(clientId, out var tokenSource);

            tokenSource.Dispose();

            return sourceResult;
        }

        //public bool RemoveByItem(CancellationTokenSource tokenSource)
        //{
        //    var item = tokens.First(v => v.Value == tokenSource);

        //    var sourceResult = tokens.TryRemove(item.Key, out var removedSource);

        //    removedSource.Dispose();

        //    return sourceResult;
        //}
    }
}
