namespace Globant.AspireDemo.Contracts;

public record TodoCreated(int Id, string Title, string Description, DateTime? DueDate, string Status);
