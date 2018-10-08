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
        public Guid Id { get; }

        public CancellationTokenSource TokenSource { get; }

        public Func<CancellationToken, Task> TaskMethod { get; }

        public CancellationTask(Func<CancellationToken, Task> taskMethod, CancellationTokenSource tokenSource)
        {
            Id = Guid.NewGuid();

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
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> tokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private readonly ILogger<TaskService> logger;
        private readonly IBackgroundTaskQueue taskQueue;

        public TaskService(IBackgroundTaskQueue taskQueue, ILogger<TaskService> logger)
        {
            this.logger = logger;
            this.taskQueue = taskQueue;
        }

        public bool AddOrUpdate(Guid appealId, Func<CancellationToken, Task> method)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));
                
                var tokenSource = tokens.AddOrUpdate(appealId, new CancellationTokenSource(), (key, current) => new CancellationTokenSource());

                if (tokenSource == null) return false;

                taskQueue.Queue(new CancellationTask(method, tokenSource));

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return false;
            }
        }

        public CancellationTokenSource GetTokenSource(Guid appealId)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));
                
                if (!tokens.TryGetValue(appealId, out var tokenSource)) return null;

                return tokenSource;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, exception.Message);

                return null;
            }
        }

        public bool RemoveByItem(CancellationTokenSource tokenSource)
        {
            var item = tokens.First(v => v.Value == tokenSource);

            var sourceResult = tokens.TryRemove(item.Key, out var removedSource);

            removedSource.Dispose();

            return sourceResult;
        }
    }
}
