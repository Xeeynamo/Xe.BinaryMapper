using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xe.BinaryMapper
{
    public partial class BinaryMapping
    {
        private class MyProperty
        {
            public PropertyInfo MemberInfo { get; set; }

            public DataAttribute DataInfo { get; set; }
        }

        public static object ReadObject(BinaryReader reader, object obj, int baseOffset = 0)
        {
            var properties = obj.GetType()
                .GetProperties()
                .Select(x => new MyProperty
                {
                    MemberInfo = x,
                    DataInfo = Attribute.GetCustomAttribute(x, typeof(DataAttribute)) as DataAttribute
                })
                .Where(x => x.DataInfo != null)
                .ToList();

            var args = new MappingReadArgs
            {
                Reader = reader
            };

            foreach (var property in properties)
            {
                if (property.DataInfo.Offset.HasValue)
                {
                    reader.BaseStream.Position = baseOffset + property.DataInfo.Offset.Value;
                }

                var value = ReadProperty(args, property.MemberInfo.PropertyType, property);
                property.MemberInfo.SetValue(obj, value);
            }

            return obj;
        }

        private static object ReadProperty(MappingReadArgs args, Type type, MyProperty property)
        {
            if (mappings.TryGetValue(type, out var mapping))
            {
                args.DataAttribute = property.DataInfo;
                return mapping.Reader(args);
            }
            else if (type.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(type);
                if (!mappings.TryGetValue(underlyingType, out mapping))
                    throw new InvalidDataException($"The enum {type.Name} has an unsupported size.");

                args.DataAttribute = property.DataInfo;
                return mapping.Reader(args);
            }
            else if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var listType = type.GetGenericArguments().FirstOrDefault();
                if (listType == null)
                    throw new InvalidDataException($"The list {property.MemberInfo.Name} does not have any specified type.");

                var addMethod = type.GetMethod("Add");
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));

                for (int i = 0; i < property.DataInfo.Count; i++)
                {
                    var oldPosition = (int)args.Reader.BaseStream.Position;

                    var item = ReadProperty(args, listType, property);
                    addMethod.Invoke(list, new[] { item });

                    var newPosition = args.Reader.BaseStream.Position;
                    args.Reader.BaseStream.Position += Math.Max(0, property.DataInfo.Stride - (newPosition - oldPosition));
                }

                return list;
            }
            else
            {
                return ReadObject(args.Reader, Activator.CreateInstance(type), (int)args.Reader.BaseStream.Position);
            }
        }

        private static string ReadString(BinaryReader reader, int length)
        {
            var data = reader.ReadBytes(length);
            var terminatorIndex = Array.FindIndex(data, x => x == 0);
            return StringEncoding.GetString(data, 0, terminatorIndex < 0 ? length : terminatorIndex);
        }
    }
}
