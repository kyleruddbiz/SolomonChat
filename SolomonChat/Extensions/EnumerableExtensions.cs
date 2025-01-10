using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SolomonChat.Extensions
{
    /// <summary>
    /// Extension methods for type <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        public static bool Any(this IEnumerable @this)
        {
            return @this.GetEnumerator().MoveNext();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> @this)
        {
            return @this == null || !@this.Any();
        }

        public static bool ContainsNull<T>(this IEnumerable<T> @this)
        {
            return @this != null && @this.Any(value => value == null);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> @this, IEnumerable<T> other)
        {
            return @this != null
                && other != null
                && other.Any(element => @this.Contains(element));
        }

        public static bool ContainsAll<T>(this IEnumerable<T> @this, IEnumerable<T> other)
        {
            return @this != null
                && other != null
                && other.All(element => @this.Contains(element));
        }

        public static int IndexOf<T>(this IEnumerable<T> @this, T item, int startIndex = 0)
        {
            return @this.IndexOf((value) => Equals(value, item), startIndex);
        }

        public static int IndexOf<T>(this IEnumerable<T> @this, Func<T, bool> predicate, int startIndex = 0)
        {
            int index = startIndex < 0 ? 0 : startIndex;

            foreach (T item in @this.Skip(index))
            {
                if (predicate(item))
                    return index;

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Compares two sequences using the default comparer for the element type, returning a value indicating
        /// whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <remarks>
        /// The enumerables are compared by comparing the enumerated elements in sequence.
        /// <para>
        /// The enumerables are equal if they enumerate the same number of elements and each pair of enumerated
        /// elements are equal.  If all pairs of elements are equal but one enumerable has fewer elements than the
        /// other, then the enumerable with fewer elements is less than the other.  Otherwise, the result from
        /// comparing the first unequal pair of elements determines which enumerable is less than the other.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// Type to compare.
        /// </typeparam>
        /// <param name="left">
        /// An enumerable.
        /// </param>
        /// <param name="right">
        /// An enumerable.
        /// </param>
        /// <returns>
        /// Returns a negative number if <paramref name="left"/> is less than <paramref name="right"/>, a positive
        /// number if <paramref name="left"/> is greater than <paramref name="right"/>, and <c>0</c> if they are
        /// equal.
        /// </returns>
        public static int CompareSequences<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            return CompareSequences(left, right, Comparer<T>.Default);
        }

        /// <summary>
        /// Compares two sequenes and returns a value indicating whether one is less than, equal to, or greater
        /// than the other.
        /// </summary>
        /// <remarks>
        /// The enumerables are compared by comparing the enumerated elements in sequence.
        /// <para>
        /// The enumerables are equal if they enumerate the same number of elements and each pair of enumerated
        /// elements are equal.  If all pairs of elements are equal but one enumerable has fewer elements than the
        /// other, then the enumerable with fewer elements is less than the other.  Otherwise, the result from
        /// comparing the first unequal pair of elements determines which enumerable is less than the other.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// Type to compare.
        /// </typeparam>
        /// <param name="left">
        /// An enumerable.
        /// </param>
        /// <param name="right">
        /// An enumerable.
        /// </param>
        /// <param name="comparer">
        /// The comparer used to compare enumerated elements.
        /// </param>
        /// <returns>
        /// Returns a negative number if <paramref name="left"/> is less than <paramref name="right"/>, a positive
        /// number if <paramref name="left"/> is greater than <paramref name="right"/>, and <c>0</c> if they are
        /// equal.
        /// </returns>
        public static int CompareSequences<T>(this IEnumerable<T> left, IEnumerable<T> right, IComparer<T> comparer)
        {
            left.ThrowIfNull(nameof(left));
            right.ThrowIfNull(nameof(right));
            comparer.ThrowIfNull(nameof(comparer));

            if (ReferenceEquals(left, right))
                return 0;

            IEnumerator<T> leftEnumerator = left.GetEnumerator();
            IEnumerator<T> rightEnumerator = right.GetEnumerator();

            while (leftEnumerator.MoveNext())
            {
                if (!rightEnumerator.MoveNext())
                    return 1;

                int comparisonResult = comparer.Compare(leftEnumerator.Current, rightEnumerator.Current);

                if (comparisonResult != 0)
                    return comparisonResult;
            }

            return rightEnumerator.MoveNext() ? -1 : 0;
        }

        /// <summary>
        /// Determines whether two enumerables contain equal items in equal quantities, without requiring that
        /// the order of the items is the same.
        /// </summary>
        /// <typeparam name="T">
        /// Type of item being enumerated.
        /// </typeparam>
        /// <param name="left">
        /// An enumerable.
        /// </param>
        /// <param name="right">
        /// An enumerable.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if <paramref name="left"/> and <paramref name="right"/> contain equal
        /// items in equal quantities, regardless of whether the items are ordered the same.
        /// </returns>
        public static bool AreContentsEqual<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            if (left.IsNullOrEmpty() || right.IsNullOrEmpty())
                return left.IsNullOrEmpty() && right.IsNullOrEmpty();

            IEnumerable<Tuple<T, int>> leftCounts = left.GetCountsByItem();
            IEnumerable<Tuple<T, int>> rightCounts = right.GetCountsByItem();

            return new HashSet<Tuple<T, int>>(leftCounts).SetEquals(rightCounts);
        }

        public static IEnumerable<Tuple<T, int>> GetCountsByItem<T>(this IEnumerable<T> @this)
        {
            return @this.GetCountsByItem(EqualityComparer<T>.Default);
        }

        public static IEnumerable<Tuple<T, int>> GetCountsByItem<T>(
            this IEnumerable<T> @this, IEqualityComparer<T> equalityComparer)
        {
            return @this
                .GroupBy(item => item, equalityComparer)
                .Select(group => Tuple.Create(group.Key, group.Count()));
        }

        public static bool ContainsMultipleReferencesToSameObject<T>(this IEnumerable<T> @this)
        {
            return @this.GetCountsByItem(new ReferenceEqualityComparer<T>()).Any(tuple => tuple.Item2 > 1);
        }

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> @this)
        {
            return @this
                .ThrowIfNull(nameof(@this))
                .Where(x => x != null);
        }

        public static IEnumerable<T> OrderByNumeric<T>(this IEnumerable<T> @this, Func<T, int> integerKeySelector)
        {
            return @this.OrderBy(key => integerKeySelector(key));
        }

        public static IEnumerable<T> OrderByNumericDescending<T>(this IEnumerable<T> @this, Func<T, int> integerKeySelector)
        {
            return @this.OrderByDescending(key => integerKeySelector(key));
        }

        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T?> @this) where T : struct
        {
            return @this
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }

        public static IEnumerable<T> ThrowIfEmpty<T>(this IEnumerable<T> @this, string propertyName)
        {
            if (@this.Count() == 0)
            {
                throw new ArgumentException($"{propertyName} cannot be empty");
            }

            return @this;
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> keySelector)
        {
            return @this.GroupBy(keySelector)
                .Select(x => x.First());
        }

        public static bool SequenceEqual(this IEnumerable @this, IEnumerable second)
        {
            @this.ThrowIfNull(nameof(@this));
            second.ThrowIfNull(nameof(second));

            IEnumerator firstEnumerator = @this.GetEnumerator();
            IEnumerator secondEnumerator = second.GetEnumerator();

            bool firstMoved;
            bool secondMoved;

            do
            {
                firstMoved = firstEnumerator.MoveNext();
                secondMoved = secondEnumerator.MoveNext();

                if (firstMoved ^ secondMoved)
                {
                    // The sequences are of different lengths
                    return false;
                }
                else if (!firstMoved)
                {
                    // Reached the end of both sequences
                    break;
                }

                object firstValue = firstEnumerator.Current;
                object secondValue = secondEnumerator.Current;

                if (!Equals(firstValue, secondValue))
                {
                    return false;
                }
            }
            while (true);

            return true;
        }
    }

    /// <summary>
    /// A comparer that considers two objects equal only if they are the same object.
    /// </summary>
    public sealed class ReferenceEqualityComparer : IEqualityComparer
    {
        public ReferenceEqualityComparer()
        {
        }

        public new bool Equals(object left, object right)
        {
            return ReferenceEquals(left, right);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T left, T right)
        {
            return ReferenceEquals(left, right);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}