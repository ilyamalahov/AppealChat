using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public interface IUserRepository
    {
        Task Create(User user);

        Task Delete(Guid id);

        Task<User> Get(Guid id);
    }
}
