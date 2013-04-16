using System;

namespace Lib.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] Fill<T>(this T[] array, T item)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = item;
            }
            return array;
        }

        public static int? FindItemCycled<T>(this T[] array, int startIndex, Predicate<T> equalityComparer)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (startIndex < 0 || startIndex >= array.Length) throw new ArgumentOutOfRangeException("startIndex");
            if (equalityComparer == null) throw new ArgumentNullException("equalityComparer");

            var currentIndex = startIndex;
            for (; currentIndex < array.Length; currentIndex++)
            {
                var currentItem = array[currentIndex];
                if (equalityComparer(currentItem)) return currentIndex;
            }

            //not found. let's search from the top
            currentIndex = 0;
            for (; currentIndex < startIndex; currentIndex++)
            {
                var currentItem = array[currentIndex];
                if (equalityComparer(currentItem)) return currentIndex;
            }
            return null; //not found
        }
    }
}
