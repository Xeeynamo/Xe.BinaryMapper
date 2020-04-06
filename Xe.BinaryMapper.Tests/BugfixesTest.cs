using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public class BugfixesTest
    {
        private class Foo
        {
            [Data(0, 1, 0x10)] public List<Bar> List { get; set; }
        }

        private class Bar
        {
            [Data(0, 0x10)] public byte[] Data { get; set; }

            [Data(8)] public int SomeValue { get; set; }
        }

        [Fact]
        public void WriteShouldNotOverwriteAnyDataWhenStrideIsConsidered()
        {
            var foo = new Foo
            {
                List = new List<Bar>
                {
                    new Bar()
                    {
                        Data = new byte[0x80],
                        SomeValue = 123
                    }
                }
            };

            foo.List[0].Data[0xC] = 231;

            var memStream = new MemoryStream();
            BinaryMapping.WriteObject(memStream, foo);
            memStream.Position = 0;

            var foo2 = BinaryMapping.ReadObject(memStream, new Foo()) as Foo;
            Assert.Equal(foo.List[0].Data[0xC], foo2.List[0].Data[0xC]);
        }
    }
}
