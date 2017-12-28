using System;

namespace ChatServiceLayer
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static int Min(this int val, int min)
        {
            return Math.Min(val, min);
        }
    }

    public static class ValueExtensions
    {
        public static T? AsNullable<T>(this T item) where T : struct
        {
            return item;
        }

        public static T? TryCastStruct<T>(object item) where T : struct
        {
            return item is T ? (T?) item : null;
        }
    }

    /// <summary> Explicit representation of return type where data has no value. More explicit than object or bool. </summary>
        public class Unit : IEquatable<Unit>
    {
        #region Equality

        public bool Equals(Unit other)
        {
            return ReferenceEquals(other, this); //Should only be single instance so always true (unless rogue reflection invocation of private constructor!)
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Unit)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Unit left, Unit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Unit left, Unit right)
        {
            return !Equals(left, right);
        }

        #endregion

        public static Unit Instance { get; } = new Unit();

        private Unit() { }

        /// <remarks> Useful for wrapping multiple params into Unit - communicate return type not required </remarks>
        public static Unit Create<T1>(T1 ignore1) { return Instance; }
        public static Unit Create2<T1, T2>(T1 ignore1, T2 ignore2) { return Instance; }
        public static Unit Create3<T1, T2, T3>(T1 ignore1, T2 ignore2, T3 ignore3) { return Instance; }
        public static Unit Create4<T1, T2, T3, T4>(T1 ignore1, T2 ignore2, T3 ignore3, T4 ignore4) { return Instance; }
    }
}
