namespace RD.Core.Models;

public class FileCategory
{
    public string Name { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public List<string> Extensions { get; set; } = [];
    public bool IsEnabled { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public static class FileCategoryDefaults
{
    public const string General = "General";
    public const string Compressed = "Compressed";
    public const string Document = "Document";
    public const string Video = "Video";
    public const string Subtitle = "Subtitle";
    public const string Music = "Music";
    public const string Program = "Program";

    public static List<FileCategory> GetDefaultCategories()
    {
        return
        [
            new FileCategory
            {
                Name = General,
                FolderName = string.Empty, 
                Extensions = [],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Compressed,
                FolderName = "Compressed",
                Extensions = ["zip", "rar", "gz", "bz2", "7z", "tar", "xz", "iso"],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Document,
                FolderName = "Documents",
                Extensions = ["doc", "pdf", "ppt", "pps", "docx", "pptx", "xlsx", "xls", "txt", "rtf", "odt", "ods", "odp"],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Video,
                FolderName = "Videos",
                Extensions = ["avi", "mpg", "mpe", "mpeg", "asf", "wmv", "mov", "qt", "rm", "mp4", "flv", "m4v", "webm", "ogv", "ogg", "mkv", "ts", "tsv", "3gp"],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Subtitle,
                FolderName = "Subtitles",
                Extensions = ["srt", "ass", "ssa", "vtt", "vobsub", "sub", "idx"],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Music,
                FolderName = "Music",
                Extensions = ["mp3", "wav", "wma", "mpa", "ram", "ra", "aac", "aif", "m4a", "tsa", "flac", "ogg", "oga", "opus"],
                IsEnabled = true,
                IsDefault = true
            },
            new FileCategory
            {
                Name = Program,
                FolderName = "Programs",
                Extensions = ["exe", "msi", "misx", "appx", "appxbundle", "dmg", "pkg", "deb", "rpm"],
                IsEnabled = true,
                IsDefault = true
            }
        ];
    }

    public static HashSet<string> GetDefaultCategoryNames()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            General, Compressed, Document, Video, Subtitle, Music, Program
        };
    }
}
