using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        private class DynamicObjectFixture
        {
            [Data] public byte Count { get; set; }
            [Data] public List<DynamicStringFixture> Items { get; set; }
        }

        private class DynamicObjectWithAutoCountFixture
        {
            [Data]
            public byte Count
            {
                get => (byte)Items.TryGetCount();
                set => Items = Items.CreateOrResize(value);
            }
            [Data] public List<DynamicStringFixture> Items { get; set; }
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

            var mapper = MappingConfiguration.DefaultConfiguration()
                .UseMemberForLength<DynamicStringFixture>(nameof(DynamicStringFixture.Text), (o, m) => o.Length)
                .Build();

            mapper.WriteObject(memStream, obj);
            Assert.Equal(length + 1, memStream.Position);

            memStream.Position = 0;
            var actual = mapper.ReadObject<DynamicStringFixture>(memStream);
            Assert.Equal(length, actual.Length);
            Assert.Equal(expected, actual.Text);
            Assert.Equal(length + 1, memStream.Position);
        }

        [Fact]
        public void DynamicObjectTest()
        {
            var obj = new DynamicObjectFixture
            {
                Count = 3,
                Items = new List<DynamicStringFixture>()
                {
                    new DynamicStringFixture
                    {
                        Length = 2,
                        Text = "hi"
                    },
                    new DynamicStringFixture
                    {
                        Length = 3,
                        Text = "hey"
                    },
                    new DynamicStringFixture
                    {
                        Length = 5,
                        Text = "hello"
                    }
                }
            };

            var mapper = MappingConfiguration.DefaultConfiguration()
                .UseMemberForLength<DynamicStringFixture>(nameof(DynamicStringFixture.Text), (o, m) => o.Length)
                .UseMemberForLength<DynamicObjectFixture>(nameof(DynamicObjectFixture.Items), (o, m) => o.Count)
                .Build();

            var memStream = new MemoryStream();
            mapper.WriteObject(memStream, obj);
            Assert.Equal(14, memStream.Position);

            memStream.Position = 0;
            var actual = mapper.ReadObject<DynamicObjectFixture>(memStream);
            Assert.Equal(obj.Count, actual.Count);
            Assert.Equal(obj.Items.Count, actual.Items.Count);
            Assert.Equal(obj.Items[0].Length, actual.Items[0].Length);
            Assert.Equal(obj.Items[0].Text, actual.Items[0].Text);
            Assert.Equal(obj.Items[1].Length, actual.Items[1].Length);
            Assert.Equal(obj.Items[1].Text, actual.Items[1].Text);
            Assert.Equal(obj.Items[2].Length, actual.Items[2].Length);
            Assert.Equal(obj.Items[2].Text, actual.Items[2].Text);
            Assert.Equal(14, memStream.Position);
        }

        [Fact]
        public void DynamicObjectWithAutoCountTest()
        {
            var obj = new DynamicObjectWithAutoCountFixture
            {
                Items = new List<DynamicStringFixture>()
                {
                    new DynamicStringFixture
                    {
                        Length = 2,
                        Text = "hi"
                    },
                    new DynamicStringFixture
                    {
                        Length = 3,
                        Text = "hey"
                    },
                    new DynamicStringFixture
                    {
                        Length = 5,
                        Text = "hello"
                    }
                }
            };

            var mapper = MappingConfiguration.DefaultConfiguration()
                .UseMemberForLength<DynamicStringFixture>(nameof(DynamicStringFixture.Text), (o, m) => o.Length)
                .UseMemberForLength<DynamicObjectWithAutoCountFixture>(nameof(DynamicObjectFixture.Items), (o, m) => o.Count)
                .Build();

            var memStream = new MemoryStream();
            mapper.WriteObject(memStream, obj);
            Assert.Equal(14, memStream.Position);

            memStream.Position = 0;
            var actual = mapper.ReadObject<DynamicObjectWithAutoCountFixture>(memStream);
            Assert.Equal(obj.Count, actual.Count);
            Assert.Equal(obj.Items.Count, actual.Items.Count);
            Assert.Equal(obj.Items[0].Length, actual.Items[0].Length);
            Assert.Equal(obj.Items[0].Text, actual.Items[0].Text);
            Assert.Equal(obj.Items[1].Length, actual.Items[1].Length);
            Assert.Equal(obj.Items[1].Text, actual.Items[1].Text);
            Assert.Equal(obj.Items[2].Length, actual.Items[2].Length);
            Assert.Equal(obj.Items[2].Text, actual.Items[2].Text);
            Assert.Equal(14, memStream.Position);
        }
    }
}
