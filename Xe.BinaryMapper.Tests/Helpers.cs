using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace Xe.BinaryMapper.Tests
{
    public static class Helpers
    {
        public interface IGeneric<T>
        {
            T Value { get; set; }
        }

        public class Generic<T> : IGeneric<T>
        {
            [Data] public T Value { get; set; }
        }

        public class ValuePair<T>
        {
            public T Expected { get; set; }

            public T Actual { get; set; }
        }

        public static void AssertReadAndWrite<T>(T value)
        {
            AssertReadAndWrite(value, Marshal.SizeOf<T>());
        }

        public static void AssertReadAndWrite<T>(T value, int expectedLength, Action<ValuePair<T>> assertion = null) =>
            AssertReadAndWrite<T>(BinaryMapping.Default, value, expectedLength, assertion);

        public static void AssertReadAndWrite<T>(IBinaryMapping mapper, T value, int expectedLength, Action<ValuePair<T>> assertion = null)
        {
            var expected = new Generic<T>
            {
                Value = value
            };
            var actual = new Generic<T>();

            var memory = new MemoryStream();
            mapper.WriteObject(memory, expected);

            Assert.Equal(expectedLength, memory.Length);

            memory.Position = 0;
            mapper.ReadObject(memory, actual);

            Assert.Equal(expectedLength, memory.Position);

            if (assertion != null)
            {
                assertion(new ValuePair<T>
                {
                    Expected = value,
                    Actual = actual.Value
                });
            }
            else
            {
                Assert.Equal(expected.Value, actual.Value);
            }
        }

        public static void AssertReadAndWrite<T>(
            IGeneric<T> expected,
            int expectedLength,
            Action<ValuePair<T>> assertion = null)
        {
            AssertReadAndWrite(expected, expected.Value, expectedLength, assertion);
        }

        public static void AssertReadAndWrite<T>(
            IGeneric<T> value,
            T expected,
            int expectedLength,
            Action<ValuePair<T>> assertion = null)
        {
            var actualType = value
                .GetType();

            if (typeof(T).IsArray)
            {
                expectedLength *= Marshal.SizeOf(typeof(T).GetMethod("Get").ReturnType);
            }

            var actual = (IGeneric<T>)Activator.CreateInstance(actualType);

            var memory = new MemoryStream();
            BinaryMapping.WriteObject(memory, value);

            Assert.Equal(expectedLength, memory.Length);

            memory.Position = 0;
            BinaryMapping.ReadObject(memory, actual);

            Assert.Equal(expectedLength, memory.Position);

            if (assertion != null)
            {
                assertion.Invoke(new ValuePair<T>
                {
                    Expected = expected,
                    Actual = actual.Value
                });
            }
            else
            {
                Assert.Equal(expected, actual.Value);
            }
        }
    }
}
