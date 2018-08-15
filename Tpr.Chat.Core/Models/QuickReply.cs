using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Core.Models
{
    [Table("QuickReplies")]
    public class QuickReply
    {
        [Key]
        public int Id { get; set; }

        public string MessageText { get; set; }
    }
}
