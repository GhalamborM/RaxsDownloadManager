using System.IO;

namespace WDM.Downloaders.Data
{
    public class DefaultCachedFolder : ICachedFolder
    {
        public string CachedFolderPath => Path.Combine(Path.GetTempPath(), Constants.AppName);

        public bool CheckExists() => Directory.Exists(CachedFolderPath);

        public void CreateIfNotExists()
        {
            if (!CheckExists())
                Directory.CreateDirectory(CachedFolderPath);
        }
    }
}
