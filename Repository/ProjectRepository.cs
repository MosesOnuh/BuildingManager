using BuildingManager.Contracts.Repository;
using BuildingManager.Enums;
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
                        new SqlParameter("@UpdatedAt", DBNull.Value),
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


        //check if user is already a member of the project
        //public async Task<int> CreateProjectMembership(ProjectMember model)
        public async Task CreateProjectMembership(ProjectMember model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@UserId", model.UserId),
                        new SqlParameter("@Role", model.Role),
                        new SqlParameter("@Owner", (int)ProjectOwner.Owner),
                        new SqlParameter("@Profession", model.Profession),
                        new SqlParameter("@CreatedAt", model.CreatedAt),
                        new SqlParameter("@UpdatedAt", DBNull.Value),
                        //new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };
                    //if returned num is 1 then project membership already exist
                    //if 0 all is well
                    SqlCommand command = new("proc_CreateProjectMembership", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully created a new project membership");
                    //return (int)command.Parameters["@ResultCode"].Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new project member in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new project member");
            }
        }

        public async Task<IList<ProjectMemberDetails>> GetProjMemberDetails(string projectId, string userId)
        {
            List<ProjectMemberDetails> memberDetails = new();
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    SqlCommand command = new("proc_GetProjMemberDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            ProjectMemberDetails memberDetail = new ProjectMemberDetails
                            {
                                ProjectName = reader.GetString("Name"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                Role = reader.GetInt32("Role"),
                                Profession = reader.GetInt32("Profession"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                //UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                // UpdatedAt = await reader.IsDBNullAsync(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt"),

                            };

                            memberDetails.Add(memberDetail);
                        }
                    }
                }
                _logger.LogInfo("Successfully got project member details");
                return memberDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting project member details from DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting project member details");
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
                                Profession = reader.GetInt32("Profession"),
                                UserAccess = reader.GetInt32("UserAccess"),
                                ProjOwner = reader.GetInt32("ProjOwner"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                //UpdatedAt = reader.GetDateTime("UpdatedAt"),
                               // UpdatedAt = await reader.IsDBNullAsync(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt"),

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

        public async Task<IList<member>> GetProjectMembers(string projectId)
        {
            List<member> projMembers = new();
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    SqlCommand command = new("proc_GetProjMembers", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            member newMember = new member
                            {
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Email = reader.GetString("Email"),
                                PhoneNumber = reader.GetString("PhoneNumber"),
                                Role = reader.GetInt32("Role"),                              
                                Profession = reader.GetInt32("Profession"),
                                UserAccess = reader.GetInt32("UserAccess"),
                                ProjOwner = reader.GetInt32("ProjOwner"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                            };

                            projMembers.Add(newMember);
                        }
                    }
                }
                _logger.LogInfo("Successfully got project members");
                return projMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting project members from DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting project members");
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
                                Id = reader.GetString("Id"),
                                Name = reader.GetString("Name"),
                                Address = reader.GetString("Address"),
                                State = reader.GetString("State"),
                                Country = reader.GetString("Country"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt")
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
                int totalCount = 0;
                IList<ProjectDto> projects = new List<ProjectDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetMemberProjectsPaged";
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            
                            while (await reader.ReadAsync())
                            {
                                ProjectDto project = new ProjectDto
                                {
                                    Id = reader.GetString("Id"),
                                    Name = reader.GetString("Name"),
                                    Address = reader.GetString("Address"),
                                    State = reader.GetString("State"),
                                    Country = reader.GetString("Country"),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate")
                                };
                                projects.Add(project);
                            }
                           
                        }
                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to get projects");

                    }
                }

                return (totalCount, projects);
            }
           
            catch (Exception ex)
            {
                _logger.LogError($"Error getting projeCts by id and page {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting projects by id and page");
            }
        }

        public async Task<(int, int)> UpdateProjectUserAccessPm(ProjectAccessDto model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@UserId", model.UserId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not pending
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful
                   // SqlCommand command = new("proc_UpdateActivityApprovalStatus", connection)
                    SqlCommand command = new("proc_UpdateProjectUserAccessPm", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update project user access status");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating project user access status in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating project user access status");
            }
        }

        public async Task<(int, int)> UpdateProjectUserAccessOwner(ProjectAccessDto model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@UserId", model.UserId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not pending
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful
                    // SqlCommand command = new("proc_UpdateActivityApprovalStatus", connection)
                    SqlCommand command = new("proc_UpdateProjectUserAccessOwner", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update project user access status");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating project user access status in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating project user access status");
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
