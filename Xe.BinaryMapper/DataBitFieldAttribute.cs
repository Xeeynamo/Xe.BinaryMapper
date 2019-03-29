using System;

namespace Xe.BinaryMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataBitFieldAttribute : DataAttribute
    {
        public int? BitIndex { get; }

        public DataBitFieldAttribute()
        { }

        public DataBitFieldAttribute(int offset) :
            base(offset)
        { }

        public DataBitFieldAttribute(int offset, int bitIndex) :
            base(offset)
        {
            if (bitIndex < 0 || bitIndex >= 8)
                throw new ArgumentOutOfRangeException($"{nameof(bitIndex)} must be between 0 and 7");
            BitIndex = bitIndex;
        }
    }
}
