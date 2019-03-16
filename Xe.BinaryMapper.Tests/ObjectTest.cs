using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class ObjectTest
    {
        private class MultipleValues
        {
            [Data] public int Key { get; set; }
            [Data(Count = 12)] public string Value { get; set; }
        }

        private class NestedObjects
        {
            [Data] public int RandomValue { get; set; }
            [Data] public MultipleValues RandomObject { get; set; }
        }

        [Fact]
        public void AssertMultipleValues()
        {
            AssertReadAndWrite(new MultipleValues
            {
                Key = 123,
                Value = "Test"
            }, 16, pair =>
            {
                Assert.Equal(pair.Expected.Key, pair.Actual.Key);
                Assert.Equal(pair.Expected.Value, pair.Actual.Value);
            });
        }

        [Fact]
        public void AssertNestedObjects()
        {
            AssertReadAndWrite(new NestedObjects
            {
                RandomValue = 999,
                RandomObject = new MultipleValues
                {
                    Key = 123,
                    Value = "Test"
                }
            }, 20, pair =>
            {
                Assert.Equal(pair.Expected.RandomValue, pair.Actual.RandomValue);
                Assert.Equal(pair.Expected.RandomObject.Key, pair.Actual.RandomObject.Key);
                Assert.Equal(pair.Expected.RandomObject.Value, pair.Actual.RandomObject.Value);
            });
        }
    }
}
