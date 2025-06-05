using Microsoft.AspNetCore.Mvc;
using EduSyncAPI.Services;
using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using edusync_api.Services;

namespace EduSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<FileController> _logger;

        public FileController(BlobStorageService blobStorageService, ILogger<FileController> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // Commenting out ListFiles and DownloadFile as they might not be directly applicable with Blob Storage serving via URL
        /*
        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var files = await _fileStorageService.ListFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, new { error = "Error listing files", details = ex.Message });
            }
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var (fileData, contentType, originalFileName) = await _fileStorageService.DownloadFileAsync(fileName);
                return File(fileData, contentType, originalFileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "File not found: {FileName}", fileName);
                return NotFound(new { error = "File not found", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { error = "Error downloading file", details = ex.Message });
            }
        }
        */

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                _logger.LogInformation($"Starting file upload. File name: {file.FileName}, Size: {file.Length} bytes");

                // Generate a unique filename for the blob to avoid conflicts
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                using (var stream = file.OpenReadStream())
                {
                    var fileUrl = await _blobStorageService.UploadFileAsync(
                        uniqueFileName,
                        stream,
                        file.ContentType
                    );

                    _logger.LogInformation($"File uploaded successfully. URL: {fileUrl}");

                    // Return the fileUrl in a JSON object, matching the frontend expectation
                    return Ok(new { fileUrl });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file to Blob Storage. Details: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // Commenting out DeleteFile for now, needs update for Blob Storage if needed
        /*
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(fileName);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, new { error = "Error deleting file", details = ex.Message });
            }
        }
        */
    }
} 