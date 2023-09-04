using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    public class ProgramSettings
    {
        /// <summary>
        /// The .RSPro2Video file for the current project.
        /// </summary>
        public String ProjectFile { get; set; }

        /// <summary>
        /// The .FmBok/.bok/.RSVideo bookmark file for the current project.
        /// </summary>
        public String BookmarkFile { get; set; }

        /// <summary>
        /// Identifies the bookmark file type, such as FmBok or RSVideo.
        /// </summary>
        public BookmarkFileType BookmarkFileType { get; set; }
    }

    public enum BookmarkFileType { None, FmBok, bok, RSVideo, Text, RTF }
}
