namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

using Microsoft.CodeAnalysis;

static class EnumerableExtensions
{
    public static Boolean None<T>(this IEnumerable<T> enumeration)
        => !enumeration.Any();
    public static Boolean None<T>(this IEnumerable<T> enumeration, Func<T, Boolean> predicate)
        => !enumeration.Any(predicate);
    public static Boolean One<T>(this IEnumerable<T> enumeration, Func<T, Boolean> predicate)
        => enumeration.Where(predicate).Count() == 1;
}