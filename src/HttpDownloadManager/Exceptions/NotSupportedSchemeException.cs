using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDownloadManager.Exceptions
{
    public class NotSupportedSchemeException : Exception
    {
        public NotSupportedSchemeException(string message) : base(message) { }
    }
}
