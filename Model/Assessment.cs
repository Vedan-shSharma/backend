using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduSyncAPI.Model
{
    public class Assessment
    {
        [Key]
        public Guid AssessmentId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CourseId { get; set; }  // FK to Course

        [ForeignKey("CourseId")]
        public Course Course { get; set; }  // Navigation property

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Questions { get; set; }  // JSON string of quiz questions

        [Required]
        public int MaxScore { get; set; }
    }
}
