using Dapper.Contrib.Extensions;
using System;

namespace Tpr.Chat.Core.Models
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}