using System;
using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class PrimitiveTest
    {
        [Theory]
        [InlineData(byte.MinValue)]
        [InlineData(byte.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteByte(byte value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(sbyte.MinValue)]
        [InlineData(sbyte.MaxValue)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteSbyte(sbyte value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(short.MinValue)]
        [InlineData(short.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteShort(short value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(ushort.MinValue)]
        [InlineData(ushort.MaxValue)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteUshort(ushort value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteInt(int value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(uint.MinValue)]
        [InlineData(uint.MaxValue)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteUint(uint value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteLong(long value)
        {
            AssertReadAndWrite(value);
        }

        [Theory]
        [InlineData(ulong.MinValue)]
        [InlineData(ulong.MaxValue)]
        [InlineData(1)]
        [InlineData(123)]
        public void ReadAndWriteUlong(ulong value)
        {
            AssertReadAndWrite(value);
        }

        [Fact]
        public void ReadAndWriteTimeSpan()
        {
            AssertReadAndWrite(TimeSpan.MinValue);
            AssertReadAndWrite(TimeSpan.MaxValue);
            AssertReadAndWrite(new TimeSpan(123456L));
        }

        [Fact]
        public void ReadAndWriteDateTime()
        {
            AssertReadAndWrite(DateTime.MinValue, 8);
            AssertReadAndWrite(DateTime.MaxValue, 8);
            AssertReadAndWrite(DateTime.Now, 8);
        }
    }
}
