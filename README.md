# About

Xe.BinaryMapper is a .Net library that is capable to deserialize and serialize a binary file into a managed object. BinaryMapper aims to be easy to use and to hack.

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
    Bar = new { 22, 44 }
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

# Types supported

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
* `TimeSpan` 8 bytes long.
* `DateTime` 8 bytes long. Ignores the Kind property.
* `Enum` customizable size based on inherted type.
* `string` fixed size based from `count` parameter.
* `byte[]` fixed array of bytes based from `count` parameter.
* `List<>` fixed list based from `count` parameter of any object or one of the types specified above.

# Future plans

* Customize the de/serialization of specific types
* Improve performance caching types
* NuGet definition
* CI/CD for testing and publishing on NuGet

# Projects that uses BinaryMapper

Kingdom Hearts 3 Save Editor 

https://github.com/Xeeynamo/KH3SaveEditor

Written by the author of BinaryMapper. This is a perfect example on a real scenario of how BinaryMapper can be used.