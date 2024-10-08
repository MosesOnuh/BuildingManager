﻿using BuildingManager.Contracts.Services;
using BuildingManager.Contracts.Repository;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Utils.Logger;
using BuildingManager.Utils.StorageManager;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using BuildingManager.Models.Entities;
using BuildingManager.Enums;
using System.Net;
using Amazon.S3.Model;
using BuildingManager.Models;
using System.Reflection;
using System.Collections.Generic;

namespace BuildingManager.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerManager _logger;
        private readonly IStorageManager _storage;
        private readonly IRepositoryManager _repository;
        public ActivityService(IConfiguration configuration, ILoggerManager logger, IStorageManager storage, IRepositoryManager repository) 
        {
            _configuration = configuration;
            _logger = logger;
            _storage = storage;
            _repository = repository;
        }      

        public async Task<SuccessResponse<ActivityDto>> CreateActivity(ActivityRequestDto model, string userId)
        {

            String? fileExt = null;
            String? documentName = null;


            await using var memoryStream = new MemoryStream();
            if (model.File != null)
            {
                await model.File.CopyToAsync(memoryStream);
                fileExt = Path.GetExtension(model.File.FileName);

                documentName = $"{Guid.NewGuid()}{fileExt}";

                var s3Object = new StorageObject()
                {
                    BucketName = _configuration["AwsConfiguration:BucketName"],
                    FileStream = memoryStream,
                    Name = documentName
                };


                await _storage.UploadFileAsync(s3Object);
            }
            

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = model.ProjectId,
                UserId = userId,
                CreatedBy = userId,
                Name = model.Name,
                Status = (int)ActivityStatus.Pending,
                Description = model.Description,
                //@Todo: when validating the request ensure that the value is only possible enum values
                ProjectPhase = model.ProjectPhase,
                FileName = model.File != null ? model.File.FileName : null,
                StorageFileName = documentName,
                FileExtension = fileExt,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CreatedAt = DateTime.Now,
            };

            try 
            {
                await _repository.ActivityRepository.CreateActivity(activity);
            } catch 
            {
                if (model.File != null) await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], documentName);
                throw new Exception("Error creating new activity");
            }
            
            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity created successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> CreateActivityPm(ActivityPmRequestDto model, string userId)
        {

            String? fileExt = null;
            String? documentName = null;


            await using var memoryStream = new MemoryStream();
            if (model.File != null)
            {
                await model.File.CopyToAsync(memoryStream);
                fileExt = Path.GetExtension(model.File.FileName);

                documentName = $"{Guid.NewGuid()}{fileExt}";

                var s3Object = new StorageObject()
                {
                    BucketName = _configuration["AwsConfiguration:BucketName"],
                    FileStream = memoryStream,
                    Name = documentName
                };


                await _storage.UploadFileAsync(s3Object);
            }


            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = model.ProjectId,
                UserId = model.AssignedTo,
                CreatedBy = userId,
                Name = model.Name,
                //Status = (int)ActivityStatus.Pending,
                Status = model.AssignedTo.Equals(userId) ? (int)ActivityStatus.AwaitingApproval : (int)ActivityStatus.Pending,
                Description = model.Description,
                //@Todo: when validating the request ensure that the value is only possible enum values
                ProjectPhase = model.ProjectPhase,
                FileName = model.File != null ? model.File.FileName : null,
                StorageFileName = documentName,
                FileExtension = fileExt,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CreatedAt = DateTime.Now,
            };

            try
            {
                await _repository.ActivityRepository.CreateActivity(activity);
            }
            catch
            {
                if (model.File != null) await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], documentName);
                throw new Exception("Error creating new activity");
            }

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity created successfully",
            };
        }

        //PM approves or rejects an activity
        public async Task<SuccessResponse<ActivityDto>> ActivityApproval(ActivityStatusUpdateDto model)
        {
            //write procedure to check the status of the activity and if the status is one then 
            // the status can be updated to 2 or 3

            //validate the StatusAction and ensure that it is either 3 or 4
            if (model.StatusAction != (int)ActivityStatus.Approved && model.StatusAction != (int)ActivityStatus.Rejected)
            //if (model.StatusAction != 2 && model.StatusAction != 3)

            {
                    throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to approve or reject an activity");
            }

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.UpdateActivityApprovalStatus(model);
            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity approval status. The required activity may not exist, check activityId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity. Invalid Activity");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to approve or reject an activity that is not awairing approval");
                throw new Exception("Error cannot approve or reject an activity that is not pending");
            }

            if (rowsUpdated == 0 )
            {
                _logger.LogError($"Error failed to approve or reject an activity");
                throw new Exception("Error failed to approve or reject an activity");
            }


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity status updated to approved successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> SendActivityForApproval(ActivityStatusUpdateDto model, string userId)
        {
            
            if (model.StatusAction != (int)ActivityStatus.AwaitingApproval)
            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to send for approval");
            }

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.SendActivityForApproval(model, userId);
            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity approval status. The required activity may not exist, check activityId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity. Invalid Activity");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to send an activity for confirmation that is not pending");
                throw new Exception("Error cannot send an activity for approval that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to send an activity for approval");
                throw new Exception("Error failed to send an activity for approval");
            }


            //return new SuccessResponse<ActivityDto>
            //{
            //    Message = "Activity status updated to approved successfully",
            //};

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity sent for approval successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> UpdateActivityToDone(ActivityStatusUpdateDto model, string userId)
        {
            //write procedure to check the status of the activity and if the status is two then and
            //there is start date and finish date
            // the status can be updated to 4

            //validate the StatusAction and ensure that it is 5
            if (model.StatusAction != (int)ActivityStatus.Done)
            {
                throw new RestException(HttpStatusCode.BadRequest, "Error StatusAction can only be to set an activity to done");
            }

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.UpdateActivityToDone(model, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity. The required activity may not exist, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error only approved activity can be changed to done");
                throw new Exception("Error only approved activity can be changed to done");
            }

            if (rowsUpdated == 0 && returnNum == 2)
            {
                _logger.LogError($"Error activity does not have actual start date or actual end date");
                throw new Exception("Error activity does not have actual start date or actual end date");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error occurred when updating the activity approval status.");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity.");
            }


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity status updated to done successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> UpdateActivityActualDates(ActivityActualDatesDto model, string userId)
        {
            //validate request model and ensure that actual start date and actual end date is not greater that todays date.

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.UpdateActivityActualDates(model, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity actual dates. The required activity may not exist, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity actual start date and actual end date.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error activity must be approved or done in order to update the actual start or end dates");
                throw new Exception("Error activity must be approved or done in order to update the actual start or end dates");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error occurred when updating the activity actual dates.");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity.");
            }


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity status updated to done successfully",
            };
        }

        //validate input
        //ensure that for project phase only 1,2 and 3 can be passed
        public async Task<SuccessResponse<ActivityDto>> UpdatePendingActivity(UpdateActivityDetailsDto model, string userId)
        {
            var(rowsUpdated, returnNum) = await _repository.ActivityRepository.UpdatePendingActivity(model, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity. The required activity may not exist, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the details of an activity that is not pending");
                throw new Exception("Error cannot update an activity that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update an activity");
                throw new Exception("Error failed to update an activity");
            }

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity details updated successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> UpdateActivityPM(UpdateActivityPmDetailsReqDto model, string userId)
        {
            var newModel = new UpdateActivityPmDetailsDto() 
            {
                ActivityId = model.ActivityId,
                ProjectId = model.ProjectId,
                Name = model.Name,
                Status = model.AssignedTo.Equals(userId) ? (int)ActivityStatus.AwaitingApproval : (int)ActivityStatus.Pending,
                Description = model.Description,
                ProjectPhase = model.ProjectPhase,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                AssignedTo = model.AssignedTo   
            };

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.UpdateActivityPM(newModel, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity. The required activity may not exist, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the details of an activity that is not awaiting approval");
                throw new Exception("Error cannot update an activity that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update an activity");
                throw new Exception("Error failed to update an activity");
            }

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity details updated successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> UpdatePendingActivityFile(AddActivityFileRequestDto model, string userId)
        {

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when updating the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be updated was not found.");
            }

            if (activity.Status != (int)ActivityStatus.Pending)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that is not pending");
                throw new Exception("Error cannot update an activity that is not pending");
            }

            if (activity.StorageFileName != null)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that has a file stored. File stored should be deleted first before adding a new file");
                throw new Exception("Error cannot add a file to an activity that already has a file.");
            }

            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(model.File.FileName);

            //use breakpoint to see file name before uploading to cloud
            var documentName = $"{Guid.NewGuid()}.{fileExt}";


            var addFile = new AddActivityFileDto
            {
                ProjectId = model.ProjectId,
                ActivityId = model.ActivityId,
                FileName = model.File.FileName,
                StorageFileName = documentName,
                FileExtension = fileExt,
            };


            //check if user has file in activity, if yes return error message that user can only have one file per activity
            //if user has no file in activity then file can be added to the activity
            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.AddActivityFile(addFile, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity file. The required activity was not found, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that is not pending");
                throw new Exception("Error cannot update an activity that is not pending");
            }

            if (rowsUpdated == 0 && returnNum == 2)
            {
                _logger.LogError($"Error activity already has a file, an activity can only have one file");
                throw new Exception("Error cannot add a file to an activity that already has a file");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update an activity");
                throw new Exception("Error failed to update an activity");
            }

            var s3Object = new StorageObject()
            {
                //use config to get value : _configuration.GetValue
                BucketName = _configuration["AwsConfiguration:BucketName"],
                FileStream = memoryStream,
                Name = documentName
            };

            await _storage.UploadFileAsync(s3Object);
            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity file updated successfully",
            };
        }


        public async Task<SuccessResponse<ActivityDto>> UpdateActivityFilePM(AddActivityFileRequestDto model, string userId)
        {

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when updating the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be updated was not found.");
            }

            if (activity.Status != (int)ActivityStatus.AwaitingApproval)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that is not awaiting approval");
                throw new Exception("Error cannot update an activity that is not awaiting approval");
            }

            if (activity.StorageFileName != null)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that has a file stored. File stored should be deleted first before adding a new file");
                throw new Exception("Error cannot add a file to an activity that already has a file.");
            }

            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(model.File.FileName);

            //use breakpoint to see file name before uploading to cloud
            var documentName = $"{Guid.NewGuid()}.{fileExt}";


            var addFile = new AddActivityFileDto
            {
                ProjectId = model.ProjectId,
                ActivityId = model.ActivityId,
                FileName = model.File.FileName,
                StorageFileName = documentName,
                FileExtension = fileExt,
            };


            //check if user has file in activity, if yes return error message that user can only have one file per activity
            //if user has no file in activity then file can be added to the activity
            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.AddActivityFilePM(addFile, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity file. The required activity was not found, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to update the file details of an activity that is not awaiting approval");
                throw new Exception("Error cannot update an activity that is not awaiting approval");
            }

            if (rowsUpdated == 0 && returnNum == 2)
            {
                _logger.LogError($"Error activity already has a file, an activity can only have one file");
                throw new Exception("Error cannot add a file to an activity that already has a file");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to update an activity");
                throw new Exception("Error failed to update an activity");
            }

            var s3Object = new StorageObject()
            {
                //use config to get value : _configuration.GetValue
                BucketName = _configuration["AwsConfiguration:BucketName"],
                FileStream = memoryStream,
                Name = documentName
            };

            await _storage.UploadFileAsync(s3Object);
            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity file updated successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> DeleteActivity (string projId, string activityId, string userId)
        {
            //get file details from activity gotten from db
            //delete file in cloud
            //deleteactivity in db

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(projId, activityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when deleting the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be deleted was not found.");
            }

            //if (activity.Status != (int)ActivityStatus.Rejected  )
            if (activity.Status == (int)ActivityStatus.AwaitingApproval || 
                activity.Status == (int)ActivityStatus.Approved || activity.Status == (int)ActivityStatus.Done)
            {
                _logger.LogError($"Error attempted to delete an activity that is not pending or rejected");
                throw new Exception("Error cannot delete an activity that is not pending or rejected");
            }

            if (activity.StorageFileName != null)
            {
                await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);
            }
           
            var (rowsDeleted, returnNum) = await _repository.ActivityRepository.DeleteActivity(projId, activityId, userId);

            if (rowsDeleted == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when deleting the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete activity.");
            }

            if (rowsDeleted == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete an activity that is not rejected");
                throw new Exception("Error cannot delete an activity that is not rejected");
            }

            if (rowsDeleted == 0)
            {
                _logger.LogError($"Error failed to delete an activity");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete an activity");
            }

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity deleted successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> DeleteActivityPM(string projId, string activityId, string userId)
        {
            //get file details from activity gotten from db
            //delete file in cloud
            //deleteactivity in db

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(projId, activityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when deleting the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be deleted was not found.");
            }

            //if (activity.Status != (int)ActivityStatus.Rejected  )
            if (activity.Status == (int)ActivityStatus.Pending ||
                activity.Status == (int)ActivityStatus.Approved || activity.Status == (int)ActivityStatus.Done)
            {
                _logger.LogError($"Error attempted to delete an activity that is not awaiting approval or rejected");
                throw new Exception("Error cannot delete an activity that is not awaiting approval or rejected");
            }

            if (activity.StorageFileName != null)
            {
                await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);
            }

            var (rowsDeleted, returnNum) = await _repository.ActivityRepository.DeleteActivityPM(projId, activityId, userId);

            if (rowsDeleted == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when deleting the activity. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete activity.");
            }

            if (rowsDeleted == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete an activity that is not awaiting approval or rejected");
                throw new Exception("Error cannot delete an activity that is not awaiting approval or rejected");
            }

            if (rowsDeleted == 0)
            {
                _logger.LogError($"Error failed to delete an activity");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to delete an activity that is not awaiting approval or rejected");
            }

            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity deleted successfully",
            };
        }

        //@todo procedure to remove FileName, StorageFileName, FileExtension after deleting a file in the database
        public async Task<SuccessResponse<ActivityDto>> DeleteActivityFile(ActivityFileDto model, string userId)
        {
            //get file details from activity gotten from db
            //delete file in cloud
            //deleteactivity in db

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);
            if (activity == null)
            {
                _logger.LogError($"Error occurred when deleting the activity File. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be delete was not found.");
            }


            if (activity.Status != (int)ActivityStatus.Pending)
            {
                _logger.LogError($"Error attempted to delete the file of an activity that is not pending");
                throw new Exception("Error cannot delete the file of an activity that is not pending");
            }

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file to delete.");
            }


            //DeleteActivity file gotten from the above function
            //use config to get value : _configuration.GetValue
            await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.RemoveActivityFileDetails(model.ProjectId, model.ActivityId, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity file. The required activity was not found, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete the file details of an activity that is not pending in the DB");
                throw new Exception("Error cannot delete the file of an activity that is not pending");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to delete the file of an activity");
                throw new Exception("Error failed to delete the file of an activity");
            }


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity file deleted successfully",
            };
        }

        public async Task<SuccessResponse<ActivityDto>> DeleteActivityFilePM(ActivityFileDto model, string userId)
        {
            //get file details from activity gotten from db
            //delete file in cloud
            //deleteactivity in db

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);
            if (activity == null)
            {
                _logger.LogError($"Error occurred when deleting the activity File. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be delete was not found.");
            }


            if (activity.Status != (int)ActivityStatus.AwaitingApproval)
            {
                _logger.LogError($"Error attempted to delete the file of an activity that is not awaiting approval");
                throw new Exception("Error cannot delete the file of an activity that is not awaiting approval");
            }

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file to delete.");
            }


            //DeleteActivity file gotten from the above function
            //use config to get value : _configuration.GetValue
            await _storage.DeleteFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.RemoveActivityFileDetailsPM(model.ProjectId, model.ActivityId, userId);

            if (rowsUpdated == 0 && returnNum == 0)
            {
                _logger.LogError($"Error occurred when updating the activity file. The required activity was not found, check ActivityId, ProjectId  and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error failed to update activity file.");
            }

            if (rowsUpdated == 0 && returnNum == 1)
            {
                _logger.LogError($"Error attempted to delete the file details of an activity that is not awaiting approval in the DB");
                throw new Exception("Error cannot delete the file of an activity that is not awaiting approval");
            }

            if (rowsUpdated == 0)
            {
                _logger.LogError($"Error failed to delete the file of an activity");
                throw new Exception("Error failed to delete the file of an activity");
            }


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity file deleted successfully",
            };
        }

        //validate request to ensure that a file name is passed
        public async Task<GetObjectResponse> DownloadActivityFileOtherPro(ActivityFileDto model, string userId) 
        {
            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when downloading the activity File. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be downloaded was not found.");
            }

            if (model.FileName != activity.FileName)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error activity does not have the file provided.");
            }

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);
            return file;
        }


        //validate request to ensure that a file name is passed
        public async Task<GetObjectResponse> DownloadActivityFilePM(ActivityFileDto model)
        {
            var activity = await _repository.ActivityRepository.GetActivityPM(model.ProjectId, model.ActivityId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when downloading the activity File. The required activity was not found, check ActivityId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be downloaded was not found.");
            }

            if (model.FileName != activity.FileName)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.NotFound, "Error activity does not have the file provided.");
            }

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file.");
            }

            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync(_configuration["AwsConfiguration:BucketName"], activity.StorageFileName);
            return file;
        }

        public async Task<SuccessResponse<ActivityDto>> GetActivityOtherPro(string projectId, string activityId, string userId)
        {
            var activity = await _repository.ActivityRepository.GetActivityOtherPro(projectId, activityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when getting the activity File. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity was not found.");
            }

            var clientActivity = new ActivityDto()
            {
                Id = activity.Id,
                ProjectId = activity.ProjectId,
                UserId = activity.UserId,
                Name = activity.Name,
                Status = activity.Status,
                Description = activity.Description,
                ProjectPhase = activity.ProjectPhase,
                FileName = activity.FileName,
                StorageFileName = activity.StorageFileName,
                //FileType = activity.FileExtension,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                ActualStartDate = activity.ActualStartDate,
                ActualEndDate = activity.ActualEndDate,
                CreatedAt = activity.CreatedAt
            };


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity gotten successfully",
                Data = clientActivity,
            };
        }

        //correct this
        public async Task<SuccessResponse<ActivityAndMemberDto>> GetActivityPM(string projectId, string activityId)
        {
            var activity = await _repository.ActivityRepository.GetActivityPM(projectId, activityId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when getting the activity File. The required activity was not found, check ActivityId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity was not found.");
            }

            return new SuccessResponse<ActivityAndMemberDto>
            {
                Message = "Activity get activity successfully",
                Data = activity,
            };
        }

        public async Task<PageResponse<IList<ActivityDto>>> GetProjectPhaseActivitiesOtherPro(ProjectActivitiesReqDto model, string UserId)
        {

            var (totalCount, activities) = await _repository.ActivityRepository.GetProjectPhaseActivitiesOtherPro(model, UserId);
            try 
            {
               
                int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

                return new PageResponse<IList<ActivityDto>>()
                {
                    Data = activities,
                    Pagination = new Pagination()

                    {
                        TotalPages = totalPages,
                        PageSize = model.PageSize,
                        ActualDataSize = activities.Count,
                        TotalCount = totalCount
                    }
                };
            } catch (Exception ex) {
                _logger.LogError($"Error getting activities per phase in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activities per phase");
            }                     
        }

        public async Task<PageResponse<IList<ActivityAndMemberDto>>> GetProjectPhaseActivitiesPM(ProjectActivitiesReqDto model)
        {
            //if model.StartDate.
            var (totalCount, activities) = await _repository.ActivityRepository.GetProjectPhaseActivitiesPM(model);

            try 
            {
                int totalPages = (int)Math.Ceiling((double)totalCount / (double)model.PageSize);

                return new PageResponse<IList<ActivityAndMemberDto>>()
                {
                    Data = activities,
                    Pagination = new Pagination()

                    {
                        TotalPages = totalPages,
                        PageSize = model.PageSize,
                        ActualDataSize = activities.Count,
                        TotalCount = totalCount
                    }
                };
            } catch (Exception ex) 
            {
                _logger.LogError($"Error getting activities per phase in the service layer {ex.StackTrace} {ex.Message}");
                throw new Exception("Error getting activities per phase");
            }
            
        }

        public async Task<SuccessResponse<IList<ActivityAndMemberDto>>> GetProjectActivities(ActivityDataReqDto model)
        {
            var (_, activities) = await _repository.ActivityRepository.GetProjectActivities(model);

                return new SuccessResponse<IList<ActivityAndMemberDto>>()
                {
                    Data = activities,
                    Message = "Successfully got activities"
                };

        }

    }    
}
