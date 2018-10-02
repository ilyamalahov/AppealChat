using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public class CustomTask
    {
        public CustomTask(Func<CancellationToken, Task> method)
        {
            Method = method;
        }

        public bool IsActive { get; set; }

        public Func<CancellationToken, Task> Method { get; }
    }

    public interface IClientService
    {
        void AddTask(Guid appealId, Func<CancellationToken, Task> task);
    }

    public class ClientService
    {
        private readonly Dictionary<Guid, CustomTask> tasks = new Dictionary<Guid, CustomTask>();
        private readonly Dictionary<Guid, Func<CancellationToken, Task>> taskItems = new Dictionary<Guid, Func<CancellationToken, Task>>();
        private readonly ILogger<ClientService> logger;

        public ClientService(ILogger<ClientService> logger)
        {
            this.logger = logger;
        }

        private void DoWork(object state)
        {
            Console.WriteLine("Work timer");
        }

        public bool AddTask(Guid appealId, Func<CancellationToken, Task> task)
        {
            lock (tasks)
            {
                taskItems.Add(appealId, TimedHandle);

                if (!taskItems.TryAdd(appealId, task)) return false;
            }

            return true;
        }

        private Task TimedHandle(CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
