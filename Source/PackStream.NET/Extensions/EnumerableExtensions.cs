﻿namespace PackStream.NET.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static IList<T> PadLeft<T>(this IList<T> list, int length, T padWith)
        {
            for (int i = 0; i < length; i++)
            {
                list.Insert(0, padWith);
            }

            return list;
        }
    }

    
}