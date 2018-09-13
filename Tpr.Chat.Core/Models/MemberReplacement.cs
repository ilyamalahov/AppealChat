using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Core.Models
{
    public class MemberReplacement
    {
        [Key]
        public Guid AppealId { get; set; }

        public DateTime RequestTime { get; set; }

        public int OldMember { get; set; }

        public DateTime? ReplaceTime { get; set; }

        public int? NewMember { get; set; }
    }
}
