using System;
using System.ComponentModel.DataAnnotations;

namespace EduSyncAPI.Model
{
    public class FileStorage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [MaxLength(255)]
        public string OriginalFileName { get; set; }

        public string UploadedBy { get; set; }
    }
} 