using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace HttpDownloadManager.Streams
{
    public class ConcatenatedStream : Stream
    {
        Queue<Stream> _streams;
        public ConcatenatedStream(params Stream[] streams) =>
            _streams = new Queue<Stream>(streams);
        ~ConcatenatedStream()
        {
            _streams = null;
            Dispose(true);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (count > 0 && _streams.Count > 0)
            {
                int bytesRead = _streams.Peek().Read(buffer, offset, count);
                if (bytesRead == 0)
                {
                    _streams.Dequeue().Dispose();
                    continue;
                }

                totalBytesRead += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }

            return totalBytesRead;
        }
        public new async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (count > 0 && _streams.Count > 0)
            {
                int bytesRead = await _streams.Peek().ReadAsync(buffer, offset, count);
                if (bytesRead == 0)
                {
                    _streams.Dequeue().Dispose();
                    continue;
                }

                totalBytesRead += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }

            return totalBytesRead;
        }
        public new async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalBytesRead = 0;

            while (count > 0 && _streams.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await _streams.Peek().ReadAsync(buffer, offset, count);
                if (bytesRead == 0)
                {
                    _streams.Dequeue().Dispose();
                    continue;
                }

                totalBytesRead += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }

            return totalBytesRead;
        }
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
