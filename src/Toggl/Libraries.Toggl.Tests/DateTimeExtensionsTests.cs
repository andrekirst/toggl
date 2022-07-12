using System;
using FluentAssertions;
using Xunit;
namespace Libraries.Toggl.Tests;

public class DateTimeExtensionsTests
{
    [Fact]
    public void TogglTestExamples()
    {
        var date = DateTime.Parse("2013-03-10T15:42:46+02:00");

        var actual = date.ToIso8601();

        actual.Should().Be("2013-03-10T15%3A42%3A46%2B02%3A00");
    }
}