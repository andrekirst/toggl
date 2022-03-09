namespace Domain;

public static class MapToSwoTimeentries
{
    public static SwoTimeentriesResult Map(this RoundedTimeentriesResult roundedTimeentriesResult, MapToSwoTimeentriesOptions options)
    {
        var x = new SwoTimeentriesResult
        {
            SwoTimeentries = roundedTimeentriesResult.RoundedTimeentries.Select(s => new SwoTimeentry
            {
                DurationInHours = TimeSpan.FromSeconds(s.Duration).TotalHours,
                Description = s.Description,
                ExternalDescription = s.Description,
                Date = s.Date
            }).ToList()
        };

        return x;
    }
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

public class MapToSwoTimeentriesOptions
{
}