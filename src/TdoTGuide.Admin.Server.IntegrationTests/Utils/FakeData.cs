using Bogus;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

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
                Id: v.Random.Uuid().ToString(),
                Title: v.Random.Words(),
                Description: v.Lorem.Sentences(),
                Type: v.Random.ArrayElement(ProjectTypes),
                Building: v.Random.Int(1, 3).ToString(),
                Floor: $"{v.Random.Int(-1, 2)}",
                Location: v.Address.BuildingNumber(),
                Organizer: organizers.First(),
                CoOrganizers: organizers.Skip(1).ToList(),
                ShowInPrintOverview: v.Random.Bool(),
                PrintOverviewGroupName: v.Random.Words().OrNull(v, 0.9f)
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
                Title: v.Random.Words(),
                Description: v.Lorem.Sentences(),
                Type: v.Random.ArrayElement(ProjectTypeDtos),
                MediaFileNames: v.Random.WordsArray(0, 5),
                MediaFileNamesToRemove: v.Random.WordsArray(0, 5),
                Building: v.Random.Int(1, 3).ToString(),
                Floor: $"{v.Random.Int(-1, 2)}",
                Location: v.Address.BuildingNumber(),
                OrganizerId: organizerIds.First(),
                CoOrganizerIds: organizerIds.Skip(1).ToList()
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

    public static ISelection[] ProjectTypes { get; } = [
        new SimpleSelection("general-info"),
        new SimpleSelection("department-independent"),
        new SimpleSelection("school-specific"),
        new MultiSelectSelection("department-specific", new Faker().Random.ArrayElements([.. Enumerable.Range(1, 5).Select(v => $"{v}")]))
    ];

    public static SelectionReferenceDto[] ProjectTypeDtos { get; } = ProjectTypes
        .Select(v => v.Accept(new AnonymousSelectionVisitor<SelectionReferenceDto>(
            (SimpleSelection selection) => new SimpleSelectionReferenceDto(selection.Name),
            (MultiSelectSelection selection) => new MultiSelectSelectionReferenceDto(selection.Name, [.. selection.SelectedValues])
        )))
        .ToArray();
}
