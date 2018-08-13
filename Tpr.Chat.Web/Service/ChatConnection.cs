using System;

namespace Tpr.Chat.Web.Service
{
    public class ChatConnection
    {
        public string AppealConnectionId { get; set; }
        public string ExpertConnectionId { get; set; }

        public string GetConnectionId(ContextType connectionType)
        {
            return connectionType == ContextType.Appeal ? AppealConnectionId : ExpertConnectionId;
        }
    }

    public enum ContextType
    {
        Appeal,
        Expert
    }
}