namespace BibleApp.Core;

public class Unit : IEquatable<Unit>
{
    #region Equality

    /// <remarks> Should only be single instance so always true (unless instantiated by reflection) </remarks>
    public bool Equals(Unit? other) => ReferenceEquals(other, this);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Unit)obj);
    }

    public override int GetHashCode() => 0;

    public static bool operator ==(Unit left, Unit right) => Equals(left, right);

    public static bool operator !=(Unit left, Unit right) => !Equals(left, right);

    #endregion

    public static Unit Instance { get; } = new();

    private Unit() { }
}