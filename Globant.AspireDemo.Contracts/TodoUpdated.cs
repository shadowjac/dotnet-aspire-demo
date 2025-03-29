namespace Globant.AspireDemo.Contracts;
public record TodoUpdated(int Id, string Title, string Description, DateTime? DueDate, string Status);
