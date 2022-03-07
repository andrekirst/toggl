using AutoFixture;
using Domain.Model;
using FluentAssertions.Extensions;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;
using System.Text.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace Domain.Tests;

public class GroupTimeentriesTests
{
    [Theory, AutoData]
    public void GroupTest1()
    {
        const long userId = 1;
        const long workspaceId = 1;

        var fixture = new Fixture();

        var timeentry1 = fixture
            .Build<Timeentry>()
            .With(t => t.Duration, 10.Minutes().TotalSeconds)
            .With(t => t.UserId, userId)
            .With(t => t.WorkspaceId, workspaceId)
            .With(t => t.ProjectId, 1)
            .Create();

        var timeentry2 = fixture
            .Build<Timeentry>()
            .With(t => t.Duration, 35.Minutes().TotalSeconds)
            .With(t => t.UserId, userId)
            .With(t => t.WorkspaceId, workspaceId)
            .With(t => t.ProjectId, 2)
            .Create();

        var timeentries = new List<Timeentry>
        {
            timeentry1,
            timeentry2
        };

        var actual = timeentries.Group();

        actual.GroupedTimeentries.Should().HaveCount(2);
    }

    [Fact]
    public async Task SumOfDurations()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 16)).ToList();
        var grouped = timeentries.Group();

        grouped.GroupedTimeentries.First(t => t.Description == "Bug EMail Produktion").Duration.Should().Be(15309);
        grouped.GroupedTimeentries.First(t => t.Description == "Weiterbildung AWS").Duration.Should().Be(889);
        grouped.GroupedTimeentries.First(t => t.Description == "Daily & Planung").Duration.Should().Be(2729);
        grouped.GroupedTimeentries.First(t => t.Description!.StartsWith("#1388:")).Duration.Should().Be(10455);
        grouped.GroupedTimeentries.First(t => t.Description == "Aufgabenbeschreibung").Duration.Should().Be(812);
        grouped.GroupedTimeentries.First(t => t.Description == "Polizei LSA - BeWeDa: Priorisierung Features Prio2").Duration.Should().Be(1539);
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
}