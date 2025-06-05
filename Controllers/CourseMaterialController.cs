using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncAPI.Data;
using EduSyncAPI.Model;
using EduSyncAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using EduSyncAPI.Dto;
using edusync_api.Services;

namespace EduSyncAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseMaterialController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<CourseMaterialController> _logger;

        public CourseMaterialController(
            EduSyncDbContext context,
            BlobStorageService blobStorageService,
            ILogger<CourseMaterialController> logger)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        // GET: api/CourseMaterial/{courseId}
        [HttpGet("{courseId}")]
        public async Task<ActionResult<IEnumerable<CourseMaterialDto>>> GetCourseMaterials(Guid courseId, [FromQuery] Guid? studentId = null)
        {
            // If studentId is provided, check enrollment
            if (studentId.HasValue)
            {
                var isEnrolled = await _context.CourseEnrollments
                    .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId.Value);

                if (!isEnrolled)
                {
                    return BadRequest(new { message = "You must be enrolled in this course to access its materials." });
                }
            }

            var materials = await _context.CourseMaterials
                .Where(m => m.CourseId == courseId)
                .Select(m => new CourseMaterialDto
                {
                    MaterialId = m.MaterialId,
                    FileName = m.FileName,
                    FileType = m.FileType,
                    FileUrl = m.FileUrl,
                    Description = m.Description,
                    UploadDate = m.UploadDate
                })
                .ToListAsync();

            return Ok(materials);
        }

        // POST: api/CourseMaterial/{courseId}
        [HttpPost("{courseId}")]
        public async Task<ActionResult<CourseMaterialDto>> UploadMaterial(Guid courseId, IFormFile file, [FromQuery] string description = "")
        {
            _logger.LogInformation("Attempting to upload material for CourseId: {CourseId}", courseId);

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Verify course exists
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound("Course not found");

            // Get file extension and validate file type
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".pptx", ".mp4", ".jpg", ".jpeg", ".png" };
            
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Allowed types: PDF, DOCX, PPTX, MP4, JPG, JPEG, PNG");

            try
            {
                // Upload to blob storage
                using var stream = file.OpenReadStream();
                var contentType = file.ContentType;
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var fileUrl = await _blobStorageService.UploadFileAsync(fileName, stream, contentType);

                // Create material record
                var material = new CourseMaterial
                {
                    CourseId = courseId,
                    FileName = file.FileName,
                    FileType = fileExtension.TrimStart('.'),
                    FileUrl = fileUrl,
                    Description = description,
                    UploadDate = DateTime.UtcNow
                };

                _context.CourseMaterials.Add(material);
                await _context.SaveChangesAsync();

                return Ok(new CourseMaterialDto
                {
                    MaterialId = material.MaterialId,
                    FileName = material.FileName,
                    FileType = material.FileType,
                    FileUrl = material.FileUrl,
                    Description = material.Description,
                    UploadDate = material.UploadDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading course material");
                return StatusCode(500, "Error uploading file");
            }
        }

        // DELETE: api/CourseMaterial/{materialId}
        [HttpDelete("{materialId}")]
        public async Task<IActionResult> DeleteMaterial(Guid materialId)
        {
            var material = await _context.CourseMaterials.FindAsync(materialId);
            if (material == null)
                return NotFound();

            try
            {
                // Delete from blob storage
                var fileName = Path.GetFileName(material.FileUrl);
                await _blobStorageService.DeleteFileAsync(fileName);

                // Delete from database
                _context.CourseMaterials.Remove(material);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting course material");
                return StatusCode(500, "Error deleting file");
            }
        }
    }
} 