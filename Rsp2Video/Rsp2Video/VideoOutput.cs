using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    public class VideoOutput
    {
        /// <summary>
        /// The filename for this video.
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// The list of clips to assemble into the video.
        /// </summary>
        public List<String> Clips { get; set; }

        /// <summary>
        /// Constructor. Initializes the list of clips to an empty list.
        /// </summary>
        public VideoOutput()
        {
            this.Clips = new List<String>();
        }

        /// <summary>
        /// Constructor. Initializes the list of clips to an empty list, and initializes the video output filename.
        /// </summary>
        /// <param name="filename"></param>
        public VideoOutput(string filename)
        {
            this.Filename = filename;
            this.Clips = new List<String>();
        }
    }
}