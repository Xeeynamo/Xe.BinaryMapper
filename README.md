# Summary

* [Overview](#about)
* [Dependencies and installation](#requirements-and-installation)
* [Usage and documentation](#usage-and-documentation)

    * [Serialization](#serialization)
    * [Deserialization](#deserialization)
    * [Properties and Data attribute](#properties-and-data-attribute)
    * [Type bool and bit fields](#type-bool-and-bit-fields)
    * [Customize type mapping](#customize-type-mapping)
    * [Dynamic length of type List< T >](#dynamic-length-of-type-list)

* [Example](#example)
* [Types supported](#types-supported)
* [Future plans](#future-plans)
* [Showcase](#showcase)


# Overview

Xe.BinaryMapper is a .Net library that is capable to deserialize and serialize a binary file into a managed object. BinaryMapper aims to be easy to use and to hack, without using additional dependencies.

[![NuGet](https://img.shields.io/nuget/v/Xe.BinaryMapper.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/Xe.BinaryMapper/)
![Last commit](https://img.shields.io/github/last-commit/xeeynamo/xe.binarymapper.svg?style=flat-square)

[![Build status](https://img.shields.io/appveyor/ci/xeeynamo/xe-binarymapper/master.svg?style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/xeeynamo/xe-binarymapper/branch/master)
[![Test status](https://img.shields.io/appveyor/tests/xeeynamo/xe-binarymapper.svg?compact_message&style=flat-square&logo=appveyor)](https://ci.appveyor.com/project/xeeynamo/xe-binarymapper/branch/master/tests)

![Downloads](https://img.shields.io/nuget/dt/xe.binarymapper.svg?style=flat-square)
[![Issues](https://img.shields.io/github/issues/xeeynamo/xe.binarymapper.svg?style=flat-square)](https://github.com/xeeynamo/xe.binarymapper/issues)

# Requirements and installation

Xe.BinaryMapper is compatible with any project compiled using .Net Framework 3.5, .Net Framework 4.x or .Net Standard 2.0. The library is standalone and does not require with any other dependencies than the framework itself.

The library is available on NuGet. A `Install-Package Xe.BinaryMapper` will make it available in your project in few seconds.

There are no known limitations on using the library using the Mono runtime (eg. Unity3d).

# Usage and documentation

## Serialization

The entire serialization happens in `BinaryMapping.WriteObject`, which accepts the following parameters:

* `Stream` / `BinaryWriter`: A writable stream where the content will be written to.

* `T item` / `object item`: The object that needs to be serialized.

* `int baseOffset`: The absolute position in the stream where the write will start. (optional)

The returned value is the same item passed as parameter.

## Deserialization

The entire de-serialization happens in `BinaryMapping.ReadObject`, which accepts the following parameters:

* `Stream` / `BinaryReader`: A readable stream where the content will be read from.

* `T item` / `object item`: An existing object where all the serializable properties will be populated by the read content. If no item is specified, an instance of `T` will be created as long as the constructor's class is parameterless. (optional)

* `int baseOffset`: The absolute position in the stream where the read will start. (optional)

## Properties and `Data` attribute

The `DataAttribute` is really important. Every property with this attribute will be evaluated during the de/serialization. It can be used only on a property that has public getter and setter. The following three parameters can be specified:

* `offset` where the data is physically located inside the file; the value is relative to the class definition. If not specified, the offset value is the same as the previous offset + its value size.
* `count` how many times the item should be de/serialized. This is only useful for `T[]` or `List<T>` types.
* `stride` how long is the actual data to de/serialize. This is very useful to skip some data when de/serializing `List<T>` data.
* `bitIndex` A custom bit index to de/serialize. -1 ignores it, while between 0 and 7 is a valid value.

# Type `bool` and bit fields

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

## Customize type mapping

To customize how the de/serialization works for a specific type, a `Mapping` object must be passed to `BinaryMapping.SetMapping`.

A `Mapping` object is defined by two actions: `Writer` and `Reader`. An example on how to customize a mapping can be found here:

```csharp
BinaryMapping.SetMapping<bool>(new BinaryMapping.Mapping
{
    Writer = x => x.Writer.Write((byte)((bool)x.Item ? 1 : 0)),
    Reader = x => x.Reader.ReadByte() != 0
});
```

## Dynamic length of type `List<>`

When you specify `[Data(Count = 5)]` on a `List<T>`, that property will be de/serialized with a fixed length of 5, no matter what. Often you do not want to be stuck on that, since you might want to be able to specify a dynamic amount of elements. This can be achieved with a method called `BinaryMapping.SetMemberLengthMapping<T>`.

Let's take the following example:
```csharp
private class ListExample
{
    [Data] public int Count { get; set; }
    [Data] public List<DynamicStringFixture> Items { get; set; }
}
```

You should be able to insert any amount of `Items` as possible, but of course you should define before a property that will read/write the amount of elements in it. TO achieve that, you need to link `Items` with `Count`, using the following statement:

```csharp
BinaryMapping.SetMemberLengthMapping<ListExample>(nameof(ListExample.Items), (o, m) => o.Count);
```

The code above says that, for the class `ListExample`, you want that the amount of elements inside `ListExample.Items` has to be taken from `Count`. Notice that in `(o, m)`, the `o` is the object instance of `ListExample` that will be processed right before `Items`, while `m` is a string that will be equal to the property name `Items`, useful if some branch condition is needed based on the property name.

The problem with the code above is that you need to need to update `Count` manually before to serialize the object back, since it is a value that lives by its own. The best way is to use an helper method contained in `BinaryMappingHelpers` to get and set automatically the size of a `List<T>`. For that, you will need to modify `ListExample` like this:

```csharp
private class ListExample
{
    [Data] public int Count
    {
        get => Items.TryGetCount();
        set => Items = Items.CreateOrResize(value);
    }
    [Data] public List<DynamicStringFixture> Items { get; set; }
}
```

In that way you will couple `Count` and `Items` together, automating the step to update `Count` manually and reducing the amount of errors on your code.


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

## How the data is de/serialized under the hood
The binary data serialized few lines ago can be break down in the following flow logic:

`[Data] public short Foo { get; set; }`

Write a `short` (or `System.Int16`), so 2 bytes, of foo that contains the `123` value: `7B 00` is written.

`[Data(offset: 4, count: 3, stride: 2)] public List<byte> Bar { get; set; }`

Move to `offset` 4, which is 4 bytes after the initial class definition. But we already written 2 bytes, so just move 2 bytes forward.

We now have a `List<>` of two `System.Byte`. The `stride` between each value is 2 bytes, so write the first element `22` (our `0x16`), skip one byte of stride and do the same with the second element `44`.

But the `count` is `3`, so we will just write other two bytes of zeroed data.

## Can be done more?

Absolutely! Many primitive values are supported and can be customized (like how to de/serialize TimeSpan for example). Plus, nested class definitions can be used.

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
* `T[]` fixed array of any type, based from `count` parameter.
* `List<>` dynamic list of any type.

# Future plans

* Improve performance caching types
* BinaryMapping object instances, without relying to a global instance
* Custom object de/serialization
* Support for existing classes without using DataAttribute
* Big-endian support

# Showcase

## Kingdom Hearts 3 Save Editor

https://github.com/Xeeynamo/KH3SaveEditor

Written by the author of BinaryMapper. This is a perfect example on a real scenario of how BinaryMapper can be used.


## OpenKH

https://github.com/Xeeynamo/OpenKH

Another example on how binary files from a videogame can be mapped into C# objects