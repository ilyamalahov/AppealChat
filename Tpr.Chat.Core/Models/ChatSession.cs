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
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset FinishTime { get; set; }
    }
}
