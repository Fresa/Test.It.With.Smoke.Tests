using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Test.It.With.Smoke.Tests.Using.NUnit.Tests
{
    public class MyProcessBuildingSpecificationAttribute : ChainedFixtureSpecificationAttribute
    {
        public MyProcessBuildingSpecificationAttribute()
        {
            FixtureIterator = ChainedFixtures < MyFixtureGeneratedTests>
                .Start(() => new MyFixtureGeneratedTests())
                .Next(tests => new MyFixtureGeneratedTests2(tests.i))
                .Build();
        }
    }

    [MyProcessBuildingSpecification]
    public class MyFixtureGeneratedTests
    {
        public int i = 0;

        [SmokeTest]
        public void Hej_test()
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
            //Assert.True(false);
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
