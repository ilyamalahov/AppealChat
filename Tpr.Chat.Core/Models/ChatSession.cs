using System;
using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    [Table("ChatSessions")]
    public class ChatSession
    {
        [ExplicitKey]
        public Guid AppealId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public int CurrentExpertKey { get; set; }

        // Appeal info

        public int AppealNumber { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Subject { get; set; }

        public DateTime ExamDate { get; set; }
    }
}
