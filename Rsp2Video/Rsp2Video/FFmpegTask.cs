using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPro2Video
{
    /// <summary>
    /// Contains the data necessary to create a video by calling ffmpeg.
    /// </summary>
    public class FFmpegTask
    {
        /// <summary>
        /// The phase of the multiprocessing process this task is in.
        /// 1 = Reverse video creation.
        /// 2 = Forward bookmark video and forward video creation.
        /// 3 = Card videos and transition videos.
        /// </summary>
        public int Phase { get; set; }

        /// <summary>
        /// The estimated duration of the video in seconds.
        /// </summary>
        public double EstimatedDuration { get; set; }

        /// <summary>
        /// The filename of the video to create, without extension.
        /// </summary>
        public String VideoFilename { get; set; }

        /// <summary>
        /// The list of ffmpeg commands to execute to create the output video.
        /// </summary>
        public List<String> FFmpegCommands { get; set; }

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public FFmpegTask()
        {
            this.FFmpegCommands = new List<String>();
        }

        /// <summary>
        /// Constructor for a single ffmpeg command. Used for forward bookmark videos, forward videos, transition videos and 
        /// card videos.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilename"></param>
        /// <param name="ffmpegCommand"></param>
        public FFmpegTask(int phase, double estimatedDuration, string videoFilename, String ffmpegCommand)
        {
            this.Phase = phase;
            this.EstimatedDuration = estimatedDuration;
            this.VideoFilename = videoFilename;
            this.FFmpegCommands = new List<String>()
            {
                ffmpegCommand
            };
        }

        /// <summary>
        /// Constructor for a multi-command ffmpeg process. Used for reverse bookmark videos.
        /// 
        /// The first command in the List ffmpegCommands creates the reverse video by using a single filter_complex.
        /// The next two commands in the List ffmpegCommands extracts frames as .PNG files, then assembles the reversed .PNG
        /// file sequence and combines it with the reversed audio.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilename"></param>
        /// <param name="ffmpegCommands"></param>
        public FFmpegTask(int phase, double estimatedDuration, string videoFilename, List<string> ffmpegCommands)
        {
            this.Phase = phase;
            this.EstimatedDuration = estimatedDuration;
            this.VideoFilename = videoFilename;
            this.FFmpegCommands = ffmpegCommands;
        }
    }
}
