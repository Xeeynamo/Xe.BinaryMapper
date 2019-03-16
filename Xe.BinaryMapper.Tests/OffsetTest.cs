using System.Collections.Generic;
using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class OffsetTest
    {
        private class FixtureOffsetTest : IGeneric<int>
        {
            [Data(offset: 16)] public int Value { get; set; }
        }

        private class FixtureWihoutStrideTest
        {
            [Data(offset: 4, count: 2)] public List<FixtureOffsetTest> Value { get; set; }
        }

        private class FixtureWihStrideTest
        {
            [Data(offset: 4, count: 2, stride: 128)] public List<FixtureOffsetTest> Value { get; set; }
        }

        [Fact]
        public void CustomOffsetTest()
        {
            AssertReadAndWrite<int>(new FixtureOffsetTest()
            {
                Value = 123
            }, 20);
        }

        [Fact]
        public void CustomOffsetWithoutTest()
        {
            AssertReadAndWrite(new FixtureWihoutStrideTest()
            {
                Value = new List<FixtureOffsetTest>()
                {
                    new FixtureOffsetTest() { Value = 456 },
                    new FixtureOffsetTest() { Value = 789 }
                }
            }, 44, pair =>
            {
                Assert.Equal(2, pair.Actual.Value.Count);
                Assert.Equal(456, pair.Actual.Value[0].Value);
                Assert.Equal(789, pair.Actual.Value[1].Value);
            });
        }

        [Fact]
        public void CustomOffsetWithStrideTest()
        {
            AssertReadAndWrite(new FixtureWihStrideTest()
            {
                Value = new List<FixtureOffsetTest>()
                {
                    new FixtureOffsetTest() { Value = 456 },
                    new FixtureOffsetTest() { Value = 789 }
                }
            }, 260, pair =>
            {
                Assert.Equal(2, pair.Actual.Value.Count);
                Assert.Equal(456, pair.Actual.Value[0].Value);
                Assert.Equal(789, pair.Actual.Value[1].Value);
            });
        }
    }
}
