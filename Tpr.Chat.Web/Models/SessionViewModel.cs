using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.Models
{
    public class SessionViewModel
    {
        /// <summary>
        /// Unique identifier of appeal
        /// </summary>
        public Guid AppealId { get; set; }

        /// <summary>
        /// Unique identifier of client
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Chat session object
        /// </summary>
        public ChatSession Session { get; set; }

        /// <summary>
        /// Is before chat?
        /// </summary>
        public bool IsBefore { get; set; }

        /// <summary>
        /// Is after chat?
        /// </summary>
        public bool IsAfter { get; set; }

        /// <summary>
        /// Is active chat?
        /// </summary>
        public bool IsActive { get; set; }
    }
}
