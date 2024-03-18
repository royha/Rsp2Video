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
        public List<ClipEntry> Clips { get; set; }

        /// <summary>
        /// Constructor. Initializes the list of clips to an empty list.
        /// </summary>
        public VideoOutput()
        {
            this.Clips = new List<ClipEntry>();
        }

        /// <summary>
        /// Constructor. Initializes the list of clips to an empty list, and initializes the video output filename.
        /// </summary>
        /// <param name="filename"></param>
        public VideoOutput(string filename)
        {
            this.Filename = filename;
            this.Clips = new List<ClipEntry>();
        }
    }

    /// <summary>
    /// An entry in the list of clips to create an output video.
    /// </summary>
    public class ClipEntry
    {
        /// <summary>
        /// The name of the clip.
        /// </summary>
        public String ClipFilename { get; set; }

        /// <summary>
        /// The start time for the clip.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Constructor. Creates a clip entry with the specific filename and a start time of zero.
        /// </summary>
        /// <param name="name">The filename of the clip.</param>
        public ClipEntry(String name)
        {
            this.ClipFilename = name;
            this.StartTime = 0.0d;
        }

        /// <summary>
        /// Constructor. Creates an object with the specified values.
        /// </summary>
        /// <param name="name">The filename of the clip.</param>
        /// <param name="startTime">The start time of the clip.</param>
        public ClipEntry(String name, double startTime)
        {
            this.ClipFilename = name;
            this.StartTime = startTime;
        }
    }
}
