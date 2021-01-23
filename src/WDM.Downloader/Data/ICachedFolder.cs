namespace WDM.Downloaders.Data
{
    public interface ICachedFolder
    {
        /// <summary>
        ///     Cached
        /// </summary>
        string CachedFolderPath { get; }
        /// <summary>
        ///     Checks folder exists
        /// </summary>
        bool CheckExists();
        /// <summary>
        ///     Create cached folder if not exists
        /// </summary>
        void CreateIfNotExists();
    }
}
