using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.Model;

namespace Domain.Tests;

public class JsonTestCaseLoading
{
    public static async Task<List<Timeentry>> LoadFromJson(string file, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(file, cancellationToken);
        var dtos = JsonSerializer.Deserialize<List<GroupTimeentriesTests.TimeentryDto>>(json);
        return dtos?.Select(s => new Timeentry
        {
            Duration = s.Duration,
            WorkspaceId = s.WorkspaceId,
            UserId = s.UserId,
            TaskId = s.TaskId,
            Billable = s.Billable,
            Description = s.Description,
            Tags = s.Tags,
            ProjectId = s.ProjectId,
            Start = s.Start,
            At = s.At,
            DurationOnly = s.DurationOnly,
            Guid = s.Guid,
            Id = s.Id,
            Stop = s.Stop
        }).ToList() ?? new List<Timeentry>();
    }
}