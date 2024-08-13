using BuildingManager.Contracts.Repository;
using BuildingManager.Contracts.Services;
using BuildingManager.Enums;
//using BuildingManager.Enums.PaymentRequestStatus;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Utils.Logger;
using BuildingManager.Utils.StorageManager;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using System;
using System.Threading.Tasks;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using BuildingManager.Models;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.S3.Model;
using System.Linq;

namespace BuildingManager.Services
{
    public class PaymentRequestService : IPaymentRequestService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;
        private readonly IStorageManager _storage;
        private readonly IRepositoryManager _repository;

        public PaymentRequestService(IConfiguration configuration, ILoggerManager logger, IStorageManager storage, IRepositoryManager repository)
        {
            _configuration = configuration;
            _logger = logger;
            _storage = storage;
            _repository = repository;
        }

        //public async Task<SuccessResponse<PaymentRequestDto>> CreatePaymentRequest(PaymentRequestReqDto model, string userId)
        //{
        //    String? fileExt = null;
        //    String? documentName = null;


        //    await using var memoryStream = new MemoryStream();
        //    if (model.File != null)
        //    {
        //        await model.File.CopyToAsync(memoryStream);
        //        fileExt = Path.GetExtension(model.File.FileName);

        //        documentName = $"{Guid.NewGuid()}{fileExt}";

        //        var s3Object = new StorageObject()
        //        {
        //            BucketName = _configuration["AwsConfiguration:BucketName"],
        //            FileStream = memoryStream,
        //            Name = documentName
        //        };


        //        await _storage.UploadFileAsync(s3Object);
        //    }

        //    var PayReqId = Guid.NewGuid().ToString();

        //    IList<PaymentRequestItem> newItems = new List<PaymentRequestItem>();

        //    if (model.Items != null)
        //    {
        //        foreach (var item in model.Items)
        //        {

        //            PaymentRequestItem input = new PaymentRequestItem
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                PaymentRequestId = PayReqId,
        //                ProjectId = item.ProjectId,
        //                UserId = userId,
        //                Name = item.Name,
        //                Price = item.Price,
        //                Quantity = item.Quantity,
        //                TotalAmount = item.TotalAmount
        //            };

        //            newItems.Add(input);
        //        }
        //    }

        //    var paymentRequest = new PaymentRequest
        //    {
        //        Id = PayReqId,
        //        ProjectId = model.ProjectId,
        //        UserId = userId,
        //        Name = model.Name,
        //        Status = (int)PaymentRequestStatus.Pending,
        //        Description = model.Description,
        //        SumTotalAmount = model.SumTotalAmount,
        //        Items = newItems,
        //        //@Todo: when validating the request ensure that the value is only possible enum values,
        //        UserFileName = model.File != null ? model.File.FileName : null,
        //        UserStorageFileName = documentName,
        //        CreatedAt = DateTime.Now,
        //    };

        //    try
        //    {
        //        await _repository.PaymentRequestRepository.CreatePaymentRequest(paymentRequest);
        //    }
        //    catch
        //    {
        //        if (model.File != null) await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], documentName);
        //        throw new Exception("Error creating new payment request");
        //    }

        //    return new SuccessResponse<PaymentRequestDto>
        //    {
        //        Message = "Payment Request created successfully",
        //    };

        //}
        public async Task<SuccessResponse<PaymentRequestDto>> CreatePaymentRequestOtherPro (PaymentRequestReqDto model, string userId) {
            var PayReqId = Guid.NewGuid().ToString();

            IList<PaymentRequestItem> newItems = new List<PaymentRequestItem>();

            if (model.Items != null && model.Type == (int)PaymentRequestType.Group) {
                foreach (var item in model.Items)
                {

                    PaymentRequestItem input = new PaymentRequestItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentRequestId = PayReqId,
                        ProjectId = item.ProjectId,
                        UserId = userId,
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        TotalAmount = item.TotalAmount
                    };

                    newItems.Add(input);
                }
            }
           
