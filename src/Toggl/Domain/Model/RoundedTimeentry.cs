namespace Domain.Model;

public record RoundedTimeentry
{
    public List<Timeentry> OriginalTimeentries { get; set; } = new List<Timeentry>();
    public List<GroupedTimeentry> OriginalGroupedTimeentries { get; set; } = new List<GroupedTimeentry>();
    public long WorkspaceId { get; set; }
    public long ProjectId { get; set; }
    public long TaskId { get; set; }
    public bool Billable { get; set; }
    public long Duration { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
    public long UserId { get; set; }
    public DateTime Date { get; set; }
    public long? RoundUpDifference { get; set; }

    public static RoundedTimeentry FromGroupedWithNewDuration(GroupedTimeentry groupedTimeentry, long duration) =>
        new RoundedTimeentry
        {
            Duration = duration,
            WorkspaceId = groupedTimeentry.WorkspaceId,
            ProjectId = groupedTimeentry.ProjectId,
            UserId = groupedTimeentry.UserId,
            TaskId = groupedTimeentry.TaskId,
            Billable = groupedTimeentry.Billable,
            Description = groupedTimeentry.Description,
            OriginalTimeentries = groupedTimeentry.OriginalTimeentries,
            OriginalGroupedTimeentries = new List<GroupedTimeentry> { groupedTimeentry },
            Date = groupedTimeentry.Date
        };

    public void RemoveRoundUpDifference() => RoundUpDifference = null;
}