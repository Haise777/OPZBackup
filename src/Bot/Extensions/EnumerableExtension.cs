namespace OPZBot.Extensions;

public static class EnumerableExtension
{
    public static IEnumerable<TSource> ExcludeFirst<TSource>(this IEnumerable<TSource> source) where TSource : class
        => source.Where(x => x != source.First());
}