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
        /// The sort order for the FFmpegTask.
        /// </summary>
        public FfmpegTaskSortOrder SortOrder { get; set; }

        /// <summary>
        /// The estimated duration of the video in seconds.
        /// </summary>
        public double EstimatedDuration { get; set; }

        /// <summary>
        /// The filenames of the videos to create, without extension.
        /// </summary>
        public List<String> VideoFilenames { get; set; }

        /// <summary>
        /// The list of ffmpeg commands to execute to create the output video.
        /// </summary>
        public List<String> FFmpegCommands { get; set; }

        /// <summary>
        /// The C# method where this task was created.
        /// </summary>
        public String CreatorMethod { get; set; }

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
        /// <param name="sortOrder></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilenames"></param>
        /// <param name="ffmpegCommand"></param>
        public FFmpegTask(FfmpegTaskSortOrder sortOrder, 
            double estimatedDuration, 
            String videoFilename, 
            String ffmpegCommand, 
            String creatorMethod)
        {
            this.SortOrder = sortOrder;
            this.EstimatedDuration = estimatedDuration;
            this.VideoFilenames = new List<String>()
            {
                videoFilename
            };
            this.FFmpegCommands = new List<String>()
            {
                ffmpegCommand
            };
            this.CreatorMethod = creatorMethod;
        }

        /// <summary>
        /// Constructor for a multi-command ffmpeg process. Used for reverse bookmark videos.
        /// 
        /// The first command in the List ffmpegCommands creates the reverse video by using a single filter_complex.
        /// The next two commands in the List ffmpegCommands extracts frames as .PNG files, then assembles the reversed .PNG
        /// file sequence and combines it with the reversed audio.
        /// </summary>
        /// <param name="sortOrder></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilenames"></param>
        /// <param name="ffmpegCommands"></param>
        public FFmpegTask(FfmpegTaskSortOrder sortOrder, 
            double estimatedDuration, 
            List<String> videoFilenames, 
            List<String> ffmpegCommands, 
            String creatorMethod)
        {
            this.SortOrder = sortOrder;
            this.EstimatedDuration = estimatedDuration;
            this.VideoFilenames = videoFilenames;
            this.FFmpegCommands = ffmpegCommands;
            this.CreatorMethod = creatorMethod;
        }
    }

    public enum FfmpegPhase { None = 0, PhaseOne, PhaseTwo, PhaseThree }
    public enum FfmpegTaskSortOrder 
    { 
        None = 0, 
        ReverseVideoPass1Minterpolate, 
        ReverseVideoPass1NonMinterpolate, 
        ReverseVideoPass2, 
        ForwardBookmarkVideo, 
        ForwardVideo, 
        CardVideo, 
        TransitionVideo 
    }
}
