using NUnit.Framework;
using Test.It.With.Smoke.Tests.Using.NUnit.Configuration;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Tests
{
    public class MyProcessBuildingSpecificationAttribute : ChainedFixtureSpecificationAttribute
    {
        public override IBuildFixtures FixtureBuilder { get; } =
            ChainedFixtures<MyFixtureGeneratedTests>
                .Start(() => new MyFixtureGeneratedTests())
                .Next(tests => new MyFixtureGeneratedTests2(tests.I));
    }

    [MyProcessBuildingSpecification]
    public class MyFixtureGeneratedTests
    {
        public int I;

        public MyFixtureGeneratedTests()
        {
            I = 0;
        }

        [SmokeTest]
        public void Hej_test()
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
            //Assert.True(false);
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

        [SmokeTest]
        public void AC()
        {
            I++;
            Assert.AreEqual(11, I);
        }
    }

    public class MyFixtureGeneratedTests2
    {
        private int _i;

        public MyFixtureGeneratedTests2(int i)
        {
            _i = i;
        }

        [SmokeTest]
        public void H()
        {
            _i++;
            Assert.AreEqual(12, _i);
        }
    }
}
