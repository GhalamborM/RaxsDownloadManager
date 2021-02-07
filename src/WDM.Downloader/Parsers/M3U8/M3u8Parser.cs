using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WDM.Downloaders.Exceptions;
using System.Linq;

namespace WDM.Downloaders.Parsers.M3U8
{
    public class M3u8Parser
    {
        const string M3U8_TAG = "#EXTM3U";

        const string PATTERN = @"#EXTINF:(?<duration>.*),\r\n(?<link>((https|http|www.)?\S+))";

        static public M3u8MediaContainer Parse(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException("M3u8Parser.Parse(content)");
            if (!content.Contains(M3U8_TAG))
                throw new ParserException("'content' is not `m3u/m3u8` file.");
            var mediaList = new List<M3u8Media>();
            foreach (Match m in Regex.Matches(content, PATTERN))
            {
                //Debug.WriteLine("Count: {2} name: {0} URL: {1}", m.Groups["duration"].Value, m.Groups["link"].Value, m.Groups.Count);
                var link = m.Groups["link"]?.Value;
                var duration = m.Groups["duration"]?.Value;
                if (!string.IsNullOrEmpty(link) && IsUrl(link) && double.TryParse(duration, out double durationAsDouble))
                    mediaList.Add(new M3u8Media { Duration = durationAsDouble, Url = link });
            }
            var durations = mediaList.Select(m => m.Duration).ToArray();
            var container = new M3u8MediaContainer
            {
                Medias = mediaList.AsReadOnly(),
                Duration = TimeSpan.FromSeconds(durations.Sum())
            };
            return container;
        }
        static bool IsUrl(string content) => content.Trim().StartsWith("http:") || content.Trim().StartsWith("https:");
    }
}
