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


[HttpGet("{id}/download")]
public async Task<IActionResult> DownloadBook(int id)
{
    var book = await _dbContext.Books.FindAsync(id);
    if (book == null)
        return NotFound();

    var pdfFileName = book.PdfFileName;

    // Download PDF file from S3
    var fileTransferUtility = new TransferUtility(_s3Client);
    var downloadRequest = new TransferUtilityDownloadRequest
    {
        BucketName = "your-s3-bucket",
        Key = pdfFileName
    };
    using (var memoryStream = new MemoryStream())
    {
        await fileTransferUtility.DownloadAsync(memoryStream, downloadRequest);

        // Return file as FileStreamResult
        return File(memoryStream.ToArray(), "application/pdf", $"{book.Name}.pdf");
    }
}

var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
return File(s3Object.ResponseStream, s3Object.Headers.ContentType);