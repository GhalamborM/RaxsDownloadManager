using System;
using System.Collections.Generic;
using System.Text;

namespace XtopDownloadManager.Exceptions
{
    public class NotSupportedSchemeException : Exception
    {
        public NotSupportedSchemeException(string message) : base(message) { }
    }
}
