public class AssessmentCreateDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; }
    public string Questions { get; set; }
    public int MaxScore { get; set; }
}
