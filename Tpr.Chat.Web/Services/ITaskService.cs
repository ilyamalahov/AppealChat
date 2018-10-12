using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Services
{
    public interface ITaskService
    {
        bool Add(Guid appealId, Func<CancellationToken, Task> method);
        CancellationTokenSource GetTokenSource(Guid appealId);
        //bool RemoveByItem(CancellationTokenSource tokenSource);

        bool Remove(Guid id);
    }
}
