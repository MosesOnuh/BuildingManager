using BuildingManager.Contracts.Repository;
using BuildingManager.Helpers;
using BuildingManager.Models;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace BuildingManager.Repository
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;
        public ActivityRepository(IConfiguration configuration, ILoggerManager logger) 
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateActivity(Activity activity)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", activity.Id),
                        new SqlParameter("@ProjectId", activity.ProjectId),
                        new SqlParameter("@UserId", activity.UserId),
                        new SqlParameter("@Name", activity.Name),
                        new SqlParameter("@Status", activity.Status),
                        new SqlParameter("@Description", activity.Description),
                        new SqlParameter("@ProjectPhase", activity.ProjectPhase),
                        new SqlParameter("@FileName", activity.FileName == null? DBNull.Value : activity.FileName),
                        new SqlParameter("@StorageFileName", activity.StorageFileName == null? DBNull.Value : activity.StorageFileName),
                        new SqlParameter("@FileExtension", activity.FileExtension == null? DBNull.Value : activity.FileExtension),
                        new SqlParameter("@StartDate", activity.StartDate),
                        new SqlParameter("@EndDate", activity.EndDate),
                        new SqlParameter("@CreatedAt", activity.CreatedAt),
                    };

                    SqlCommand command = new("proc_CreateActivity", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to create a new activity");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new activity in DB {ex.StackTrace} {ex.Message}");
                //throw new Exception("Error creating new activity");
                throw;
            }
        }


        //write procedure to check the status of the activity and if the status is one then 
        // the status can be updated to 2 or 3
        public async Task<(int, int)> UpdateActivityApprovalStatus(ActivityStatusUpdateDto model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@ActivityId", model.ActivityId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not pending
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful
                    SqlCommand command = new("proc_UpdateActivityApprovalStatus", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    //int returnedNum = (int)command.Parameters["@ResultCode"].Value;
                    _logger.LogInfo("Successfully ran query to update activity approval status");
                    
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating activity approval status in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating activity approval status");
            }
        }

        //write procedure to check the status of the activity and if the status is two then and
        //there is start date and finish date
        // the status can be updated to 4
        public async Task<(int, int)> UpdateActivityToDone(ActivityStatusUpdateDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@ActivityId", model.ActivityId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UserId", userId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not approved
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 0  if the activity does not have an ActualStartDate or ActualEndDate
                    //procedure will return @ResultCode = 3, , @RowsUpdated = 1  if the activity status has been updated to done 
                    SqlCommand command = new("proc_UpdateActivityToDone", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to change activity status to done");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating activity status to done in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating activity status to done");
            }
        }


        public async Task<(int, int)> UpdateActivityActualDates(ActivityActualDatesDto model, string userId) 
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@ActivityId", model.ActivityId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UserId", userId),
                            new SqlParameter("@ActualStartDate", model.ActualStartDate),
                            new SqlParameter("@ActualEndDate", model.ActualEndDate),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity does not have actual start date and actual end date
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 1  if the activity actual start date and actual end date has been updated
                    SqlCommand command = new("proc_UpdateActivityActualDates", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to change activity status to done");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating activity status to done in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating activity status to done");
            }
        }

        public async Task<(int, int)> UpdatePendingActivity(UpdateActivityDetailsDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", model.ActivityId),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@Name", model.Name),
                        new SqlParameter("@Description", model.Description),
                        new SqlParameter("@ProjectPhase", model.ProjectPhase),
                        new SqlParameter("@StartDate", model.StartDate),
                        new SqlParameter("@EndDate", model.EndDate),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //check if the activity belongs to the project with the userId passed-- This is done in the controller
                    //Procedure should ensure that only pending activity can be updated and it is for the user

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 1  if the activity has been updated.
                    SqlCommand command = new("proc_UpdatePendingActivityDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update pending activity");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the details of an activity in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the details of an activity");
            }
        }

        public async Task<(int, int)> AddActivityFile(AddActivityFileDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", model.ActivityId),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@FileName", model.FileName),
                        new SqlParameter("@StorageFileName", model.StorageFileName),
                        new SqlParameter("@FileExtension", model.FileExtension),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 0  if the activity already has file 
                    //procedure will return @ResultCode = 3, , @RowsUpdated = 1  if the activity file have been updated 
                    SqlCommand command = new("proc_AddActivityFileDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update pending activity file");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of an activity in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of an activity");
            }
        }

        public async Task<(int, int)> RemoveActivityFileDetails(string projId, string activityId, string userId) 
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", activityId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 1  if the activity file details have been removed 
                    SqlCommand command = new("proc_RemoveActivityFileDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to remove activity file details");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of an activity in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of an activity");
            }
        }

        public async Task<Activity> GetActivityOtherPro(string projId, string activityId, string userId)
        {
            Activity? activity = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", activityId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                    };


                    SqlCommand command = new("proc_GetActivity", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            activity = new Activity
                            {
                                Id = reader.GetString("Id"),
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                Name = reader.GetString("Name"),
                                Status = reader.GetInt32("Status"),
                                Description = reader.GetString("Description"),
                                ProjectPhase = reader.GetInt32("ProjectPhase"),
                                FileName = await reader.IsDBNullAsync(reader.GetOrdinal("FileName")) ? null : reader.GetString("FileName"),
                                StorageFileName = await reader.IsDBNullAsync(reader.GetOrdinal("StorageFileName")) ? null : reader.GetString("StorageFileName"),
                                FileExtension = await reader.IsDBNullAsync(reader.GetOrdinal("FileExtension")) ? null : reader.GetString("FileExtension"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                ActualStartDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualStartDate")) ? null : reader.GetDateTime("ActualStartDate"),
                                ActualEndDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualEndDate")) ? null : reader.GetDateTime("ActualEndDate"),
                                CreatedAt = reader.GetDateTime("CreatedAt")
                            };
                        }
                    }
                }

                _logger.LogInfo("Successfully ran query to get activity");

                if (activity != null)
                {
                    _logger.LogInfo("Successfully got activity");
                }

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activity {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activity");
            }
        }


        //@todo: Procedure for this function is not yet created
        public async Task<ActivityAndMemberDto> GetActivityPM(string projId, string activityId)
        {
            ActivityAndMemberDto? activity = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", activityId),
                        new SqlParameter("@ProjectId", projId),
                    };


                    SqlCommand command = new("proc_GetActivityPM", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            activity = new ActivityAndMemberDto
                            {
                                UserId = reader.GetString("UserId"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Role = reader.GetInt32("Role"),
                                Profession = reader.GetInt32("Profession"),
                                ProjectId = reader.GetString("ProjectId"),
                                ActivityId = reader.GetString("ActivityId"),
                                ActivityName = reader.GetString("ActivityName"),
                                Status = reader.GetInt32("Status"),
                                Description = reader.GetString("Description"),
                                ProjectPhase = reader.GetInt32("ProjectPhase"),
                                FileName = await reader.IsDBNullAsync(reader.GetOrdinal("FileName")) ? null : reader.GetString("FileName"),
                                StorageFileName = await reader.IsDBNullAsync(reader.GetOrdinal("StorageFileName")) ? null : reader.GetString("StorageFileName"),
                                //FileType = await reader.IsDBNullAsync(reader.GetOrdinal("FileExtension")) ? null : reader.GetString("FileExtension"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                ActualStartDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualStartDate")) ? null : reader.GetDateTime("ActualStartDate"),
                                ActualEndDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualEndDate")) ? null : reader.GetDateTime("ActualEndDate"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                            };
                        }
                    }
                }

                if (activity != null)
                {
                    _logger.LogInfo("Successfully got activity");
                }

                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activity {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activity");
            }
        }

        public async Task<(int, int)> DeleteActivity(string projId, string activityId, string userId) 
        {
            try {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@ActivityId", activityId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsDeleted", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsDeleted = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsDeleted = 0  if the activity is not rejected
                    //procedure will return @ResultCode = 2, , @RowsDeleted = 1  if the activity was successfully deleted 
                    SqlCommand command = new("proc_DeleteRejectedActivity", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to delete pending activity");
                    return ((int)command.Parameters["@RowsDeleted"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting an activity in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting an activity");
            }
        }     

        public async Task<(int, IList<ActivityDto>)> GetProjectPhaseActivitiesOtherPro(ActivitiesDtoPaged model, string userId)
        {
            try 
            {
                int totalCount = 0;
                IList<ActivityDto> activities = new List<ActivityDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetProjectPhaseActivitiesPaged";

                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@ProjectPhase", model.ProjectPhase);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ActivityDto activity = new ActivityDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    UserId = reader.GetString("UserId"),
                                    Name = reader.GetString("Name"),
                                    Status = reader.GetInt32("Status"),
                                    Description = reader.GetString("Description"),
                                    ProjectPhase = reader.GetInt32("ProjectPhase"),
                                    FileName = reader.IsDBNull(reader.GetOrdinal("FileName")) ? null : reader.GetString("FileName"),
                                    StorageFileName = reader.IsDBNull(reader.GetOrdinal("StorageFileName")) ? null : reader.GetString("StorageFileName"),
                                    //FileExtension = reader.IsDBNull(reader.GetOrdinal("FileExtension")) ? null : reader.GetString("FileExtension"),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate"),
                                    ActualStartDate = reader.IsDBNull(reader.GetOrdinal("ActualStartDate")) ? null : reader.GetDateTime("ActualStartDate"),
                                    ActualEndDate = reader.IsDBNull(reader.GetOrdinal("ActualEndDate")) ? null : reader.GetDateTime("ActualEndDate"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
                                activities.Add(activity);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to activity per phase");
                    }
                }

                return (totalCount, activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activities per phase {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activities per phase");
            }           
        }

        public async Task<(int, IList<ActivityAndMemberDto>)> GetProjectPhaseActivitiesPM(ActivitiesDtoPaged model)
        {
            try
            {
                int totalCount = 0;
                IList<ActivityAndMemberDto> activities = new List<ActivityAndMemberDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetProjectPhaseActivitiesPagedPM";

                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        command.Parameters.AddWithValue("@ProjectPhase", model.ProjectPhase);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ActivityAndMemberDto activity = new ActivityAndMemberDto
                                {
                                    UserId = reader.GetString("UserId"),
                                    FirstName = reader.GetString("FirstName"),
                                    LastName = reader.GetString("LastName"),
                                    Role = reader.GetInt32("Role"),
                                    Profession = reader.GetInt32("Profession"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    ActivityId = reader.GetString("ActivityId"),
                                    ActivityName = reader.GetString("ActivityName"),
                                    Status = reader.GetInt32("Status"),
                                    Description = reader.GetString("Description"),
                                    ProjectPhase = reader.GetInt32("ProjectPhase"),
                                    FileName = await reader.IsDBNullAsync(reader.GetOrdinal("FileName")) ? null : reader.GetString("FileName"),
                                    StorageFileName = await reader.IsDBNullAsync(reader.GetOrdinal("StorageFileName")) ? null : reader.GetString("StorageFileName"),
                                    //FileType = await reader.IsDBNullAsync(reader.GetOrdinal("FileExtension")) ? null : reader.GetString("FileExtension"),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate"),
                                    ActualStartDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualStartDate")) ? null : reader.GetDateTime("ActualStartDate"),
                                    ActualEndDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualEndDate")) ? null : reader.GetDateTime("ActualEndDate"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                };
                                activities.Add(activity);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to activity per phase");
                    }
                }

                return (totalCount, activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activities per phase {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activities per phase");
            }
        }



        public async Task<(int, IList<ActivityAndMemberDto>)> GetProjectActivities(string projectId)
        {
            try
            {
                int totalCount = 0;
                IList<ActivityAndMemberDto> activities = new List<ActivityAndMemberDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        //command.CommandText = "proc_GetProjectPhaseActivitiesPagedPM";
                        command.CommandText = "proc_GetProjectActivities";

                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        //command.Parameters.AddWithValue("@ProjectPhase", model.ProjectPhase);
                        //command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        //command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ActivityAndMemberDto activity = new ActivityAndMemberDto
                                {
                                    UserId = reader.GetString("UserId"),
                                    FirstName = reader.GetString("FirstName"),
                                    LastName = reader.GetString("LastName"),
                                    Role = reader.GetInt32("Role"),
                                    Profession = reader.GetInt32("Profession"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    ActivityId = reader.GetString("ActivityId"),
                                    ActivityName = reader.GetString("ActivityName"),
                                    Status = reader.GetInt32("Status"),
                                    Description = reader.GetString("Description"),
                                    ProjectPhase = reader.GetInt32("ProjectPhase"),
                                    FileName = await reader.IsDBNullAsync(reader.GetOrdinal("FileName")) ? null : reader.GetString("FileName"),
                                    StorageFileName = await reader.IsDBNullAsync(reader.GetOrdinal("StorageFileName")) ? null : reader.GetString("StorageFileName"),
                                    //FileType = await reader.IsDBNullAsync(reader.GetOrdinal("FileExtension")) ? null : reader.GetString("FileExtension"),
                                    StartDate = reader.GetDateTime("StartDate"),
                                    EndDate = reader.GetDateTime("EndDate"),
                                    ActualStartDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualStartDate")) ? null : reader.GetDateTime("ActualStartDate"),
                                    ActualEndDate = await reader.IsDBNullAsync(reader.GetOrdinal("ActualEndDate")) ? null : reader.GetDateTime("ActualEndDate"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                };
                                activities.Add(activity);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                        _logger.LogInfo("Successfully ran query to activities");
                    }
                }

                return (totalCount, activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting activities {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activities");
            }
        }
    }
}
        
