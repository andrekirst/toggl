using System.Text.Json.Serialization;

namespace Domain.Model;

#nullable disable
public class ProjectTask
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("wid")]
    public int WorkspaceId { get; set; }

    [JsonPropertyName("pid")]
    public int ProjectId { get; set; }
}
#nullable restore