using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    /// <summary>
    /// The output filename for a video and the list of clips to create that video.
    /// </summary>
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

    /// <summary>
    /// An entry in the list of clips to create an output video.
    /// </summary>
    public class VideoOutputClipEntry
    {
        /// <summary>
        /// The name of the clip.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The start time for the clip.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Constructor. Creates an object with the specified values.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startTime"></param>
        public VideoOutputClipEntry(String name, double startTime)
        {
            this.Name = name;
            this.StartTime = startTime;
        }
    }
}
