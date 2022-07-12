using System.Text.Json.Serialization;

namespace Domain.Model;

#nullable disable
public class Project
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("wid")]
    public long WorkspaceId { get; set; }

    [JsonPropertyName("cid")]
    public long ClientId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("at")]
    public DateTime? At { get; set; }
}

#nullable restore