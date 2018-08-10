using System;

namespace Tpr.Chat.Web.Service
{
    public class ChatConnection
    {
        public string AppealConnectionId { get; set; }
        public string ExpertConnectionId { get; set; }

        public string GetConnectionId(string expertKey)
        {
            return expertKey != null ? AppealConnectionId : ExpertConnectionId;
        }
    }

    public enum ConnectionType
    {
        Appeal,
        Expert
    }
}