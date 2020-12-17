using System.Collections.Generic;
using System.IO;
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

        private class EnumArray
        {
            [Data(Count = 3)] public ShortEnum[] Array { get; set; }
        }

        private class EnumList
        {
            [Data(Count = 3)] public List<ShortEnum> Array { get; set; }
        }

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

        [Fact]
        public void ArrayOfEnums()
        {
            var obj = new EnumArray()
            {
                Array = new ShortEnum[]
                {
                    (ShortEnum)1,
                    (ShortEnum)3,
                    (ShortEnum)5,
                }
            };

            var stream = new MemoryStream();
            BinaryMapping.WriteObject(stream, obj);
            Assert.Equal(6, stream.Length);

            stream.Position = 0;
            var actual = BinaryMapping.ReadObject<EnumArray>(stream);
            Assert.Equal(obj.Array[0], actual.Array[0]);
            Assert.Equal(obj.Array[1], actual.Array[1]);
            Assert.Equal(obj.Array[2], actual.Array[2]);
        }

        [Fact]
        public void ListOfEnums()
        {
            var obj = new EnumList()
            {
                Array = new List<ShortEnum>
                {
                    (ShortEnum)1,
                    (ShortEnum)3,
                    (ShortEnum)5,
                }
            };

            var stream = new MemoryStream();
            BinaryMapping.WriteObject(stream, obj);
            Assert.Equal(6, stream.Length);

            stream.Position = 0;
            var actual = BinaryMapping.ReadObject<EnumArray>(stream);
            Assert.Equal(obj.Array[0], actual.Array[0]);
            Assert.Equal(obj.Array[1], actual.Array[1]);
            Assert.Equal(obj.Array[2], actual.Array[2]);
        }
    }
}
