using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Monads;
using NUnit.Framework;

namespace Tests.Containers
{
    [TestFixture]
    public class MaybeTests
    {

        [Test]
        public void FunctorTest()
        {
            var result = Maybe<int>.Just(10).Fmap(x => x + 2);
            result.Run(x => Assert.AreEqual(12, x));

        }
    }
}
