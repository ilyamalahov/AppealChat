using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tpr.Chat.Core.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        
        [ExplicitKey]
        public int RoleId { get; set; }

        public Role Role { get; set; }

        public bool IsExpired { get; set; }
    }
}
