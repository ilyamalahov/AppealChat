using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tpr.Chat.Core.Constants;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Web.ViewModels
{
    public class IndexViewModel
    {
        public Guid AppealId { get; set; }

        public ChatRole Role { get; internal set; }

        public ICollection<ChatMessage> Messages { get; set; }
    }
}
