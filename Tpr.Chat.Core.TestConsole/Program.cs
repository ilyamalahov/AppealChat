using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Repositories;

namespace Tpr.Chat.Core.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new ChatRepository("Data Source=zubrus.rbc.ru\\r2;Initial Catalog=tpr8018chat;User=sa;Password=2222;");

            var appealId = new Guid("B8819639-82A0-4C34-92A6-747006A164F7");
            var session = repository.GetChatSession(appealId);
            var messages = repository.GetChatMessages(appealId);
            repository.WriteJoined(appealId, "Эксперт 123");
            repository.WriteMessage(appealId, "Эксперт 123", "Все оценки согласно критериям");
            repository.WriteLeave(appealId, "Эксперт 123");
            var newmessages = repository.GetChatMessages(appealId);
        }
    }
}
