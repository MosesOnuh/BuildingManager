using Amazon.S3.Model;
using System.Threading.Tasks;

namespace BuildingManager.Utils.StorageManager
{
    public interface IStorageManager
    {
        Task UploadFileAsync(StorageObject obj);
        Task DeleteFileAsync(string bucketName, string storageFileName);
        Task<GetObjectResponse> DownloadFileAsync(string bucketName, string storageFileName);
    }
}



