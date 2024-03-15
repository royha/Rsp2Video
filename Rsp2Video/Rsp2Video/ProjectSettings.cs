using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    // [Serializable]
    public class ProjectSettings
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

        /// <summary>
        /// Original video file, typically downloaded from YouTube.
        /// </summary>
        public String SourceVideoFile { get; set; }

        /// <summary>
        /// Transcript file output by Reverse Speech Pro.
        /// </summary>
        // public String RspTranscriptFile { get; set; }

        /// <summary>
        /// ClipFilename of the output file. Either a video file, or a video project file as indicated by OutputType.
        /// </summary>
        public String OutputVideoFile { get; set; }

        /// <summary>
        /// The video offset amount, in frames.
        /// </summary>
        public Double VideoDelay { get; set; }

        /// <summary>
        /// The audio delay to sync the audio with the video.
        /// </summary>
        public VideoQuality VideoQuality { get; set; }


        /// <summary>
        /// When true, the bookmkark types to use in creating this video are Forward and Reverse.
        /// </summary>
        public Boolean BookmarkTypeFnR { get; set; }

        /// <summary>
        /// When true, the bookmkark types to use in creating this video are the Quick check bookmarks.
        /// </summary>
        public Boolean BookmarkTypeQuickCheck { get; set; }

        /// <summary>
        /// When true, the bookmkark types to use in creating this video are the orphaned reversals.
        /// </summary>
        public Boolean BookmarkTypeOrphanedReversals { get; set; }

        /// <summary>
        /// When true, the source to use for bookmark text is the transcript file (.txt/.bok).
        /// </summary>
        // public Boolean SourceTranscriptFile { get; set; }


        /// <summary>
        /// Indicates the contents of the video. The output video could be based on the full source video
        /// (VideoContents.FullVideo), or just the bookmark sections (VideoContents.BookmarksOnly).
        /// </summary>
        public VideoContents VideoContents { get; set; }

        /// <summary>
        /// The rates for the reversals.
        /// </summary>
        public ReversalRate ReversalRate1 { get; set; }
        public ReversalRate ReversalRate2 { get; set; }
        public ReversalRate ReversalRate3 { get; set; }
        public ReversalRate ReversalRate4 { get; set; }

        /// <summary>
        /// The foreground color for text and captions.
        /// </summary>
        public String TextForegroundColor { get; set; }

        /// <summary>
        /// The color of the background for captions.
        /// </summary>
        public String TextBackgroundColor { get; set; }

        /// <summary>
        /// The alpha value for the text background box.
        /// </summary>
        public int TextBackgroundTransparency { get; set; }

        /// <summary>
        /// The maximum number of lines of text to be visible on screen.
        /// </summary>
        public int TextLinesOnScreen { get; set; }

        /// <summary>
        /// The assumed number of characters the reader can read in one second.
        /// </summary>
        public Double ReadingCharactersPerSecond { get; set; }

        /// <summary>
        /// The time, in seconds, for transitions to and from a card.
        /// </summary>
        public Double TransitionLengthCard { get; set; }

        /// <summary>
        /// The time, in seconds, for major transitions.
        /// </summary>
        public Double TransitionLengthMajor { get; set; }

        /// <summary>
        /// The time, in seconds, for minor transitions.
        /// </summary>
        public Double TransitionLengthMinor { get; set; }

        /// <summary>
        /// True if motion interpolated frames are to be created for non-100% reversal clips.
        /// </summary>
        public MotionInterpolation MotionInterpolation { get; set; }

        /// <summary>
        /// If true, a clip will be added to show the reversal at 100% forward speed, followed immediately
        /// by the reversal at 100% reverse.
        /// </summary>
        public Boolean IncludeBackAndForth { get; set; }

        /// <summary>
        /// If true, the forward video is replayed after all reversals have been played. This reminds the
        /// listener of their context in the overall video.
        /// </summary>
        public Boolean ReplayForwardVideo { get; set; }

        /// <summary>
        /// When true, the forward bookmark clip is played in its entirety before any reversal clips play.
        /// 
        /// When false, the forward bookmark clip is played up to the end of a reversal clip, which then plays,
        /// then the forward clip resumes at the beginning of the reversal clip and plays to the end of the next
        /// reversal clip, or the end of the forward clip.
        /// </summary>
        public Boolean PlayForwardBookmarkCompletely { get; set; }
        public Boolean IncludeBookmarkNameInTextOverlays { get; set; }
        public TransitionType TransitionType { get; set; }
        public String XFadeTransitionType { get; set;}
        public Boolean IncludeOpeningCard { get; set; }
        public Boolean IncludeClosingCard { get; set; }
        public Boolean IncludeForwardExplanations { get; set; }
        public Boolean IncludeReverseExplanations { get; set; }
        public Boolean DeleteWorkingDirectoriesAtEnd { get; set; }

        /// <summary>
        /// Constructor creates needed field values.
        /// </summary>
        public ProjectSettings()
        {
            this.ReversalRate1 = new ReversalRate();
            this.ReversalRate2 = new ReversalRate();
            this.ReversalRate3 = new ReversalRate();
            this.ReversalRate4 = new ReversalRate();
        }
    }

    public class ReversalRate
    {
        /// <summary>
        /// Indicates whether this reveral rate should be output. True to use this rate object; otherwise false.
        /// </summary>
        public Boolean UseThisRate { get; set; }

        /// <summary>
        /// The reversal speed for this reversal. Speed changes rate and tone simultaneously (like slowing down a 
        /// vinyl record).
        /// </summary>
        public int ReversalSpeed { get; set; }

        /// <summary>
        /// The reversal tone for this reversal. If ReversalSpeed is 50, and ReversalTone is 70, then the reversal
        /// will change the speed and tone to 70% of normal, then stretch the sound file to the full 50% while
        /// maintaining the 70% tone.
        /// </summary>
        public int ReversalTone { get; set; }
    }

    /// <summary>
    /// The locations of the starting square bracet and the ending square bracket in a forward bookmark.
    /// </summary>
    public class BracketPair
    {
        /// <summary>
        /// The location of the opening square bracket.
        /// </summary>
        public int BracketOpen { get; set; }

        /// <summary>
        /// The location of the Closing square bracket.
        /// </summary>
        public int BracketClose { get; set; }

        /// <summary>
        /// Constructs a new object with the specified values.
        /// </summary>
        /// <param name="open">The index of the opening square bracket.</param>
        /// <param name="close">The index of the closing square bracket.</param>
        public BracketPair(int open, int close)
        {
            this.BracketOpen = open;
            this.BracketClose = close;
        }
    }

    /// <summary>
    /// Clip duration determined by reading the ffmpeg .progress file.
    /// </summary>
    public class ClipDuration
    {
        /// <summary>
        /// The final "frame=" value from the ffmpeg .progress file.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// The duration in seconds calculated from the frame count in the ffmpeg .progress file.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Constructs a new ClipDuration object with the specified values.
        /// </summary>
        /// <param name="frameCount">The frame count.</param>
        /// <param name="duration">The duration in seconds.</param>
        public ClipDuration(int frameCount, double duration)
        {
            this.FrameCount = frameCount;
            this.Duration = duration;
        }
    }

    public enum VideoContents { None, FullVideo, BookmarksOnly, SeparateVideos }
    public enum OutputType { None, VideoFile, VideoProject }
    public enum BookmarkFileType { None, FmBok, bok, RSVideo, Text, RTF }
    public enum VideoQuality { Fast, YouTube, High }
    public enum TransitionType { None, XFade, HoldLastFrame }
    public enum MotionInterpolation { None, BlendFrames, MotionGood, MotionBest }
}
