using System.Diagnostics;
using Domain.Model;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Libraries.Toggl;

namespace Domain;

public interface IMapToSwoTimeentries
{
    Task<SwoTimeentriesResult> Map(RoundedTimeentriesResult roundedTimeentriesResult, MapToSwoTimeentriesOptions options, CancellationToken cancellationToken = default);
}

public class MapToSwoTimeentries : IMapToSwoTimeentries
{
    private readonly ITogglClient _togglClient;

    public MapToSwoTimeentries(ITogglClient togglClient)
    {
        _togglClient = togglClient;
    }

    public async Task<SwoTimeentriesResult> Map(RoundedTimeentriesResult roundedTimeentriesResult, MapToSwoTimeentriesOptions options, CancellationToken cancellationToken = default)
    {
        var projects = await _togglClient.GetProjects(roundedTimeentriesResult.DistinctProjectIds, cancellationToken);
        var projectTasks = new List<ProjectTask>();
        foreach (var project in projects)
        {
            projectTasks.AddRange(await _togglClient.GetProjectTasks(project.Id, cancellationToken));
        }

        var projectTaskMapping = projectTasks.ToDictionary(s => $"{s.ProjectId}:{s.Id}", s => s.Name);
        var projectIdNameMapping = projects.ToDictionary(s => s.Id, s => s.Name);
        var projectTypeMapping = options.Mapping.ProjectId?.ToDictionary(s => s.Id, s => s.Type) ?? new Dictionary<long, string>();
        var swoJobMapping = options.Mapping?.ProjectId?.ToDictionary(s => s.Id, s => s.SwoJob) ?? new Dictionary<long, string>();
        var roleMapping = options.Mapping?.ProjectId?.ToDictionary(s => s.Id, s => s.Role) ?? new Dictionary<long, string>();
        var contractingUnitProjectMapping = options.Mapping?.ProjectId?.ToDictionary(s => s.Id, s => s.ContractingUnitProject) ?? new Dictionary<long, string>();

        return new SwoTimeentriesResult
        {
            SwoTimeentries = roundedTimeentriesResult.RoundedTimeentries.Select(s => new SwoTimeentry
            {
                DurationInHours = TimeSpan.FromSeconds(s.Duration).TotalHours,
                Description = s.Description,
                ExternalDescription = s.Description,
                Date = s.Date,
                BookableResource = options.Mapping?.BookableResource,
                EntryStatus = options.Mapping?.EntryStatus,
                Project = projectIdNameMapping.GetValueOrDefault(s.ProjectId),
                SwoJobProject = swoJobMapping.GetValueOrDefault(s.ProjectId),
                ProjectTask = projectTaskMapping.GetValueOrDefault($"{s.ProjectId}:{s.TaskId}"),
                TaskType = options.Mapping?.ProjectId?.FirstOrDefault(p => p.Id == s.ProjectId)?.ProjectIdTaskIdMappings?.FirstOrDefault(p => p.TaskId == s.TaskId)?.TaskType ?? options.Mapping?.DefaultTaskType,
                Role = roleMapping.GetValueOrDefault(s.ProjectId) ?? options.Mapping?.DefaultRole,
                ContractingUnitProject = contractingUnitProjectMapping.GetValueOrDefault(s.ProjectId),
                Type = projectTypeMapping.GetValueOrDefault(s.ProjectId) ?? options.Mapping?.DefaultType
            }).ToList()
        };
    }
}

public interface ITogglClient
{
    Task<List<Project>> GetProjects(IEnumerable<long> projectIds, CancellationToken cancellationToken = default);
    IAsyncEnumerable<TimeentryDto> GetTimeentries(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<ProjectTask>> GetProjectTasks(long projectId, CancellationToken cancellationToken = default);
}

public class TogglClient : ITogglClient
{
    private readonly IOptions<TogglClientOptions> _options;
    private readonly HttpClient _httpClient;

    public TogglClient(
        IHttpClientFactory httpClientFactory,
        IOptions<TogglClientOptions> options)
    {
        _options = options;
        _httpClient = httpClientFactory.CreateClient("Toggl");
    }

