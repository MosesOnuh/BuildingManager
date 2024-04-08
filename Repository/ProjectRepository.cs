using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
                    _logger.LogInfo("Successfully created a new project");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new project in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new project");
            }
        }


        public async Task<IList<ProjectMember>> GetProjectMemberInfo(string projectId)
        {
            List<ProjectMember> roleDetails = new();
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    SqlCommand command = new("proc_GetProjectMemberInfo", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            ProjectMember memberDetails = new ProjectMember
                            {
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                Role = reader.GetInt32("Role"),
                                Profession = reader.GetString("Profession"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                UpdatedAt = reader.GetDateTime("UpdatedAt"),

                            };

                            roleDetails.Add(memberDetails);
                        }
                    }
                }
                _logger.LogInfo("Successfully got project member details");
                return roleDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting project member details from DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting project member details");
            }
        }

        public async Task<Project?> GetProject(string projectId)
        {
            Project? project = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {


                    SqlCommand command = new("proc_GetProjectById", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@ProjectId", projectId); ;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            project = new Project
                            {
                                Id = reader.GetString(0),
                                Name = reader.GetString(1),
                                Address = reader.GetString(2),
                                State = reader.GetString(3),
                                Country = reader.GetString(4),
                                StartDate = reader.GetDateTime(5),
                                EndDate = reader.GetDateTime(6),
                                CreatedAt = reader.GetDateTime(7),
                                UpdatedAt = reader.GetDateTime(8),
                            };
                        }
                    }
                }

                if (project != null)
                {
                    _logger.LogInfo("Successfully got project");
                }

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting projeCt by id {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting project by id");
            }
        }


        public async Task UpdateProject(ProjectDto project)
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
                        new SqlParameter("@UpdatedAt", DateTime.Now)
                    };

                    SqlCommand command = new("proc_UpdateProject", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully updated a project");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating a project in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating a project");
            }
        }

        public async Task<(int, IList<ProjectDto>)> GetProjectsPaged(string userId, int pageNumber, int pageSize)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@PageNumber", pageNumber),
                        new SqlParameter("@PageSize", pageSize),
                        new SqlParameter("@TotalCount", SqlDbType.Int) {Direction = ParameterDirection.Output},
                    };

                    using (SqlCommand command = new SqlCommand("proc_GetProjectsPaged"))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(parameters);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            List<ProjectDto> projects = new List<ProjectDto>();
                            while (await reader.ReadAsync())
                            {
                                projects.Add(new ProjectDto
                                {
                                    Id = reader.GetString("Id"),
                                    Name = reader.GetString("Name"),
                                    Address = reader.GetString("Address"),
                                    State = reader.GetString("State"),
                                    Country = reader.GetString("Country"),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate")
                                });
                            }

                            int totalCount = (int)command.Parameters["@TotalCount"].Value;

                            return (totalCount, projects);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting projeCts by id and page {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting projects by id and page");
            }
        }
    }
}


//Create Procedure for Get Projects Paged
//CREATE PROCEDURE GetMemberProjectsPaged
//    @UserId int,
//    @PageNumber int,
//    @PageSize int,
//    @TotalCount OUT int
//AS
//BEGIN
//    SET NOCOUNT OFF;

//--Get total count of projects the user is a member of
//    SELECT @TotalCount = COUNT(DISTINCT p.ProjectId)
//    FROM Projects p
//    INNER JOIN Members m ON p.ProjectId = m.ProjectId
//    WHERE m.UserId = @UserId;

//--Select paginated project data
//    SELECT p.*
//    FROM Projects p
//    INNER JOIN Members m ON p.ProjectId = m.ProjectId
//    WHERE m.UserId = @UserId
//    ORDER BY p.ProjectName
//    OFFSET (@PageNumber - 1) *@PageSize ROWS
//   FETCH NEXT @PageSize ROWS ONLY;
//END
