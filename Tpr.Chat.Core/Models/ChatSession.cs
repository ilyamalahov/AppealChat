using System;
using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        [ExplicitKey]
        public Guid AppealId { get; set; }
        public int AppealNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

        public int CurrentExpert { get; set; }
    }
}
