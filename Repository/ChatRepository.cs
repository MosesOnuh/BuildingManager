using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using BuildingManager.Models.Dto;
using System.Collections.Generic;
using System.Data;
using BuildingManager.Models.Entities;
using BuildingManager.Contracts.Repository;
using Newtonsoft.Json;

namespace BuildingManager.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;

        public ChatRepository(IConfiguration configuration, ILoggerManager logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreateGroupChatMessage(ChatMessage model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", model.Id),
                        new SqlParameter("@ProjectId", model.GroupId),
                        new SqlParameter("@UserId", model.UserId),
                        new SqlParameter("@SenderName", model.From),
                        new SqlParameter("@Profession", model.Profession),
                        new SqlParameter("@Content", model.Content),                       
                        new SqlParameter("@CreatedAt", model.CreatedAt),
                    };

                    SqlCommand command = new("proc_CreateUserGroupMessage", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to insert a group chat message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new group chat message in DB {ex.StackTrace} {ex.Message}");
                //_logger.LogError($"Error object ==> {JsonConvert.SerializeObject(model)} {ex.Message}");
                throw;
            }
        }


        //check in the controller and ensure that the user is a member of the project geting the messages
        public async Task<IList<ChatMessage>> GetGroupChatMessages (string groupId)
        {
            try
            {
                IList<ChatMessage> messages = new List<ChatMessage>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetGroupChatMessages";
                        command.Parameters.AddWithValue("@ProjectId", groupId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                //inner join invite notification tb, user tb,  
                                ChatMessage message = new ChatMessage
                                {
                                    Id = reader.GetString("Id"),
                                    GroupId = reader.GetString("ProjectId"),
                                    UserId = reader.GetString("UserId"),
                                    From = reader.GetString("SenderName"),
                                    Profession = reader.GetInt32("Profession"),
                                    Content = reader.GetString("Content"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };
                                messages.Add(message);
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get group chat messages");
                    }
                }

                return (messages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting group chat messages {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting group chat messages");
            }
        }

    }
}
