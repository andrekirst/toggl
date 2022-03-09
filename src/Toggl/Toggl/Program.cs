﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Domain;
using Domain.Model;

namespace Toggl.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = CreateConfiguration(args);

        var apiToken = configuration["Toggl:Api:Token"];
        var url = configuration["Toggl:Api:BaseUrl"];

        var services = CreateServices(url);

        var mapper = services.GetRequiredService<IMapper>();

        var httpClient = services.GetRequiredService<IHttpClientFactory>().CreateClient("Toggl");

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        //var json = await GetJsonFromApi(apiToken, httpClient, cancellationToken);
        var json = GetJsonFromFile();

        var timeEntriesDataSource = JsonSerializer.Deserialize<List<TimeentryDto>>(json);

        var timeEntries = timeEntriesDataSource?.Select(t => mapper.Map<Timeentry>(t)) ?? new List<Timeentry>();

        var roundedTimeentriesResult = timeEntries.ToList()
            .Group()
            .Round();

        foreach (var roundedTimeentry in roundedTimeentriesResult.RoundedTimeentries.OrderBy(r => r.Date).ThenBy(r => r.Description))
        {
            System.Console.WriteLine($"{roundedTimeentry.Date:D} - {TimeSpan.FromSeconds(roundedTimeentry.Duration)} - {roundedTimeentry.Description}");
        }
    }

    private static async Task<string> GetJsonFromApi(string apiToken, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var password = $"{apiToken}:api_token";
        var passwordBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(password.Trim()));

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, "time_entries");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", passwordBase64);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static string GetJsonFromFile() => File.ReadAllText("testcase1.json");

    private static ServiceProvider CreateServices(string url)
    {
        var services = new ServiceCollection();
        services
            .AddHttpClient("Toggl", client => { client.BaseAddress = new Uri(url); });
        services.AddAutoMapper(typeof(Program).Assembly);
        return services.BuildServiceProvider();
    }

    public class AutoMapperMappings : Profile
    {
        public AutoMapperMappings()
        {
            CreateMap<TimeentryDto, Timeentry>();
        }
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
