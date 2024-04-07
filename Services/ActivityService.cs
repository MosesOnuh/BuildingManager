using BuildingManager.Contracts.Services;
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

            //send file to S3 bucket
            //write procedure to create Activity

            await using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            var fileExt = Path.GetExtension(model.File.FileName);

            //use breakpoint to see file name before uploading to cloud
            var documentName = $"{ Guid.NewGuid()}.{fileExt}";

            var s3Object = new StorageObject()
            {
                //use config to get value : _configuration.GetValue
                BucketName = "",
                FileStream = memoryStream,
                Name = documentName
            };

            
            await _storage.UploadFileAsync(s3Object);

            var activity = new Activity
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = model.ProjectId,
                UserId = userId,
                Name = model.Name,
                //@Todo: check to ensure type casting works well with Emum
                Status = (int)ActivityStatus.Pending,
                Description = model.Description,
                //@Todo: when validating the request ensure that the value is only possible enum values
                ProjectPhase = model.ProjectPhase,
                FileName = model.File.FileName,
                StorageFileName = documentName,
                FileExtension = fileExt,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                CreatedAt = DateTime.Now,
                //UpdatedAt = DateTime.Now,
                UpdatedAt = null,
            };

            //@todo: try catch: delete file if error occurs here
            await _repository.ActivityRepository.CreateActivity(activity);
            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity created successfully",
            };

            //  if error occurs delete activityfile for cloud and notify user in error message
        }


        //PM approves or rejects an activity
        public async Task<SuccessResponse<ActivityDto>> ActivityApproval(ActivityStatusUpdateDto model)
        {
            //write procedure to check the status of the activity and if the status is one then 
            // the status can be updated to 2 or 3

            //validate the StatusAction and ensure that it is either 2 or 3
            if (model.StatusAction != (int)ActivityStatus.Approved || model.StatusAction != (int)ActivityStatus.Rejected)
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
                _logger.LogError($"Error attempted to approve or reject an activity that is not pending");
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

        public async Task<SuccessResponse<ActivityDto>> UpdateActivityToDone(ActivityStatusUpdateDto model, string userId)
        {
            //write procedure to check the status of the activity and if the status is two then and
            //there is start date and finish date
            // the status can be updated to 4

            //validate the StatusAction and ensure that it is 4
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
                _logger.LogError($"Error activity has not been approved, only approved activity can be changed to done");
                throw new Exception("Error activity has not been approved, only approved activity can be changed to done");
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

        public async Task<SuccessResponse<ActivityDto>> UpdatePendingActivityFile(AddActivityFileRequestDto model, string userId)
        {
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
                BucketName = "",
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
                throw new RestException(HttpStatusCode.NotFound, "Error activity to be delete was not found.");
            }

            if (activity.Status != (int)ActivityStatus.Rejected)
            {
                _logger.LogError($"Error attempted to delete an activity that is not rejected");
                throw new Exception("Error cannot delete an activity that is not rejected");
            }

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file to delete.");
            }

            //DeleteActivity file gotten from the above function
            //use config to get value : _configuration.GetValue
            await _storage.DeleteFileAsync("@todo__BucketName", activity.StorageFileName);

            //Write procedure to delete only pending activities
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


        //@todo procedure to remove FileName, StorageFileName, FileExtension after deleting a file in the database
        public async Task<SuccessResponse<ActivityDto>> DeleteActivityFile(string projId, string activityId, string userId)
        {
            //get file details from activity gotten from db
            //delete file in cloud
            //deleteactivity in db

            var activity = await _repository.ActivityRepository.GetActivityOtherPro(projId, activityId, userId);

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
            await _storage.DeleteFileAsync("@todo__BucketName", activity.StorageFileName);

            var (rowsUpdated, returnNum) = await _repository.ActivityRepository.RemoveActivityFileDetails(projId, activityId, userId);

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
                Message = "Activity deleted successfully",
            };
        }

        public async Task<GetObjectResponse> DownloadActivityFileOtherPro(DownloadActivityFileDto model, string userId) 
        {
            var activity = await _repository.ActivityRepository.GetActivityOtherPro(model.ProjectId, model.ActivityId, userId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when downloading the activity File. The required activity was not found, check ActivityId, ProjectId and UserId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be downloaded was not found.");
            }

            //if (activity.Status != (int)ActivityStatus.Pending)
            //{
            //    _logger.LogError($"Error attempted to delete the file of an activity that is not pending");
            //    throw new Exception("Error cannot delete the file of an activity that is not pending");
            //}

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file.");
            }


            //DeleteActivity file gotten from the above function
            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync("@todo__BucketName", activity.StorageFileName);
            return file;
        }

        public async Task<GetObjectResponse> DownloadActivityFilePM(DownloadActivityFileDto model)
        {
            var activity = await _repository.ActivityRepository.GetActivityPM(model.ProjectId, model.ActivityId);

            if (activity == null)
            {
                _logger.LogError($"Error occurred when downloading the activity File. The required activity was not found, check ActivityId and ProjectId provided");
                throw new RestException(HttpStatusCode.NotFound, "Error activity whose file is to be downloaded was not found.");
            }

            //if (activity.Status != (int)ActivityStatus.Pending)
            //{
            //    _logger.LogError($"Error attempted to delete the file of an activity that is not pending");
            //    throw new Exception("Error cannot delete the file of an activity that is not pending");
            //}

            if (activity.StorageFileName == null)
            {
                _logger.LogError($"Error activity does not have any file stored");
                throw new RestException(HttpStatusCode.InternalServerError, "Error activity does not have a file.");
            }


            //DeleteActivity file gotten from the above function
            //use config to get value : _configuration.GetValue
            var file = await _storage.DownloadFileAsync("@todo__BucketName", activity.StorageFileName);
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
                FileType = activity.FileExtension,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                //ActualEndDate = activity.ActualEndDate.HasValue ? activity.ActualEndDate.Value : null,
                ActualStartDate = activity.ActualStartDate,
                ActualEndDate = activity.ActualEndDate,
                CreatedAt = activity.CreatedAt
            };


            return new SuccessResponse<ActivityDto>
            {
                Message = "Activity deleted successfully",
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

            //var clientActivity = new ActivityDto()
            //{
            //    Id = activity.Id,
            //    ProjectId = activity.ProjectId,
            //    UserId = activity.UserId,
            //    Name = activity.Name,
            //    Status = activity.Status,
            //    Description = activity.Description,
            //    ProjectPhase = activity.ProjectPhase,
            //    FileName = activity.FileName,
            //    StorageFileName = activity.StorageFileName,
            //    FileType = activity.FileExtension,
            //    StartDate = activity.StartDate,
            //    EndDate = activity.EndDate,
            //    ActualStartDate = activity.ActualStartDate,
            //    ActualEndDate = activity.ActualEndDate,
            //    CreatedAt = activity.CreatedAt
            //};

            return new SuccessResponse<ActivityAndMemberDto>
            {
                Message = "Activity get activity successfully",
                Data = activity,
            };
        }

        public async Task<PageResponse<IList<ActivityDto>>> GetProjectPhaseActivitiesOtherPro(ActivitiesDtoPaged model, string UserId)
        {
            var (totalCount, activities) = await _repository.ActivityRepository.GetProjectPhaseActivitiesOtherPro(model, UserId);

            //try-catch here
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
        }

        public async Task<PageResponse<IList<ActivityAndMemberDto>>> GetProjectPhaseActivitiesPM(ActivitiesDtoPaged model)
        {
            var (totalCount, activities) = await _repository.ActivityRepository.GetProjectPhaseActivitiesPM(model);

            //try-catch here
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
        }
    }    
}
