using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Domain.Tests;

public class MapToSwoTimeentryTests
{
    [Fact]
    public async Task EinenEintrag()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        var date = new DateTime(2022, 2, 16);

        var rounded = timeentries
            .Where(t => t.Start.Date == date)
            .Group()
            .Round()
            .Map(new MapToSwoTimeentriesOptions());

        rounded.SwoTimeentries.First(f => f.Description == "Weiterbildung AWS").DurationInHours.Should().Be(0.25);
        rounded.SwoTimeentries.First(f => f.Description == "Weiterbildung AWS").Date.Should().Be(date);
    }
}