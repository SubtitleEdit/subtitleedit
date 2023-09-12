using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.Common
{
    public readonly struct StreamCopyContext
    {
        public StreamCopyContext(int bufferSize, long contentLength)
        {
            BufferSize = bufferSize;
            ContentLength = contentLength;
        }

        public int BufferSize { get; }
        public long ContentLength { get; }
    }
    
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, StreamCopyContext context, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (context.BufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(context.BufferSize));
            }

            var buffer = new byte[context.BufferSize];
            long readBytes = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                readBytes += bytesRead;
                // ReSharper disable once PossibleLossOfFraction
                progress?.Report(readBytes / context.ContentLength);
            }

            progress?.Report(1);
        }
    }
}