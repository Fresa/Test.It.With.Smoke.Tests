using NUnit.Framework;
using Test.It.With.Smoke.Tests.Using.NUnit.Configuration;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Tests
{
    [ChainedFixtureSpecification]
    public class AnotherProcessBuildingSpecification
    {
        public static IBuildFixtures FixtureBuilder =>
            ChainedFixtures<AnotherFixtureGeneratedTests>
                .Start(() => new AnotherFixtureGeneratedTests())
                .Next(tests => new MyFixtureGeneratedTests2(tests.I));
    }

    public class AnotherFixtureGeneratedTests
    {
        public int I;

        public AnotherFixtureGeneratedTests()
        {
            I = 0;
        }

        [SmokeTest]
        public void D()
        {
            I++;
            Assert.AreEqual(1, I);
        }

        [SmokeTest]
        public void B()
        {
            I++;
            Assert.AreEqual(2, I);
        }

        [SmokeTest]
        public void A()
        {
            I++;
            Assert.AreEqual(3, I);
        }

        [SmokeTest]
        public void F()
        {

            I++;
            Assert.AreEqual(4, I);
        }

        [SmokeTest]
        public void H()
        {
            I++;
            Assert.AreEqual(5, I);
        }

        [SmokeTest]
        public void K()
        {
            I++;
            Assert.AreEqual(6, I);
        }

        [SmokeTest]
        public void J()
        {
            I++;
            Assert.AreEqual(7, I);
        }

        [SmokeTest]
        public void V()
        {
            I++;
            Assert.AreEqual(8, I);
        }

        [SmokeTest]
        public void AA()
        {
            I++;
            Assert.AreEqual(9, I);
        }

        [SmokeTest]
        public void AB()
        {
            I++;
            Assert.AreEqual(10, I);
        }
    }
}
