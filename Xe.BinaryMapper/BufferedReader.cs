using System;
using System.IO;

namespace Xe.BinaryMapper
{
    public class BufferedReader : Stream
    {
        private const int BufferSize = 8192;
        private static readonly IOException EndOfStreamException = new IOException("End of the stream");
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private long _basePosition;
        private int _bufferPosition;
        private int _bufferSize;
        private long _newBasePosition;
        private bool _willInvalidatePosition;
        private bool _isLittleEndian;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _stream.Length;

        public override long Position
        {
            get
            {
                if (_willInvalidatePosition)
                    return _newBasePosition;

                return _basePosition + _bufferPosition;
            }

            set
            {
                if (_willInvalidatePosition || _basePosition > value || _basePosition + _buffer.Length < value)
                {
                    _willInvalidatePosition = true;
                    _newBasePosition = value;
                }
                else
                {
                    _willInvalidatePosition = false;
                    _bufferPosition = (int)(value - _basePosition);
                }

                _stream.Seek(value, SeekOrigin.Begin);
            }
        }

        public BufferedReader(Stream stream, bool isLittleEndian)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new IOException("Stream must be readable and seekable");

            _stream = stream;
            _buffer = new byte[BufferSize];
            _bufferPosition = 0;
            _basePosition = stream.Position;
            _newBasePosition = _basePosition;
            _willInvalidatePosition = true;
            _isLittleEndian = isLittleEndian;
        }