    public async Task<List<Project>> GetProjects(IEnumerable<long> projectIds, CancellationToken cancellationToken = default)
    {
        var projects = new List<Project>();
        foreach (var projectId in projectIds.Where(p => p != 0))
        {
            var json = await GetJsonFromApi(_httpClient, new HttpRequestMessage(HttpMethod.Get, $"projects/{projectId}"), cancellationToken);
            json = JsonDocument.Parse(json).RootElement.GetProperty("data").ToString();
            Debug.Print(json);
            var project = JsonSerializer.Deserialize<Project>(json);
            if (project != null)
            {
                projects.Add(project);
            }
        }
        return projects;
    }

    public async IAsyncEnumerable<TimeentryDto> GetTimeentries(DateTime? startDate = null, DateTime? endDate = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var dateRangeFilterQuery = "";
        if (startDate != null && endDate != null)
        {
            dateRangeFilterQuery = $"?start_date={startDate.Value.ToIso8601()}&end_date={endDate.Value.ToIso8601()}";
        }

        var json = await GetStreamFromApi(_httpClient, new HttpRequestMessage(HttpMethod.Get, $"time_entries{dateRangeFilterQuery}"), cancellationToken);
        await foreach (var timeentry in JsonSerializer.DeserializeAsyncEnumerable<TimeentryDto>(json, cancellationToken: cancellationToken))
        {
            if (timeentry != null)
            {
                yield return timeentry;
            }
        }
    }

    public async Task<List<ProjectTask>> GetProjectTasks(long projectId, CancellationToken cancellationToken = default)
    {
        var json = await GetJsonFromApi(_httpClient, new HttpRequestMessage(HttpMethod.Get, $"projects/{projectId}/tasks"), cancellationToken);
        var tasks = JsonSerializer.Deserialize<List<ProjectTask>>(json) ?? new List<ProjectTask>();
        return tasks;
    }

    private async Task<string> GetJsonFromApi(HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var apiToken = _options.Value.Api.Token;

        var password = $"{apiToken}:api_token";
        var passwordBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(password.Trim()));

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", passwordBase64);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<Stream> GetStreamFromApi(HttpClient httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
    {
        var apiToken = _options.Value.Api.Token;

        var password = $"{apiToken}:api_token";
        var passwordBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(password.Trim()));

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", passwordBase64);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
}

public class TogglClientOptions
{
    public TogglClientOptionsApi Api { get; set; }
}

public class TogglClientOptionsApi
{
    public string Token { get; set; }
}

#nullable disable
public class SwoTimeentriesResult
{
    public List<SwoTimeentry> SwoTimeentries { get; set; }
}

public class SwoTimeentry
{
    public DateTime Date { get; set; }
    public string EntryStatus { get; set; }
    public string Type { get; set; }
    public string BookableResource { get; set; }
    public double DurationInHours { get; set; }
    public string Description { get; set; }
    public string ExternalDescription { get; set; }
    public string Project { get; set; }
    public string SwoJobProject { get; set; }
    public string ProjectTask { get; set; }
    public string TaskType { get; set; }
    public string Role { get; set; }
    public string ContractingUnitProject { get; set; }
}
#nullable restore

#nullable disable
public class MapToSwoTimeentriesOptions
{
    public MapToSwoTimeentriesOptionsMapping Mapping { get; set; }
}

public class MapToSwoTimeentriesOptionsMapping
{
    public string BookableResource { get; set; }
    public string EntryStatus { get; set; }
    public List<ProjectIdMapping> ProjectId { get; set; }
    public string DefaultType { get; set; }
    public string DefaultTaskType { get; set; }
    public string DefaultRole { get; set; }
}

public class ProjectIdMapping
{
    public long Id { get; set; }
    public string SwoJob { get; set; }
    public string Role { get; set; }
    public List<ProjectIdTaskIdMapping> ProjectIdTaskIdMappings { get; set; }
    public string ContractingUnitProject { get; set; }
    public string Type { get; set; }
}

public class ProjectIdTaskIdMapping
{
    public long TaskId { get; set; }
    public string TaskType { get; set; }
}
#nullable restore