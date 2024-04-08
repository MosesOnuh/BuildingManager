//using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BuildingManager.ConfigurationModels;
using BuildingManager.Utils.Logger;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BuildingManager.Utils.StorageManager
{
    public class StorageManager : IStorageManager
    {
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _configuration;
        private readonly AwsConfiguration _awsConfiguration;
        public StorageManager (IConfiguration configuration, ILoggerManager logger) 
        {
            _configuration = configuration;
            _logger = logger;

            _awsConfiguration = new AwsConfiguration()
            {
                AccessKey = _configuration["AwsConfiguration:AWSAccessKey"],
                SecretKey = _configuration["AwsConfiguration:AWSSecretKey"]
            };

           

        }
        public async Task UploadFileAsync(StorageObject obj)
        {
            try 
            {
                var credentials = new BasicAWSCredentials(_awsConfiguration.AccessKey, _awsConfiguration.SecretKey);
                //var config = new AmazonS3Config()
                //{
                //    RegionEndpoint: Amazon.RegionEndpoint.USEast1
                //};
            using var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);

                var transferUtility = new TransferUtility(s3Client);
                var uploadRequest = new TransferUtilityUploadRequest()
                {
                    BucketName = obj.BucketName,
                    Key = obj.Name,
                    InputStream = obj.FileStream,
                    CannedACL = S3CannedACL.NoACL
                };

                await transferUtility.UploadAsync(uploadRequest);
          
            } catch (AmazonS3Exception s3Ex){
                _logger.LogError($"S3 file upload error {s3Ex.StackTrace} {s3Ex.Message}");
                throw new Exception("Error uploading file");
            }
                       
            catch (Exception ex) 
            {
                _logger.LogError($"Error Error uploading file {ex.StackTrace} {ex.Message}");
                throw new Exception("Error uploading file");
            }            
        }

        public async Task DeleteFileAsync(string bucketName, string storageFileName)
        {
            try
            {
                var credentials = new BasicAWSCredentials(_awsConfiguration.AccessKey, _awsConfiguration.SecretKey);

                using var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);

                var transferUtility = new TransferUtility(s3Client);
                var deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = storageFileName,
                };

                await transferUtility.S3Client.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError($"S3 file delete error {s3Ex.StackTrace} {s3Ex.Message}");
                throw new Exception("Error deleting file");
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error Error deleting file {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting file");
            }
        }

        public async Task<GetObjectResponse> DownloadFileAsync(string bucketName, string storageFileName)
        {
            try
            {
                var credentials = new BasicAWSCredentials(_awsConfiguration.AccessKey, _awsConfiguration.SecretKey);

                using var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);

                var transferUtility = new TransferUtility(s3Client);

                var s3Object = await s3Client.GetObjectAsync(bucketName, storageFileName);
                //return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
                return s3Object;
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError($"S3 file delete error {s3Ex.StackTrace} {s3Ex.Message}");
                throw new Exception("Error deleting file");
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error Error deleting file {ex.StackTrace} {ex.Message}");
                throw new Exception("Error deleting file");
            }
        }


    }

    public class StorageObject
    {
        public string Name { get; set; } = null!;
        public string BucketName { get; set; } = null!;
        public MemoryStream FileStream { get; set; } = null!;

    }
}