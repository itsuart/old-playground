using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Containers;
using NUnit.Framework;

namespace Tests.Containers
{
    [TestFixture]
    public class FuncChainTests
    {

        [Test]
        public void Ordering()
        {
            var chain = FuncChain<int>.Wrap(2).Fmap(x => x + 10).Fmap(x => x * 10);
            Assert.AreEqual((2 + 10) * 10, chain.Run());
        }
    }
}
