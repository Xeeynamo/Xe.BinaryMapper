using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class BufferedReaderTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(9)]
        [InlineData(100000)]
        public void ReadBufferTest(int length)
        {
            var expect = RandomData().Take(length).ToArray();

            var stream = new MemoryStream();
            stream.Write(expect);

            stream.Position = 0;
            var actual = new BufferedReader(stream, true).ReadBytes(length);

            Assert.Equal(expect, actual);
        }

        [Fact]
        public void ReadSomeIntegers()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(2020);
            writer.Write(9999);

            stream.Position = 0;
            var reader = new BufferedReader(stream, true);
            Assert.Equal(2020, reader.ReadInt32());
            Assert.Equal(9999, reader.ReadInt32());
        }

        [Fact]
        public void MovePositionBeforeAndRead()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(0x01234567);
            writer.Write(0x89abcdef);

            stream.Position = 2;
            var reader = new BufferedReader(stream, true);

            Assert.Equal(2, reader.Position);
            Assert.Equal(0xcdef0123, reader.ReadUInt32());
            Assert.Equal(6, reader.Position);
        }

        [Fact]
        public void MovePositionBackwardAndRead()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(0x01234567);
            writer.Write(0x89abcdef);

            var reader = new BufferedReader(stream, true);
            reader.Position = 0;

            Assert.Equal(0x01234567U, reader.ReadUInt32());
            Assert.Equal(0x89abcdefU, reader.ReadUInt32());
            reader.Position -= 4;
            Assert.Equal(0x89abcdefU, reader.ReadUInt32());
        }

        [Fact]
        public void UpdatePositionCorrectly()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(0x01234567);
            writer.Write(0x89abcdef);

            var reader = new BufferedReader(stream, true);
            reader.Position = 4;
            Assert.Equal(4, reader.Position);
        }

        private static IEnumerable<byte> RandomData()
        {
            while (true)
            {
                var guid = Guid.NewGuid().ToByteArray();
                yield return guid[0x0];
                yield return guid[0x1];
                yield return guid[0x2];
                yield return guid[0x3];
                yield return guid[0x4];
                yield return guid[0x5];
                yield return guid[0x6];
                yield return guid[0x7];
                yield return guid[0x8];
                yield return guid[0x9];
                yield return guid[0xa];
                yield return guid[0xb];
                yield return guid[0xc];
                yield return guid[0xd];
                yield return guid[0xe];
                yield return guid[0xf];
            }
        }
    }
}
