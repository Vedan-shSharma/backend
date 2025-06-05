namespace EduSyncAPI.Dto
{
    public class AssessmentAttemptDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public List<int> SelectedAnswers { get; set; } // Indices chosen by the student
    }
}
