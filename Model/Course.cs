using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace EduSyncAPI.Model
{
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual User Instructor { get; set; }

        public virtual ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
    }
}
