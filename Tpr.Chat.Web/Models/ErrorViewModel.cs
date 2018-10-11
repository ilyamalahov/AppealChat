using System;

namespace Tpr.Chat.Web.Models
{
    public class ErrorViewModel
    {
        public string Message { get; set; }

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}