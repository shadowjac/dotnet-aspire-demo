using MongoDB.Bson;

namespace Globant.AspireDemo.Worker.Collections;

// mongo collection
public class Todo
{
    public ObjectId Id { get; set; }
    public int RelatedId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
}
