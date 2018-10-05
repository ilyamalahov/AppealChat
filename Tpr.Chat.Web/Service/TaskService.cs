using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class CancellationTask
    {
        public CancellationTokenSource TokenSource { get; }

        public Func<CancellationToken, Task> TaskMethod { get; }

        public CancellationTask(Func<CancellationToken, Task> taskMethod)
        {
            TokenSource = new CancellationTokenSource();

            TaskMethod = taskMethod;
        }
    }

    public interface ITaskService
    {
        bool Add(Guid appealId, Func<CancellationToken, Task> method);
        CancellationTokenSource GetTokenSource(Guid appealId);
    }

    public class TaskService
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> tokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();

        public IBackgroundTaskQueue TaskQueue { get; }

        public TaskService(IBackgroundTaskQueue taskQueue)
        {
            TaskQueue = taskQueue;
        }

        public bool Add(Guid appealId, Func<CancellationToken, Task> method)
        {
            try
            {
                if (appealId == null) throw new ArgumentNullException(nameof(appealId));

                var cancellationTask = new CancellationTask(method);

                TaskQueue.Queue(cancellationTask);

                return tokens.TryAdd(appealId, cancellationTask.TokenSource);
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                return null;
            }
        }
    }
}
