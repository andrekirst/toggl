using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toggl.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = CreateConfiguration(args);

        var apiToken = configuration["Toggl:Api:Token"];
        var url = configuration["Toggl:Api:BaseUrl"];

        var services = CreateServices(url);
        var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Toggl");

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;


        var password = $"{apiToken}:api_token";
        var passwordBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(password.Trim()));

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "time_entries");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", passwordBase64);

        var x = await httpClient.SendAsync(requestMessage, cancellationToken);
        var json = await x.Content.ReadAsStringAsync(cancellationToken);

        var timeEntriesDataSource = JsonSerializer.Deserialize<List<TimeentryDto>>(json);

        var query = from timeEntry in timeEntriesDataSource
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
                    select new
                    {
                        grp.Key,
                        Duration = grp.Sum(s => s.Duration),
                        DurationTime = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(grp.Sum(s => s.Duration))),
                        Count = grp.Count(),
                        Items = grp.Select(s => s).ToList()
                    };

        var list = query.ToList();
    }

    private static ServiceProvider CreateServices(string url)
    {
        var services = new ServiceCollection();
        services.AddHttpClient("Toggl", client => { client.BaseAddress = new Uri(url); });
        return services.BuildServiceProvider();
    }

    private static IConfigurationRoot CreateConfiguration(string[] args) =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false)
            .AddUserSecrets<Program>(true, false)
            .AddCommandLine(args)
            .AddEnvironmentVariables()
            .Build();
}

public interface IToggl
{
    IEnumerable<TimeentryDto> GetTimeentries(DateTime? startDate = null, DateTime? endDate = null);
}

//[JsonSerializable(typeof(List<TimeentryDto>))]
//public partial class TimeentryDtoJsonContext : JsonSerializerContext
//{
//}

public record TimeentryDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("guid")]
    public string? Guid { get; set; }

    [JsonPropertyName("wid")]
    public long WorkspaceId { get; set; }

    [JsonPropertyName("pid")]
    public long ProjectId { get; set; }

    [JsonPropertyName("tid")]
    public long TaskId { get; set; }

    [JsonPropertyName("billable")]
    public bool Billable { get; set; }

    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("stop")]
    public DateTime? Stop { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duronly")]
    public bool? DurationOnly { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("at")]
    public DateTime? At { get; set; }

    [JsonPropertyName("uid")]
    public long UserId { get; set; }
}
