using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDownloadManager.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
    }
}
