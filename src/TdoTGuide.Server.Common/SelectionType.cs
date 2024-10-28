namespace TdoTGuide.Server.Common;

public interface ISelectionType
{
    string Id { get; }
    string Title { get; }

    T Accept<T>(ISelectionTypeVisitor<T> visitor);
}

public record SimpleSelectionType(string Id, string Title, string Color) : ISelectionType
{
    public T Accept<T>(ISelectionTypeVisitor<T> visitor)
    {
        return visitor.VisitSimpleSelectionType(this);
    }
}

public record MultiSelectSelectionType(string Id, string Title, IReadOnlyCollection<SelectionChoice> Choices) : ISelectionType
{
    public T Accept<T>(ISelectionTypeVisitor<T> visitor)
    {
        return visitor.VisitMultiSelectSelectionType(this);
    }
}

public record SelectionChoice(string Id, string Color, string ShortName, string LongName);

public interface ISelectionTypeVisitor<T>
{
    T VisitMultiSelectSelectionType(MultiSelectSelectionType multiSelectSelectionType);
    T VisitSimpleSelectionType(SimpleSelectionType selectionType);
}

public class AnonymousSelectionTypeVisitor<T>(
        Func<SimpleSelectionType, T> visitSimpleSelectionType,
        Func<MultiSelectSelectionType, T> visitMultiSelectSelectionType
    ) : ISelectionTypeVisitor<T>
{
    public T VisitSimpleSelectionType(SimpleSelectionType selectionType)
    {
        return visitSimpleSelectionType(selectionType);
    }

    public T VisitMultiSelectSelectionType(MultiSelectSelectionType selectionType)
    {
        return visitMultiSelectSelectionType(selectionType);
    }
}
