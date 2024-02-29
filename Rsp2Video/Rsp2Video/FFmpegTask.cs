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
        /// 1 = Reverse, Forward bookmark, Forward video creation.
        /// 2 = Explanation and Transition video creation.
        /// </summary>
        public FfmpegPhase Phase { get; set; }

        /// <summary>
        /// The sort order within a phase.
        /// PhaseTwo:
        ///   1 = Reverse videos.
        ///   2 = Forward bookmark videos.
        ///   3 = Forward videos.
        /// PhaseThree:
        ///   4 = Card videos.
        ///   5 = Transition videos.
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
        /// <param name="sortOrder></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilenames"></param>
        /// <param name="ffmpegCommand"></param>
        public FFmpegTask(FfmpegPhase phase, FfmpegTaskSortOrder sortOrder, double estimatedDuration, string videoFilename, String ffmpegCommand)
        {
            this.Phase = phase;
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
        }

        /// <summary>
        /// Constructor for a multi-command ffmpeg process. Used for reverse bookmark videos.
        /// 
        /// The first command in the List ffmpegCommands creates the reverse video by using a single filter_complex.
        /// The next two commands in the List ffmpegCommands extracts frames as .PNG files, then assembles the reversed .PNG
        /// file sequence and combines it with the reversed audio.
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="sortOrder></param>
        /// <param name="estimatedDuration"></param>
        /// <param name="videoFilenames"></param>
        /// <param name="ffmpegCommands"></param>
        public FFmpegTask(FfmpegPhase phase, FfmpegTaskSortOrder sortOrder, double estimatedDuration, List<String> videoFilenames, List<String> ffmpegCommands)
        {
            this.Phase = phase;
            this.SortOrder = sortOrder;
            this.EstimatedDuration = estimatedDuration;
            this.VideoFilenames = videoFilenames;
            this.FFmpegCommands = ffmpegCommands;
        }
    }

    public enum FfmpegPhase { None = 0, PhaseOne, PhaseTwo, PhaseThree }
    public enum FfmpegTaskSortOrder { None = 0, ReverseMinterpolateVideo, ReverseVideo, ForwardBookmarkVideo, ForwardVideo, CardVideo, TransitionVideo }
}
