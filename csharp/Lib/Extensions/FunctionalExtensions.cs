using System;
using System.Collections.Generic;

namespace Lib.Extensions
{
    public static class FunctionalExtensions
    {
        public static Func<T1, T2> Memoize<T1,T2>(this Func<T1, T2> f, IDictionary<T1, T2> cache)
        {
            if (f == null) throw new ArgumentNullException("Function to memoize is null");
            if (cache == null) throw new ArgumentNullException("Memoization cache is null");
            if (cache.IsReadOnly) throw new ArgumentException("Memoization cache is read only");

            Func<T1, T2> memoized = input =>
                {
                    var cachedValue = default(T2);
                    if (cache.TryGetValue(input, out cachedValue)) return cachedValue;

                    var output = f(input);
                    cache.Add(input, output);
                    return output;
                };

            return memoized;
        }

        /// <summary>
        /// Creates *infinite* stream of initialElement, f(initialElement), f(f(initialElement)), ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialElement"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IEnumerable<T> MakeStream<T>(this T initialElement, Func<T, T> f)
        {
            yield return initialElement;
            var element = initialElement;
            while (true)
            {
                element = f(element);
                yield return element;
            }
        }

        public static T2 SendTo<T1, T2>(this T1 arg, Func<T1, T2> f)
        {
            return f(arg);
        }

        public static void SendTo<T>(this T arg, Action<T> f)
        {
            f(arg);
        }
    }
}
