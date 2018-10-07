using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface ITaskService
    {
        bool AddOrUpdate(Guid appealId, Func<CancellationToken, Task> method);
        CancellationTokenSource GetTokenSource(Guid appealId);
        bool RemoveByItem(CancellationTokenSource tokenSource);
    }
}
