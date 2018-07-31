using Dapper.Contrib.Extensions;

namespace Tpr.Chat.Core.Models
{
    public class Expert
    {
        [Key]
        public int Id { get; set; }

        public int Key { get; set; }

        public string SecretKey { get; set; }
    }
}
