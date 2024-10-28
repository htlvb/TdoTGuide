namespace TdoTGuide.Server.Common;

public sealed record Project(
    string Id,
    string Title,
    string Description,
    ISelection Type,
    string Building,
    string Location,
    ProjectOrganizer Organizer,
    IReadOnlyList<ProjectOrganizer> CoOrganizers
);

public interface ISelection
{
    T Accept<T>(ISelectionVisitor<T> visitor);
}

public class SimpleSelection(string name) : ISelection
{
    public string Name { get; } = name;

    public T Accept<T>(ISelectionVisitor<T> visitor)
    {
        return visitor.VisitSimpleSelection(this);
    }
}

public class MultiSelectSelection(string name, IReadOnlyCollection<string> SelectedValues) : ISelection
{
    public string Name { get; } = name;

    public IReadOnlyCollection<string> SelectedValues { get; } = SelectedValues;

    public T Accept<T>(ISelectionVisitor<T> visitor)
    {
        return visitor.VisitMultiSelectSelection(this);
    }
}

public interface ISelectionVisitor<T>
{
    T VisitSimpleSelection(SimpleSelection selection);
    T VisitMultiSelectSelection(MultiSelectSelection selection);
}

public class AnonymousSelectionVisitor<T>(
        Func<SimpleSelection, T> visitSimpleSelection,
        Func<MultiSelectSelection, T> visitMultiSelectSelection
    ) : ISelectionVisitor<T>
{
    public T VisitSimpleSelection(SimpleSelection selection)
    {
        return visitSimpleSelection(selection);
    }

    public T VisitMultiSelectSelection(MultiSelectSelection selection)
    {
        return visitMultiSelectSelection(selection);
    }
}
