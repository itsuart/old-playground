using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.Containers;
using Lib.Extensions;
using NUnit.Framework;

namespace Tests.Containers
{
    [TestFixture]
    public class AbortableComputationTests
    {
        private static readonly Func<bool> NeverAbort = () => false;

        private static readonly Func<bool> AlwaysAbort = () => true;
            
        [Test]
        public void FmapOrderingTest()
        {

            var m = AbortableComputation<int>
                .Seed(NeverAbort, 13)
                .Fmap(x => x + 2)
                .Fmap(x => x * 2);

            var val = m.Run();
            Assert.IsTrue(val.HasValue);
            Assert.AreEqual(2 * (13 + 2), val.Value);
        }

        [Test]
        public void SeqOrderingTest1()
        {
            var v13 = AbortableComputation<int>.Seed(NeverAbort, 13);
            var vStr = AbortableComputation<string>.Seed(NeverAbort, "test");

            var composition = v13.Seq(vStr);
            var val = composition.Run();
            Assert.AreEqual("test", val.Value);
        }

        [Test]
        public void SeqOrderingTest2()
        {
            var strBranchRun = false;

            var v13 = AbortableComputation<int>.Seed(AlwaysAbort, 13);
            var vStr = new AbortableComputation<string>(NeverAbort, () =>
                {
                    strBranchRun = true;
                    return "test";
                });

            var composition = v13.Seq(vStr);
            var val = composition.Run();
            Assert.IsFalse(strBranchRun);
            Assert.IsTrue(val.IsNothing);
        }

        [Test]
        public void SeqOrderingTest3()
        {

            var strBranchRun = false;

            var v13 = AbortableComputation<int>.Seed(NeverAbort, 13);
            var vStr = new AbortableComputation<string>(AlwaysAbort, () =>
            {
                strBranchRun = true;
                return "test";
            });

            var composition = v13.Seq(vStr);
            var val = composition.Run();
            Assert.IsFalse(strBranchRun);
            Assert.IsTrue(val.IsNothing);
        }

        public void BindOrderingTest()
        {
            
        }
    }
}
