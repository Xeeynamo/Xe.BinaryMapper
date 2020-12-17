using System;
using System.IO;
using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class CustomMappingTest
    {
        [Fact]
        public void ReadAndWriteTimeSpan()
        {
            var binaryMapping = MappingConfiguration
                .DefaultConfiguration()
                .ForType<TimeSpan>(
                    x => new TimeSpan(0, 0, seconds: x.Reader.ReadInt32()),
                    x => x.Writer.Write((int)((TimeSpan)x.Item).TotalSeconds))
                .Build();

            AssertReadAndWrite(binaryMapping, new TimeSpan(hours: 0, minutes: 0, seconds: 500), 4);
        }

        public class Foo
        {
            public int Bar { get; set; }
        }

        [Fact]
        public void ReadAndWriteCustomClass()
        {
            var mapping = MappingConfiguration
                .DefaultConfiguration()
                .ForType<Foo>(
                    x => new Foo
                    {
                        Bar = 123
                    },
                    x =>
                    {
                        x.Writer.Write((byte)1);
                        x.Writer.Write((byte)2);
                        x.Writer.Write((byte)3);
                    })
                .Build();

            Assert.Equal(123, mapping.ReadObject<Foo>(new MemoryStream()).Bar);

            using var memStream = new MemoryStream();
            mapping.WriteObject(memStream, new Foo());

            memStream.Position = 0;
            Assert.Equal(1, memStream.ReadByte());
            Assert.Equal(2, memStream.ReadByte());
            Assert.Equal(3, memStream.ReadByte());

        }
    }
}
