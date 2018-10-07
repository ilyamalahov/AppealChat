using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class SessionViewModel
    {
        // 
        public Guid AppealId { get; set; }

        //
        public ChatSession Session { get; set; }

        //
        public bool IsBefore { get; set; }

        //
        public bool IsAfter { get; set; }

        //
        public bool IsActive { get; set; }
    }
}
