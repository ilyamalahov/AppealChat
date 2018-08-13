using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    public class Expert
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}