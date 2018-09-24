using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class IndexViewModel
    {
        public Guid AppealId { get; set; }

        public ChatSession Session { get; set; }

        public bool IsBefore { get; set; }

        public bool IsAfter { get; set; }

        public IEnumerable<ChatMessage> Messages { get; set; }

        public IEnumerable<QuickReply> QuickReplies { get; set; }

        public int ExpertKey { get; set; }

        public bool IsExpertChanged { get; set; }

        public bool IsWaiting { get; set; }
    }
}
