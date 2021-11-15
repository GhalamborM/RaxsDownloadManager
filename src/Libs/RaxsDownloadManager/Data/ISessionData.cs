using System;
using System.Collections.Generic;
using System.Text;

namespace RaxsDownloadManager.Data
{
    public interface ISessionData
    {
        /// <summary>
        ///     Save database
        /// </summary>
        void Save();
        /// <summary>
        ///     Load database
        /// </summary>
        void Load();
    }
}
