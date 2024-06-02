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

        public async Task<(int, int)> RejectProjectInvite(ProjectInviteStatusUpdateDto model, string userId)
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

        //Task<IList<ReceivedInviteRespDto>> GetReceivedProjectInvites(string userId) { }
        //public async Task<(int, IList<ActivityDto>)> GetProjectPhaseActivitiesOtherPro(ActivitiesDtoPaged model, string userId)
        public async Task<IList<ReceivedInviteRespDto>> GetReceivedProjectInvites(string userId)
        {
            try
            {
                IList<ReceivedInviteRespDto> invites = new List<ReceivedInviteRespDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetReceivedProjectInvites";
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                //inner join invite notification tb, user tb,  
                                ReceivedInviteRespDto invite = new ReceivedInviteRespDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    ProjectName = reader.GetString("ProjectName"),
                                    PmId = reader.GetString("PmId"),
                                    PmFirstName = reader.GetString("PmFirstName"),
                                    PmLastName = reader.GetString("PmLastName"),
                                    UserEmail = reader.GetString("Email"),
                                    UserRole = reader.GetInt32("Role"),
                                    UserProfession = reader.GetInt32("Profession"),
                                    Status = reader.GetInt32("Status"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
                                invites.Add(invite);
                            }
                        }

                        _logger.LogInfo("Successfully ran query to received invites");
                    }
                }

                return (invites);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting received invites {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting received invites");
            }
        }

        public async Task<(int, IList<SentInviteRespDto>)> GetSentProjectInvites(SentProjInvitesDtoPaged model)
        {
            try
            {
                int totalCount = 0;
                IList<SentInviteRespDto> invites = new List<SentInviteRespDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetSentProjectInvitesPaged";
                        command.Parameters.AddWithValue("@PmId", model.PmId);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SentInviteRespDto invite = new SentInviteRespDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    ProjectName = reader.GetString("ProjectName"),
                                    PmId = reader.GetString("PmId"),
                                    UserFirstName = reader.GetString("UserFirstName"),
                                    UserLastName = reader.GetString("UserLastName"),
                                    //PmPhoneNum = reader.GetString("PmPhoneNum"),
                                    //PmProjectProfession = reader.GetInt32("PmProjectProfession"),
                                    UserEmail = reader.GetString("UserEmail"),
                                    UserRole = reader.GetInt32("UserRole"),
                                    UserProfession = reader.GetInt32("UserProfession"),
                                    Status = reader.GetInt32("Status"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = await reader.IsDBNullAsync(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime("UpdatedAt"),
                                };
                                invites.Add(invite);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to get sent project invites");
                    }
                }

                return (totalCount, invites);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting sent project invites {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting sent project invites");
            }
        }



        //public async Task<(int, IList<InviteResponseDto>)> GetProjectInvites(ProjectInvitesDtoPaged model, string userId)
        //{
        //    try
        //    {
        //        int totalCount = 0;
        //        IList<InviteResponseDto> invites = new List<InviteResponseDto>();

        //        using (SqlConnection connection = new SqlConnection(_connectionString))
        //        {
        //            await connection.OpenAsync();

        //            using (SqlCommand command = connection.CreateCommand())
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.CommandText = "proc_GetProjectInvitesPaged";
        //                command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
        //                command.Parameters.AddWithValue("@UserId", userId);
        //                command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
        //                command.Parameters.AddWithValue("@PageSize", model.PageSize);

        //                SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
        //                totalCountParameter.Direction = ParameterDirection.Output;
        //                command.Parameters.Add(totalCountParameter);

        //                using (SqlDataReader reader = await command.ExecuteReaderAsync())
        //                {
        //                    while (await reader.ReadAsync())
        //                    {
        //                        InviteResponseDto invite = new InviteResponseDto
        //                        {
        //                            Id = reader.GetString("Id"),
        //                            ProjectId = reader.GetString("ProjectId"),
        //                            ProjectName = reader.GetString("ProjectName"),
        //                            PmId = reader.GetString("PmId"),
        //                            PmFirstName = reader.GetString("PmFirstName"),
        //                            PmLastName = reader.GetString("PmLastName"),
        //                            PmEmail = reader.GetString("PmEmail"),
        //                            PmPhoneNum = reader.GetString("PmPhoneNum"),
        //                            PmProjectProfession = reader.GetInt32("PmProjectProfession"),
        //                            UserEmail = reader.GetString("UserEmail"),
        //                            UserRole = reader.GetInt32("Role"),
        //                            UserProfession = reader.GetInt32("Profession"),
        //                            Status = reader.GetInt32("Status"),
        //                            CreatedAt = reader.GetDateTime("CreatedAt"),
        //                        };
        //                        invites.Add(invite);
        //                    }
        //                }

        //                totalCount = (int)command.Parameters["@TotalCount"].Value;
        //                _logger.LogInfo("Successfully ran query to get project invites");
        //            }
        //        }

        //        return (totalCount, invites);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error getting project invites {ex.StackTrace} {ex.Message}");
        //        throw new Exception("Error getting project invites");
        //    }
        //}
    }
}