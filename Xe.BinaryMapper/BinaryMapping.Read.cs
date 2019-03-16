using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        public static object ReadObject(BinaryReader reader, object obj, int baseOffset = 0)
        {
            var properties = obj.GetType()
                .GetProperties()
                .Select(x => new
                {
                    MemberInfo = x,
                    DataInfo = Attribute.GetCustomAttribute(x, typeof(DataAttribute)) as DataAttribute
                })
                .Where(x => x.DataInfo != null)
                .ToList();

            foreach (var property in properties)
            {
                object value;
                var type = property.MemberInfo.PropertyType;
                var offset = property.DataInfo.Offset;

                if (offset.HasValue)
                {
                    reader.BaseStream.Position = baseOffset + offset.Value;
                }

                if (ReadPrimitive(reader, type, out var outValue)) value = outValue;
                else if (type == typeof(string)) value = ReadString(reader, property.DataInfo.Count);
                else if (type == typeof(byte[])) value = reader.ReadBytes(property.DataInfo.Count);
                else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var listType = type.GetGenericArguments().FirstOrDefault();
                    if (listType == null)
                        throw new InvalidDataException($"The list {property.MemberInfo.Name} does not have any specified type.");

                    var addMethod = type.GetMethod("Add");
                    value = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                    for (int i = 0; i < property.DataInfo.Count; i++)
                    {
                        var oldPosition = (int)reader.BaseStream.Position;
                        if (ReadPrimitive(reader, listType, out var listValue))
                        {
                            addMethod.Invoke(value, new[] { listValue });
                        }
                        else
                        {
                            addMethod.Invoke(value, new[] { ReadObject(reader, Activator.CreateInstance(listType), oldPosition) });
                        }

                        var newPosition = reader.BaseStream.Position;
                        reader.BaseStream.Position += Math.Max(0, property.DataInfo.Stride - (newPosition - oldPosition));
                    }
                }
                else
                {
                    value = ReadObject(reader, Activator.CreateInstance(type), (int)reader.BaseStream.Position);
                }

                property.MemberInfo.SetValue(obj, value);
            }

            return obj;
        }


        private static bool ReadPrimitive(BinaryReader reader, Type type, out object value)
        {
            if (type == typeof(bool)) value = reader.ReadByte() != 0;
            else if (type == typeof(byte)) value = reader.ReadByte();
            else if (type == typeof(sbyte)) value = reader.ReadSByte();
            else if (type == typeof(short)) value = reader.ReadInt16();
            else if (type == typeof(ushort)) value = reader.ReadUInt16();
            else if (type == typeof(int)) value = reader.ReadInt32();
            else if (type == typeof(uint)) value = reader.ReadUInt32();
            else if (type == typeof(long)) value = reader.ReadInt64();
            else if (type == typeof(ulong)) value = reader.ReadUInt64();
            else if (type == typeof(float)) value = reader.ReadSingle();
            else if (type == typeof(double)) value = reader.ReadDouble();
            else if (type == typeof(TimeSpan)) value = new TimeSpan(reader.ReadInt64());
            else if (type == typeof(DateTime)) value = new DateTime(reader.ReadInt64());
            else if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                if (!ReadPrimitive(reader, underlyingType, out value))
                    throw new InvalidDataException($"The enum {type.Name} has an unsupported size.");
            }
            else
            {
                value = null;
                return false;
            }

            return true;
        }

        private static string ReadString(BinaryReader reader, int length)
        {
            var data = reader.ReadBytes(length);
            var terminatorIndex = Array.FindIndex(data, x => x == 0);
            return Encoding.UTF8.GetString(data, 0, terminatorIndex < 0 ? length : terminatorIndex);
        }
    }
}
