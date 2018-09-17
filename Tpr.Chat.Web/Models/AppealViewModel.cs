using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class AppealViewModel : SessionViewModel
    {
        public ICollection<ChatMessage> Messages { get; set; }

        public bool IsWaiting { get; set; }
    }
}
