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
        public async Task<int> CreateProjectMembership(ProjectMember model)
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
                        new SqlParameter("@Profession", model.Profession),
                        new SqlParameter("@CreatedAt", model.CreatedAt),
                        new SqlParameter("@UpdatedAt", DBNull.Value),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
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
                    return (int)command.Parameters["@ResultCode"].Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new project member in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new project member");
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
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                //UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                UpdatedAt = await reader.IsDBNullAsync(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt"),

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


        //ProjectRepository.model, userId){}
        public async Task<(int, int)> AcceptProjectInvite(ProjectInviteStatusUpdateDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                    new SqlParameter("@InviteNotificationId", model.InviteNotificationId),
                    new SqlParameter("@ProjectId", model.ProjectId),
                    new SqlParameter("@UserId", userId),
                    //new SqlParameter("@UpdatedStatus", model.StatusAction),
                    new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    new SqlParameter("@Success", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //R = 1, success = 0-- Invite to project not found for User
                    //R = 2, success = 0-- User has already accepted or rejected invite
                    //R = 0, success = 1  success
                    SqlCommand command = new("proc_AcceptProjectInvite", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to accept project invite");

                    return ((int)command.Parameters["@Success"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accepting project invit in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error accepting project invite");
            }
        }

        public async Task<(int, int)> RejectProjectInvite(ProjectInviteStatusUpdateDto model, string userId) { 
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@InviteNotificationId", model.InviteNotificationId),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@UserId", userId),
                        //new SqlParameter("@UpdatedStatus", model.StatusAction),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                     };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not pending
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful
                    SqlCommand command = new("proc_RejectProjectInvite", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to reject project invite");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting project invit in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error rejecting project invite");
            }
        }

        

        public async Task<(int, IList<InviteResponseDto>)> GetProjectInvites(ProjectInvitesDtoPaged model, string userId)
        {
            try
            {
                int totalCount = 0;
                IList<InviteResponseDto> invites = new List<InviteResponseDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetProjectInvitesPaged";
                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                InviteResponseDto invite = new InviteResponseDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    ProjectName = reader.GetString("ProjectName"),
                                    PmId = reader.GetString("PmId"),
                                    PmFirstName = reader.GetString("PmFirstName"),
                                    PmLastName = reader.GetString("PmLastName"),
                                    PmEmail = reader.GetString("PmEmail"),
                                    PmPhoneNum = reader.GetString("PmPhoneNum"),
                                    PmProjectProfession = reader.GetInt32("PmProjectProfession"),
                                    UserEmail = reader.GetString("UserEmail"),
                                    UserRole = reader.GetInt32("Role"),
                                    UserProfession = reader.GetInt32("Profession"),
                                    Status = reader.GetInt32("Status"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                };
                                invites.Add(invite);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to get project invites");
                    }
                }

                return (totalCount, invites);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting project invites {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting project invites");
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
