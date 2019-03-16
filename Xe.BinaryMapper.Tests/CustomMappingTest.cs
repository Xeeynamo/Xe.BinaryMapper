using System;
using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class CustomMappingTest
    {
        private class Fixture : IGeneric<TimeSpan>
        {
            [Data] public TimeSpan Value { get; set; }
        }


        [Fact]
        public void ReadAndWriteTimeSpan()
        {
            BinaryMapping.SetMapping<TimeSpan>(new BinaryMapping.Mapping
            {
                Writer = x => x.Writer.Write((int)((TimeSpan)x.Item).TotalSeconds),
                Reader = x => new TimeSpan(0, 0, seconds: x.Reader.ReadInt32())
            });

            AssertReadAndWrite(new TimeSpan(hours: 0, minutes: 0, seconds: 500), 4);
            BinaryMapping.RemoveCustomMappings();
        }
    }
}
