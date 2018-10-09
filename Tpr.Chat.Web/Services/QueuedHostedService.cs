using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Services
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue taskQueue;
        private readonly ILogger logger;
        private readonly ITaskService taskService;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue, ITaskService taskService, ILogger<QueuedHostedService> logger)
        {
            this.taskQueue = taskQueue;
            this.logger = logger;
            this.taskService = taskService;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var task = await taskQueue.DequeueAsync(cancellationToken);

                try
                {
                    using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, task.TokenSource.Token))
                    {
                        await task.TaskMethod(linkedSource.Token);
                    }
                }
                //catch(OperationCanceledException exception)
                //{
                //    logger.LogWarning(exception, exception.Message);
                //}
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error occurred executing {nameof(task.TaskMethod)}.");
                }
                finally
                {
                    if (!taskService.Remove(task.ClientId))
                    {
                        logger.LogError("Error occurred removing task row");
                    }
                }
            }

            logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
