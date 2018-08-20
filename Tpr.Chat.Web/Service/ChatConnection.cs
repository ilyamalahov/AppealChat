using System;
using System.Collections.Generic;

namespace Tpr.Chat.Web.Service
{
    public class ChatConnection
    {
        public ChatConnection()
        {
            ExpertConnections = new Dictionary<string, string>();
        }
        public string AppealConnectionId { get; set; }
        public Dictionary<string, string> ExpertConnections { get; set; }
    }

    public enum ContextType
    {
        Appeal,
        Expert
    }
}