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

        [Fact]
        public void ReadSimpleBitfieldTest()
        {
            var data = new byte[] {0xAD, 0x02};
            var actual = BinaryMapping.ReadObject(new BinaryReader(new MemoryStream(data)), new ReadSimpleBitfieldFixture()) as ReadSimpleBitfieldFixture;

            Assert.NotNull(actual);
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
    }
}
