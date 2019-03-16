using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class EnumTest
    {
        private enum GenericEnum
        {
            RandomValue = 123,
            AnotherRandomValue
        };

        private enum ByteEnum : byte
        {
            RandomValue = 123,
            AnotherRandomValue
        };

        private enum ShortEnum : short
        {
            RandomValue = 123,
            AnotherRandomValue
        };

        [Fact]
        public void GenericEnumTest()
        {
            AssertReadAndWrite(GenericEnum.RandomValue, 4);
            AssertReadAndWrite(GenericEnum.AnotherRandomValue, 4);
        }

        [Fact]
        public void ByteEnumTest()
        {
            AssertReadAndWrite(ByteEnum.RandomValue, 1);
            AssertReadAndWrite(ByteEnum.AnotherRandomValue, 1);
        }

        [Fact]
        public void ShortEnumTest()
        {
            AssertReadAndWrite(ShortEnum.RandomValue, 2);
            AssertReadAndWrite(ShortEnum.AnotherRandomValue, 2);
        }
    }
}
