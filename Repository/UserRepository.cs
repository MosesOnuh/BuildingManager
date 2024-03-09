using BuildingManager.Contracts.Repository;
using BuildingManager.Models;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BuildingManager.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString; 

        public UserRepository(IConfiguration configuration, ILoggerManager logger) 
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> CheckEmailExists(string userEmail)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new("proc_checkEmailExists", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@UserEmail", userEmail);

                    await connection.OpenAsync();

                    //Execute the stored procedure and get the result
                    var result = await command.ExecuteScalarAsync();

                    if (result != null)
                    {
                        if ((int)result == 1) 
                        { 
                            return true; 
                        }
                        else {
                            return false; 
                        }
                    }
                    else
                    {
                        // Invalid result returned from the stored procedure
                        _logger.LogError($"Error invalid result returned from proc_checkEmailExists for {userEmail}");
                        throw new Exception("Error checking if user email exist");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error at checkEmailExists procedure with {userEmail} {ex.StackTrace} {ex.Message}");
                throw;
            }
        }

        public async Task SignUp(User user)
        {
            try 
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@Id", user.Id),
                        new SqlParameter("@FirstName", user.FirstName),
                        new SqlParameter("@LastName", user.LastName),
                        new SqlParameter("@Email", user.Email),
                        new SqlParameter("@PhoneNumber", user.PhoneNumber),
                        new SqlParameter("@Password", user.Password),
                        new SqlParameter("@CreatedAt", user.CreatedAt),
                        new SqlParameter("@UpdatedAt", user.UpdatedAt),
                        new SqlParameter("@EmailVerified", user.EmailVerified)
                    };

                    SqlCommand command = new("proc_CreateUserAccount", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully Created a new user");

                }
            } catch (Exception ex) 
            {
                _logger.LogError($"Error inserting new user in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error creating new user account");
            }
        }

        public async Task<User?> GetUserByEmail (string userEmail) 
        {
            User? user = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    

                    SqlCommand command = new("proc_GetUserByEmail", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddWithValue("@UserEmail", userEmail); ;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetString(0),
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Email = reader.GetString(3),
                                PhoneNumber = reader.GetString(4),
                                Password = reader.GetString(5),
                                CreatedAt = reader.GetDateTime(6),
                                UpdatedAt = reader.GetDateTime(7),
                                EmailVerified = reader.GetInt32(8),
                            }; 
                        }
                    }
                }

                if (user != null) {
                    _logger.LogInfo("Successfully got user");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting user by email");
            }
        }
    }
}
