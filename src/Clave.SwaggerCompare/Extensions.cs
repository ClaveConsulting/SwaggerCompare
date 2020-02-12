using System;
using System.Collections.Generic;

namespace Clave.SwaggerCompare
{
    public static class Extensions
    {
        public static string Prepend(this string value, string addThis) => addThis + value;
        public static string Pluralize(this string word, int count) => count == 1 ? word : $"{word}s";

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}