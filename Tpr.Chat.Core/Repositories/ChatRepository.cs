using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly string _connectionString;

        public ChatRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ChatSession GetChatSession(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Get<ChatSession>(appealId);
            }
        }

        public IList<ChatMessage> GetChatMessages(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @AppealId";
                return connection.Query<ChatMessage>(sql, new {appealId}).ToList();
            }
        }

        public long WriteMessage(Guid appealId, string nickName, string messageString)
        {
            return WriteChatMessage(appealId, nickName, messageString, ChatMessageTypes.Message);
        }

        public long WriteJoined(Guid appealId, string nickName)
        {
            return WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);
        }

        public long WriteLeave(Guid appealId, string nickName)
        {
            return WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);
        }

        long WriteChatMessage(Guid appealId, string nickName, string messageString, ChatMessageTypes chatMessageType)
        {
            var chatMessage = new ChatMessage()
            {
                AppealId = appealId,
                ChatMessageTypeId = chatMessageType,
                CreateDate = DateTime.Now,
                NickName = nickName,
                MessageString = messageString
            };

            return WriteChatMessage(chatMessage);
        }

        public long WriteChatMessage(ChatMessage message)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    return connection.Insert(message);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public IEnumerable<int> GetExperts(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM dbo.SessionExperts WHERE AppealId = @AppealId";

                return connection.Query<int>(sql, new { appealId });
            }
        }

        public bool IsExists(int key, Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT COUNT(Id) FROM dbo.SessionExperts WHERE Key = @Key AND AppealId = @AppealId";

                return connection.ExecuteScalar<int>(sql, new { key, appealId }) > 0;
            }
        }
    }
}
