namespace RaxsDownloadManager.Streams
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Threading;
    /// <summary>
    ///     Attach/merge multiple streams via <see cref="ConcatenatedStream"/>
    /// </summary>
    /// <remarks>
    ///     CanRead is only implemented!
    /// </remarks>
    public class ConcatenatedStream : Stream
    {
        #region Private members

        // We can use Stack as well, but let we use Queue!
        Queue<Stream> _streams;

        #endregion

        #region Properties

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        #endregion

        #region Constructor/Desctructor
        public ConcatenatedStream(params Stream[] streams) =>
            _streams = new Queue<Stream>(streams);

        ~ConcatenatedStream()
        {
            _streams = null;
            Dispose(true);
        }

        #endregion

        #region Public methods

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

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
        
        #endregion

        #region Not implemented functions

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

        #endregion
    }
}
