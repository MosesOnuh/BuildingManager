using BuildingManager.Contracts.Repository;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BuildingManager.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;

        public TokenRepository(IConfiguration configuration, ILoggerManager logger) 
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task SaveRefreshTokenDetails(string userId, string refreshToken)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))

                {
                    var parameters = new[]
                    {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@RefreshToken", refreshToken)
                    };

                    SqlCommand command = new SqlCommand("proc_SaveRefreshToken", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully inserted Refresh token details");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting Refresh token details in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error inserting Refresh token details");
            }
        }

        public async Task<int> CheckAndDeleteToken(string userId, string prevToken)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("proc_CheckAndDeleteToken", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Token", prevToken);

                    SqlParameter resultCodeParam = new SqlParameter("@ResultCode", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultCodeParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                   
                    int resultCode = Convert.ToInt32(resultCodeParam.Value);
                    //if (resultCode == 1) _logger.LogInfo("Successfully ran query to Check and Delete Token");
                    _logger.LogInfo("Successfully ran query to Check and Delete Token");

                    return resultCode;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Error deleting Refresh token details in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting Refresh token details");
            }
        }
    }
}
