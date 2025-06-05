using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSyncAPI.Model
{
    public class CourseEnrollment
    {
        [Key]
        public Guid EnrollmentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }
} 