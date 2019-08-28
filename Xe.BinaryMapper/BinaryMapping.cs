using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        public class MappingWriteArgs
        {
            public BinaryWriter Writer { get; set; }

            public object Item { get; set; }

            public DataAttribute DataAttribute { get; set; }

            public int Count { get; set; }

            public byte BitData { get; set; }

            public int BitIndex { get; set; }
        }

        public class MappingReadArgs
        {
            public BinaryReader Reader { get; set; }

            public DataAttribute DataAttribute { get; set; }

            public int Count { get; set; }

            public byte BitData { get; set; }

            public int BitIndex { get; set; }
        }

        public class Mapping
        {
            public Action<MappingWriteArgs> Writer { get; set; }

            public Func<MappingReadArgs, object> Reader { get; set; }
        }

        private static Dictionary<Type, Mapping> mappings = DefaultMapping();
        private static Dictionary<Type, Dictionary<string, Func<object, int>>> memberMappings = new Dictionary<Type, Dictionary<string, Func<object, int>>>();

        public static Encoding StringEncoding { get; set; } = Encoding.UTF8;

        public static void SetMapping<T>(Mapping mapping) => SetMapping(typeof(T), mapping);

        public static void SetMapping(Type type, Mapping mapping) => mappings[type] = mapping;

        public static void SetMemberLengthMapping<T>(string memberName, Func<T, string, int> getLengthFunc)
            where T : class
        {
            var classType = typeof(T);
            UseMappings(mappings =>
            {
                if (!mappings.TryGetValue(classType, out var classMapping))
                {
                    classMapping = new Dictionary<string, Func<object, int>>();
                    mappings.Add(classType, classMapping);
                    classMapping[memberName] = o => getLengthFunc((T)o, memberName);
                }
            });
        }

        public static void RemoveCustomMappings() => mappings = DefaultMapping();

        private static MyProperty GetPropertySettings(Type classType, PropertyInfo propertyInfo)
        {
            var property = new MyProperty
            {
                MemberInfo = propertyInfo,
                DataInfo = Attribute.GetCustomAttribute(propertyInfo, typeof(DataAttribute)) as DataAttribute
            };

            UseMappings(mappings =>
            {
                if (mappings.TryGetValue(classType, out var classMapping))
                {
                    if (classMapping.TryGetValue(propertyInfo.Name, out var func))
                    {
                        property.GetLengthFunc = func;
                    }
                }
            });

            return property;
        }

        private static void UseMappings(Action<Dictionary<Type, Dictionary<string, Func<object, int>>>> action)
        {
            lock (memberMappings)
                action(memberMappings);
        }

        private static Dictionary<Type, Mapping> DefaultMapping() => new Dictionary<Type, Mapping>
        {
            [typeof(bool)] = new Mapping
            {
                Writer = x =>
                {
                    if (x.BitIndex >= 8)
                        FlushBitField(x);
                    if (x.DataAttribute.BitIndex >= 0)
                        x.BitIndex = x.DataAttribute.BitIndex;

                    if (x.Item is bool bit && bit)
                        x.BitData |= (byte)(1 << x.BitIndex);

                    x.BitIndex++;
                },
                Reader = x =>
                {
                    if (x.BitIndex >= 8)
                        x.BitIndex = 0;
                    if (x.BitIndex == 0)
                        x.BitData = x.Reader.ReadByte();
                    if (x.DataAttribute.BitIndex >= 0)
                        x.BitIndex = x.DataAttribute.BitIndex;

                    return (x.BitData & (1 << x.BitIndex++)) != 0;
                }
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
                Writer = x => Write(x.Writer, (string)x.Item, x.Count),
                Reader = x => ReadString(x.Reader, x.Count)
            },
            [typeof(byte[])] = new Mapping
            {
                Writer = x =>
                {
                    var data = (byte[])x.Item;
                    var bytesToWrite = Math.Min(data.Length, x.Count);
                    x.Writer.Write(data, 0, bytesToWrite);
                    
                    var remainingBytes = x.Count - bytesToWrite;
                    if (remainingBytes > 0)
                        x.Writer.Write(new byte[remainingBytes], 0, remainingBytes);
                },
                Reader = x => x.Reader.ReadBytes(x.Count)
            },
        };
    }
}
