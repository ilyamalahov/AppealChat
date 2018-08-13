using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tpr.Chat.Core.Models;
using Dapper;
using Dapper.Contrib.Extensions;
using System.Data.SqlClient;

namespace Tpr.Chat.Core.Repositories
{
    public class AppealInfoRepository : IAppealInfoRepository
    {
        private readonly string connectionString;

        public AppealInfoRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<bool> Add(AppealInfo appealInfo)
        {
            try
            {
                long result;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    result = await connection.InsertAsync(appealInfo);
                }

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<AppealInfo> Get(int appealNumber)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return await connection.GetAsync<AppealInfo>(appealNumber);
            }
        }

        public async Task<IEnumerable<AppealInfo>> GetAll()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                return await connection.GetAllAsync<AppealInfo>();
            }
        }
    }
}
