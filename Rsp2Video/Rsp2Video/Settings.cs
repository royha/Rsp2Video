using System;
using System.Collections.Generic;

namespace RSPro2Video
{
    // [Serializable]
    public class Settings
    {
        /// <summary>
        /// Original video file, typically downloaded from YouTube.
        /// </summary>
        public String SourceVideoFile { get; set; }

        /// <summary>
        /// Sound file processed in Reverse Speech Pro.
        /// </summary>
        public String RspSoundFile { get; set; }

        /// <summary>
        /// Transcript file output by Reverse Speech Pro.
        /// </summary>
        public String RspTranscriptFile { get; set; }

        /// <summary>
        /// The bookmark file.
        /// </summary>
        public String BookmarkFile { get; set; }

        /// <summary>
        /// Identifies the bookmark file type, such as FmBok or RSVideo.
        /// </summary>
        public BookmarkFileType BookmarkFileType { get; set; }

        /// <summary>
        /// Name of the output file. Either a video file, or a video project file as indicated by OutputType.
        /// </summary>
        public String OutputVideoFile { get; set; }

        /// <summary>
        /// The video offset amount, in milliseconds.
        /// </summary>
        public int AudioDelay { get; set; }

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
        /// When true, the source to use for bookmark text is the .FmBok/.bok file.
        /// </summary>
        public String SourceBookmarkFile { get; set; }

        /// <summary>
        /// When true, the source to use for bookmark text is the transcript file (.txt/.bok).
        /// </summary>
        public Boolean SourceTranscriptFile { get; set; }


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
        /// If true, a clip will be added to show the reversal at 100% forward speed, followed immediately
        /// by the reversal at 100% reverse.
        /// </summary>
        public Boolean IncludeBackAndForth { get; set; }

        /// <summary>
        /// If true, the forward video is replayed after all reversals have been played. This reminds the
        /// listener of their context in the overall video.
        /// </summary>
        public Boolean ReplayForwardVideo { get; set; }

        // The strings to process the video for the selected quality setting.
        public List<String> OutputOptionsInterimSettings { get; set; }
        public List<String> OutputOptionsInterimSettingsQmelt { get; set; }
        public List<String> OutputOptionsImageSequenceSettings { get; set; }
        public List<String> OutputOptionsFinalSettings { get; set; }
        public List<String> OutputOptionsFinalSettingsQmelt { get; set; }
        public List<String> OutputOptionsVideoInterimExtension { get; set; }
        public List<String> OutputOptionsVideoFinalExtension { get; set; }
        public List<String> OutputOptionsAudioInterimExtension { get; set; }

        /// <summary>
        /// Constructor creates needed field values.
        /// </summary>
        public Settings()
        {
            this.ReversalRate1 = new ReversalRate();
            this.ReversalRate2 = new ReversalRate();
            this.ReversalRate3 = new ReversalRate();
            this.ReversalRate4 = new ReversalRate();

            OutputOptionsInterimSettings = new List<string>();
            OutputOptionsInterimSettingsQmelt = new List<string>();
            OutputOptionsImageSequenceSettings = new List<string>();
            OutputOptionsFinalSettings = new List<string>();
            OutputOptionsFinalSettingsQmelt = new List<string>();
            OutputOptionsVideoInterimExtension = new List<string>();
            OutputOptionsVideoFinalExtension = new List<string>();
            OutputOptionsAudioInterimExtension = new List<string>();
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

    public enum BookmarkFileType { None, FmBok, bok, RSVideo, Text, RTF }
    public enum VideoContents { None, FullVideo, BookmarksOnly, SeparateVideos }
    public enum OutputType { None, VideoFile, VideoProject }
    public enum VideoQuality { Fast, Small, High }
}
