namespace Domain.Model;

public class Timeentry
{
    public long Id { get; set; }
    public string? Guid { get; set; }
    public long WorkspaceId { get; set; }
    public long ProjectId { get; set; }
    public long TaskId { get; set; }
    public bool Billable { get; set; }
    public DateTime Start { get; set; }
    public DateTime? Stop { get; set; }
    public long Duration { get; set; }
    public string? Description { get; set; }
    public bool? DurationOnly { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime? At { get; set; }
    public long UserId { get; set; }
}