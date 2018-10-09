using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Services
{
    public interface IClientService
    {
        //bool Add(Guid appealId, string clientId);
        //bool Remove(Guid appealId);
        //string Get(Guid appealId);

        ChatClient Get(Guid appealId);
        IEnumerable<Guid> GetClients(Guid appealId, int? expertKey);

        bool AddItem(Guid appealId, int? expertKey, Guid clientId);
        bool Remove(Guid appealId, int? expertKey);

        //Client GetClient(Guid appealId);

        //bool AddExpert(Guid appealId, int expertKey);
        //bool AddAppeal(Guid appealId, Guid clientId);

        //bool RemoveExpert(Guid appealId, int expertKey);
        //bool RemoveAppeal(Guid appealId);

        bool RemoveItem(Guid appealId, int? expertKey, Guid clientId);
    }
}
