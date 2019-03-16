using System;
using System.Collections.Generic;
using System.IO;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        private class Mapping
        {
            public Action<BinaryWriter, object> Writer { get; set; }

            public Func<BinaryReader, object> Reader { get; set; }
        }

        private static Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>
        {
            [typeof(bool)] = new Mapping
            {
                Writer = (w, o) => w.Write((bool)o ? 1 : 0),
                Reader = x => x.ReadByte() != 0
            },
            [typeof(byte)] = new Mapping
            {
                Writer = (w, o) => w.Write((byte)o),
                Reader = x => x.ReadByte()
            },
            [typeof(sbyte)] = new Mapping
            {
                Writer = (w, o) => w.Write((sbyte)o),
                Reader = x => x.ReadSByte()
            },
            [typeof(short)] = new Mapping
            {
                Writer = (w, o) => w.Write((short)o),
                Reader = x => x.ReadInt16()
            },
            [typeof(ushort)] = new Mapping
            {
                Writer = (w, o) => w.Write((ushort)o),
                Reader = x => x.ReadUInt16()
            },
            [typeof(int)] = new Mapping
            {
                Writer = (w, o) => w.Write((int)o),
                Reader = x => x.ReadInt32()
            },
            [typeof(uint)] = new Mapping
            {
                Writer = (w, o) => w.Write((uint)o),
                Reader = x => x.ReadUInt32()
            },
            [typeof(long)] = new Mapping
            {
                Writer = (w, o) => w.Write((long)o),
                Reader = x => x.ReadInt64()
            },
            [typeof(ulong)] = new Mapping
            {
                Writer = (w, o) => w.Write((ulong)o),
                Reader = x => x.ReadUInt64()
            },
            [typeof(float)] = new Mapping
            {
                Writer = (w, o) => w.Write((float)o),
                Reader = x => x.ReadSingle()
            },
            [typeof(double)] = new Mapping
            {
                Writer = (w, o) => w.Write((double)o),
                Reader = x => x.ReadDouble()
            },
            [typeof(TimeSpan)] = new Mapping
            {
                Writer = (w, o) => w.Write(((TimeSpan)o).Ticks),
                Reader = x => new TimeSpan(x.ReadInt64())
            },
            [typeof(DateTime)] = new Mapping
            {
                Writer = (w, o) => w.Write(((DateTime)o).Ticks),
                Reader = x => new DateTime(x.ReadInt64())
            },
        };
    }
}
