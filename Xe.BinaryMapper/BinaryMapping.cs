using System;
using System.Collections.Generic;
using System.IO;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        private class MappingWriteArgs
        {
            public BinaryWriter Writer { get; set; }

            public object Item { get; set; }

            public DataAttribute DataAttribute { get; set; }
        }

        private class MappingReadArgs
        {
            public BinaryReader Reader { get; set; }

            public DataAttribute DataAttribute { get; set; }
        }

        private class Mapping
        {
            public Action<MappingWriteArgs> Writer { get; set; }

            public Func<MappingReadArgs, object> Reader { get; set; }
        }

        private static Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>
        {
            [typeof(bool)] = new Mapping
            {
                Writer = x => x.Writer.Write((bool)x.Item ? 1 : 0),
                Reader = x => x.Reader.ReadByte() != 0
            },
            [typeof(byte)] = new Mapping
            {
                Writer = x => x.Writer.Write((byte)x.Item),
                Reader = x => x.Reader.ReadByte()
            },
            [typeof(sbyte)] = new Mapping
            {
                Writer = x => x.Writer.Write((sbyte)x.Item),
                Reader = x => x.Reader.ReadSByte()
            },
            [typeof(short)] = new Mapping
            {
                Writer = x => x.Writer.Write((short)x.Item),
                Reader = x => x.Reader.ReadInt16()
            },
            [typeof(ushort)] = new Mapping
            {
                Writer = x => x.Writer.Write((ushort)x.Item),
                Reader = x => x.Reader.ReadUInt16()
            },
            [typeof(int)] = new Mapping
            {
                Writer = x => x.Writer.Write((int)x.Item),
                Reader = x => x.Reader.ReadInt32()
            },
            [typeof(uint)] = new Mapping
            {
                Writer = x => x.Writer.Write((uint)x.Item),
                Reader = x => x.Reader.ReadUInt32()
            },
            [typeof(long)] = new Mapping
            {
                Writer = x => x.Writer.Write((long)x.Item),
                Reader = x => x.Reader.ReadInt64()
            },
            [typeof(ulong)] = new Mapping
            {
                Writer = x => x.Writer.Write((ulong)x.Item),
                Reader = x => x.Reader.ReadUInt64()
            },
            [typeof(float)] = new Mapping
            {
                Writer = x => x.Writer.Write((float)x.Item),
                Reader = x => x.Reader.ReadSingle()
            },
            [typeof(double)] = new Mapping
            {
                Writer = x => x.Writer.Write((double)x.Item),
                Reader = x => x.Reader.ReadDouble()
            },
            [typeof(TimeSpan)] = new Mapping
            {
                Writer = x => x.Writer.Write(((TimeSpan)x.Item).Ticks),
                Reader = x => new TimeSpan(x.Reader.ReadInt64())
            },
            [typeof(DateTime)] = new Mapping
            {
                Writer = x => x.Writer.Write(((DateTime)x.Item).Ticks),
                Reader = x => new DateTime(x.Reader.ReadInt64())
            },
            [typeof(string)] = new Mapping
            {
                Writer = x => Write(x.Writer, (string)x.Item, x.DataAttribute.Count),
                Reader = x => ReadString(x.Reader, x.DataAttribute.Count)
            },
        };
    }
}
