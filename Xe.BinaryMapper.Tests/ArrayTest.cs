﻿using System;
using System.Linq;
using Xunit;
using static Xe.BinaryMapper.Tests.Helpers;

namespace Xe.BinaryMapper.Tests
{
    public class ArrayTest
    {
        private class FixtureWithoutData<T> : IGeneric<T[]>
        {
            public T[] Value { get; set; }
        }

        private class FixtureWithData<T> : IGeneric<T[]>
        {
            [Data] public T[] Value { get; set; }
        }

        private class FixtureWithDataAndCustomLength<T> : IGeneric<T[]>
        {
            [Data(Count = 5)] public T[] Value { get; set; }
        }

        [Fact]
        public void ByteArrayWithoutData()
        {
            AssertReadAndWrite(new FixtureWithoutData<byte>()
            {
                Value = new byte[1]
            }, null, 0);
        }

        [Theory]
        //[InlineData(0)]
        //[InlineData(1)]
        [InlineData(10)]
        public void ByteArrayWithData(int length)
        {
            GenericArrayWithDataAndCustomLength<byte, FixtureWithData<byte>>(1, length);
        }

        [Theory]
        //[InlineData(5, 0)]
        //[InlineData(5, 4)]
        [InlineData(5, 5)]
        [InlineData(5, 6)]
        public void ByteArrayWithDataAndCustomLength(int expectedLength, int length)
        {
            GenericArrayWithDataAndCustomLength<byte, FixtureWithDataAndCustomLength<byte>>(expectedLength, length);
        }

        private static void GenericArrayWithDataAndCustomLength<T, TFixture>(int expectedLength, int length)
            where TFixture : IGeneric<T[]>
        {
            var actual = Enumerable.Range(0, length)
                    .Select(x => (T)Convert.ChangeType(x, typeof(T)))
                    .ToArray();

            var expected = Enumerable.Range(0, expectedLength)
                    .Select(x => x < length ? x : 0)
                    .Select(x => (T)Convert.ChangeType(x, typeof(T)))
                    .ToArray();

            var obj = (IGeneric<T[]>)Activator.CreateInstance(typeof(TFixture));
            obj.Value = actual;

            AssertReadAndWrite(obj, expected, expectedLength);
        }
    }
}