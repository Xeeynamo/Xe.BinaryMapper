using System;

namespace Xe.BinaryMapper
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataBitFieldAttribute : DataAttribute
    {
        public DataBitFieldAttribute()
        { }

        public DataBitFieldAttribute(int offset) :
            base(offset)
        { }
    }
}
