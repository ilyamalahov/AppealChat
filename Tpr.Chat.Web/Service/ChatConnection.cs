using System;
using System.Collections.Generic;

namespace Tpr.Chat.Web.Service
{
    public class ChatConnection
    {
        public string AppealConnectionId { get; set; }

        public string ExpertConnectionId { get; set; }
    }

    public enum ContextType
    {
        Appeal,
        Expert
    }
}