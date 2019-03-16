using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        private static readonly byte[] dummy = new byte[1024];

        public static object WriteObject(BinaryWriter writer, object obj, int baseOffset = 0)
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
                object value = property.MemberInfo.GetValue(obj);
                var type = property.MemberInfo.PropertyType;
                var offset = property.DataInfo.Offset;

                if (offset.HasValue)
                {
                    writer.BaseStream.Position = baseOffset + offset.Value;
                }

                if (WritePrimitive(writer, type, value)) { }
                else if (type == typeof(string)) Write(writer, value as string, property.DataInfo.Count);
                else if (type == typeof(byte[])) writer.Write((byte[])value, 0, property.DataInfo.Count);
                else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var listType = type.GetGenericArguments().FirstOrDefault();
                    if (listType == null)
                        throw new InvalidDataException($"The list {property.MemberInfo.Name} does not have any specified type.");

                    var missing = property.DataInfo.Count;
                    foreach (var item in value as IEnumerable)
                    {
                        if (missing-- < 1)
                            break;

                        WriteObject(writer, item, listType, property.DataInfo.Stride);
                    }

                    while (missing-- > 0)
                    {
                        var item = Activator.CreateInstance(listType);
                        WriteObject(writer, item, listType, property.DataInfo.Stride);
                    }
                }
                else
                {
                    WriteObject(writer, value, (int)writer.BaseStream.Position);
                }

                property.MemberInfo.SetValue(obj, value);
            }

            return obj;
        }

        private static void WriteObject(BinaryWriter writer, object item, Type listType, int stride)
        {
            var oldPosition = (int)writer.BaseStream.Position;
            if (!WritePrimitive(writer, listType, item))
                WriteObject(writer, item, oldPosition);

            var newPosition = writer.BaseStream.Position;
            if (stride > 0)
            {
                var missingBytes = stride - (int)(newPosition - oldPosition);
                if (missingBytes < 0)
                {
                    throw new InvalidOperationException($"The stride is smaller than {listType.Name} definition.");
                }
                else if (missingBytes > 0)
                {
                    do
                    {
                        int toWrite = Math.Min(dummy.Length, missingBytes);
                        writer.Write(dummy, 0, toWrite);
                        missingBytes -= toWrite;
                    } while (missingBytes > 0);
                }
            }
        }

        private static bool WritePrimitive(BinaryWriter writer, Type type, object value)
        {
            if (type == typeof(bool)) writer.Write((bool)value ? 1 : 0);
            else if (type == typeof(byte)) writer.Write((byte)value);
            else if (type == typeof(sbyte)) writer.Write((sbyte)value);
            else if (type == typeof(short)) writer.Write((short)value);
            else if (type == typeof(ushort)) writer.Write((ushort)value);
            else if (type == typeof(int)) writer.Write((int)value);
            else if (type == typeof(uint)) writer.Write((uint)value);
            else if (type == typeof(long)) writer.Write((long)value);
            else if (type == typeof(ulong)) writer.Write((ulong)value);
            else if (type == typeof(TimeSpan)) writer.Write(((TimeSpan)value).Ticks);
            else if (type == typeof(DateTime)) writer.Write(((DateTime)value).Ticks);
            else if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                if (!WritePrimitive(writer, underlyingType, value))
                    throw new InvalidDataException($"The enum {type.Name} has an unsupported size.");
            }
            else return false;

            return true;
        }

        public static void Write(BinaryWriter writer, string str, int length)
        {
            var data = Encoding.UTF8.GetBytes(str);
            if (data.Length <= length)
            {
                writer.Write(data, 0, data.Length);
                int remainsBytes = length - data.Length;
                if (remainsBytes > 0)
                {
                    writer.Write(new byte[remainsBytes]);
                }
            }
            else
            {
                writer.Write(data, 0, length);
            }
        }
    }
}
