using System;
using System.Collections.Generic;
using System.Text;

namespace WDM.Downloaders.Parsers.M3U8
{
    public class M3u8MediaContainer
    {
        public IReadOnlyList<M3u8Media> Medias { get; internal set; } 

        public TimeSpan Duration { get; internal set; }
    }
    public class M3u8Media
    {
        public double Duration { get; set; }

        public string Path { get; set; }
    }
}
