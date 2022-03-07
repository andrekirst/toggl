using Domain.Model;

namespace Domain;

public static class GroupTimeentries
{
    public static GroupTimeentriesResult Group(this IEnumerable<Timeentry> timeentries)
    {
        var query = from timeEntry in timeentries
                    group timeEntry by new
                    {
                        timeEntry.WorkspaceId,
                        timeEntry.ProjectId,
                        timeEntry.TaskId,
                        timeEntry.Billable,
                        timeEntry.Description,
                        timeEntry.UserId,
                        timeEntry.Start.Date
                    } into grp
                    orderby grp.Key.Date
                    select new GroupedTimeentry
                    {
                        Duration = grp.Sum(s => s.Duration),
                        ProjectId = grp.Key.ProjectId,
                        WorkspaceId = grp.Key.WorkspaceId,
                        UserId = grp.Key.UserId,
                        Billable = grp.Key.Billable,
                        Description = grp.Key.Description,
                        OriginalTimeentries = grp.Select(s => s).ToList(),
                        TaskId = grp.Key.TaskId,
                        Date = grp.Key.Date,
                        Count = grp.Count()
                    };

        var result = query.ToList();

        return new GroupTimeentriesResult
        {
            GroupedTimeentries = result,
            TotalDuration = result.Sum(s => s.Duration)
        };
    }
}

public class GroupTimeentriesResult
{
    public List<GroupedTimeentry> GroupedTimeentries { get; set; } = new List<GroupedTimeentry>();
    public long TotalDuration { get; set; }
}