using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.ComponentModel;

namespace RaxsDownloadManager
{
    public struct DownloadInfo
    {
        public string ServerSideFileName { get; internal set; }
        public string ContentType { get; internal set; }
        public long? ContentLength { get; internal set; }
        public bool PartialContent { get; internal set; }
        public bool CanDetectPartialContent { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(this);
                sb.AppendLine(string.Format("{0}={1}", name, value));
            }
            return sb.ToString();
        }
    }
}
