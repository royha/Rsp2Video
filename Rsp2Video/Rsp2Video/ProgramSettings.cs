using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    public class ProgramSettings
    {
        /// <summary>
        /// Stores the file path on panel1.
        /// </summary>
        public string LastUsedFile { get; set; }
    }

    public enum BookmarkFileType { None, FmBok, bok, RSVideo, Text, RTF }
}
