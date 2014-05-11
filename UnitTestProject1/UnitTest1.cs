using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    using System.Text;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRaziq()
        {
            string primary, secondary;
            var success = DoubleMetaphone.TryParse("Raziq", out primary, out secondary);
            Assert.IsTrue(success);
            Assert.AreEqual("RSK", primary);
            Assert.IsNull(secondary);
        }

        [TestMethod]
        public void TestCopperthwaite()
        {
            string primary, secondary;
            var success = DoubleMetaphone.TryParse("Copperthwaite", out primary, out secondary);
            Assert.IsTrue(success);
            Assert.AreEqual("KPR0", primary);
            Assert.AreEqual("KPRT", secondary
                );
        }
    }
}
