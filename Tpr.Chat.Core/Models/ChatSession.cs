using System;
using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        public Guid AppealId { get; set; }
        public int AppealNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
    }
}
