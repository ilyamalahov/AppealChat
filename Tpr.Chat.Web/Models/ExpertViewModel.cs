using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class ExpertViewModel
    {
        public SessionViewModel SessionModel { get; set; }

        public IEnumerable<ChatMessage> Messages { get; set; }

        public IEnumerable<QuickReply> QuickReplies { get; set; }

        public int ExpertKey { get; set; }

        public bool IsReadonly { get; set; }
    }
}
