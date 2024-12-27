namespace onlinesinavsistemifinal.Models;

public class ExamResult
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int ExamId { get; set; }
    public int Score { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public virtual Exam? Exam { get; set; }
}
