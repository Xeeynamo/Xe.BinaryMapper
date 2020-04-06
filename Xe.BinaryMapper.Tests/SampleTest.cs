using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class SampleTest
    {
        class Sample
        {
            [Data] public short Foo { get; set; }
            [Data(offset: 4, count: 3, stride: 2)] public List<byte> Bar { get; set; }
        }

        [Fact]
        public void ReadmeSampleTest()
        {
            var memory = new MemoryStream(10);

            var obj = new Sample
            {
                Foo = 123,
                Bar = new List<byte>(){ 22, 44 }
            };
            BinaryMapping.WriteObject(memory, obj);

            var buffer = memory.GetBuffer();
            Assert.Equal(new byte[]
            {
                0x7B, 0x00, 0x00, 0x00, 0x16, 0x00, 0x2C, 0x00, 0x00, 0x00
            }, buffer);
        }
    }
}
