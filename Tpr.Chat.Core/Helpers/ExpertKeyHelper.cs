using System;

namespace Tpr.Chat.Core.Helpers
{
    public static class ExpertKeyHelper
    {
        public static string GetKey(Guid appealId, string nickname)
        {
            return HashHelper.Md5($"{appealId}{nickname}tprchat");
        }
    }
}
