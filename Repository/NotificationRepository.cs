using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BuildingManager.Repository
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;
        public NotificationRepository(IConfiguration configuration, ILoggerManager logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
       public async Task<int> CreateInviteNotification(InviteNotification model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", model.Id),
                        new SqlParameter("@PmId", model.PmId),
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@Role", model.Role),
                        new SqlParameter("@Profession", model.Profession),
                        new SqlParameter("@CreatedAt", model.CreatedAt),
                        new SqlParameter("@UpdatedAt", DBNull.Value),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    SqlCommand command = new("proc_CreateInviteNotification", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully Created a Project Invite Notification");
                    return (int)command.Parameters["@ResultCode"].Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new project notification invite in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new  project notification invite");
            }

        }
    }
}
