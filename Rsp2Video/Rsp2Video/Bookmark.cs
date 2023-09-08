using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    public class Bookmark
    {
        /// <summary>
        /// The name of the bookmark. Commonly "F1" for the first forward bookmark, and "R1" for the first reverse bookmark.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The text of the bookmark, which is the transcription of the speech in forward or reverse.
        /// </summary>
        public String Text { get; set; }

        /// <summary>
        /// The explanatory text for this forward or reverse speech.
        /// </summary>
        public String Explanation { get; set; }

        /// <summary>
        /// The start location of the bookmark in samples.
        /// </summary>
        public int SampleStart { get; set; }

        /// <summary>
        /// The end location of the bookmark in samples.
        /// </summary>
        public int SampleEnd { get; set; }

        /// <summary>
        /// Stored as true if the bookmark is selected for use in the video.
        /// </summary>
        public Boolean Selected { get; set; }

        /// <summary>
        /// The list of bookmarks that reference this bookmark. For example, "F1" references "R1", and "R1" references "F1".
        /// </summary>
        public List<Bookmark> ReferencedBookmarks { get; set; }

        /// <summary>
        /// Constructor. Creates a Bookmark object, initializing ReferencedBookmarks to an empty list. 
        /// </summary>
        public Bookmark()
        {
            this.ReferencedBookmarks = new List<Bookmark>();
        }
    }
}