            var paymentRequest = new PaymentRequest
            {
                Id = PayReqId,
                ProjectId = model.ProjectId,
                UserId = userId,
                CreatedBy = userId,
                Name = model.Name,
                Status = (int)PaymentRequestStatus.Pending,
                Type = model.Type,
                Description = model.Description,
                SumTotalAmount = model.SumTotalAmount,
                Items = newItems,
                UserFileName = null,
                UserStorageFileName = null,
                CreatedAt = DateTime.Now,
            };

            try
            {
                await _repository.PaymentRequestRepository.CreatePaymentRequest(paymentRequest);
            }
            catch
            {
                throw new Exception("Error creating new payment request");
            }

            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment Request created successfully",
            };
        }

        public async Task<SuccessResponse<PaymentRequestDto>> CreatePaymentRequestPm (PaymentRequestPmReqDto model, string userId)
        {
            var PayReqId = Guid.NewGuid().ToString();

            IList<PaymentRequestItem> newItems = new List<PaymentRequestItem>();

            if (model.Items != null && model.Type == (int)PaymentRequestType.Group)
            {
                foreach (var item in model.Items)
                {
                    PaymentRequestItem input = new PaymentRequestItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        PaymentRequestId = PayReqId,
                        ProjectId = item.ProjectId,
                        UserId = userId,
                        Name = item.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        TotalAmount = item.TotalAmount
                    };

                    newItems.Add(input);
                }
            }

            var paymentRequest = new PaymentRequest
            {
                Id = PayReqId,
                ProjectId = model.ProjectId,
                UserId = model.AssignedTo,
                CreatedBy = userId,
                Name = model.Name,
                Status =    model.AssignedTo.Equals(userId)  ? (int)PaymentRequestStatus.AwaitingConfirmation : (int)PaymentRequestStatus.Pending,
                Type = model.Type,
                Description = model.Description,
                SumTotalAmount = model.SumTotalAmount,
                Items = newItems,
                UserFileName = null,
                UserStorageFileName = null,
                CreatedAt = DateTime.Now,
            };

            try
            {
                await _repository.PaymentRequestRepository.CreatePaymentRequest(paymentRequest);
            }
            catch
            {
                throw new Exception("Error creating new payment request");
            }

            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment Request created successfully",
            };

        }

        public async Task<PageResponse<IList<PaymentRequestDto>>> GetPaymentRequestsOtherPro(PaymentRequestReqPagedDto model, string UserId)
        {

            var (totalCount, payReqs) = await _repository.PaymentRequestRepository.GetPaymentRequestsOtherPro(model, UserId);
            try
            {

                int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

                return new PageResponse<IList<PaymentRequestDto>>()
                {
                    Data = payReqs,
                    Pagination = new Pagination()
                    {
                        TotalPages = totalPages,
                        PageSize = model.PageSize,
                        ActualDataSize = payReqs.Count,
                        TotalCount = totalCount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment requests in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment requests");
            }
        }

        public async Task<PageResponse<IList<PaymentRequestAndMemberDto>>> GetPaymentRequestsPM(PaymentRequestReqPagedDto model)
        {
            var (totalCount, payReqs) = await _repository.PaymentRequestRepository.GetPaymentRequestsPM(model);
            try
            {
                int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

                //foreach (var v in payReqs)
                //{
                //    if (v.Items != null) v.Items.OrderBy(v => v.CreatedAt);

                //}

                return new PageResponse<IList<PaymentRequestAndMemberDto>>()
                {
                    Data = payReqs,
                    Pagination = new Pagination()
                    {
                        TotalPages = totalPages,
                        PageSize = model.PageSize,
                        ActualDataSize = payReqs.Count,
                        TotalCount = totalCount
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting payment requests in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting payment requests");
            }
        }

        public async Task<SuccessResponse<IList<PayReqMonthlyDataDto>>> GetPayReqMonthlyData(string projectId, int year)
        {
            var data = await _repository.PaymentRequestRepository.GetPayReqMonthlyData(projectId, year);

            var professionals = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

            var newData = new List<PayReqMonthlyDataDto>();

            if (data.Count == 0)
            {
                for (int i = 0; i < professionals.Count; i++) 
                {
                    var defaultStat = new PayReqMonthlyDataDto()
                    {
                        Profession = professionals[i]

                    };

                    newData.Add(defaultStat);
                }
                return new SuccessResponse<IList<PayReqMonthlyDataDto>>
                {
                    Message = "Payment Request monthly data gotten successfully",
                    Data = newData
                };

            } 


                var mappedData = data.ToDictionary(data => data.Profession, x => x);

                for (int i = 0; i < professionals.Count; i++)
                {

                    if (mappedData.ContainsKey(professionals[i])){
                        continue;
                    }else
                    {
                        data.Add(new PayReqMonthlyDataDto() { Profession = professionals[i] });
                    }
                }

            var sortedData = data.OrderBy(v => v.Profession).ToList();

            return new SuccessResponse<IList<PayReqMonthlyDataDto>>
            {
                Message = "Payment Request monthly data gotten successfully",
                Data = sortedData
            };
        }

        public async Task<SuccessResponse<IList<PayReqWeeklyDataDto>>> GetPayReqWeeklyData(string projectId, int year, int month)
        {
            var data = await _repository.PaymentRequestRepository.GetPayReqWeeklyData(projectId, year, month);

            var professionals = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

            var newData = new List<PayReqWeeklyDataDto>();

            if (data.Count == 0)
            {
                for (int i = 0; i < professionals.Count; i++)
                {
                    var defaultStat = new PayReqWeeklyDataDto()
                    {
                        Profession = professionals[i]

                    };

                    newData.Add(defaultStat);
                }
                return new SuccessResponse<IList<PayReqWeeklyDataDto>>
                {
                    Message = "Payment Request weekly data gotten successfully",
                    Data = newData
                };

            }


            var mappedData = data.ToDictionary(data => data.Profession, x => x);

            for (int i = 0; i < professionals.Count; i++)
            {

                if (mappedData.ContainsKey(professionals[i]))
                {
                    continue;
                }
                else
                {
                    data.Add(new PayReqWeeklyDataDto() { Profession = professionals[i] });
                }
            }

            var sortedData = data.OrderBy(v => v.Profession).ToList();

            return new SuccessResponse<IList<PayReqWeeklyDataDto>>
            {
                Message = "Payment Request weekly data gotten successfully",
                Data = sortedData
            };
        }

        public async Task<SuccessResponse<IList<PayReqDailyDataDto>>> GetPayReqDailyData(string projectId, int year, int month, int week)
        {
            var data = await _repository.PaymentRequestRepository.GetPayReqDailyData(projectId, year, month, week);
            var professionals = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

            var newData = new List<PayReqDailyDataDto>();

            if (data.Count == 0)
            {
                for (int i = 0; i < professionals.Count; i++)
                {
                    var defaultStat = new PayReqDailyDataDto()
                    {
                        Profession = professionals[i]

                    };

                    newData.Add(defaultStat);
                }
                return new SuccessResponse<IList<PayReqDailyDataDto>>
                {
                    Message = "Payment Request daily data gotten successfully",
                    Data = newData
                };

            }


            var mappedData = data.ToDictionary(data => data.Profession, x => x);

            for (int i = 0; i < professionals.Count; i++)
            {

                if (mappedData.ContainsKey(professionals[i]))
                {
                    continue;
                }
                else
                {
                    data.Add(new PayReqDailyDataDto() { Profession = professionals[i] });
                }
            }

            var sortedData = data.OrderBy(v => v.Profession).ToList();

            return new SuccessResponse<IList<PayReqDailyDataDto>>
            {
                Message = "Payment Request daily data gotten successfully",
                Data = sortedData
            };
        }

        public async Task<SuccessResponse<PaymentRequestDto>> PaymentRequestConfirmation(PaymentRequestStatusUpdateDto model)
        {
            //validate the StatusAction and ensure that it is either 3 or 4
            if (model.StatusAction != (int)PaymentRequestStatus.Confirmed && model.StatusAction != (int)PaymentRequestStatus.Rejected)

            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to confirm or reject a payment request");
            }


            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.UpdatePaymentConfirmationStatus(model);
            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request confirmation status. The required payment request may not exist, check Ids provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request. Invalid payment request ");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to approve or reject a payment request that is not awaiting confirmation");
                throw new Exception("Error cannot approve or reject a payment request that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to confirm or reject a payment request");
                throw new Exception("Error failed to confirm or reject a payment request");
            }


            if (model.StatusAction == (int)PaymentRequestStatus.Confirmed)
            {
                return new SuccessResponse<PaymentRequestDto>
                {
                    Message = "Payment Request status updated to confirmed successfully",
                };
            }


            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment Request status updated to rejected successfully",
            };
        }

        public async Task<SuccessResponse<PaymentRequestDto>> SendPaymentRequestForConfirmation(PaymentRequestStatusUpdateDto model, string userId)
        {
            //validate the StatusAction and ensure that it is either 3 or 4
            if (model.StatusAction != (int)PaymentRequestStatus.AwaitingConfirmation)

            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to send for confirmation");
            }


            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.SendPayReqForConfirmation(model, userId);
            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request confirmation status. The required payment request may not exist, check Ids provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request. Invalid payment request ");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to send a payment request for confirmation that is not pending");
                throw new Exception("Error cannot send a payment request for confirmation that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to send a payment request for confirmation");
                throw new Exception("Error failed to send a payment request for confirmation");
            }

            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment Request sent for confirmation successfully",
            };
        }

        public async Task<SuccessResponse<PaymentRequestDto>> UpdatePendingPaymentRequest(UpdateGroupPaymentRequestDto model, string userId)
        {
            if (model.Items != null)
            {
                foreach (var item in model.Items) 
                {
                    if (item.Id == null) item.Id = Guid.NewGuid().ToString(); 
                }

            }

            List<string> deleteItemsId = new();
            if (model.DeletedItems != null && model.DeletedItems.Count > 0)
            {
                deleteItemsId = model.DeletedItems.Select(v => v.Id).ToList();
            }

            var newModel = new UpdatePaymentRequestDto
            {
                Id = model.Id,
                ProjectId = model.ProjectId,
                Name = model.Name,
                Description = model.Description,
                Items = model.Items,
                SumTotalAmount = model.SumTotalAmount,
            };
            //List<string> itemIdsToDelete
           
          
            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.UpdatePendingPaymentRequest(newModel, userId, deleteItemsId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request. The required payment request may not exist, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the details of a payment request that is not pending");
                throw new Exception("Error cannot update a payment request that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update a payment request");
                throw new Exception("Error failed to update a payment request");
            }

            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Activity details updated successfully",
            };
        }

        public async Task<SuccessResponse<PaymentRequestDto>> UpdatePaymentRequestPm(UpdateGroupPaymentRequestPmDto model, string userId)
        {
            if (model.Items != null)
            {
                foreach (var item in model.Items)
                {
                    if (item.Id == null) item.Id = Guid.NewGuid().ToString();
                }

            }

            List<string> deleteItemsId = new();
            if (model.DeletedItems != null && model.DeletedItems.Count > 0)
            {
                deleteItemsId = model.DeletedItems.Select(v => v.Id).ToList();
            }

            //var newModel = new UpdatePaymentRequestPmDto
            //{
            //    Id = model.Id,
            //    ProjectId = model.ProjectId,
            //    Name = model.Name,
            //    Description = model.Description,
            //    Items = model.Items,
            //    SumTotalAmount = model.SumTotalAmount,
            //    AssignedTo = model.AssignedTo,
            //};
          

            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.UpdatePaymentRequestPm(model, userId, deleteItemsId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request. The required payment request may not exist, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the details of a payment request that is that is not awaiting confirmation");
                throw new Exception("Error cannot update a payment request that is that is not awaiting confirmation");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update a payment request");
                throw new Exception("Error failed to update a payment request");
            }

            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Activity details updated successfully",
            };
        }

        public async Task DeletePaymentRequest(string projId, string paymentRequestId, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(projId, paymentRequestId, userId);

            if (payReq == null)
            {
                _logger.LogError($"Error occurred when deleting the payment request. The required payment request was not found, check paymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be deleted was not found.");
            }

            //if (payReq.Status != (int)PaymentRequestStatus.Rejected)
            if (payReq.Status == (int)PaymentRequestStatus.AwaitingConfirmation || payReq.Status == (int)PaymentRequestStatus.Confirmed)
            {
                _logger.LogError($"Error attempted to delete a payment request that is not pending or rejected");
                throw new Exception("Error cannot delete a payment request that is not pending or rejected");
            }

            if (payReq.UserStorageFileName != null)
            {
                await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.UserStorageFileName);
            }

            var (rowsDeleted, returnNum) = await _repository.PaymentRequestRepository.DeletePaymentRequest(projId, paymentRequestId, userId);

            if (rowsDeleted == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when deleting the payment request. The required payment request was not found, check paymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete payment request.");
            }

            if (rowsDeleted == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete a payment request that is not pending or rejected");
                throw new Exception("Error cannot delete a payment request that is not pending or rejected");
            }

            if (rowsDeleted == 0)
            {
                _logger.LogError($"Error failed to delete a payment request");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete a payment request");
            }          
        }

        public async Task DeletePaymentRequestPM(string projId, string paymentRequestId, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(projId, paymentRequestId, userId);

            if (payReq == null)
            {
                _logger.LogError($"Error occurred when deleting the payment request. The required payment request was not found, check paymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be deleted was not found.");
            }

            //if (payReq.Status != (int)PaymentRequestStatus.Rejected)
            if (payReq.Status == (int)PaymentRequestStatus.Pending || payReq.Status == (int)PaymentRequestStatus.Confirmed)
            {
                _logger.LogError($"Error attempted to delete a payment request that is not awaiting confirmation or rejected");
                throw new Exception("Error cannot delete a payment request that is not awaiting confirmation or rejected");
            }


            //check and remove if not relevant
            if (payReq.UserStorageFileName != null)
            {
                await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.UserStorageFileName);
            }

            var (rowsDeleted, returnNum) = await _repository.PaymentRequestRepository.DeletePaymentRequestPM(projId, paymentRequestId, userId);

            if (rowsDeleted == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when deleting the payment request. The required payment request was not found, check paymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete payment request.");
            }

            if (rowsDeleted == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete a payment request that is not awaiting confirmation or rejected");
                throw new Exception("Error cannot delete a payment request that is not awaiting confirmation or rejected");
            }

            if (rowsDeleted == 0)
            {
                _logger.LogError($"Error failed to delete a payment request");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete a payment request");
            }
        }

        public async Task<SuccessResponse<PaymentRequestDto>> UpdatePendingPaymentRequestFile(AddPaymentRequestFileReqDto model, string userId)
        //public async Task<SuccessResponse<ActivityDto>> UpdatePendingActivityFile(AddActivityFileRequestDto model, string userId)
        {

            //var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(model.ProjectId, model.PaymentRequestId, userId);

            if (payReq == null)
            {
                _logger.LogError($"Error occurred when updating the  payment request. The required  payment request was not found, check PaymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request to be updated was not found.");
            }

            if (payReq.Status != (int)PaymentRequestStatus.Pending)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that is not pending");
                throw new Exception("Error cannot update a payment request that is not pending");
            }

            if (payReq.UserStorageFileName != null)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that has a file stored. File stored should be deleted first before adding a new file");
                throw new Exception("Error cannot add a file to a payment request that already has a file.");
            }

            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(model.File.FileName);

            //use breakpoint to see file name before uploading to cloud
            var documentName = $"{Guid.NewGuid()}.{fileExt}";


            var addFile = new AddPaymentRequestFileDto
            {
                ProjectId = model.ProjectId,
                PaymentRequestId = model.PaymentRequestId,
                FileName = model.File.FileName,
                StorageFileName = documentName,
                FileExtension = fileExt,
            };

            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.AddPendingPaymentRequestFile(addFile, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request file. The required payment request was not found, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that is not pending");
                throw new Exception("Error cannot update a payment request that is not pending");
            }

            if (rowsUpdated == 0 && returnNum == 2)
            {
                _logger.LogError($"Error payment request already has a file, a payment request can only have one file");
                throw new Exception("Error cannot add a file to a payment request that already has a file");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update a payment request");
                throw new Exception("Error failed to update a payment request");
            }

            var s3Object = new StorageObject()
            {
                //use config to get value : _configuration.GetValue
                BucketName = _configuration["AwsConfiguration:BucketName"],
                FileStream = memoryStream,
                Name = documentName
            };

            await _storage.UploadFileAsync(s3Object);
            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment request file updated successfully",
            };
        }


        //update this method - status awaiting approval
        public async Task<SuccessResponse<PaymentRequestDto>> UpdateConfirmationPaymentRequestFile(AddPaymentRequestFileReqDto model)
        //public async Task<SuccessResponse<ActivityDto>> UpdatePendingActivityFile(AddActivityFileRequestDto model, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsPM(model.ProjectId, model.PaymentRequestId);

            if (payReq == null)
            {
                _logger.LogError($"Error occurred when updating the  payment request. The required  payment request was not found, check ProjectId and PaymentRequestId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request to be updated was not found.");
            }

            if (payReq.Status == (int)PaymentRequestStatus.Rejected)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that is not pending or confirmed");
                throw new Exception("Error cannot update a payment request that is not pending or confirmed");
            }

            if (payReq.PmStorageFileName != null)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that has a file stored. File stored should be deleted first before adding a new file");
                throw new Exception("Error cannot add a file to a payment request that already has a file.");
            }

            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(model.File.FileName);

            //use breakpoint to see file name before uploading to cloud
            var documentName = $"{Guid.NewGuid()}.{fileExt}";


            var addFile = new AddPaymentRequestFileDto
            {
                ProjectId = model.ProjectId,
                PaymentRequestId = model.PaymentRequestId,
                FileName = model.File.FileName,
                StorageFileName = documentName,
                FileExtension = fileExt,
            };

            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.AddConfirmationPaymentRequestFile(addFile);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request file. The required payment request was not found, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update payment request file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the file details of a payment request that is not pending");
                throw new Exception("Error cannot update a payment request that is not pending");
            }

            if (rowsUpdated == 0 && returnNum == 2)
            {
                _logger.LogError($"Error payment request already has a file, a payment request can only have one file");
                throw new Exception("Error cannot add a file to a payment request that already has a file");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update a payment request");
                throw new Exception("Error failed to update a payment request");
            }

            var s3Object = new StorageObject()
            {
                //use config to get value : _configuration.GetValue
                BucketName = _configuration["AwsConfiguration:BucketName"],
                FileStream = memoryStream,
                Name = documentName
            };

            await _storage.UploadFileAsync(s3Object);
            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Payment request file updated successfully",
            };
        }


        
        public async Task<SuccessResponse<PaymentRequestDto>> DeletePaymentRequestFileOtherPro(PaymentRequestFileDto model, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(model.ProjectId, model.PaymentRequestId, userId);
            if (payReq == null)
            {
                _logger.LogError($"Error occurred when updating the  payment request. The required  payment request was not found, check PaymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request to be updated was not found.");
            }


            if (payReq.Status != (int)PaymentRequestStatus.Pending)
            {
                _logger.LogError($"Error attempted to delete the file of a payment request that is not pending");
                throw new Exception("Error cannot delete the file of a payment request that is not pending");
            }

            if (payReq.UserStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a file to delete.");
            }


            await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.UserStorageFileName);

            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.RemovePaymentRequestFileDetailsOtherPro(model.ProjectId, model.PaymentRequestId, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request file. The required payment request was not found, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete the file details of a payment request that is not pending in the DB");
                throw new Exception("Error cannot delete the file of a payment request that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to delete the file of an activity");
                throw new Exception("Error failed to delete the file of an activity");
            }


            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Activity file deleted successfully",
            };
        }


        //update to delete file for payment request that has a status of -awaiting confirmation
        public async Task<SuccessResponse<PaymentRequestDto>> DeleteConfirmationPaymentRequestFilePM(PaymentRequestFileDto model)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsPM(model.ProjectId, model.PaymentRequestId);
            if (payReq == null)
            {
                _logger.LogError($"Error occurred when updating the  payment request. The required  payment request was not found, check ProjectId and PaymentRequestId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request to be updated was not found.");
            }


            if (payReq.Status == (int)PaymentRequestStatus.Pending)
            {
                _logger.LogError($"Error attempted to delete the file of a payment request that is not pending");
                throw new Exception("Error cannot delete the file of a payment request that is not pending");
            }

            if (payReq.UserStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a file to delete.");
            }


            await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.PmStorageFileName);

            var (rowsUpdated, returnNum) = await _repository.PaymentRequestRepository.RemoveConfirmationPaymentRequestFileDetailsPM(model.ProjectId, model.PaymentRequestId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the payment request file. The required payment request was not found, check PaymentRequestId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete the file details of a payment request that is not pending in the DB");
                throw new Exception("Error cannot delete the file of a payment request that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to delete the file of an activity");
                throw new Exception("Error failed to delete the file of an activity");
            }


            return new SuccessResponse<PaymentRequestDto>
            {
                Message = "Activity file deleted successfully",
            };
        }

        public async Task<GetObjectResponse> DownloadPayReqFileOtherPro(PaymentRequestFileDto model, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(model.ProjectId, model.PaymentRequestId, userId);
            if (payReq == null)
            {
                _logger.LogError($"Error the required  payment request was not found, check PaymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request was not found.");
            }


            if (model.FileName != payReq.UserFileName)
            {
                _logger.LogError($"Error payment request does not have any User file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request does not have the file provided.");
            }

            if (payReq.UserStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any User file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a User file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.UserStorageFileName);
            return file;
        }

        public async Task<GetObjectResponse> DownloadPayReqConfirmationFileOtherPro(PaymentRequestFileDto model, string userId)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsOtherPro(model.ProjectId, model.PaymentRequestId, userId);
            if (payReq == null)
            {
                _logger.LogError($"Error the required  payment request was not found, check PaymentRequestId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request was not found.");
            }


            if (model.FileName != payReq.PmFileName)
            {
                _logger.LogError($"Error payment request does not have any Confirmation file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request does not have the file provided.");
            }

            if (payReq.PmStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any Confirmation file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a Confirmation file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.PmStorageFileName);
            return file;
        }

        public async Task<GetObjectResponse> DownloadPayReqFilePM(PaymentRequestFileDto model)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsPM(model.ProjectId, model.PaymentRequestId);
            if (payReq == null)
            {
                _logger.LogError($"Error the required  payment request was not found, check PaymentRequestId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request was not found.");
            }


            if (model.FileName != payReq.UserFileName)
            {
                _logger.LogError($"Error payment request does not have any User file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request does not have the file provided.");
            }

            if (payReq.UserStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any User file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a User file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.UserStorageFileName);
            return file;
        }

        public async Task<GetObjectResponse> DownloadPayReqConfirmationFilePM(PaymentRequestFileDto model)
        {
            var payReq = await _repository.PaymentRequestRepository.GetPaymentRequestDetailsPM(model.ProjectId, model.PaymentRequestId);
            if (payReq == null)
            {
                _logger.LogError($"Error the required  payment request was not found, check PaymentRequestId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request was not found.");
            }


            if (model.FileName != payReq.PmFileName)
            {
                _logger.LogError($"Error payment request does not have any Confirmation file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error payment request does not have the file provided.");
            }

            if (payReq.PmStorageFileName == null)
            {
                _logger.LogError($"Error payment request does not have any Confirmation file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error payment request does not have a Confirmation file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], payReq.PmStorageFileName);
            return file;
        }
    }
}
