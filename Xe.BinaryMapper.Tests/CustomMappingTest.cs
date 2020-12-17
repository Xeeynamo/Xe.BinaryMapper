using System;
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
    }
}
