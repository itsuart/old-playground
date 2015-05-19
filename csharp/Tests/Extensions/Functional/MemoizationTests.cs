using System;
using System.Collections.Generic;
using System.Globalization;
using Lib.Extensions;
using NUnit.Framework;

namespace Tests.Extensions.Functional
{
    [TestFixture]
    public class MemoizationTests
    {

        [Test]
        public void TestMemoizationDoNotAlterBehaviour()
        {
            Func<int, string> f = i => i.ToString(CultureInfo.InvariantCulture);
            var memoizedF = f.Memoize(new Dictionary<int, string>());

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(f(i), memoizedF(i));
                //and one more time - test cached value
                Assert.AreEqual(f(i), memoizedF(i));
            }
        }

        [Test]
        public void TestMemoizationStoresValue()
        {
            var cache = new Dictionary<int, string>();
            Func<int, string> f = i => i.ToString(CultureInfo.InvariantCulture);
            var memoizedF = f.Memoize(cache);

            const int input = 10;
            var expectedOutput = f(input);

            memoizedF(input);
            Assert.IsTrue(cache.ContainsKey(input));
            Assert.AreEqual(expectedOutput, cache[input]);
        }

        [Test]
        public void TestMemoizationUsesCache()
        {
            var fCalls = 0;
            Func<int, string> f = i =>
                {
                    fCalls++;
                    return i.ToString(CultureInfo.InvariantCulture);
                };
            var memoizedF = f.Memoize(new Dictionary<int, string>());

            memoizedF(1);
            Assert.AreEqual(1, fCalls);
            memoizedF(1);
            Assert.AreEqual(1, fCalls, "Memoization doesn't use cache.");
        }
    }
}
