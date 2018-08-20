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
        //public Guid AppealId { get; set; }

        public bool IsFinished { get; set; }

        public int ExpertKey { get; set; }

        public ChatSession ChatSession { get; set; }

        public ICollection<ChatMessage> Messages { get; set; }

        public IEnumerable<QuickReply> QuickReplies { get; set; }

        public IEnumerable<string> OnlineExpertKeys { get; set; }
    }
}
