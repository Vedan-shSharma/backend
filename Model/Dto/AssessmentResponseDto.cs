using System;

namespace EduSyncAPI.Dto
{
    public class AssessmentResponseDto
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; }
        public string Questions { get; set; }
        public int MaxScore { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string InstructorName { get; set; }
    }
} 