using System.Collections.Generic;

namespace EduSyncAPI.Dto
{
    public class CourseWithInstructorDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public List<CourseMaterialDto> Materials { get; set; } = new List<CourseMaterialDto>();
    }

    public class CourseMaterialDto
    {
        public Guid MaterialId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
    }
}
