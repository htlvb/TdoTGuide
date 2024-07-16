using Bogus;
using TdoTGuide.WebAsm.Server;
using TdoTGuide.WebAsm.Server.Data;
using TdoTGuide.WebAsm.Shared;

namespace TdoTGuide.WebAsm.Server.IntegrationTests.Utils;

public static class FakeData
{
    public static Faker<Project> ProjectFaker { get; } = new Faker<Project>()
        .CustomInstantiator(v =>
        {
            var date = v.Date.SoonDateOnly(20);
            var organizers = ProjectOrganizers!
                .OrderBy(_ => v.Random.Double())
                .Take(v.Random.Number(1, 5))
                .ToList();
            return new Project(
                v.Random.Uuid().ToString(),
                v.Random.Words(),
                v.Lorem.Sentences(),
                v.Address.BuildingNumber(),
                organizers.First(),
                organizers.Skip(1).ToList(),
                date,
                new TimeOnly(7, 0).AddMinutes(v.Random.Number(0, 8) * 15),
                v.Random.Bool() ? new TimeOnly(12, 0).AddMinutes(v.Random.Number(0, 12) * 15) : null
            );
        });

    public static Faker<EditingProjectDataDto> EditingProjectDataDtoFaker { get; } = new Faker<EditingProjectDataDto>()
        .CustomInstantiator(v =>
        {
            var date = v.Date.SoonDateOnly(20);
            var organizerIds = ProjectOrganizers!
                .OrderBy(_ => v.Random.Double())
                .Take(v.Random.Number(1, 5))
                .Select(v => v.Id)
                .ToList();
            return new EditingProjectDataDto(
                v.Random.Words(),
                v.Lorem.Sentences(),
                v.Address.BuildingNumber(),
                organizerIds.First(),
                organizerIds.Skip(1).ToList(),
                date,
                new TimeOnly(7, 0).AddMinutes(v.Random.Number(0, 8) * 15),
                v.Random.Bool() ? new TimeOnly(12, 0).AddMinutes(v.Random.Number(0, 12) * 15) : null
            );
        });

    public static IReadOnlyList<ProjectOrganizer> ProjectOrganizers { get; } =
        new Faker<ProjectOrganizer>()
            .CustomInstantiator(v => new ProjectOrganizer(
                v.Random.Uuid().ToString(),
                v.Name.FirstName(),
                v.Name.LastName(),
                v.Random.String2(4).ToUpper()
            ))
        .Generate(10);
}
