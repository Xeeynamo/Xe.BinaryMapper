using System.IO;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class BitfieldTest
    {
        private class SimpleBitfieldFixture
        {
            [Data] public bool Bit0 { get; set; }
            [Data] public bool Bit1 { get; set; }
            [Data] public bool Bit2 { get; set; }
            [Data] public bool Bit3 { get; set; }
            [Data] public bool Bit4 { get; set; }
            [Data] public bool Bit5 { get; set; }
            [Data] public bool Bit6 { get; set; }
            [Data] public bool Bit7 { get; set; }
            [Data] public bool Bit8 { get; set; }
            [Data] public bool Bit9 { get; set; }
        }

        private class DiscontinuedBitfieldFixture
        {
            [Data] public bool Bit10 { get; set; }
            [Data] public bool Bit11 { get; set; }
            [Data] public short RandomData { get; set; }
            [Data] public bool Bit20 { get; set; }
            [Data] public bool Bit21 { get; set; }
        }

        private class OffsetBitfieldFixture
        {
            [Data(1)] public bool Bit10 { get; set; }
            [Data(1)] public bool Bit11 { get; set; }
            [Data(1)] public bool Bit12 { get; set; }
            [Data(0)] public bool Bit00 { get; set; }
            [Data(0)] public bool Bit01 { get; set; }
        }

        private class IndexedBitfieldFixture
        {
            [Data(1, BitIndex = 1)] public bool Bit11 { get; set; }
            [Data(2, BitIndex = 2)] public bool Bit22 { get; set; }
            [Data] public bool Bit23 { get; set; }
            [Data(2, BitIndex = 5)] public bool Bit25 { get; set; }
        }

        [Fact]
        public void SimpleBitfieldTest()
        {
            var rawData = new byte[] { 0xAD, 0x02 };
            var memStream = new MemoryStream(rawData);
            var actual = BinaryMapping.ReadObject(memStream, new SimpleBitfieldFixture()) as SimpleBitfieldFixture;

            Assert.NotNull(actual);
            Assert.Equal(2, memStream.Position);
            Assert.True(actual.Bit0);
            Assert.False(actual.Bit1);
            Assert.True(actual.Bit2);
            Assert.True(actual.Bit3);
            Assert.False(actual.Bit4);
            Assert.True(actual.Bit5);
            Assert.False(actual.Bit6);
            Assert.True(actual.Bit7);
            Assert.False(actual.Bit8);
            Assert.True(actual.Bit9);

            memStream = new MemoryStream();
            BinaryMapping.WriteObject(memStream, actual);

            Assert.Equal(2, memStream.Position);
            memStream.Position = 0;
            Assert.Equal(rawData[0], memStream.ReadByte());
            Assert.Equal(rawData[1], memStream.ReadByte());
        }

        [Fact]
        public void DiscontinuedBitfieldTest()
        {
            var rawData = new byte[] { 0x01, 0xFF, 0xFF, 0x02 };
            var memStream = new MemoryStream(rawData);
            var actual = BinaryMapping.ReadObject(memStream, new DiscontinuedBitfieldFixture()) as DiscontinuedBitfieldFixture;

            Assert.NotNull(actual);
            Assert.Equal(4, memStream.Position);
            Assert.True(actual.Bit10);
            Assert.False(actual.Bit11);
            Assert.Equal(-1, actual.RandomData);
            Assert.False(actual.Bit20);
            Assert.True(actual.Bit21);

            memStream = new MemoryStream();
            BinaryMapping.WriteObject(memStream, actual);

            Assert.Equal(4, memStream.Position);
            memStream.Position = 0;
            Assert.Equal(rawData[0], memStream.ReadByte());
            Assert.Equal(rawData[1], memStream.ReadByte());
            Assert.Equal(rawData[2], memStream.ReadByte());
            Assert.Equal(rawData[3], memStream.ReadByte());
        }

        [Fact]
        public void OffsetBitfieldTest()
        {
            var rawData = new byte[] { 2, 5 };
            var memStream = new MemoryStream(rawData);
            var actual = BinaryMapping.ReadObject(memStream, new OffsetBitfieldFixture()) as OffsetBitfieldFixture;

            Assert.NotNull(actual);
            Assert.True(actual.Bit10);
            Assert.False(actual.Bit11);
            Assert.True(actual.Bit12);
            Assert.False(actual.Bit00);
            Assert.True(actual.Bit01);

            memStream = new MemoryStream();
            BinaryMapping.WriteObject(memStream, actual);

            Assert.Equal(2, memStream.Length);
            memStream.Position = 0;
            Assert.Equal(rawData[0], memStream.ReadByte());
            Assert.Equal(rawData[1], memStream.ReadByte());
        }

        [Fact]
        public void IndexedBitfieldTest()
        {
            var rawData = new byte[] { 0, 2, 0x2c };
            var memStream = new MemoryStream(rawData);
            var actual = BinaryMapping.ReadObject(memStream, new IndexedBitfieldFixture()) as IndexedBitfieldFixture;

            Assert.NotNull(actual);
            Assert.True(actual.Bit11);
            Assert.True(actual.Bit22);
            Assert.True(actual.Bit23);
            Assert.True(actual.Bit25);

            memStream = new MemoryStream();
            BinaryMapping.WriteObject(memStream, actual);

            Assert.Equal(3, memStream.Length);
            memStream.Position = 0;
            Assert.Equal(rawData[0], memStream.ReadByte());
            Assert.Equal(rawData[1], memStream.ReadByte());
            Assert.Equal(rawData[2], memStream.ReadByte());
        }
    }
}
