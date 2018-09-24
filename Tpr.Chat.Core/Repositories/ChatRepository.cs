using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ChatSession> GetChatSession(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return await connection.GetAsync<ChatSession>(appealId);
            }
        }

        public async Task<bool> UpdateSession(ChatSession chatSession)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return await connection.UpdateAsync(chatSession);
            }
        }

        #endregion

        #region Messages

        public async Task<ChatMessage> GetWelcomeMessage(Guid appealId, string expertKey)
        {
            var nickname = "Член КК № " + expertKey;
            var messageType = (int)ChatMessageTypes.FirstExpert;

            string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @appealId AND NickName = @nickname AND ChatMessageTypeId = @messageType";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.QuerySingleOrDefaultAsync<ChatMessage>(sql, new { appealId, nickname, messageType });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }

        public async Task<IEnumerable<ChatMessage>> GetChatMessages(Guid appealId)
        {
            string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @AppealId";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.QueryAsync<ChatMessage>(sql, new { appealId });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }

        public async Task<bool> WriteChatMessage(Guid appealId, string nickName, string messageString, ChatMessageTypes messageType)
        {
            var chatMessage = new ChatMessage()
            {
                AppealId = appealId,
                ChatMessageTypeId = messageType,
                CreateDate = DateTime.Now,
                NickName = nickName,
                MessageString = messageString
            };

            return await WriteChatMessage(chatMessage);
        }

        public async Task<bool> AddStatusMessage(Guid appealId, string nickName, ChatMessageTypes messageType)
        {
            return await WriteChatMessage(appealId, nickName, null, messageType);
        }

        public async Task<bool> WriteChatMessage(ChatMessage message)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.InsertAsync(message) > 0;
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
        
        public async Task<IEnumerable<QuickReply>> GetQuickReplies()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return await connection.GetAllAsync<QuickReply>();
            }
        }

        #endregion

        #region Member Replacement

        public async Task<MemberReplacement> GetMemberReplacement(Guid appealId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var sql = "SELECT * FROM dbo.MemberReplacements WHERE AppealId = @appealId";

                    return await connection.QuerySingleOrDefaultAsync<MemberReplacement>(sql, new { appealId });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return null;
            }
        }

        public async Task<bool> AddMemberReplacement(Guid appealId, int expertKey)
        {
            var replacement = new MemberReplacement
            {
                AppealId = appealId,
                RequestTime = DateTime.Now,
                OldMember = expertKey
            };

            return await AddMemberReplacement(replacement);
        }

        public async Task<bool> AddMemberReplacement(MemberReplacement replacement)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    await connection.InsertAsync(replacement);

                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);

                return false;
            }
        }

        public async Task<bool> UpdateMemberReplacement(MemberReplacement replacement)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    return await connection.UpdateAsync(replacement);
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
