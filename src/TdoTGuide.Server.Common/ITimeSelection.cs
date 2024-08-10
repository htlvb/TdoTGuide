using System.Collections;

namespace TdoTGuide.Server.Common
{
    public interface ITimeSelection
    {
        public T Accept<T>(ITimeSelectionVisitor<T> visitor);
    }

    public class ContinuousTimeSelection : ITimeSelection, IEquatable<ContinuousTimeSelection>
    {
        public T Accept<T>(ITimeSelectionVisitor<T> visitor)
        {
            return visitor.VisitContinuousTimeSelection(this);
        }

        public bool Equals(ContinuousTimeSelection? other)
        {
            return other != null;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ContinuousTimeSelection);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class RegularTimeSelection(int intervalMinutes) : ITimeSelection, IEquatable<RegularTimeSelection>
    {
        public int IntervalMinutes { get; } = intervalMinutes;

        public T Accept<T>(ITimeSelectionVisitor<T> visitor)
        {
            return visitor.VisitRegularTimeSelection(this);
        }

        public bool Equals(RegularTimeSelection? other)
        {
            return other != null && other.IntervalMinutes == IntervalMinutes;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RegularTimeSelection);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IntervalMinutes);
        }
    }

    public class IndividualTimeSelection(List<DateTime> times) : ITimeSelection, IEquatable<IndividualTimeSelection>
    {
        public IReadOnlyCollection<DateTime> Times { get; } = [.. times.Order()];

        public T Accept<T>(ITimeSelectionVisitor<T> visitor)
        {
            return visitor.VisitIndividualTimeSelection(this);
        }

        public bool Equals(IndividualTimeSelection? other)
        {
            return other != null && other.Times.SequenceEqual(Times);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as IndividualTimeSelection);
        }

        public override int GetHashCode()
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(Times);
        }
    }

    public interface ITimeSelectionVisitor<T>
    {
        public T VisitContinuousTimeSelection(ContinuousTimeSelection v);
        public T VisitRegularTimeSelection(RegularTimeSelection v);
        public T VisitIndividualTimeSelection(IndividualTimeSelection v);
    }
}
