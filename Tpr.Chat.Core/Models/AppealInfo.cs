using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Core.Models
{
    [Table("AppealInfo")]
    public class AppealInfo
    {
        [Key]
        public int Number { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Subject { get; set; }

        public DateTime ExamDate { get; set; }
    }
}
