namespace EduSyncAPI.Dto
{
    public class AssessmentUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Questions { get; set; } = string.Empty;
        public int MaxScore { get; set; }
    }
}