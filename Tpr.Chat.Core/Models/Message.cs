using System;
using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    [Table("ChatMessages")]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreateDate { get; set; }

        public string MessageString { get; set; }

        public string NickName { get; set; }

        [ExplicitKey]
        public Guid SessionId { get; set; }
    }
}