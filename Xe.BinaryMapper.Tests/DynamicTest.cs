using System.IO;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class DynamicTest
    {
        private class DynamicStringFixture
        {
            [Data] public byte Length { get; set; }
            [Data] public string Text { get; set; }
        }

        [Theory]
        [InlineData(0, "hello", "")]
        [InlineData(1, "hello", "h")]
        [InlineData(2, "hello", "he")]
        [InlineData(5, "hello", "hello")]
        [InlineData(10, "hello", "hello")]
        public void DynamicStringTest(int length, string text, string expected)
        {
            var obj = new DynamicStringFixture
            {
                Length = (byte)length,
                Text = text
            };

            var memStream = new MemoryStream();
            BinaryMapping.SetMemberLengthMapping<DynamicStringFixture>(nameof(obj.Text), (o, m) => o.Length);
            BinaryMapping.WriteObject(new BinaryWriter(memStream), obj);
            Assert.Equal(length + 1, memStream.Position);
        }
    }
}
