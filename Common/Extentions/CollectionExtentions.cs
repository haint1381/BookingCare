
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingCare.Common.Extentions
{
    public static class CollectionExtentions
    {
        public static List<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }


        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static void Loop<T>(this IEnumerable<T> enumerable, Action<int, T> action)
        {
            var index = 0;

            if (!enumerable.IsNullOrEmpty())
            {
                foreach (var item in enumerable)
                {
                    action(index++, item);
                }
            }
        }

        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }
    }
}
