using System;
using System.Collections.Generic;
using System.Data;
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
                return connection.Query<ChatMessage>(sql, new { appealId }).ToList();
            }
        }

        public bool WriteMessage(Guid appealId, string nickName, string messageText)
        {
            return WriteChatMessage(appealId, nickName, messageText, ChatMessageTypes.Message);
        }

        public bool WriteJoined(Guid appealId, string nickName)
        {
            return WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Joined);
        }

        public bool WriteLeave(Guid appealId, string nickName)
        {
            return WriteChatMessage(appealId, nickName, null, ChatMessageTypes.Leave);
        }

        public bool WriteChatMessage(Guid appealId, string nickName, string messageString, ChatMessageTypes messageType)
        {
            var chatMessage = new ChatMessage()
            {
                AppealId = appealId,
                ChatMessageTypeId = messageType,
                CreateDate = DateTime.Now,
                NickName = nickName,
                MessageString = messageString
            };

            return WriteChatMessage(chatMessage);
        }

        public bool AddStatusMessage(Guid appealId, string nickName, ChatMessageTypes messageType)
        {
            return WriteChatMessage(appealId, nickName, null, messageType);
        }

        public bool WriteChatMessage(ChatMessage message)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    return connection.Insert(message) > 0;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }
        }

        #endregion

        #region Quick Reply

        //public IEnumerable<int> GetExperts(Guid appealId)
        //{
        //    string sql = "SELECT ExpertKey FROM dbo.SessionExperts WHERE AppealId = @appealId";

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();

        //        return connection.Query<int>(sql, new { appealId });
        //    }
        //}

        //public bool AddExpert(Guid appealId, int expertKey)
        //{
        //    string sql = "INSERT INTO dbo.SessionExperts (ExpertKey, AppealId) VALUES (@expertKey, @appealId)";

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        connection.Open();

        //        return connection.Execute(sql, new { expertKey, appealId }) > 0;
        //    }
        //}

        public IEnumerable<QuickReply> GetQuickReplies()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return connection.GetAll<QuickReply>();
            }
        }

        #endregion

        #region Member Replacement

        public MemberReplacement GetMemberReplacement(Guid appealId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var sql = "SELECT * FROM dbo.MemberReplacements WHERE AppealId = @appealId";

                    return connection.QuerySingle<MemberReplacement>(sql, new { appealId });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }
        public MemberReplacement GetMemberReplacement(Guid appealId, string expertKey)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var sql = "SELECT * FROM dbo.MemberReplacements WHERE AppealId = @appealId AND OldMember = @expertKey";

                    return connection.QuerySingle<MemberReplacement>(sql, new { appealId, expertKey });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }

        public bool AddMemberReplacement(Guid appealId, int expertKey)
        {
            var replacement = new MemberReplacement
            {
                AppealId = appealId,
                RequestTime = DateTime.Now,
                OldMember = expertKey
            };

            return AddMemberReplacement(replacement);
        }

        public bool AddMemberReplacement(MemberReplacement replacement)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    return connection.Insert(replacement) > 0;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }
        }

        public bool UpdateMemberReplacement(MemberReplacement replacement)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    return connection.Update(replacement);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }
        }

        #endregion
    }
}
