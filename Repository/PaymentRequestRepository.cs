using BuildingManager.Contracts.Repository;
using BuildingManager.Models.Entities;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using BuildingManager.Models.Dto;
using System.Collections.Generic;
using System.Linq;
using BuildingManager.Models;
using BuildingManager.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BuildingManager.Repository
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly ILoggerManager _logger;
        private readonly string _connectionString;

        public PaymentRequestRepository(IConfiguration configuration, ILoggerManager logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CreatePaymentRequest (PaymentRequest paymentRequest)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            var parameters = new[]
                            {
                                new SqlParameter("@Id", paymentRequest.Id),
                                new SqlParameter("@ProjectId", paymentRequest.ProjectId),
                                new SqlParameter("@UserId", paymentRequest.UserId),
                                new SqlParameter("@CreatedBy", paymentRequest.CreatedBy),
                                new SqlParameter("@Name", paymentRequest.Name),
                                new SqlParameter("@Status", paymentRequest.Status),
                                new SqlParameter("@Type", paymentRequest.Type),
                                new SqlParameter("@Description", paymentRequest.Description),
                                new SqlParameter("@SumTotalAmount", paymentRequest.SumTotalAmount),
                                new SqlParameter("@UserFileName", paymentRequest.UserFileName ?? (object)DBNull.Value ),
                                new SqlParameter("@UserStorageFileName", paymentRequest.UserStorageFileName ?? (object)DBNull.Value),
                                //new SqlParameter("@FileExtension", paymentRequest.UserFileExtension == null? DBNull.Value : paymentRequest.UserFileExtension),
                                new SqlParameter("@CreatedAt", paymentRequest.CreatedAt),
                            };

                            SqlCommand PayReqCommand = new("proc_CreatePaymentRequest", connection, transaction)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            PayReqCommand.Parameters.AddRange(parameters);

                            //Add Payment Request to the Db
                            await PayReqCommand.ExecuteNonQueryAsync();

                            //Create DataTable for bulk insert
                            DataTable itemsTable = new DataTable();
                            itemsTable.Columns.Add("Id", typeof(string));
                            itemsTable.Columns.Add("PrId", typeof(string));
                            itemsTable.Columns.Add("ProjectId", typeof(string));
                            itemsTable.Columns.Add("UserId", typeof(string));
                            itemsTable.Columns.Add("Name", typeof(string));
                            itemsTable.Columns.Add("Price", typeof(decimal));
                            itemsTable.Columns.Add("Quantity", typeof(decimal));
                            itemsTable.Columns.Add("TotalAmount", typeof(decimal));
                            itemsTable.Columns.Add("CreatedAt", typeof(DateTime));

                            //Populate DataTable with items
                            foreach (var item in paymentRequest.Items)
                            {
                                itemsTable.Rows.Add(
                                    item.Id,
                                    item.PaymentRequestId,
                                    item.ProjectId,
                                    item.UserId,
                                    item.Name,
                                    item.Price,
                                    item.Quantity,
                                    item.TotalAmount,
                                    paymentRequest.CreatedAt
                                    );
                            }

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.DestinationTableName = "PaymentRequestItem";
                                bulkCopy.BulkCopyTimeout = 600;

                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("PrId", "PrId");
                                bulkCopy.ColumnMappings.Add("ProjectId", "ProjectId");
                                bulkCopy.ColumnMappings.Add("UserId", "UserId");
                                bulkCopy.ColumnMappings.Add("Name", "Name");
                                bulkCopy.ColumnMappings.Add("Price", "Price");
                                bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                                bulkCopy.ColumnMappings.Add("TotalAmount", "TotalAmount");
                                bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");

                                await bulkCopy.WriteToServerAsync(itemsTable);
                            }

                            await transaction.CommitAsync();

                            _logger.LogInfo("Successfully ran query to create a new payment request");
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting new payment request in DB {ex.StackTrace} {ex.Message}");
                throw;
            }
        }


        public async Task<(int, IList<PaymentRequestDto>)> GetPaymentRequestsOtherPro(PaymentRequestReqPagedDto model, string userId)
        {
            try
            {
                int totalCount = 0;
                IList<PaymentRequestDto> paymentRequests = new List<PaymentRequestDto>();
                //IList<PaymentRequestItemDto> paymentRequestItems = new List<PaymentRequestItemDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetPaymentRequestPaged";

                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@RequiredStatus", model.RequiredStatus);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PaymentRequestDto payReq = new PaymentRequestDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    UserId = reader.GetString("UserId"),
                                    Name = reader.GetString("Name"),
                                    Status = reader.GetInt32("Status"),
                                    Type = reader.GetInt32("Type"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                    SumTotalAmount = reader.GetDecimal("SumTotalAmount"),
                                    UserFileName = reader.IsDBNull(reader.GetOrdinal("UserFileName")) ? null : reader.GetString("UserFileName"),
                                    UserStorageFileName = reader.IsDBNull(reader.GetOrdinal("UserStorageFileName")) ? null : reader.GetString("UserStorageFileName"),
                                    PmFileName = reader.IsDBNull(reader.GetOrdinal("PmFileName")) ? null : reader.GetString("PmFileName"),
                                    PmStorageFileName = reader.IsDBNull(reader.GetOrdinal("PmStorageFileName")) ? null : reader.GetString("PmStorageFileName"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    ConfirmedAt = reader.IsDBNull(reader.GetOrdinal("ConfirmedAt")) ? null : reader.GetDateTime("ConfirmedAt")
                                   
                                };
                                paymentRequests.Add(payReq);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                    }


                    using (SqlCommand PayReqItemCommand = connection.CreateCommand())
                    {
                        PayReqItemCommand.CommandType = CommandType.StoredProcedure;
                        PayReqItemCommand.CommandText = "proc_GetPaymentRequestItems";
                        PayReqItemCommand.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        PayReqItemCommand.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = await PayReqItemCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PaymentRequestItemDto payReqItem = new PaymentRequestItemDto
                                {
                                    Id = reader.GetString("Id"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    PaymentRequestId = reader.GetString("PrId"),
                                    Name = reader.GetString("Name"),
                                    Price = reader.GetDecimal("Price"),
                                    Quantity = reader.GetDecimal("Quantity"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                };

                                var paymentRequest = paymentRequests.FirstOrDefault(pr => pr.Id == payReqItem.PaymentRequestId);
                                if (paymentRequest != null)
                                {
                                    paymentRequest.Items.Add(payReqItem);
                                }
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get payment request");
                    }
                }

                return (totalCount, paymentRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment requests {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment requests  per phase");
            }
        }

        public async Task<(int, IList<PaymentRequestAndMemberDto>)> GetPaymentRequestsPM(PaymentRequestReqPagedDto model)
        {
            try
            {
                int totalCount = 0;
                IList<PaymentRequestAndMemberDto> paymentRequests = new List<PaymentRequestAndMemberDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetPaymentRequestPagedPM";

                        command.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        //command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@RequiredStatus", model.RequiredStatus);
                        command.Parameters.AddWithValue("@PageNumber", model.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", model.PageSize);

                        SqlParameter totalCountParameter = new SqlParameter("@TotalCount", SqlDbType.Int);
                        totalCountParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(totalCountParameter);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PaymentRequestAndMemberDto payReq = new PaymentRequestAndMemberDto
                                {
                                    UserId = reader.GetString("UserId"),
                                    FirstName = reader.GetString("FirstName"),
                                    LastName = reader.GetString("LastName"),
                                    Role = reader.GetInt32("Role"),
                                    Profession = reader.GetInt32("Profession"),
                                    PaymentRequestId = reader.GetString("PaymentRequestId"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    PaymentRequestName = reader.GetString("PaymentRequestName"),
                                    Status = reader.GetInt32("Status"),
                                    Type = reader.GetInt32("Type"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                    SumTotalAmount = reader.GetDecimal("SumTotalAmount"),
                                    UserFileName = reader.IsDBNull(reader.GetOrdinal("UserFileName")) ? null : reader.GetString("UserFileName"),
                                    UserStorageFileName = reader.IsDBNull(reader.GetOrdinal("UserStorageFileName")) ? null : reader.GetString("UserStorageFileName"),
                                    PmFileName = reader.IsDBNull(reader.GetOrdinal("PmFileName")) ? null : reader.GetString("PmFileName"),
                                    PmStorageFileName = reader.IsDBNull(reader.GetOrdinal("PmStorageFileName")) ? null : reader.GetString("PmStorageFileName"),
                                    CreatedAt = reader.GetDateTime("CreatedAt"),
                                    ConfirmedAt = reader.IsDBNull(reader.GetOrdinal("ConfirmedAt")) ? null : reader.GetDateTime("ConfirmedAt")

                                };
                                paymentRequests.Add(payReq);
                            }
                        }

                        totalCount = (int)command.Parameters["@TotalCount"].Value;
                    }


                    using (SqlCommand PayReqItemCommand = connection.CreateCommand())
                    {
                        PayReqItemCommand.CommandType = CommandType.StoredProcedure;
                        PayReqItemCommand.CommandText = "proc_GetPaymentRequestItemsPM";

                        PayReqItemCommand.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                        //PayReqItemCommand.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = await PayReqItemCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PaymentRequestItemDto payReqItem = new PaymentRequestItemDto
                                {
                                    Id = reader.GetString("Id"),
                                    PaymentRequestId = reader.GetString("PrId"),
                                    //UserId = reader.GetString("UserId"),
                                    ProjectId = reader.GetString("ProjectId"),
                                    Name = reader.GetString("Name"),
                                    Price = reader.GetDecimal("Price"),
                                    Quantity = reader.GetDecimal("Quantity"),
                                    TotalAmount = reader.GetDecimal("TotalAmount"),
                                    CreatedAt = reader.GetDateTime("CreatedAt")
                                };

                                //var paymentRequest = paymentRequests.First(pr => pr.PaymentRequestId == payReqItem.PaymentRequestId && pr.UserId == payReqItem.UserId);
                                var paymentRequest = paymentRequests.FirstOrDefault(pr => pr.PaymentRequestId == payReqItem.PaymentRequestId);
                                

                                if (paymentRequest != null)
                                {
                                    paymentRequest.Items.Add(payReqItem);
                                }
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get payment request");
                    }
                }

                return (totalCount, paymentRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment requests  {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment requests ");
            }
        }

        public async Task<(int, int)> UpdatePaymentConfirmationStatus(PaymentRequestStatusUpdateDto model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@PaymentRequestId", model.PaymentRequestId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ConfirmedAt", DateTime.Now),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not awaiting confirmation
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful

                    //SqlCommand command = new("proc_UpdateActivityApprovalStatus", connection)

                    SqlCommand command = new("proc_UpdatePaymentRequestConfirmationStatus", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update payment request confirmation status");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating payment request confirmation status in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating payment request confirmationstatus");
            }
        }

        public async Task<(int, int)> UpdatePendingPaymentRequest(UpdatePaymentRequestDto model, string userId, List<string> itemIdsToDelete)
        {
            try 
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    int rowsUpdated;
                    int resultCode;
                    DateTime timeStamp = DateTime.Now;
                    await connection.OpenAsync();
                    using (SqlTransaction transaction = (SqlTransaction) await connection.BeginTransactionAsync()) 
                    {
                        try 
                        {
                            var parameters = new[]
                           {
                                new SqlParameter("@PaymentRequestId", model.Id),
                                new SqlParameter("@ProjectId", model.ProjectId),
                                new SqlParameter("@UserId", userId),
                                new SqlParameter("@Name", model.Name),
                                new SqlParameter("@Description", model.Description ?? (object)DBNull.Value ),
                                new SqlParameter("@SumTotalAmount", model.SumTotalAmount),
                                new SqlParameter("@UpdatedAt", timeStamp),
                                new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                                new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            };

                            SqlCommand PayReqCommand = new("proc_UpdatePendingPaymentRequest", connection, transaction)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            PayReqCommand.Parameters.AddRange(parameters);

                            //Add Payment Request to the Db
                            await PayReqCommand.ExecuteNonQueryAsync();
                            rowsUpdated = (int)PayReqCommand.Parameters["@RowsUpdated"].Value;
                            resultCode = (int)PayReqCommand.Parameters["@ResultCode"].Value;

                            if (rowsUpdated == 1 && resultCode == 2 && model.Items?.Count > 0) 
                            {
                                // Deleting specified items from the database
                                if (itemIdsToDelete?.Count > 0)
                                {
                                    string deleteQuery = $"DELETE FROM PaymentRequestItem WHERE Id IN ({string.Join(",", itemIdsToDelete.Select(id => $"'{id}'"))})";
                                    SqlCommand deleteCommand = new(deleteQuery, connection, transaction);
                                    await deleteCommand.ExecuteNonQueryAsync();
                                }

                                //SqlCommand PayReqItemsCommand = new("SELECT Id FROM PaymentRequestItem", connection, transaction);
                                SqlCommand PayReqItemsCommand = new SqlCommand( "SELECT Id FROM PaymentRequestItem WHERE ProjectId = @ProjectId AND UserId = @UserId", connection, transaction);
                                PayReqItemsCommand.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                                PayReqItemsCommand.Parameters.AddWithValue("@UserId", userId);
                                SqlDataReader reader = await PayReqItemsCommand.ExecuteReaderAsync();
                                HashSet<string> existingIds = new HashSet<string>();
                                while (await reader.ReadAsync())
                                {
                                    existingIds.Add(reader.GetString("Id"));
                                }
                                reader.Close();

                                //In the service layer, loop through items and create Ids for those without Id
                                // Filter items to only include new ones
                                var newItems = model.Items?.Where(item => !existingIds.Contains(item.Id)).ToList();
                                if (newItems.Any())
                                {
                                    DataTable itemsTable = new DataTable();
                                    itemsTable.Columns.Add("Id", typeof(string));
                                    itemsTable.Columns.Add("PrId", typeof(string));
                                    itemsTable.Columns.Add("ProjectId", typeof(string));
                                    itemsTable.Columns.Add("UserId", typeof(string));
                                    itemsTable.Columns.Add("Name", typeof(string));
                                    itemsTable.Columns.Add("Price", typeof(decimal));
                                    itemsTable.Columns.Add("Quantity", typeof(decimal));
                                    itemsTable.Columns.Add("TotalAmount", typeof(decimal));
                                    itemsTable.Columns.Add("CreatedAt", typeof(DateTime));

                                    //Populate DataTable with items
                                    foreach (var item in newItems)
                                    {
                                        itemsTable.Rows.Add(
                                            item.Id,
                                            item.PaymentRequestId,
                                            item.ProjectId,
                                            userId,
                                            item.Name,
                                            item.Price,
                                            item.Quantity,
                                            item.TotalAmount,
                                            timeStamp
                                            );
                                    }

                                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                                    {
                                        bulkCopy.DestinationTableName = "PaymentRequestItem";
                                        bulkCopy.BulkCopyTimeout = 600;

                                        bulkCopy.ColumnMappings.Add("Id", "Id");
                                        bulkCopy.ColumnMappings.Add("PrId", "PrId");
                                        bulkCopy.ColumnMappings.Add("ProjectId", "ProjectId");
                                        bulkCopy.ColumnMappings.Add("UserId", "UserId");
                                        bulkCopy.ColumnMappings.Add("Name", "Name");
                                        bulkCopy.ColumnMappings.Add("Price", "Price");
                                        bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                                        bulkCopy.ColumnMappings.Add("TotalAmount", "TotalAmount");
                                        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");

                                        await bulkCopy.WriteToServerAsync(itemsTable); 
                                    }
                                }                           
                            }

                            await transaction.CommitAsync();
                            _logger.LogInfo("Successfully ran query to update payment request");
                            return (rowsUpdated, resultCode);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError($"Error updating payment request in DB {ex.StackTrace} {ex.Message}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            } catch(Exception ex) {
                //_logger.LogError($"Error updating payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating payment request");

            }
        }

        //UpdateSinglePaymentRequestPmDto
        //public async Task<(int, int)> UpdatePaymentRequestPm(UpdatePaymentRequestPmDto model, string userId, List<string> itemIdsToDelete)
            public async Task<(int, int)> UpdatePaymentRequestPm(UpdateGroupPaymentRequestPmDto model, string userId, List<string> itemIdsToDelete)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    int rowsUpdated;
                    int resultCode;
                    DateTime timeStamp = DateTime.Now;
                    await connection.OpenAsync();
                    using (SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            var parameters = new[]
                           {
                                new SqlParameter("@PaymentRequestId", model.Id),
                                new SqlParameter("@ProjectId", model.ProjectId),
                                new SqlParameter("@UserId", userId),
                                new SqlParameter("@Name", model.Name),
                                new SqlParameter("@Description", model.Description ?? (object)DBNull.Value ),
                                new SqlParameter("@SumTotalAmount", model.SumTotalAmount),
                                new SqlParameter("@Status", model.Status),
                                new SqlParameter("@AssignedTo", model.AssignedTo),
                                new SqlParameter("@UpdatedAt", timeStamp),
                                new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                                new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            };

                            SqlCommand PayReqCommand = new("proc_UpdatePaymentRequestPM", connection, transaction)
                            {
                                CommandType = CommandType.StoredProcedure
                            };
                            PayReqCommand.Parameters.AddRange(parameters);

                            //Add Payment Request to the Db
                            await PayReqCommand.ExecuteNonQueryAsync();
                            rowsUpdated = (int)PayReqCommand.Parameters["@RowsUpdated"].Value;
                            resultCode = (int)PayReqCommand.Parameters["@ResultCode"].Value;

                            string updateQuery = $"UPDATE PaymentRequestItem SET UserId = @AssignedTo WHERE PrId = @PrId";
                            SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction);
                            updateCommand.Parameters.AddWithValue("@AssignedTo", model.AssignedTo);
                            updateCommand.Parameters.AddWithValue("@PrId", model.Id);

                            await updateCommand.ExecuteNonQueryAsync();


                            if (rowsUpdated == 1 && resultCode == 2 && model.Items?.Count > 0)
                            {
                                if (itemIdsToDelete?.Count > 0)
                                {
                                    string deleteQuery = $"DELETE FROM PaymentRequestItem WHERE Id IN ({string.Join(",", itemIdsToDelete.Select(id => $"'{id}'"))})";
                                    SqlCommand deleteCommand = new(deleteQuery, connection, transaction);
                                    await deleteCommand.ExecuteNonQueryAsync();
                                }

                                //SqlCommand PayReqItemsCommand = new("SELECT Id FROM PaymentRequestItem", connection, transaction);
                                SqlCommand PayReqItemsCommand = new($"SELECT Id, UserId FROM PaymentRequestItem WHERE ProjectId = @ProjectId", connection, transaction);
                                PayReqItemsCommand.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                                SqlDataReader reader = await PayReqItemsCommand.ExecuteReaderAsync();
                                HashSet<string> existingIds = new HashSet<string>();
                                while (await reader.ReadAsync())
                                {
                                    existingIds.Add(reader.GetString("Id"));
                                }
                                reader.Close();

                                //In the service layer, loop through items and create Ids for those without Id
                                // Filter items to only include new ones
                                var newItems = model.Items?.Where(item => !existingIds.Contains(item.Id)).ToList();
                                if (newItems.Any())
                                {
                                    DataTable itemsTable = new DataTable();
                                    itemsTable.Columns.Add("Id", typeof(string));
                                    itemsTable.Columns.Add("PrId", typeof(string));
                                    itemsTable.Columns.Add("ProjectId", typeof(string));
                                    itemsTable.Columns.Add("UserId", typeof(string));
                                    itemsTable.Columns.Add("Name", typeof(string));
                                    itemsTable.Columns.Add("Price", typeof(decimal));
                                    itemsTable.Columns.Add("Quantity", typeof(decimal));
                                    itemsTable.Columns.Add("TotalAmount", typeof(decimal));
                                    itemsTable.Columns.Add("CreatedAt", typeof(DateTime));

                                    //Populate DataTable with items
                                    foreach (var item in newItems)
                                    {
                                        itemsTable.Rows.Add(
                                            item.Id,
                                            item.PaymentRequestId,
                                            item.ProjectId,
                                            userId,
                                            item.Name,
                                            item.Price,
                                            item.Quantity,
                                            item.TotalAmount,
                                            timeStamp
                                            );
                                    }

                                    //use Assignrd To Id
                                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                                    {
                                        bulkCopy.DestinationTableName = "PaymentRequestItem";
                                        bulkCopy.BulkCopyTimeout = 600;

                                        bulkCopy.ColumnMappings.Add("Id", "Id");
                                        bulkCopy.ColumnMappings.Add("PrId", "PrId");
                                        bulkCopy.ColumnMappings.Add("ProjectId", "ProjectId");
                                        bulkCopy.ColumnMappings.Add("UserId", "UserId");
                                        bulkCopy.ColumnMappings.Add("Name", "Name");
                                        bulkCopy.ColumnMappings.Add("Price", "Price");
                                        bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
                                        bulkCopy.ColumnMappings.Add("TotalAmount", "TotalAmount");
                                        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");

                                        await bulkCopy.WriteToServerAsync(itemsTable);
                                    }
                                }
                            }

                            await transaction.CommitAsync();
                            _logger.LogInfo("Successfully ran query to update payment request");
                            return (rowsUpdated, resultCode);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError($"Error updating payment request in DB {ex.StackTrace} {ex.Message}");
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // _logger.LogError($"Error updating payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating payment request");

            }
        }

        public async Task<(int, int)> DeletePaymentRequest(string projId, string paymentRequestId, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@Success", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @success = 0 if the payment request is not found
                    //procedure will return @ResultCode = 1, @success = 0  if the payment request is not pending or rejected
                    //procedure will return @ResultCode = 2, , @success = 1  if the payment request was successfully deleted 

                    SqlCommand command = new("proc_DeletePaymentRequest", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to delete pending or rejected request");
                    return ((int)command.Parameters["@Success"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting a pending or rejected request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting payment request");
            }
        }

        public async Task<(int, int)> DeletePaymentRequestPM(string projId, string paymentRequestId, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@Success", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @success = 0 if the payment request is not found
                    //procedure will return @ResultCode = 1, @success = 0  if the payment request is not awaiting confirmation or rejected
                    //procedure will return @ResultCode = 2, , @success = 1  if the payment request was successfully deleted 

                    SqlCommand command = new("proc_DeletePaymentRequestPM", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to delete pending or rejected request");
                    return ((int)command.Parameters["@Success"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting a pending or rejected request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting payment request");
            }
        }

        public async Task<PaymentRequest> GetPaymentRequestDetailsOtherPro(string projId, string paymentRequestId, string userId)
        {
            PaymentRequest? paymentRequest = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                    };


                    SqlCommand command = new("proc_GetPaymentRequestOtherPro", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            paymentRequest = new PaymentRequest
                            {
                                Id = reader.GetString("Id"),
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                Name = reader.GetString("Name"),
                                Status = reader.GetInt32("Status"),
                                Type = reader.GetInt32("Type"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                SumTotalAmount = reader.GetDecimal("SumTotalAmount"),
                                UserFileName = reader.IsDBNull(reader.GetOrdinal("UserFileName")) ? null : reader.GetString("UserFileName"),
                                UserStorageFileName = reader.IsDBNull(reader.GetOrdinal("UserStorageFileName")) ? null : reader.GetString("UserStorageFileName"),
                                PmFileName = reader.IsDBNull(reader.GetOrdinal("PmFileName")) ? null : reader.GetString("PmFileName"),
                                PmStorageFileName = reader.IsDBNull(reader.GetOrdinal("PmStorageFileName")) ? null : reader.GetString("PmStorageFileName"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                ConfirmedAt = reader.IsDBNull(reader.GetOrdinal("ConfirmedAt")) ? null : reader.GetDateTime("ConfirmedAt")
                            };
                        }
                    }
                }

                _logger.LogInfo("Successfully ran query to get payment request");

                if (paymentRequest != null)
                {
                    _logger.LogInfo("Successfully got payment request");
                }

                return paymentRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment request {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment request");
            }
        }

        public async Task<PaymentRequest> GetPaymentRequestDetailsPM(string projId, string paymentRequestId)
        {
            PaymentRequest? paymentRequest = null;
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                    };


                    SqlCommand command = new("proc_GetPaymentRequestPM", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            paymentRequest = new PaymentRequest
                            {
                                Id = reader.GetString("Id"),
                                ProjectId = reader.GetString("ProjectId"),
                                UserId = reader.GetString("UserId"),
                                Name = reader.GetString("Name"),
                                Status = reader.GetInt32("Status"),
                                Type = reader.GetInt32("Type"),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                                SumTotalAmount = reader.GetDecimal("SumTotalAmount"),
                                UserFileName = reader.IsDBNull(reader.GetOrdinal("UserFileName")) ? null : reader.GetString("UserFileName"),
                                UserStorageFileName = reader.IsDBNull(reader.GetOrdinal("UserStorageFileName")) ? null : reader.GetString("UserStorageFileName"),
                                PmFileName = reader.IsDBNull(reader.GetOrdinal("PmFileName")) ? null : reader.GetString("PmFileName"),
                                PmStorageFileName = reader.IsDBNull(reader.GetOrdinal("PmStorageFileName")) ? null : reader.GetString("PmStorageFileName"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                ConfirmedAt = reader.IsDBNull(reader.GetOrdinal("ConfirmedAt")) ? null : reader.GetDateTime("ConfirmedAt")
                            };
                        }
                    }
                }

                _logger.LogInfo("Successfully ran query to get payment request");

                if (paymentRequest != null)
                {
                    _logger.LogInfo("Successfully got payment request");
                }

                return paymentRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment request {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment request");
            }
        }

        public async Task<(int, int)> AddPendingPaymentRequestFile(AddPaymentRequestFileDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", model.PaymentRequestId),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@UserFileName", model.FileName),
                        new SqlParameter("@UserStorageFileName", model.StorageFileName),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 0  if the activity already has file 
                    //procedure will return @ResultCode = 3, , @RowsUpdated = 1  if the activity file have been updated 
                    SqlCommand command = new("proc_AddPendingRequestFileDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update payment request file");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of a payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of a payment request");
            }
        }

        public async Task<(int, int)> AddConfirmationPaymentRequestFile(AddPaymentRequestFileDto model)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", model.PaymentRequestId),
                        new SqlParameter("@ProjectId", model.ProjectId),
                        new SqlParameter("@PmFileName", model.FileName),
                        new SqlParameter("@PmStorageFileName", model.StorageFileName),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 0  if the activity already has file 
                    //procedure will return @ResultCode = 3, , @RowsUpdated = 1  if the activity file have been updated 
                    SqlCommand command = new("proc_AddConfirmationFileDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to update payment request file");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of a payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of a payment request");
            }
        }

        public async Task<(int, int)> RemovePaymentRequestFileDetailsOtherPro(string projId, string paymentRequestId, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UserId", userId),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 1  if the activity file details have been removed 
                    SqlCommand command = new("proc_RemovePendingRequestFileDetailsOtherPro", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to remove payment request file details");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of a payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of a payment request");
            }
        }

        public async Task<(int, int)> RemoveConfirmationPaymentRequestFileDetailsPM(string projId, string paymentRequestId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                        new SqlParameter("@PaymentRequestId", paymentRequestId),
                        new SqlParameter("@ProjectId", projId),
                        new SqlParameter("@UpdatedAt", DateTime.Now),
                        new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                    };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0  if the activity is not pending
                    //procedure will return @ResultCode = 2, , @RowsUpdated = 1  if the activity file details have been removed 
                    SqlCommand command = new("proc_RemovePendingRequestFileDetailsPM", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to remove payment request file details");
                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating the file details of a payment request in DB {ex.StackTrace} {ex.Message}");
                throw new Exception("Error updating the file details of a payment request");
            }
        }

        public async Task<(int, int)> SendPayReqForConfirmation(PaymentRequestStatusUpdateDto model, string userId)
        {
            try
            {
                using (SqlConnection connection = new(_connectionString))
                {
                    var parameters = new[]
                    {
                            new SqlParameter("@PaymentRequestId", model.PaymentRequestId),
                            new SqlParameter("@ProjectId", model.ProjectId),
                            new SqlParameter("@UserId", userId),
                            new SqlParameter("@UpdatedStatus", model.StatusAction),
                            new SqlParameter("@ResultCode", SqlDbType.Int){ Direction = ParameterDirection.Output},
                            new SqlParameter("@RowsUpdated", SqlDbType.Int){ Direction = ParameterDirection.Output},
                        };

                    //procedure returns @ResultCode = 0, @RowsUpdated = 0 if the activity is not found
                    //procedure will return @ResultCode = 1, @RowsUpdated = 0 if the activity is not awaiting confirmation
                    //procedure will return  @ResultCode = 2, @RowsUpdated = 1; if the update is successful

                    //SqlCommand command = new("proc_UpdateActivityApprovalStatus", connection)

                    SqlCommand command = new("proc_SendPayReqForConfirmation", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    command.Parameters.AddRange(parameters);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                    _logger.LogInfo("Successfully ran query to send payment request for confirmation ");

                    return ((int)command.Parameters["@RowsUpdated"].Value, (int)command.Parameters["@ResultCode"].Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending payment request for confirmation in DB layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error sending payment request for confirmation ");
            }
        }

        public async Task<IList<PayReqMonthlyDataDto>> GetPayReqMonthlyData(string projectId, int year)
        {
            try
            {
                IList<PayReqMonthlyDataDto> data = new List<PayReqMonthlyDataDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetPayReqMonthlyData";

                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Year", year);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PayReqMonthlyDataDto item = new PayReqMonthlyDataDto
                                {
                                    Profession = reader.GetInt32("Profession"),
                                    Jan = reader.GetDecimal("Jan"),
                                    Feb = reader.GetDecimal("Feb"),
                                    Mar = reader.GetDecimal("Mar"),
                                    Apr = reader.GetDecimal("Apr"),
                                    May = reader.GetDecimal("May"),
                                    Jun = reader.GetDecimal("Jun"),
                                    Jul = reader.GetDecimal("Jul"),
                                    Aug = reader.GetDecimal("Aug"),
                                    Sep = reader.GetDecimal("Sep"),
                                    Oct = reader.GetDecimal("Oct"),
                                    Nov = reader.GetDecimal("Nov"),
                                    Dec = reader.GetDecimal("Dec"),
                            };
                                item.CalculateTotal();
                                data.Add(item);
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get payment Request monthly data");
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment Request monthly data {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment Request monthly data");
            }
        }


        public async Task<IList<PayReqWeeklyDataDto>> GetPayReqWeeklyData(string projectId, int year, int month)
        {
            try
            {
                IList<PayReqWeeklyDataDto> data = new List<PayReqWeeklyDataDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetPayReqWeeklyData";

                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Year", year);
                        command.Parameters.AddWithValue("@Month", month);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PayReqWeeklyDataDto item = new PayReqWeeklyDataDto
                                {
                                    Profession = reader.GetInt32("Profession"),
                                    Wk1 = reader.GetDecimal("Wk1"),
                                    Wk2 = reader.GetDecimal("Wk2"),
                                    Wk3 = reader.GetDecimal("Wk3"),
                                    Wk4 = reader.GetDecimal("Wk4"),
                                    Wk5 = reader.GetDecimal("Wk5"),
                                };
                                item.CalculateTotal();
                                data.Add(item);
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get payment Request weekly data");
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment Request weekly data {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment Request weekly data");
            }
        }

        public async Task<IList<PayReqDailyDataDto>> GetPayReqDailyData(string projectId, int year, int month, int week)
        {
            try
            {
                IList<PayReqDailyDataDto> data = new List<PayReqDailyDataDto>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "proc_GetPayReqDailyData";

                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Year", year);
                        command.Parameters.AddWithValue("@Month", month);
                        command.Parameters.AddWithValue("@Week", week);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                PayReqDailyDataDto item = new PayReqDailyDataDto
                                {
                                    Profession = reader.GetInt32("Profession"),
                                    Mon = reader.GetDecimal("Mon"),
                                    Tue = reader.GetDecimal("Tue"),
                                    Wed = reader.GetDecimal("Wed"),
                                    Thu = reader.GetDecimal("Thu"),
                                    Fri = reader.GetDecimal("Fri"),
                                    Sat = reader.GetDecimal("Sat"),
                                    Sun = reader.GetDecimal("Sun"),
                                };
                                item.CalculateTotal();
                                data.Add(item);
                            }
                        }

                        _logger.LogInfo("Successfully ran query to get payment Request daily data");
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment Request weekly data {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment Request weekly data");
            }
        }
    }
      
}

