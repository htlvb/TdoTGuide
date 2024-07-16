using FsCheck;
using FsCheck.Fluent;
using TdoTGuide.WebAsm.Server.Data;
using TdoTGuide.WebAsm.Shared;
using System.Net.Mail;

namespace TdoTGuide.WebAsm.Server.IntegrationTests;

public static class CustomGenerators
{
    public static Arbitrary<ProjectOrganizer> ProjectOrganizerGenerator()
    {
        var gen =
            from id in ArbMap.Default.GeneratorFor<Guid>()
            from firstName in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from lastName in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from shortName in ArbMap.Default.GeneratorFor<NonEmptyString>()
            select new ProjectOrganizer(id.ToString(), firstName.Get, lastName.Get, shortName.Get);
        return Arb.From(gen);
    }

    public static Arbitrary<DateOnly> ProjectDateGenerator()
    {
        var gen =
            from offset in Gen.Choose(-100, 100)
            let date = DateTime.Today.AddDays(offset)
            select DateOnly.FromDateTime(date);
        return Arb.From(gen);
    }

    public record StartTime(TimeOnly Time);

    public static Arbitrary<StartTime> StartTimeGenerator()
    {
        var gen =
            from hour in Gen.Choose(5, 22)
            from quarter in Gen.Choose(0, 3)
            let time = new TimeOnly(hour, quarter * 15)
            select new StartTime(time);
        return Arb.From(gen);
    }

    public static Arbitrary<TimeOnly?> EndTimeGenerator(StartTime startTime)
    {
        var maxDurationQuarters = 24 * 4 - 1 - startTime.Time.Hour * 4 - startTime.Time.Minute / 15;
        var gen = Gen.OneOf(
            Gen.Constant<TimeOnly?>(default),

            from durationQuarters in Gen.Choose(1, maxDurationQuarters)
            let time = startTime.Time.Add(TimeSpan.FromMinutes(15 * durationQuarters))
            select (TimeOnly?)time
        );
        return Arb.From(gen);
    }

    public record ClosingDate(DateTime DateTime);

    public static Arbitrary<ClosingDate> ClosingDateGenerator(DateOnly projectDate)
    {
        var daysToProject = Math.Abs(projectDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber);
        var gen =
            from offsetMinutes in Gen.Choose(0, 2 * daysToProject * 24 * 60)
            let date = projectDate.ToDateTime(TimeOnly.MinValue).AddMinutes(-offsetMinutes)
            select new ClosingDate(date);
        return Arb.From(gen);
    }

    public static Arbitrary<Project> ProjectGenerator()
    {
        var gen =
            from id in ArbMap.Default.GeneratorFor<Guid>()
            from title in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from description in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from location in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from organizer in ProjectOrganizerGenerator().Generator
            from coOrganizers in ProjectOrganizerGenerator().Array().Generator
            from date in ProjectDateGenerator().Generator
            from startTime in StartTimeGenerator().Generator
            from endTime in EndTimeGenerator(startTime).Generator
            select new Project(
                id.ToString(),
                title.Get,
                description.Get,
                location.Get,
                organizer,
                coOrganizers,
                date,
                startTime.Time,
                endTime
            );
        return Arb.From(gen);
    }

    public record FutureProject(Project Project);

    public static Arbitrary<FutureProject> FutureProjectGenerator()
    {
        var gen =
            from project in ProjectGenerator().Generator
            where project.Date > DateOnly.FromDateTime(DateTime.Today)
            select new FutureProject(project);
        return Arb.From(gen);
    }

    public static Arbitrary<EditingProjectDataDto> EditingProjectDataDtoGenerator()
    {
        var gen =
            from title in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from description in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from location in ArbMap.Default.GeneratorFor<NonEmptyString>()
            from organizerId in ArbMap.Default.GeneratorFor<Guid>()
            from coOrganizerIds in ArbMap.Default.ArbFor<Guid>().Array().Generator
            from date in ProjectDateGenerator().Generator
            from startTime in StartTimeGenerator().Generator
            from endTime in EndTimeGenerator(startTime).Generator
            select new EditingProjectDataDto(
                title.Get,
                description.Get,
                location.Get,
                organizerId.ToString(),
                coOrganizerIds.Select(v => v.ToString()).ToArray(),
                date,
                startTime.Time,
                endTime
            );
        return Arb.From(gen);
    }
}
