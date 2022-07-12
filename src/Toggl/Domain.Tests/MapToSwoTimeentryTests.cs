using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Domain.Model;
using Domain.Tests.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace Domain.Tests;

public class MapToSwoTimeentryTests
{
    [Theory]
    [AutoMoqData]
    public async Task EinenEintrag(
        [Frozen] Mock<ITogglClient> togglClient,
        MapToSwoTimeentries sut)
    {
        togglClient
            .Setup(s => s.GetProjects(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<Project>
            {
                new Project{ Id = 176351743, Name = "Application Services DACH recruiting, training, internal improvements"}
            });

        togglClient
            .Setup(s => s.GetProjectTasks(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<ProjectTask>
            {
                new ProjectTask { Id = 78058952, ProjectId = 176351743, Name = "Trainings and Certifications", WorkspaceId = 2535378 }
            });

        var timeentries = await JsonTestCaseLoading.LoadFromJson("testcase1.json");
        var date = new DateTime(2022, 2, 16);

        var fixture = new Fixture();

        var mapToSwoTimeentriesOptions = GetMapToSwoTimeentriesOptions20200216(fixture);

        var rounded = timeentries
            .Where(t => t.Start.Date == date)
            .Group()
            .Round();

        var result = await sut.Map(rounded, mapToSwoTimeentriesOptions);

        TestWeiterbildungAws(result, date, mapToSwoTimeentriesOptions);
    }

    private static MapToSwoTimeentriesOptions GetMapToSwoTimeentriesOptions20200216(ISpecimenBuilder fixture)
    {
        var mapToSwoTimeentriesOptions = new MapToSwoTimeentriesOptions
        {
            Mapping = new MapToSwoTimeentriesOptionsMapping
            {
                BookableResource = fixture.Create<string>(),
                EntryStatus = fixture.Create<string>(),
                ProjectId = new List<ProjectIdMapping>
                {
                    new ProjectIdMapping
                    {
                        Id = 176351743,
                        SwoJob = null,
                        Role = "Consultant",
                        ContractingUnitProject = "GSDC Germany",
                        Type = "Work",
                        ProjectIdTaskIdMappings = new List<ProjectIdTaskIdMapping>
                        {
                            new ProjectIdTaskIdMapping
                            {
                                TaskId = 78058952,
                                TaskType = "Standard"
                            }
                        }
                    }
                }
            }
        };
        return mapToSwoTimeentriesOptions;
    }

    private static void TestWeiterbildungAws(SwoTimeentriesResult result, DateTime date, MapToSwoTimeentriesOptions mapToSwoTimeentriesOptions)
    {
        var weiterbildungAwsTimeentry = result.SwoTimeentries.First(f => f.Description == "Weiterbildung AWS");
        weiterbildungAwsTimeentry.DurationInHours.Should().Be(0.25);
        weiterbildungAwsTimeentry.Date.Should().Be(date);
        weiterbildungAwsTimeentry.BookableResource.Should().Be(mapToSwoTimeentriesOptions.Mapping.BookableResource);
        weiterbildungAwsTimeentry.EntryStatus.Should().Be(mapToSwoTimeentriesOptions.Mapping.EntryStatus);
        weiterbildungAwsTimeentry.Project.Should().Be("Application Services DACH recruiting, training, internal improvements");
        weiterbildungAwsTimeentry.SwoJobProject.Should().BeNullOrEmpty();
        weiterbildungAwsTimeentry.ProjectTask.Should().Be("Trainings and Certifications");
        weiterbildungAwsTimeentry.TaskType.Should().Be("Standard");
        weiterbildungAwsTimeentry.Role.Should().Be("Consultant");
        weiterbildungAwsTimeentry.ContractingUnitProject.Should().Be("GSDC Germany");
        weiterbildungAwsTimeentry.Type.Should().Be("Work");
    }
}