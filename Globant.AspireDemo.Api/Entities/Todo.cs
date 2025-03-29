namespace Globant.AspireDemo.Api.Entities;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed
}
