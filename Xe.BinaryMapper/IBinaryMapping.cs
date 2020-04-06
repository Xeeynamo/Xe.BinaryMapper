using System.IO;

namespace Xe.BinaryMapper
{
    public interface IBinaryMapping
    {
        T ReadObject<T>(Stream stream, int baseOffset = 0) where T : class;
        T ReadObject<T>(Stream stream, T item, int baseOffset = 0) where T : class;
        T WriteObject<T>(Stream stream, T item, int baseOffset = 0) where T : class;
    }
}
