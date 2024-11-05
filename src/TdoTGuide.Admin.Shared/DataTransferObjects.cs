using System.Text.Json.Serialization;

namespace TdoTGuide.Admin.Shared
{
    public record EditingProjectDto(
        EditingProjectDataDto Data,
        IReadOnlyList<SelectionTypeDto> AllProjectTypes,
        IReadOnlyList<BuildingDto> AllBuildings,
        IReadOnlyList<string> AllFloors,
        IReadOnlyList<ProjectOrganizerDto> OrganizerCandidates,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizerCandidates,
        EditingProjectLinksDto Links
    );

    public record EditingProjectDataDto(
        string Title,
        string Description,
        SelectionReferenceDto Type,
        IReadOnlyList<string> MediaFileNames,
        IReadOnlyList<string> MediaFileNamesToRemove,
        string? Building,
        string? Floor,
        string Location,
        string OrganizerId,
        IReadOnlyList<string> CoOrganizerIds
    );

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(SimpleSelectionReferenceDto), "simple")]
    [JsonDerivedType(typeof(MultiSelectSelectionReferenceDto), "multi-select")]
    public abstract record SelectionReferenceDto(string Name)
    {
        public abstract T Accept<T>(ISelectionReferenceDtoVisitor<T> visitor);
    }
    public record SimpleSelectionReferenceDto(string Name) : SelectionReferenceDto(Name)
    {
        public override T Accept<T>(ISelectionReferenceDtoVisitor<T> visitor)
        {
            return visitor.VisitSimpleSelectionReference(this);
        }
    }

    public record MultiSelectSelectionReferenceDto(string Name, string[] SelectedValues) : SelectionReferenceDto(Name)
    {
        public override T Accept<T>(ISelectionReferenceDtoVisitor<T> visitor)
        {
            return visitor.VisitMultiSelectSelectionReference(this);
        }
    }

    public interface ISelectionReferenceDtoVisitor<T>
    {
        T VisitSimpleSelectionReference(SimpleSelectionReferenceDto selectionReference);
        T VisitMultiSelectSelectionReference(MultiSelectSelectionReferenceDto selectionReference);
    }
    public class AnonymousSelectionReferenceDtoVisitor<T>(
        Func<SimpleSelectionReferenceDto, T> visitSimpleSelectionReferenceDto,
        Func<MultiSelectSelectionReferenceDto, T> visitMultiSelectSelectionReferenceDto
    ) : ISelectionReferenceDtoVisitor<T>
    {
        public T VisitSimpleSelectionReference(SimpleSelectionReferenceDto selectionReference)
        {
            return visitSimpleSelectionReferenceDto(selectionReference);
        }

        public T VisitMultiSelectSelectionReference(MultiSelectSelectionReferenceDto selectionReference)
        {
            return visitMultiSelectSelectionReferenceDto(selectionReference);
        }
    }

    public record EditingProjectLinksDto(
        string? Save
    );

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(SimpleSelectionTypeDto), "simple")]
    [JsonDerivedType(typeof(MultiSelectSelectionTypeDto), "multi-select")]
    public abstract record SelectionTypeDto(string Id, string Title)
    {
        public abstract T Accept<T>(ISelectionTypeDtoVisitor<T> visitor);
    }
    public record SimpleSelectionTypeDto(string Id, string Title, string Color) : SelectionTypeDto(Id, Title)
    {
        public override T Accept<T>(ISelectionTypeDtoVisitor<T> visitor)
        {
            return visitor.VisitSimpleSelectionType(this);
        }
    }

    public record MultiSelectSelectionTypeDto(string Id, string Title, SelectionItemDto[] Items) : SelectionTypeDto(Id, Title)
    {
        public override T Accept<T>(ISelectionTypeDtoVisitor<T> visitor)
        {
            return visitor.VisitMultiSelectSelectionType(this);
        }
    }

    public record SelectionItemDto(string Id, string Color, string ShortName, string LongName);
    public interface ISelectionTypeDtoVisitor<T>
    {
        T VisitSimpleSelectionType(SimpleSelectionTypeDto selectionType);
        T VisitMultiSelectSelectionType(MultiSelectSelectionTypeDto selectionType);
    }
    public class AnonymousSelectionTypeDtoVisitor<T>(
        Func<SimpleSelectionTypeDto, T> visitSimpleSelectionTypeDto,
        Func<MultiSelectSelectionTypeDto, T> visitMultiSelectSelectionTypeDto) : ISelectionTypeDtoVisitor<T>
    {
        public T VisitSimpleSelectionType(SimpleSelectionTypeDto selectionType)
        {
            return visitSimpleSelectionTypeDto(selectionType);
        }

        public T VisitMultiSelectSelectionType(MultiSelectSelectionTypeDto selectionType)
        {
            return visitMultiSelectSelectionTypeDto(selectionType);
        }
    }

    public record ProjectListDto(
        IReadOnlyList<ProjectDto> Projects,
        IReadOnlyList<ProjectTagDto> AllProjectTags,
        IReadOnlyList<BuildingDto> AllBuildings,
        ProjectListLinksDto Links
    );

    public record ProjectListLinksDto(
        string? CreateProject
    );

    public record ProjectDto(
        string Title,
        string Description,
        IReadOnlyCollection<ProjectTagDto> Tags,
        string Building,
        string? Floor,
        string Location,
        ProjectOrganizerDto Organizer,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizers,
        UserRoleForProjectDto CurrentUserStatus,
        List<ProjectMediaDto> Media,
        ProjectLinksDto Links
    )
    {
        public bool IsUserProject =>
            CurrentUserStatus == UserRoleForProjectDto.Organizer
            || CurrentUserStatus == UserRoleForProjectDto.CoOrganizer;
    }

    public record ProjectTagDto(string Name, string Color);

    public record BuildingDto(
        string Id,
        string Name
    );

    public record ProjectOrganizerDto(
        string Id,
        string DisplayName
    );

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRoleForProjectDto
    {
        NotRelated,
        Organizer,
        CoOrganizer
    }

    public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

    public enum ProjectMediaTypeDto
    {
        Image,
        Video
    }

    public record ProjectLinksDto(
        string? Edit,
        string? Delete
    );
}
