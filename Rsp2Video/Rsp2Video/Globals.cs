#define LOGSTRING

using Rsp2Video;
using System;
using System.Collections.Concurrent;
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
        int SampleRate;                                                 // The sample rate of the sound file.
        double FramesPerSecond;                                         // The frames per second of the source and output video.
        int HorizontalResolution;                                       // The horizontal resolution of the video.
        int VerticalResolution;                                         // The vertical resolution of the video.
        double SourceVideoDuration;                                     // The duration of the source video in seconds.
        String SampleAspectRatio;                                       // The Sample Aspect Ratio.
        String DisplayAspectRatio;                                      // The Display Aspect Ratio.
        double VideoOffset;                                             // The video offset to align the video with the audio.
        double ClosingFrameTime;
        // String Transcript;                                           // The text of the transcript file.
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
        DirectoryInfo diPngDirectory;                                   // Stores information for the TEMP_DIR directory.
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
        Font FontForward;                                               // The font to use for forward text.
        Font FontReverse;                                               // The font to use for reverse text.
        Font FontForwardUnderline;                                      // The font to use for forward text that is currently playing in reverse.

        int CalculatedFontHeight;

        List<FFmpegTask> FFmpegTasks = new List<FFmpegTask>();          // The list of ffmpeg tasks to run to create the video clips.
        Dictionary<String,double> ClipsToCreate = new Dictionary<String,double>();  // The list of video clips to create.
        Dictionary<String,String> ClipsToDuplicate = new Dictionary<String,String>();   // The list of video clips that need to be duplicated.
        List<VideoOutput> VideoOutputs = new List<VideoOutput>();       // The list of videos to output, and the clips needed to assemble them.
        int VideoOutputIndex = 0;                                       // The index to VideoOutputs specifying which video clip list is being accessed.
        ConcurrentDictionary<String, Double> ClipDuration = new ConcurrentDictionary<String, Double>(); // Durations of each video clip.
        List<String> OutputOptionsInterimSettings;
        List<String> OutputOptionsImageSequenceSettings;
        List<String> OutputOptionsFinalSettings;
        List<String> OutputOptionsVideoInterimExtension;
        List<String> OutputOptionsVideoFinalExtension;
        List<String> OutputOptionsAudioInterimExtension;

        String OutputInterimSettings = String.Empty;
        String OutputImageSequenceSettings = String.Empty;
        String OutputFinalSettings = String.Empty;
        String OutputVideoInterimExtension = String.Empty;
        String OutputVideoFinalExtension = String.Empty;
        String OutputAudioInterimExtension = String.Empty;

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

        TimeSpan timeToRun;

        String[] VideoQualityString = { "Fast / Draft quality", "Slow / YouTube upload quality", "Slowest / High quality" };
    }
}