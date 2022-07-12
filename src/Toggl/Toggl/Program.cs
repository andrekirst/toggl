using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Domain;
using Domain.Model;
using Libraries.Toggl;

namespace Toggl.Console;
public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = CreateConfiguration(args);
        var services = CreateServices(configuration);
        var mapper = services.GetRequiredService<IMapper>();
        var togglClient = services.GetRequiredService<ITogglClient>();
        var tokenSource = new CancellationTokenSource();
        var cancellationtoken = tokenSource.Token;
        var startDate = DateTime.Today.FirstDayOfMonth();
        var endDate = DateTime.Today;
        var timeentries = await togglClient.GetTimeentries(startDate, endDate, cancellationtoken).ToListAsync(cancellationtoken);
        var timeentriesMapped = timeentries.Select(t => mapper.Map<Timeentry>(t)).ToList();
        var rounded = timeentriesMapped.Group().Round();
        var swoMapping = services.GetRequiredService<IMapToSwoTimeentries>();

        const string optionsFileName = "options.json";

        if (File.Exists(optionsFileName))
        {
            var options = JsonSerializer.DeserializeAsync<MapToSwoTimeentriesOptions>(File.Open("options.json", FileMode.Open), cancellationToken: cancellationtoken);
        }

        var mapToSwoTimeentriesOptions = new MapToSwoTimeentriesOptions
        {
            Mapping = new MapToSwoTimeentriesOptionsMapping
            {
                BookableResource = "Andre Kirst",
                EntryStatus = "Draft",
                DefaultType = "Work",
                DefaultTaskType = "Standard",
                DefaultRole = "Consultant",
                ProjectId = new List<ProjectIdMapping>
                {
                    new ProjectIdMapping
                    {
                        Id = 179562207,
                        Role = "Consultant",
                        ContractingUnitProject = "GSDC Germany"
                    },
                    new ProjectIdMapping
                    {
                        Id = 177237396,
                        ContractingUnitProject = "Germany (CPX_DE)"
                    }
                }
            }
        };

        var x = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(mapToSwoTimeentriesOptions));
        var swoEntries = await swoMapping.Map(rounded, mapToSwoTimeentriesOptions, cancellationtoken);
        
        // TODO Save To File
        await using var streamWriter = new StreamWriter("export.csv");
        await using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CsvMap>();
        await csv.WriteRecordsAsync(swoEntries.SwoTimeentries, cancellationtoken);
    }

    public sealed class CsvMap : ClassMap<SwoTimeentry>
    {
        public CsvMap()
        {
            var germanCulture = CultureInfo.GetCultureInfo(1031);

            Map(m => m.Date).Convert(args => args.Value.Date.ToString("dd.MM.yyyy", germanCulture));
            Map(m => m.EntryStatus).Name("Entry Status");
            Map(m => m.Type);
            Map(m => m.BookableResource).Name("Bookable Resource");
            Map(m => m.DurationInHours).Name("Duration in hours");
            Map(m => m.Description);
            Map(m => m.ExternalDescription).Name("External Description");
            Map(m => m.Project);
            Map(m => m.SwoJobProject).Name("SWO Job (Project) (Project)");
            Map(m => m.ProjectTask).Name("Project Task");
            Map(m => m.TaskType).Name("Task Type");
            Map(m => m.Role);
            Map(m => m.ContractingUnitProject).Name("Contracting Unit (Project) (Project)");
        }
    }

    private static async Task<string> GetJsonFromApi(string apiToken, HttpMessageInvoker httpClient, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        var password = $"{apiToken}:api_token";
        var passwordBase64 = Convert.ToBase64String(Encoding.Default.GetBytes(password.Trim()));

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", passwordBase64);

        var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static ServiceProvider CreateServices(IConfiguration configuration)
    {
        var url = configuration["Toggl:Api:BaseUrl"];

        var services = new ServiceCollection();
        services
            .AddHttpClient("Toggl", client => { client.BaseAddress = new Uri(url); });
        services.AddAutoMapper(typeof(Program).Assembly);
        services.Configure<TogglClientOptions>(configuration.GetSection("Toggl"));
        services
            .AddSingleton<IMapToSwoTimeentries, MapToSwoTimeentries>()
            .AddScoped<ITogglClient, TogglClient>();
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


