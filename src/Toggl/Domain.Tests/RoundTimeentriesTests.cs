using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using Domain.Model;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;

namespace Domain.Tests;

public class RoundTimeentriesTests
{
    [Theory, AutoData]
    public void RoundTest1()
    {
        const long userId = 1;
        const long workspaceId = 1;

        var fixture = new Fixture();

        var timeentry1 = fixture
            .Build<GroupedTimeentry>()
            .With(t => t.Duration, 10.Minutes().TotalSeconds)
            .With(t => t.UserId, userId)
            .With(t => t.WorkspaceId, workspaceId)
            .With(t => t.ProjectId, 1)
            .Create();

        var timeentry2 = fixture
            .Build<GroupedTimeentry>()
            .With(t => t.Duration, 35.Minutes().TotalSeconds)
            .With(t => t.UserId, userId)
            .With(t => t.WorkspaceId, workspaceId)
            .With(t => t.ProjectId, 2)
            .Create();

        var groupedTimeentries = new GroupTimeentriesResult
        {
            GroupedTimeentries = new List<GroupedTimeentry>
            {
                timeentry1,
                timeentry2
            },
            TotalDuration = (long)45.Minutes().TotalSeconds
        };

        var roundedTimeentries = groupedTimeentries
            .Round();

        var actualTimeentry1 = roundedTimeentries.RoundedTimeentries.First(r => r.ProjectId == 1);
        var actualTimeentry2 = roundedTimeentries.RoundedTimeentries.First(r => r.ProjectId == 2);

        actualTimeentry1.Duration.Should().Be((long)15.Minutes().TotalSeconds);
        actualTimeentry2.Duration.Should().Be((long)30.Minutes().TotalSeconds);
    }

    [Fact]
    public async Task TestCase20220216()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 16)).ToList();
        
        var roundedTimeentries = timeentries
            .Group()
            .Round();

        ValidateEqual(roundedTimeentries, "Weiterbildung AWS", 15.Minutes());
        ValidateEqual(roundedTimeentries, "Daily & Planung", 45.Minutes());
        ValidateEqual(roundedTimeentries, "Bug EMail Produktion", 4.25.Hours());
        ValidateStartsWith(roundedTimeentries, "#1388:", 2.75.Hours());
        ValidateEqual(roundedTimeentries, "Aufgabenbeschreibung", 0.25.Hours());
        ValidateEqual(roundedTimeentries, "Polizei LSA - BeWeDa: Priorisierung Features Prio2", 0.5.Hours());
        roundedTimeentries.CalculatedTotalDuration.Should().Be((long)8.75.Hours().TotalSeconds);
    }

    [Fact]
    public async Task TestCase20220217()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 17)).ToList();

        var roundedTimeentries = timeentries
            .Group()
            .Round();

        ValidateEqual(roundedTimeentries, "Weiterbildung AWS", 45.Minutes());
        ValidateEqual(roundedTimeentries, "AppSvc Devlivery monthly", 1.5.Hours());

        roundedTimeentries.CalculatedTotalDuration.Should().Be((long)8.5.Hours().TotalSeconds);
    }

    [Fact]
    public async Task TestCase20220218()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 18)).ToList();

        var roundedTimeentries = timeentries
            .Group()
            .Round();

        ValidateEqual(roundedTimeentries, "Aufwandsschätzung PSA Erweiterung", 45.Minutes());
        ValidateEqual(roundedTimeentries, "meet - Twicely", 15.Minutes());
        ValidateEqual(roundedTimeentries, "meet, daily", 15.Minutes());
        ValidateStartsWith(roundedTimeentries, "#1290: #1400:", 15.Minutes());
        ValidateEqual(roundedTimeentries, "#1353: Statusanzeige Hogrefe", 2.5.Hours());
        ValidateEqual(roundedTimeentries, "#1327: #1331: AWK - Bewerberauswahl optimieren", 30.Minutes());
        ValidateEqual(roundedTimeentries, "#1328: Drei Rater dürfen pro Bewerber Ergebnisse eingeben", 1.75.Hours());
        ValidateEqual(roundedTimeentries, "Aufgabenverteilung", 30.Minutes());
        ValidateEqual(roundedTimeentries, "Polizei LSA - BeWeDa: wöchentliches Update", 30.Minutes());

        roundedTimeentries.CalculatedTotalDuration.Should().Be((long)7.25.Hours().TotalSeconds);
    }

    [Fact]
    public async Task TestCase20220221()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 21)).ToList();

        var roundedTimeentries = timeentries
            .Group()
            .Round();

        ValidateEqual(roundedTimeentries, "Autofixture", 1.75.Hours());
        ValidateEqual(roundedTimeentries, "PSA Erweiterung Update", 30.Minutes());
        ValidateEqual(roundedTimeentries, "meet, daily", 45.Minutes());
        ValidateEqual(roundedTimeentries, "#1353: Statusanzeige Hogrefe", 2.5.Hours());
        ValidateEqual(roundedTimeentries, "#1328: Drei Rater dürfen pro Bewerber Ergebnisse eingeben", 45.Minutes());
        ValidateEqual(roundedTimeentries, "#1369: Testergebnis farblich darstellen - Deutschtest", 30.Minutes());
        ValidateEqual(roundedTimeentries, "Bug - Build läuft nicht durch", 30.Minutes());
        ValidateEqual(roundedTimeentries, "Hilfe Karl", 15.Minutes());
        ValidateEqual(roundedTimeentries, "UX-Abstimmung Weiterentwicklung AWK", 1.25.Hours());

        roundedTimeentries.CalculatedTotalDuration.Should().Be((long)8.75.Hours().TotalSeconds);
    }

    [Fact]
    public async Task TestCase20220223()
    {
        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        timeentries = timeentries.Where(t => t.Start.Date == new DateTime(2022, 2, 23)).ToList();

        var roundedTimeentries = timeentries
            .Group()
            .Round();

        ValidateEqual(roundedTimeentries, "Hilfe Rocco", 1.Hours());
        ValidateEqual(roundedTimeentries, "[Ctors] Brainstorming", 2.25.Hours());
        ValidateEqual(roundedTimeentries, "#1321: #1429: Analyse - Unterlisten", 3.25.Hours());
        ValidateEqual(roundedTimeentries, "#1403: #1422: Test in lokaler Umgebung mit VS2022", 1.5.Hours());

        roundedTimeentries.CalculatedTotalDuration.Should().Be((long)8.Hours().TotalSeconds);
    }

    private static void ValidateEqual(RoundedTimeentriesResult roundedTimeentriesResult, string description, TimeSpan duration) =>
        ValidateDelegate(roundedTimeentriesResult, description, duration, (s, s1) => s == s1);

    private static void ValidateStartsWith(RoundedTimeentriesResult roundedTimeentriesResult, string description, TimeSpan duration) =>
        ValidateDelegate(roundedTimeentriesResult, description, duration, (s, s1) => s.StartsWith(s1));

    private static void ValidateDelegate(RoundedTimeentriesResult roundedTimeentriesResult, string description, TimeSpan duration, Func<string, string, bool> func) =>
        roundedTimeentriesResult.RoundedTimeentries
            .Where(t => t.Description != null)
            .First(t => func(t.Description!, description))
            .Duration.Should().Be((long)duration.TotalSeconds);
}