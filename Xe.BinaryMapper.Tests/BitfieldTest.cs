using System.IO;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class BitfieldTest
    {
        private class ReadSimpleBitfieldFixture
        {
            [DataBitField] public bool Bit0 { get; set; }
            [DataBitField] public bool Bit1 { get; set; }
            [DataBitField] public bool Bit2 { get; set; }
            [DataBitField] public bool Bit3 { get; set; }
            [DataBitField] public bool Bit4 { get; set; }
            [DataBitField] public bool Bit5 { get; set; }
            [DataBitField] public bool Bit6 { get; set; }
            [DataBitField] public bool Bit7 { get; set; }
            [DataBitField] public bool Bit8 { get; set; }
            [DataBitField] public bool Bit9 { get; set; }
        }

        private class ReadDiscontinuedBitfieldFixture
        {
            [DataBitField] public bool Bit10 { get; set; }
            [DataBitField] public bool Bit11 { get; set; }
            [Data] public short RandomData { get; set; }
            [DataBitField] public bool Bit20 { get; set; }
            [DataBitField] public bool Bit21 { get; set; }
        }

        [Fact]
        public void ReadSimpleBitfieldTest()
        {
            var memStream = new MemoryStream(new byte[] { 0xAD, 0x02 });
            var actual = BinaryMapping.ReadObject(new BinaryReader(memStream), new ReadSimpleBitfieldFixture()) as ReadSimpleBitfieldFixture;

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
        }

        [Fact]
        public void ReadDiscontinuedBitfieldTest()
        {
            var memStream = new MemoryStream(new byte[] { 0x01, 0xFF, 0xFF, 0x02 });
            var actual = BinaryMapping.ReadObject(new BinaryReader(memStream), new ReadDiscontinuedBitfieldFixture()) as ReadDiscontinuedBitfieldFixture;

            Assert.NotNull(actual);
            Assert.Equal(4, memStream.Position);
            Assert.True(actual.Bit10);
            Assert.False(actual.Bit11);
            Assert.Equal(-1, actual.RandomData);
            Assert.False(actual.Bit20);
            Assert.True(actual.Bit21);
        }
    }
}
