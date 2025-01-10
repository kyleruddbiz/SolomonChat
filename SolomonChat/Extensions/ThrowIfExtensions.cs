using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolomonChat.Extensions
{
    public static class ThrowIfExtensions
    {
        public static T ThrowIf<T>(this T @this, bool condition, string parameterName)
        {
            return @this.ThrowIf(condition, () => new ArgumentException($"Argument '{parameterName}' failed condition."));
        }

        public static T ThrowIf<T>(this T @this, Func<T, bool> condition, string parameterName)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return @this.ThrowIf(condition(@this), parameterName);
        }

        public static T ThrowIf<T>(this T @this, Func<T, bool> condition, Func<Exception> exceptionFactory)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            return @this.ThrowIf(condition(@this), exceptionFactory);
        }

        public static T ThrowIf<T>(this T @this, bool condition, Func<Exception> exceptionFactory)
        {
            if (exceptionFactory == null)
            {
                throw new ArgumentNullException(nameof(exceptionFactory));
            }

            if (condition)
            {
                var exceptionToThrow = exceptionFactory()
                    ?? throw new ArgumentException($"{nameof(exceptionFactory)} returned null when it should have returned an exception.");

                throw exceptionToThrow;
            }

            return @this;
        }

        public static T ThrowIfNull<T>(this T @this, string parameterName)
        {
            return ThrowIfNull(@this, () => new ArgumentNullException(parameterName));
        }

        public static T ThrowIfNull<T>(this T @this, Func<Exception> exceptionFactory)
        {
            return @this.ThrowIf(@this == null, exceptionFactory);
        }

        public static string ThrowIfNullOrEmpty(this string @this, string parameterName)
        {
            return ThrowIfNullOrEmpty(@this, () => new ArgumentException($"Argument '{parameterName}' cannot be null or empty."));
        }

        public static string ThrowIfNullOrEmpty(this string @this, Func<Exception> exceptionFactory)
        {
            return ThrowIf(@this, string.IsNullOrEmpty, exceptionFactory);
        }

        public static IEnumerable<T> ThrowIfNullOrEmpty<T>(this IEnumerable<T> @this, string parameterName)
        {
            return ThrowIfNullOrEmpty(@this, () => new ArgumentException($"Argument '{parameterName}' cannot be null or empty."));
        }

        public static IEnumerable<T> ThrowIfNullOrEmpty<T>(this IEnumerable<T> @this, Func<Exception> exceptionFactory)
        {
            return ThrowIf(@this, EnumerableExtensions.IsNullOrEmpty, exceptionFactory);
        }

        public static IEnumerable<T> ThrowIfContainsNull<T>(this IEnumerable<T> @this, string parameterName)
        {
            return ThrowIfNullOrEmpty(@this, () => new ArgumentException($"Argument '{parameterName}' cannot be null or empty."));
        }

        public static IEnumerable<T> ThrowIfContainsNull<T>(this IEnumerable<T> @this, Func<Exception> exceptionFactory)
        {
            return ThrowIf(@this, EnumerableExtensions.ContainsNull, exceptionFactory);
        }

        public static int ThrowIfNegative(this int @this, string parameterName) => @this.ThrowIf(@this < 0, parameterName);

        public static int ThrowIfNotPositive(this int @this, string parameterName) => @this.ThrowIf(@this <= 0, parameterName);

        public static int ThrowIfOutsideRange(this int @this, int minValue, int maxValue, string parameterName)
        {
            return @this.ThrowIf(
                @this < minValue || @this > maxValue,
                () => new ArgumentOutOfRangeException(
                    parameterName,
                    @this,
                    $"Argument '{parameterName}' must be within the range {minValue} - {maxValue} (inclusive)."));
        }

        public static double ThrowIfNegative(this double @this, string parameterName) => @this.ThrowIf(@this < 0, parameterName);

        public static double ThrowIfNotPositive(this double @this, string parameterName) => @this.ThrowIf(@this <= 0, parameterName);

        public static double ThrowIfOutsideRange(this double @this, double minValue, double maxValue, string parameterName)
        {
            return @this.ThrowIf(
                @this < minValue || @this > maxValue,
                () => new ArgumentOutOfRangeException(
                    parameterName,
                    @this,
                    $"Argument '{parameterName}' must be within the range {minValue} - {maxValue} (inclusive)."));
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if one specified <see cref="Type"/> is not assignable to a variable
        /// of another specified type.
        /// </summary>
        public static Type ThrowIfNotAssignableTo(this Type @this, Type targetType, string parameterName)
        {
            return @this.ThrowIf(
                !targetType.IsAssignableFrom(@this),
                () => new ArgumentException(
                    $"Argument '{parameterName}' must be a type that is assignable to type '{targetType.FullName}'.  Actual type:  '{@this.FullName}'.",
                    parameterName));
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> if a specified <see cref="Type"/> is null or does not have a
        /// zero-parameter constructor.
        /// </summary>
        public static Type ThrowIfMissingZeroParameterConstructor(this Type @this, string parameterName)
        {
            @this.ThrowIfNull(parameterName);

            return @this.ThrowIf(
                @this.GetConstructor(Type.EmptyTypes) == null,
                () => new ArgumentException(
                   "The specified type must have a zero-parameter constructor.  Type:  '{@this.FullName}'.",
                   parameterName));
        }
    }
}