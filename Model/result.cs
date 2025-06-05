using EduSyncAPI.Model;
using System;

public class Result
{
    public Guid ResultId { get; set; } = Guid.NewGuid();

    public Guid AssessmentId { get; set; }
    public Assessment Assessment { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public int Score { get; set; }

    public DateTime AttemptDate { get; set; } = DateTime.UtcNow;
}
