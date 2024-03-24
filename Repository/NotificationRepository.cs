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
       public async Task CreateInviteNotification(InviteNotification model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", model.Id),
                        new SqlParameter("@Email", model.Email),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@userRole", model.UserRole),
                        new SqlParameter("@CreatedAt", model.CreatedAt),
                    };

                    SqlCommand command = new("proc_CreateInviteNotification", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully Created a Project Invite Notification");
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
