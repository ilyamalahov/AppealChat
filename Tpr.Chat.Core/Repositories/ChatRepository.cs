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

        #region Session
        
        public ChatSession GetChatSession(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Get<ChatSession>(appealId);
            }
        }

        public bool UpdateSession(ChatSession chatSession)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Update(chatSession);
            }
        }

        #endregion
        
        #region Messages

        public IList<ChatMessage> GetChatMessages(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @AppealId";
                return connection.Query<ChatMessage>(sql, new {appealId}).ToList();
            }
        }

        public IEnumerable<ChatMessage> GetExpertMessages(Guid appealId, string nickName, ChatMessageTypes messageType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @appealId AND NickName = @nickName AND ChatMessageTypeId = @messageType";

                return connection.Query<ChatMessage>(sql, new { appealId, nickName, messageType = (int)messageType });
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
        
        #endregion

        #region Experts

        public IEnumerable<int> GetExperts(Guid appealId)
        {
            string sql = "SELECT ExpertKey FROM dbo.SessionExperts WHERE AppealId = @appealId";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Query<int>(sql, new { appealId });
            }
        }

        public bool AddExpert(Guid appealId, int expertKey)
        {
            string sql = "INSERT INTO dbo.SessionExperts (ExpertKey, AppealId) VALUES (@expertKey, @appealId)";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.Execute(sql, new { expertKey, appealId }) > 0;
            }
        }

        #endregion
    }
}
