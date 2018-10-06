using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly ITaskService taskService;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ITaskService taskService, ILogger<QueuedHostedService> logger)
        {
            TaskQueue = taskQueue;

            this.logger = logger;
            this.taskService = taskService;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var cancellationTask = await TaskQueue.DequeueAsync(cancellationToken);

                var tokenSource = cancellationTask?.TokenSource;

                try
                {
                    if (tokenSource == null) throw new NullReferenceException();

                    using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, tokenSource.Token))
                    {
                        await cancellationTask.TaskMethod(linkedSource.Token);
                    }
                }
                //catch(OperationCanceledException exception)
                //{
                //    logger.LogWarning(exception, exception.Message);
                //}
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error occurred executing {nameof(cancellationTask.TaskMethod)}.");
                }
                finally
                {
                    if(taskService.RemoveByItem(tokenSource))
                    {
                        logger.LogWarning("Cancellation token source removed");
                    }
                }
            }

            logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
