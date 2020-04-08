using System;
using System.IO;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Exposes a <see cref="Stream"/> around a file, supporting synchronous read operations.
    /// </summary>
    internal class FastFileStream : FileStream
    {
        private readonly long _length;
        private long _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastFileStream"/> class with the specified path.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="FastFileStream"/> object will encapsulate.</param>
        public FastFileStream(string path)
            : base(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        {
            _length = base.Length;
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length => _length;

        /// <summary>
        /// Gets or sets the current position of the stream.
        /// </summary>
        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to <paramref name="origin"/> from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for origin, using a value of type <see cref="SeekOrigin"/>.</param>
        /// <returns>The new position in the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    base.Seek(offset, SeekOrigin.Begin);
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    base.Seek(_position, SeekOrigin.Begin);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return _position;
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in <paramref name="array"/> at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        public override int Read(byte[] array, int offset, int count)
        {
            var bytesRead = base.Read(array, offset, count);
            _position += bytesRead;
            return bytesRead;
        }

        /// <summary>
        /// Reads a byte from the file and advances the read position one byte.
        /// </summary>
        /// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been reached.</returns>
        public override int ReadByte()
        {
            _position++;
            return base.ReadByte();
        }
    }
}