        public override void Flush()
        { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            // If the requested amount of bytes is bigger than the internal buffer
            // then not bother
            if (count > BufferSize)
            {
                _newBasePosition = _basePosition + _bufferPosition + count;
                _willInvalidatePosition = true;

                Position = _basePosition + _bufferPosition;
                return _stream.Read(buffer, offset, count);
            }

            if (_willInvalidatePosition)
            {
                _willInvalidatePosition = false;
                ResetBlock(_newBasePosition);
            }

            // Check if the buffer will overflow
            if (IsBuffered(count))
            {
                var remain = _bufferSize - _bufferPosition;
                Copy(buffer, offset, _buffer, _bufferPosition, remain);
                _bufferPosition += remain;

                ReadNextBlock();
                Copy(buffer, offset + remain, _buffer, _bufferPosition, count - remain);
                _bufferPosition += count - remain;
            }
            else
            {
                Copy(buffer, offset, _buffer, _bufferPosition, count);
                _bufferPosition += count;
            }

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;
                case SeekOrigin.Current:
                    return Position += offset;
                case SeekOrigin.End:
                    return Position = Length + offset;
                default:
                    return _stream.Seek(offset, origin);
            }
        }

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public byte[] ReadBytes(int count)
        {
            if (count == 0)
                return new byte[0];

            var data = new byte[count];
            Read(data, 0, count);

            return data;
        }

        public byte ReadUInt8()
        {
            RequestBufferAccess();

            return _buffer[_bufferPosition++];
        }

        public sbyte ReadInt8() => (sbyte)ReadUInt8();

        [System.Security.SecuritySafeCritical]
        public unsafe ushort ReadUInt16()
        {
            RequestBufferAccess();

            if (_bufferSize < 2)
                throw EndOfStreamException;

            if (_isLittleEndian)
                fixed (byte* p = _buffer)
                {
                    var value = *(ushort*)(p + _bufferPosition);
                    _bufferPosition += 2;
                    return value;
                }
                //return (ushort)(_buffer[_bufferPosition++] | (_buffer[_bufferPosition++] << 8));
            else
                return (ushort)((_buffer[_bufferPosition++] << 8) | _buffer[_bufferPosition++]);
        }

        public short ReadInt16() => (short)ReadUInt16();

        [System.Security.SecuritySafeCritical]
        public unsafe uint ReadUInt32()
        {
            RequestBufferAccess();

            if (_bufferSize < 4)
                throw EndOfStreamException;

            if (_isLittleEndian)
                fixed (byte* p = _buffer)
                {
                    var value = *(uint*)(p + _bufferPosition);
                    _bufferPosition += 4;
                    return value;
                }
                //return _buffer[_bufferPosition++] | ((uint)_buffer[_bufferPosition++] << 8) |
                // ((uint)_buffer[_bufferPosition++] << 16) | ((uint)_buffer[_bufferPosition++] << 24);
            else
                return ((uint)_buffer[_bufferPosition++] << 24) | ((uint)_buffer[_bufferPosition++] << 16) |
                     ((uint)_buffer[_bufferPosition++] << 8) | _buffer[_bufferPosition++];
        }

        public int ReadInt32() => (int)ReadUInt32();

        [System.Security.SecuritySafeCritical]
        public unsafe ulong ReadUInt64()
        {
            RequestBufferAccess();

            if (_bufferSize < 8)
                throw EndOfStreamException;

            if (_isLittleEndian)
                fixed (byte* p = _buffer)
                {
                    var value = *(ulong*)(p + _bufferPosition);
                    _bufferPosition += 8;
                    return value;
                }
                //return _buffer[_bufferPosition++] | ((ulong)_buffer[_bufferPosition++] << 8) |
                // ((ulong)_buffer[_bufferPosition++] << 16) | ((ulong)_buffer[_bufferPosition++] << 24) |
                // ((ulong)_buffer[_bufferPosition++] << 32) | ((ulong)_buffer[_bufferPosition++] << 40) |
                // ((ulong)_buffer[_bufferPosition++] << 48) | ((ulong)_buffer[_bufferPosition++] << 56);
            else
                return ((ulong)_buffer[_bufferPosition++] << 56) | ((ulong)_buffer[_bufferPosition++] << 48) |
                     ((ulong)_buffer[_bufferPosition++] << 40) | ((ulong)_buffer[_bufferPosition++] << 32) |
                     ((ulong)_buffer[_bufferPosition++] << 24) | ((ulong)_buffer[_bufferPosition++] << 16) |
                     ((ulong)_buffer[_bufferPosition++] << 8) | _buffer[_bufferPosition++];
        }

        public long ReadInt64() => (long)ReadUInt64();

        [System.Security.SecuritySafeCritical]
        public unsafe float ReadSingle()
        {
            RequestBufferAccess();

            if (_bufferSize < 4)
                throw EndOfStreamException;

            fixed (byte* p = _buffer)
            {
                var value = *(float*)(p + _bufferPosition);
                _bufferPosition += 4;
                return value;
            }
        }

        [System.Security.SecuritySafeCritical]
        public unsafe double ReadDouble()
        {
            RequestBufferAccess();

            if (_bufferSize < 8)
                throw EndOfStreamException;

            fixed (byte* p = _buffer)
            {
                var value = *(double*)(p + _bufferPosition);
                _bufferPosition += 8;
                return value;
            }
        }

        private void RequestBufferAccess()
        {
            if (_willInvalidatePosition)
            {
                _willInvalidatePosition = false;
                ResetBlock(_newBasePosition);
            }
            else if (_bufferPosition >= _bufferSize)
                ReadNextBlock();
        }

        private bool IsBuffered(int requiredBytes) => _bufferPosition + requiredBytes > _bufferSize;

        private void ReadNextBlock() => ResetBlock(_basePosition + BufferSize);

        private void ResetBlock(long basePosition)
        {
            _basePosition = basePosition;
            _bufferPosition = 0;
            FillBlock();
        }

        private void FillBlock()
        {
            _bufferSize = (int)Math.Max(0, Math.Min(_buffer.Length, _stream.Length - _newBasePosition));
            if (_bufferSize > 0)
                _stream.Read(_buffer, 0, _bufferSize);
            else
                throw EndOfStreamException;
        }

        private void Copy(byte[] dst, int dstIndex, byte[] src, int srcIndex, int count)
        {
            if (count > 8)
            {
                // Unroll loop to go faaaaast
                do
                {
                    dst[dstIndex + 0] = src[srcIndex + 0];
                    dst[dstIndex + 1] = src[srcIndex + 1];
                    dst[dstIndex + 2] = src[srcIndex + 2];
                    dst[dstIndex + 3] = src[srcIndex + 3];
                    dst[dstIndex + 4] = src[srcIndex + 4];
                    dst[dstIndex + 5] = src[srcIndex + 5];
                    dst[dstIndex + 6] = src[srcIndex + 6];
                    dst[dstIndex + 7] = src[srcIndex + 7];

                    dstIndex += 8;
                    srcIndex += 8;
                    count -= 8;
                } while (count >= 8);
            }

            while (count-- > 0)
                dst[dstIndex++] = src[srcIndex++];
        }
    }
}
