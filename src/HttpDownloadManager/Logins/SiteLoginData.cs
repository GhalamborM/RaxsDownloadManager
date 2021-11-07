using System;
using System.Collections.Generic;
using System.Text;

namespace HttpDownloadManager.Logins
{
    /// <summary>
    ///     Site login data
    /// </summary>
    [Serializable]
    public class SiteLoginData
    {
        /// <summary>
        ///     Site or path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        ///     Username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }
    }
}
