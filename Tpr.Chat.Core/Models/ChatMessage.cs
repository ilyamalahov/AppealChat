using System;
using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public Guid AppealId { get; set; }
        public DateTime CreateDate { get; set; }
        public string MessageString { get; set; }
        public string NickName { get; set; }
        public ChatMessageTypes ChatMessageTypeId { get; set; }
    }

    public enum ChatMessageTypes
    {
        Joined = 1,
        Message = 2,
        Leave = 3,
        ChangeExpert = 4,
        FirstExpert = 5
    }
}