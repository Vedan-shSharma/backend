using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSyncAPI.Model
{
    public class CourseMaterial
    {
        [Key]
        public Guid MaterialId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty; // e.g., "pdf", "docx", "pptx", "mp4", "jpg"

        [Required]
        [Url]
        public string FileUrl { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
} 