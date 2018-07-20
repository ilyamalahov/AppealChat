﻿using System;
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

        public IList<Message> GetChatMessages(Guid appealId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sql = "SELECT * FROM dbo.ChatMessages WHERE AppealId = @AppealId";
                return connection.Query<Message>(sql, new {appealId}).ToList();
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
            var chatMessage = new Message()
            {
                AppealId = appealId,
                ChatMessageTypeId = chatMessageType,
                CreateDate = DateTime.Now,
                NickName = nickName,
                MessageString = messageString
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Insert(chatMessage);
            }
        }
    }
}
