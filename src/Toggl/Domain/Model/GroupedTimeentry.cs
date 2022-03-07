namespace Domain.Model;

public record GroupedTimeentry
{
    public List<Timeentry> OriginalTimeentries { get; set; } = new List<Timeentry>();
    public long WorkspaceId { get; set; }
    public long ProjectId { get; set; }
    public long TaskId { get; set; }
    public bool Billable { get; set; }
    public long Duration { get; set; }
    public string? Description { get; set; }
    public long UserId { get; set; }
    public int Count { get; set; }
    public DateTime Date { get; set; }
}