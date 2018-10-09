using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class AppealViewModel
    {
        public SessionViewModel SessionModel { get; set; }

        public IEnumerable<ChatMessage> Messages { get; set; }

        public bool IsWaiting { get; set; }

        public Guid? ClientId { get; set; }
    }
}
