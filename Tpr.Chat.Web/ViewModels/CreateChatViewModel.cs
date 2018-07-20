using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tpr.Chat.Web.ViewModels
{
    public class CreateChatViewModel
    {
        public Guid ConflictGroupId { get; set; }
        public Guid AppealCandidateId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int AppealNumber { get; set; }
        public DateTime AppealDate { get; set; }
    }
}
