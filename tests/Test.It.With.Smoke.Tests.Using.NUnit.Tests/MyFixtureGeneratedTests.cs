using NUnit.Framework;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Tests
{
    [SmokeTestFixture]
    public class MyFixtureGeneratedTests
    {
        private int i = 0;

        [SmokeTest]
        public void Hej()
        {
            i++; 
            Assert.AreEqual(1, i);
        }
         
        [SmokeTest]
        public void B()
        { 
            i++;
            Assert.AreEqual(2, i);
        }

        [SmokeTest]
        public void A()
        {
            i++;
            Assert.AreEqual(3, i);
        }

        [SmokeTest]
        public void F()
        {
            i++;
            Assert.AreEqual(4, i);
        }

        [SmokeTest]
        public void H()
        {
            i++;
            Assert.AreEqual(5, i);
        }

        [SmokeTest]
        public void K()
        {
            i++;
            Assert.AreEqual(6, i);
        }

        [SmokeTest]
        public void J()
        {
            i++;
            Assert.AreEqual(7, i);
        }

        [SmokeTest]
        public void I()
        {
            i++;
            Assert.AreEqual(8, i);
        }

        [SmokeTest]
        public void AA()
        {
            i++;
            Assert.AreEqual(9, i);
        }

        [SmokeTest]
        public void AB()
        {
            i++;
            Assert.AreEqual(10, i);
        }

        [SmokeTest]
        public void AC()
        {
            i++;
            Assert.AreEqual(11, i);
        }
    }

}
