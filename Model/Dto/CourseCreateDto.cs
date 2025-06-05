using EduSyncAPI.Model;

namespace EduSyncAPI.Dto
{
    public class CourseCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid InstructorId { get; set; }  // FK to User
    }
} 