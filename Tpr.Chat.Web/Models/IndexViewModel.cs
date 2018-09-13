using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Constants;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class IndexViewModel
    {
        public int ExpertKey { get; set; }

        public ChatSession ChatSession { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }

        public IEnumerable<QuickReply> QuickReplies { get; set; }

        public bool IsBefore { get; set; }

        public bool IsAfter { get; set; }

        public bool IsReadOnly { get; set; }
    }
}
