

# About

Xe.BinaryMapper is a .Net library that is capable to deserialize and serialize a binary file into a managed object. BinaryMapper aims to be easy to use and to hack, without using additional dependencies.

The library is available on NuGet and a `Install-Package Xe.BinaryMapper` will make it available in your project in few seconds.

[![NuGet](https://img.shields.io/nuget/v/Xe.BinaryMapper.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Xe.BinaryMapper/)
![Last commit](https://img.shields.io/github/last-commit/xeeynamo/xe.binarymapper.svg?style=flat-square)

[![Build status](https://img.shields.io/appveyor/ci/xeeynamo/xe-binarymapper/master.svg?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/xeeynamo/xe-binarymapper/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/xeeynamo/xe-binarymapper.svg?compact_message&style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/xeeynamo/xe-binarymapper/branch/master/tests)

![Downloads](https://img.shields.io/nuget/dt/xe.binarymapper.svg?style=flat-square)
[![Issues](https://img.shields.io/github/issues/xeeynamo/xe.binarymapper.svg?style=flat-square)](https://github.com/xeeynamo/xe.binarymapper/issues)

# Example

 ```csharp
 class Sample
 {
     [Data] public short Foo { get; set; }
     [Data(offset: 4, count: 3, stride: 2)] public List<byte> Bar { get; set; }
 }

 ...

var obj = new Sample
{
    Foo = 123,
    Bar = new List<byte>(){ 22, 44 }
};
BinaryMapping.WriteObject(writer, obj);
```
will be serialized into `7B 00 00 00 16 00 2C 00 00 00`.

## How the data is de/serialized
The binary data serialized few lines ago can be break down in the following flow logic:

`[Data] public short Foo { get; set; }`

Write a `short` (or `System.Int16`), so 2 bytes, of foo that contains the `123` value: `7B 00` is written.

`[Data(offset: 4, count: 3, stride: 2)] public List<byte> Bar { get; set; }`

Move to `offset` 4, which is 4 bytes after the initial class definition. But we already written 2 bytes, so just move 2 bytes forward.

We now have a `List<>` of two `System.Byte`. The `stride` between each value is 2 bytes, so write the first element `22` (our `0x16`), skip one byte of stride and do the same with the second element `44`.

But the `count` is `3`, so we will just write other two bytes of zeroed data.

## Can be done more?

Absolutely! Many primitive values are supported and can be customized (like how to de/serialize TimeSpan for example). Plus, nested class definitions can be used.

# Usage

## Serialization

The entire serialization happens in `BinaryMapping.WriteObject`, which accepts a `BinaryWriter` to write into a stream and the object to serialize.

The serialization always starts from `BinaryWriter.BaseStream.Position`.

## Deserialization

The entire de-serialization happens in `BinaryMapping.ReadObject`, which accepts a `BinaryReader` to read from the specified object and an existing object that will be used to store the read data.

The deserialization always starts from `BinaryReader.BaseStream.Position`.

## The Data attribute

The `DataAttribute` is really important because, without it, the mapping does not happen.

The DataAttribute can be used only on a property that has public getter and setter, and has the following three optional parameters:

* `offset` where the data is physically located inside the file; the value is relative to the class definition. If not specified, the offset value is the same as the previous offset + its value size.
* `count` how many times the item should be de/serialized. This is only useful for `byte[]` or `List<T>` types.
* `stride` how long is the actual data to de/serialize. This is very useful to skip some data when de/serializing `List<T>` data.
* `bitIndex` A custom bit index to de/serialize. -1 ignores it, while between 0 and 7 is a valid value.

## The type `bool` and bit fields

By default, boolean types are read bit by bit if they are aligned. Infact, 8 consecutive boolean properties are considered 1 byte long.

```csharp
[Data] public bool Bit0 { get; set; }
[Data] public bool Bit1 { get; set; }
[Data] public bool Bit2 { get; set; }
[Data] public bool Bit3 { get; set; }
[Data] public byte SomeRandomData { get; set; }
```

The code snippet above will read a total of 2 bytes and only the first 4 bits of the first byte will be considered.

```csharp
[Data] public bool Bit0 { get; set; }
[Data] public bool Bit1 { get; set; }
[Data] public byte SomeRandomData { get; set; }
[Data] public bool Bit2 { get; set; }
[Data] public bool Bit3 { get; set; }
```

The code snippet above will read a total of 3 bytes. The first two bits will be read, then a byte and then the first two bits of the next byte. This is why order is important for alignment.

```csharp
[Data(0)] public bool Bit0 { get; set; }
[Data] public bool Bit1 { get; set; }
[Data] public byte SomeRandomData { get; set; }
[Data(0, BitIndex = 2)] public bool Bit2 { get; set; }
[Data] public bool Bit3 { get; set; }
```

The code snippet above will read again only 2 bytes. After reading the 2nd byte, it will return to the position 0 and to the 3rd bit (0 based index), continuing the read from there.

## Custom mapping

To customize how the de/serialization works for a specific type, a `Mapping` object must be passed to `BinaryMapping.SetMapping`.

A `Mapping` object is defined by two actions: `Writer` and `Reader`. An example on how to customize a mapping can be found here:

```csharp
BinaryMapping.SetMapping<bool>(new BinaryMapping.Mapping
{
    Writer = x => x.Writer.Write((byte)((bool)x.Item ? 1 : 0)),
    Reader = x => x.Reader.ReadByte() != 0
});
```

# Types supported

* `bool` / `System.Boolean` 1 bit long.
* `byte` / `System.Byte` 1 byte long.
* `sbyte` / `System.SByte` 1 byte long.
* `short` / `System.Int16` 2 bytes long.
* `ushort` / `System.UInt16` 2 bytes long.
* `int` / `System.Int32` 4 bytes long.
* `uint` / `System.UInt32` 4 bytes long.
* `long` / `System.Int64` 8 bytes long.
* `ulong` / `System.UInt64` 8 bytes long.
* `float` / `System.Single` 4 bytes long.
* `double` / `System.Double` 8 bytes long.
* `Enum` variable length.
* `TimeSpan` 8 bytes long.
* `DateTime` 8 bytes long. Ignores the Kind property.
* `Enum` customizable size based on inherted type.
* `string` fixed size based from `count` parameter.
* `byte[]` fixed array of bytes based from `count` parameter.
* `List<>` fixed list based from `count` parameter of any object or one of the types specified above.

# Future plans

* Improve performance caching types
* Array and IEnumerable support

# Projects that uses BinaryMapper

Kingdom Hearts 3 Save Editor

https://github.com/Xeeynamo/KH3SaveEditor

Written by the author of BinaryMapper. This is a perfect example on a real scenario of how BinaryMapper can be used.