using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public interface IAppealInfoRepository
    {
        Task<IEnumerable<AppealInfo>> GetAll();

        Task<AppealInfo> Get(int appealNumber);

        Task<bool> Add(AppealInfo appealInfo);
    }
}
