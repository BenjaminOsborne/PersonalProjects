using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

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



    public static class CollectionKey
    {
        public static CollectionKey<TKey> CreateSingle<TKey>(TKey key) where TKey : IEquatable<TKey>
        {
            return new CollectionKey<TKey>(key);
        }

        public static CollectionKey<TKey> Create<TKey>(IEnumerable<TKey> keys) where TKey : IEquatable<TKey>
        {
            return new CollectionKey<TKey>(keys.ToImmutableList());
        }

        public static CollectionKey<TKey> CreateDistinctAndOrder<TKey>(IEnumerable<TKey> keys) where TKey : IComparable, IEquatable<TKey>
        {
            return new CollectionKey<TKey>(keys.Distinct().OrderBy(x => x).ToImmutableList());
        }
    }

    public class CollectionKey<TKey> : IEquatable<CollectionKey<TKey>> where TKey : IEquatable<TKey>
    {
        public static CollectionKey<TKey> Empty { get; } = new CollectionKey<TKey>(ImmutableList<TKey>.Empty);

        private readonly Lazy<int> _lazyHashCode;

        public CollectionKey(ImmutableList<TKey> keys)
        {
            Items = keys;
            _lazyHashCode = LazyHelper.Create(() => HashCodeExtensions.Aggregate(Items));
        }

        public CollectionKey(TKey key)
        {
            Items = ImmutableList.Create(key);
            _lazyHashCode = LazyHelper.Create(key.GetHashCode);
        }

        public ImmutableList<TKey> Items { get; }

        #region Equality

        public bool Equals(CollectionKey<TKey> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Items.Count == other.Items.Count && Items.SequenceEqual(other.Items);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CollectionKey<TKey>)obj);
        }

        public override int GetHashCode()
        {
            return _lazyHashCode.Value;
        }

        public static bool operator ==(CollectionKey<TKey> left, CollectionKey<TKey> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CollectionKey<TKey> left, CollectionKey<TKey> right)
        {
            return !Equals(left, right);
        }

        #endregion
    }

    public static class LazyHelper
    {
        public static Lazy<T> Create<T>(Func<T> fnCreate, bool isThreadSafe = true)
        {
            return new Lazy<T>(fnCreate, isThreadSafe);
        }

        public static Lazy<Unit> CreateUnit(Action fnCreate, bool isThreadSafe = true)
        {
            return new Lazy<Unit>(() =>
            {
                fnCreate();
                return Unit.Instance;
            }, isThreadSafe);
        }

        public static Lazy<T> CreateEager<T>(T item, bool isThreadSafe = true)
        {
            return new Lazy<T>(() => item, isThreadSafe);
        }

        public static Lazy<T> CreateOnTask<T>(Func<T> fnCreate, bool isThreadSafe = true)
        {
            var task = Task.Run(fnCreate);
            return new Lazy<T>(() => task.Result, isThreadSafe);
        }

        /// <remarks> Enables passing of getter of value as function</remarks>
        public static T Get<T>(this Lazy<T> lazy)
        {
            return lazy.Value;
        }

        public static void Evaluate<T>(this Lazy<T> lazy)
        {
            var _ = lazy.Value;
        }

        public static void IfCreatedDo<T>(this Lazy<T> lazy, Action<T> fnIfEvaluted)
        {
            if (lazy.IsValueCreated)
            {
                fnIfEvaluted(lazy.Value);
            }
        }
    }

    public static class HashCodeExtensions
    {
        public static int AggregateParams<T>(params T[] items)
        {
            return _Aggregate(items);
        }

        public static int Aggregate<T>(IEnumerable<T> items)
        {
            return _Aggregate(items);
        }

        private static int _Aggregate<T>(IEnumerable<T> items)
        {
            return items.Aggregate(0, (agg, key) => (agg * 397) ^ key.GetHashCode());
        }
    }
}
