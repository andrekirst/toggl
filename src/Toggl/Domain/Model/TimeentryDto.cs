using System.Text.Json.Serialization;

namespace Domain.Model;

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