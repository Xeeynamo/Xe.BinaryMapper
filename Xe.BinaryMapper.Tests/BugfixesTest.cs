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

#region issue #3
        public class Issue3_SimpleStructure
        {
            [Data] public byte Value { get; set; }
        }

        public class Issue3_SampleClass
        {
            static Issue3_SampleClass()
            {
            }

            [Data]
            public int Length
            {
                get => Structures.TryGetCount();
                set => Structures = Structures.CreateOrResize(value);
            }
            [Data] public List<Issue3_SimpleStructure> Structures { get; set; }

            [Data]
            public int Length2
            {
                get => Structures2.TryGetCount();
                set => Structures2 = Structures2.CreateOrResize(value);
            }
            [Data] public List<Issue3_SimpleStructure> Structures2 { get; set; }
        }

        [Fact]
        public void ShouldAddMoreThanOneMemberLengthForClass()
        {
            var mapping = MappingConfiguration
                .DefaultConfiguration()
                .UseMemberForLength<Issue3_SampleClass>(nameof(Issue3_SampleClass.Structures), (o, m) => o.Length)
                .UseMemberForLength<Issue3_SampleClass>(nameof(Issue3_SampleClass.Structures2), (o, m) => o.Length2)
                .Build();

            var stream = new MemoryStream(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            var content = mapping.ReadObject<Issue3_SampleClass>(stream);
        }
#endregion
    }
}
