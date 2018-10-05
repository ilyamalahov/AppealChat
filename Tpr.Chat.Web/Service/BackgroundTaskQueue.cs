using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IBackgroundTaskQueue
    {
        void Queue(CancellationTask workItem);

        Task<CancellationTask> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<CancellationTask> _workItems = new ConcurrentQueue<CancellationTask>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void Queue(CancellationTask workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);

            _signal.Release();
        }

        public async Task<CancellationTask> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);

            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
