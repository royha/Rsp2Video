#define LOGSTRING

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace RSPro2Video
{
    public partial class RSPro2VideoForm : Form
    {
        ProgramSettings ProgramSettings;                                // The settings object that stores the settings for the program.
        ProjectSettings ProjectSettings;                                // The settings object that stores the settings for the video project.
        String ProjectFile;                                             // The .RSPro2Video file for the current project.
        String BookmarkFile;                                            // The .FmBok/.bok/.RSVideo file for the current project.
        int SampleRate;                                                 // The sample rate of the sound file.
        double FramesPerSecond;                                         // The frames per second of the source and output video.
        int HorizontalResolution;                                       // The horizontal resolution of the video.
        int VerticalResolution;                                         // The vertical resolution of the video.
        double SourceVideoDuration;                                     // The duration of the source video in seconds.
        String SampleAspectRatio;                                       // The Sample Aspect Ratio.
        String DisplayAspectRatio;                                      // The Display Aspect Ratio.
        double VideoOffset;                                             // The video offset to align the video with the audio.
        double ClosingFrameTime;
        String Transcript;                                              // The text of the transcript file.
        List<Bookmark> ForwardBookmarks = new List<Bookmark>();         // The list of bookmarks for forward speech.
        List<Bookmark> ReverseBookmarks = new List<Bookmark>();         // The list of bookmarks for reverse speech
        List<Bookmark> BokBookmarks = new List<Bookmark>();             // The list of bookmarks from the .FmBok/.bok file.
        List<Bookmark> BokForwardBookmarks = new List<Bookmark>();      // The list of forward bookmarks from the .FmBok/.bok file.
        List<Bookmark> BokReverseBookmarks = new List<Bookmark>();      // The list of reverse bookmarks with 60% overlap with at least one forward bookmark from the .FmBok/.bok file.
        List<Bookmark> BokOrphanReverseBookmarks = new List<Bookmark>();// The list of reverse bookmarks without 60% overlap with at least one forward bookmark from the .FmBok/.bok file.
        List<Bookmark> BokQuickCheckBookmarks = new List<Bookmark>();   // The list of quick check bookmarks from the .FmBok/.bok file.
        List<Bookmark> TxtForwardBookmarks = new List<Bookmark>();      // The list of forward bookmarks from the .txt/.rtf file.
        List<Bookmark> TxtReverseBookmarks = new List<Bookmark>();      // The list of reverse bookmarks from the .txt/.rtf file.
        ReversalDefinition RSVideoReversalDefinition;                   // The deserialized RSVideo file.
        List<Bookmark> RsvForwardBookmarks = new List<Bookmark>();      // The list of forward bookmarks from the .RSVideo file.
        List<Bookmark> RsvReverseBookmarks = new List<Bookmark>();      // The list of reverse bookmarks from the .RSVideo file.
        double MinBookmarkOverlap = 0.6d;                               // The minimum overlap for a reverse bookmark to be autoconnected to a forward bookmark (60% overlap).
        String StoredCurrentDirectory;                                  // The current directory before this program starts changing current directories.
        String WorkingDirectory;                                        // The directory of the _tmp directory under the output video file (settings.OutputFile).
        String FramesDirectory;                                         // The directory of the _frames directory under the WorkingDirectory.
        String WorkingInputVideoFile;                                   // The working input file. Typically "v.mp4" in the working directory.
        DirectoryInfo diTmpDirectory;                                   // Stores information for the TEMP_DIR directory.
        DirectoryInfo fiTmpDirectory;                                   // Stores information for the FRAMES_DIR directory.
        IProgress<string> Progress;                                     // Allows the long-running process to update the UI.

        String SoxApp = @"C:\Program Files (x86)\sox-14-4-2\sox.exe";
        String FfmpegApp = @"C:\Program Files\ffmpeg\bin\ffmpeg.exe";
        String FfmprobeApp = @"C:\Program Files\ffmpeg\bin\ffprobe.exe";
        String QmeltApp = @"C:\Program Files\Shotcut\qmelt.exe";

        String TEMP_DIR = "_tmp";                                       // The temp working directory to store intermediate files.
        String FRAMES_DIR = "_frames";                                  // The temp directory to store .png frames.

        // The fully-qualified path to the settings file.
        String ProgramSettingsFile = Path.Combine(Application.StartupPath, "RSPro2Video.settings");
        String AnimationFile = Path.Combine(Application.StartupPath, "animation.gif");
        String LogFile;

        // Text settings.
        String FontName;                                                // The base font, such as "Calibri".
        int BackgroundAlpha = 210;                                      // The alpha value for the text background box.
        Font FontForward;                                               // The font to use for forward text.
        Font FontReverse;                                               // The font to use for reverse text.
        Font FontForwardUnderline;                                      // The font to use for forward text that is currently playing in reverse.

        enum TextTool { None, DynamicText, Watermark }                  // The text tool to use for overlay text.
        TextTool textTool = TextTool.Watermark;
        int FontHeight;
        int LinesOnScreen = 20;                                         // The number of text lines that can appear on sceen (smaller numbers == larger text).
        int TextPadSize;
        int LeftMarginSpaces;
        String TextBackgroundColor = "#9f000000";
        String TextForegroundColor = "#ffffffff";
        double ReadingCharactersPerSecond = 19;                         // How long to display explanation cards in characters per second.

        List<String> CreatedClipList = new List<String>();              // The list of video clips that have already been created.
        List<VideoOutput> VideoOutputs = new List<VideoOutput>();       // The list of videos to output, and the clips needed to assemble them.
        int VideoOutputIndex = 0;                                       // The index to VideoOutputs specifying which video clip list is being accessed.
        List<String> OutputOptionsInterimSettings;
        List<String> OutputOptionsImageSequenceSettings;
        List<String> OutputOptionsFinalSettings;
        List<String> OutputOptionsVideoInterimExtension;
        List<String> OutputOptionsVideoFinalExtension;
        List<String> OutputOptionsAudioInterimExtension;

        String OutputInterimSettings = String.Empty;
        String OutputImageSequenceSettings = String.Empty;
        String OutputFinalSettings = String.Empty;
        String OutputFinalSettingsQmelt = String.Empty;
        String OutputVideoInterimExtension = String.Empty;
        String OutputVideoFinalExtension = String.Empty;
        String OutputAudioInterimExtension = String.Empty;

        TimeSpan timeToRun;

        // MELT related globals.
        StringBuilder MeltStringClip;
        List<StringBuilder> MeltStrings;
        int MeltStringsIndex;
        int MaxMeltString = 28500;
        int CurrentFrame;
        String RelativePathToWorkingInputVideoFile;
        String TransitionFromFrame = String.Empty;
        String TransitionToFrame = String.Empty;

        enum CardTextAlignment { None, Center, Left }
        String BokOpeningCard = String.Empty;
        String BokClosingCard = String.Empty;
        String TxtOpeningCard = String.Empty;
        String TxtClosingCard = String.Empty;
        String OpeningCard = String.Empty;
        String ClosingCard = String.Empty;
        CardTextAlignment OpeningCardAlignment = CardTextAlignment.Left;
        CardTextAlignment ClosingCardAlignment = CardTextAlignment.Center;

        bool AddMeltLineBreaks = true;

        bool DeleteWorkingDirectories = true;

        String[] VideoQualityString = { "Fast / Draft quality", "Slow / YouTube upload quality", "Slowest / High quality" };

        // Debug related globals.
#if (LOGSTRING)
        StringBuilder LogString;
#endif
    }
}