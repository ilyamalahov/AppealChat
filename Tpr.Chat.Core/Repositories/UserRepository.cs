using System;
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;

namespace Tpr.Chat.Core.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task Create(User user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync("INSERT INTO dbo.Users (Id, RoleId) VALUES (@Id, @RoleId)", user);
            }
        }

        public async Task Delete(Guid id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync("DELETE FROM dbo.Users WHERE Id = @Id", id);
            }
        }

        public async Task<User> Get(Guid id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var result = await connection.QueryAsync<User>("SELECT * FROM dbo.Users WHERE Id = @Id", id);

                return result.FirstOrDefault();
            }
        }
    }
}
