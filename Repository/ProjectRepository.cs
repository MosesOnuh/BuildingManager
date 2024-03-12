using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BuildingManager.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;

        public ProjectRepository(IConfiguration configuration, ILoggerManager logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateProject(Project project)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", project.Id),
                        new SqlParameter("@Name", project.Name),
                        new SqlParameter("@Address", project.Address),
                        new SqlParameter("@State", project.State),
                        new SqlParameter("@Country", project.Country),
                        new SqlParameter("@StartDate", project.StartDate),
                        new SqlParameter("@EndDate", project.EndDate),
                        new SqlParameter("@CreatedAt", project.CreatedAt),
                        new SqlParameter("@UpdatedAt", project.UpdatedAt),
                    };

                    SqlCommand command = new("proc_CreateProject", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully Created a new project");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new project in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new project");
            }
        }
    }
}
