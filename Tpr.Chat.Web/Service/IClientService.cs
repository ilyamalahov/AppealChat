using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.Service
{
    public interface IClientService
    {
        //bool Add(Guid appealId, string clientId);
        //bool Remove(Guid appealId);
        //string Get(Guid appealId);

        Client GetClient(Guid appealId);

        bool AddExpert(Guid appealId, int expertKey);
        bool AddAppeal(Guid appealId, Guid clientId);

        bool RemoveExpert(Guid appealId, int expertKey);
        bool RemoveAppeal(Guid appealId);
    }
}
