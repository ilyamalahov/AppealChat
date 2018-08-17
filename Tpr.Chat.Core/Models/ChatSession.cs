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

        public string SubjectName { get; set; }

        public DateTime ExamDate { get; set; }

        // Commission info

        public DateTime CommissionStartTime { get; set; }

        public DateTime CommissionFinishTime { get; set; }

        public string CommissionLink { get; set; }
    }
}
