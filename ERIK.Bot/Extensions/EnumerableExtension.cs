 using System;
using System.Collections.Generic;
using System.Linq;

namespace ERIK.Bot.Extensions
{
    public static class EnumerableExtension
    {
        public static bool ContainsItem<T>(this IEnumerable<T> source, T target)
        {
            foreach (var item in source)
            {
                if (item.Equals(target))
                {
                    return true;
                }
            }

            return false;
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}
