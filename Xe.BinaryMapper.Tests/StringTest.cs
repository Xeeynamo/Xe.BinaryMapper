using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class StringTest
    {
        private class FixtureWithoutData : IGeneric<string>
        {
            public string Value { get; set; }
        }

        private class FixtureWithData : IGeneric<string>
        {
            [Data] public string Value { get; set; }
        }

        private class FixtureWithDataAndCustomLength : IGeneric<string>
        {
            [Data(Count = 5)] public string Value { get; set; }
        }

        [Fact]
        public void StringWithoutData()
        {
            AssertReadAndWrite(new FixtureWithoutData()
            {
                Value = "This will not be serialized"
            }, null, 0);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("test", "t")]
        [InlineData("teeest", "t")]
        [InlineData("x", "x")]
        public void StringWithData(string value, string expectedValue)
        {
            AssertReadAndWrite(new FixtureWithData()
            {
                Value = value
            }, expectedValue, 1);
        }

        [Theory]
        [InlineData("", "", 5)]
        [InlineData("test", "test", 5)]
        [InlineData("teest", "teest", 5)]
        [InlineData("teeest", "teees", 5)]
        public void StringWithDataAndCustomLength(string value, string expectedValue, int expectedLength)
        {
            AssertReadAndWrite(new FixtureWithDataAndCustomLength()
            {
                Value = value
            }, expectedValue, expectedLength);
        }
    }
}
