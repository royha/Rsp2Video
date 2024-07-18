using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace RSPro2VideoTool
{
    public partial class VideoSyncForm : Form
    {
        RSPro2VideoToolForm MainForm;                                   // The form that instantiated this form.
        String StoredCurrentDirectory;                                  // The current directory before this program starts changing current directories.
        String WorkingDirectory;                                        // The directory of the _tmp directory under the output video file (settings.OutputFile).
        String WorkingInputVideoFile;                                   // The working input file. Typically "v.mp4" in the working directory.
        String WorkingInputVideoFileWithoutExtension;                   // The working input file without extension. Typically "v" in the working directory.
        String RelativePathToWorkingInputVideoFile;
        String RelativePathToWorkingInputVideoFileWithoutExtension;
        DirectoryInfo diPngDirectory;                                   // Stores information for the TEMP_DIR directory.
        String TEMP_DIR = "_tmp";                                       // The temp working directory to store intermediate files.
        double VideoOffset;                                             // The video offset, in seconds, to align the video with the audio.
        Double VideoDelay;                                              // The video offset, in frames, to align the video with the audio.
        List<String> ffmpegCommands;                                    // The list of ffmpeg command strings to create the test run video.
        double ClipStartTime;                                           // The start time of the test run clip.
        double ClipEndTime;                                             // The end time of the test run clip.
        double ClipDuration;                                            // The ClipDuration of the test run clip.
        String OutputVideoInterimExtension = ".mkv";
        String OutputInterimSettings = "-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 18 -c:a aac -q:a 1 -movflags +faststart";

        public VideoSyncForm()
        {
            InitializeComponent();
        }

        public VideoSyncForm(Form callingForm)
        {
            MainForm = callingForm as RSPro2VideoToolForm;
            InitializeComponent();

            // Start with the "View test run video" disabled.
            buttonViewTestRunVideo.Enabled = false;
        }

        private void buttonMakeTestRun_Click(object sender, EventArgs e)
        {
            InitializeTestRunVideo();

            MakeForwardClipString();

            MakeReverseClipStrings();

            MakeVideosFromClipStrings();

            MakeTransitionClipStrings();

            MakeTransitionClipsFromStrings();

            AssembleTestRunVideo();

            // Now that a video is available, enable the button that plays it.
            buttonMakeFinalVideo.Enabled = true;
        }

        private void InitializeTestRunVideo()
        {
            // Initialize the list of ffmpeg command strings.
            ffmpegCommands = new List<String>();

            // Create the _tmp directory.
            CreateDirectories();

            // Copy and sync the source video into the _tmp directory as v.mp4.
            CopySourceVideoToWorkingDirectory();

            // Calculate the start and end points of the clip.
            ClipStartTime = (double)numericStartTimeMinutes.Value * 60.0d + (double)numericStartTimeSeconds.Value;
            ClipDuration = (double)numericDuration.Value;
            ClipEndTime = ClipStartTime + ClipDuration;
        }

        private void MakeForwardClipString()
        {
            AddForwardBookmarkVideo();
        }

        private void MakeReverseClipStrings()
        {
            foreach (int rate in new List<int>() { 100, 85, 70 })
            {
                CreateReverseVideoTask(rate);
            }
        }

        private void MakeVideosFromClipStrings()
        {
            throw new NotImplementedException();
        }

        private void MakeTransitionClipStrings()
        {
            throw new NotImplementedException();
        }

        private void MakeTransitionClipsFromStrings()
        {
            throw new NotImplementedException();
        }

        private void AssembleTestRunVideo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the working directories TEMP_DIR and FRAMES_DIR.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateDirectories()
        {
            // Store current working directory.
           StoredCurrentDirectory = Directory.GetCurrentDirectory();

            // Set the working directory to _tmp under the output video directory.
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(MainForm.SourceVideoFile)), TEMP_DIR);

            // Create the working directory.
            try
            {
                // Try to create the directory.
                diPngDirectory = Directory.CreateDirectory(WorkingDirectory);
            }
            catch { return false; }

            // Set the current directory to the working directory.
            Directory.SetCurrentDirectory(WorkingDirectory);

            return true;
        }

        /// <summary>
        /// Removes the TEMP_DIR directory and the log file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RemoveTemp_DirDirectory()
        {
            Directory.SetCurrentDirectory(StoredCurrentDirectory);

            // Delete the working directory.
            try
            {
                // Delete the directory and any files and directories in that directory.
                diPngDirectory.Delete(true);
            }
            catch { return false; }

            return true;
        }

        /// <summary>
        /// Copies the source video file to the working directory. 
        /// </summary>
        private Boolean CopySourceVideoToWorkingDirectory()
        {
            // Set the name of the working input file. Typically, "v.mp4".
            WorkingInputVideoFileWithoutExtension = Path.Combine(WorkingDirectory, "v");
            WorkingInputVideoFile = WorkingInputVideoFileWithoutExtension + Path.GetExtension(MainForm.SourceVideoFile);
            RelativePathToWorkingInputVideoFile = Path.GetFileName(WorkingInputVideoFile);
            RelativePathToWorkingInputVideoFileWithoutExtension = Path.GetFileNameWithoutExtension(WorkingInputVideoFile);

            // Create the Process to call the external program.
            Process process = new Process();

            String arguments = $"-y -hide_banner -i \"{MainForm.SourceVideoFile}\" "
                + $"-itsoffset {VideoDelay / MainForm.FramesPerSecond:0.#######} "
                + $"-i \"{MainForm.SourceVideoFile}\" -map 1:v -map 0:a -c copy "
                + $"-progress \"{WorkingInputVideoFileWithoutExtension}.progress\" "
                + $"\"{WorkingInputVideoFile}\"";

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = MainForm.FfmpegApp,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            // Return success or failure.
            if (!(ExitCode == 0))
            {
                MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nComment: Sync audio and video while copying the source video file to the working directory.\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n***Error: Exit code {ExitCode}\r\n\r\n{FfmpegOutput}\r\n");
                return false;
            }

            // Log the ffmpeg command line options and the ffmpeg output.
            MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nComment: Sync audio and video while copying the source video file to the working directory.\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n{FfmpegOutput}\r\n");

            // Get the working video ClipDuration.
            //ClipDuration clipDuration = GetProgressDuration(RelativePathToWorkingInputVideoFileWithoutExtension);
            //if (clipDuration.FrameCount < 0)
            //{
            //    return false;
            //}

            // Get the first and last frame of the working video.
            //if (CreateFirstAndLastFrameFromClip(RelativePathToWorkingInputVideoFileWithoutExtension,
            //    clipDuration.Duration,
            //    Path.GetExtension(RelativePathToWorkingInputVideoFile)) == false)
            //{
            //    return false;
            //}

            return true;
        }

        /// <summary>
        /// Adds the specified forward video clipEntry with the forward text overlay image added.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private void AddForwardBookmarkVideo()
        {
            String filename = "F1";
            String command;

            // Create the command for ffmpeg.
            command = $"-y -hide_banner -threads {{0}} "
                + $"-ss {ClipStartTime:0.##########} -i \"{RelativePathToWorkingInputVideoFile}\" "
                + $"-ss {ClipStartTime:0.##########} -i \"{RelativePathToWorkingInputVideoFile}\" "
                + $"-ss {ClipStartTime + ClipDuration:0.##########} -t {ClipDuration:0.##########} -i \"{RelativePathToWorkingInputVideoFile}\" "
                + $"-filter_complex \"[0:a] asetpts=PTS-STARTPTS [a]; [0:v] setpts=PTS-STARTPTS [v]\" "
                + $"-map [v] -map [a] -progress \"{filename}.progress\" -t {ClipDuration:0.##########} {OutputInterimSettings} \"{filename}{OutputVideoInterimExtension}\" "
                + $"-map 1:v -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"F1.First.png\" "
                + $"-map 2:v -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"F1.Last.png\"";

            // Add the command to the list.
            ffmpegCommands.Add(command);

            return;
        }

        private bool CreateReverseVideoTask(int reversalRate)
        {
            double reversalSpeed = Math.Round((double)reversalRate / 100.0d, 15);
            double reversalTone = Math.Round((double)reversalRate / 100.0d, 15);

            // Calculate start, end, and length.
            double sampleBasedStartSeconds = ClipStartTime;
            double sampleBasedEndSeconds = ClipEndTime;
            double sampleBasedDuration = Math.Round(ClipEndTime - ClipStartTime, 15);

            double originalFrameBasedStartSeconds = Math.Round(Math.Floor(sampleBasedStartSeconds * MainForm.FramesPerSecond) / MainForm.FramesPerSecond, 15);
            double originalFrameBasedEndSeconds = Math.Round(Math.Ceiling(sampleBasedEndSeconds * MainForm.FramesPerSecond) / MainForm.FramesPerSecond, 15);
            double originalFrameBasedDuration = Math.Round(originalFrameBasedEndSeconds - originalFrameBasedStartSeconds, 15);

            double silentAudioBeforeDuration = Math.Round(sampleBasedStartSeconds - originalFrameBasedStartSeconds, 15);
            double silentAudioAfterDuration = Math.Round(originalFrameBasedEndSeconds - sampleBasedEndSeconds, 15);

            // The initial silence is the fraction of a frame (in seconds) from the frame end to the sample end
            // (because the audio will be reversed), extended by the reversal speed.
            double originalFinalSilenceDuration = Math.Round(originalFrameBasedEndSeconds - sampleBasedEndSeconds, 15);

            double calculatedFinalSilenceDuration = Math.Round(originalFinalSilenceDuration / reversalSpeed, 15);

            // The extended duration is end time of the reversal clip, taking into account the gap between the end sample and
            // end frame, the extended time of the audio at the reversal rate (100, 85, 70%), and the gap to the start frame.
            // It is calculated as the original (100% rate) duration in seconds of the reversal bookmark (0.84s), plus the
            // fraction of a second between the end of the reversal and the last frame of the reversal (0.02s), extended by the
            // reversal rate (0.84s + 0.02s = 0.86s / 0.7 = 1.228571428571429s). That time is ceilinged to the frame count
            // (1.228571428571429s * 29.97 = 36.82028571428571, Math.Ceilng = 37). That frame count is then applied at the
            // output frame rate (37 frames / 29.97 fps = 1.234567901234568s). That number is subtracted from the frame based
            // end seconds to arrive at the calculated frame based start in seconds.
            double calculatedFrameBasedEndSeconds = Math.Round(Math.Ceiling((originalFrameBasedEndSeconds + originalFinalSilenceDuration)
                * MainForm.FramesPerSecond) / MainForm.FramesPerSecond, 15);
            double calculatedFrameBasedStartSeconds =
                (originalFrameBasedEndSeconds -
                    Math.Ceiling(
                        Math.Round(originalFrameBasedDuration / reversalSpeed, 15)      // Original duration, stretched.
                    * MainForm.FramesPerSecond)                                                  // Frame count, celininged.
                / MainForm.FramesPerSecond                                                       // Converted back to a duration.
                );                                                                      // Converted back to a specific time.
            double calculatedFrameBasedDuration = Math.Round(Math.Ceiling(Math.Round((calculatedFrameBasedEndSeconds - originalFrameBasedStartSeconds)
                / reversalSpeed * MainForm.FramesPerSecond, 11)) / MainForm.FramesPerSecond, 15);

            // To walk it through ...
            //double temp;

            //temp = originalFrameBasedDuration / reversalSpeed;      // Original duration, stretched.
            //temp = Math.Round(temp, 15);                            // Frame count, celininged.
            //temp = Math.Ceiling(temp * FramesPerSecond);            // Converted back to a duration.
            //temp = temp / FramesPerSecond;                          // Converted back to a specific time.
            //calculatedFrameBasedStartSeconds = originalFrameBasedEndSeconds - temp;

            //temp = calculatedFrameBasedEndSeconds - originalFrameBasedStartSeconds;
            //temp = temp / reversalSpeed;
            //temp = temp * FramesPerSecond;
            //temp = Math.Round(temp, 11);
            //temp = Math.Ceiling(temp);
            //temp = temp / FramesPerSecond;
            //temp = Math.Round(temp, 15);
            //calculatedFrameBasedEndSeconds = temp;

            String videoFilename1;
            String videoFilename2;
            String command1;
            String command2;

            // Create the filter_complex filtergraphs.
            String audioFiltergraph1;
            String interpolationFiltergraph1 = String.Empty;
            // String reverseAudioFiltergraph = "[SlowAudio]areverse[a]";

            //
            // Audio filtergraphs.
            //

            // Normal audio slowdown.

            videoFilename1 = $"R1.{reversalRate}";

            audioFiltergraph1 = $"[0:a] atrim=0:{silentAudioBeforeDuration:0.############}, asetpts=PTS-STARTPTS, "
                + $"volume=volume=0 [SilentAudioBefore];"
                + $"[0:a] atrim={silentAudioBeforeDuration:0.############}:duration={sampleBasedDuration:0.############}, "
                + $"asetpts=PTS-STARTPTS [SelectedAudio]; "
                + $"[0:a] atrim={silentAudioBeforeDuration + sampleBasedDuration:0.############}:"
                + $"duration={originalFrameBasedEndSeconds - sampleBasedEndSeconds:0.############}, asetpts=PTS-STARTPTS, "
                + $"volume=volume=0 [SilentAudioAfter];"
                + $"[SilentAudioBefore] [SelectedAudio] [SilentAudioAfter] concat=n=3:v=0:a=1, asetpts=PTS-STARTPTS [AllAudio]; "
                + $"[AllAudio] asetpts=PTS-STARTPTS, "
                + $"asetrate={MainForm.SampleRate}*{reversalSpeed:0.###}, aresample={MainForm.SampleRate} [a]";

            videoFilename2 = videoFilename1 + ".Text";

            //
            // Video filtergraphs, without reverse.
            //

            // No motion interpolation requested (MotionInterpolation.None) or required (reversalRate.ReversalSpeed == 100).

            interpolationFiltergraph1 = $"[0:v] trim=0:{originalFrameBasedDuration}, "
                + $"setpts=PTS-STARTPTS, "
                + $"setpts=PTS/{reversalSpeed:0.#######}, "
                + $"fps=fps={MainForm.FramesPerSecond:0.############}:eof_action=pass [SlowForwardV]; "
                + $"[SlowForwardV] split [v] [SlowForwardV1]";

            // The command to use when memory requirements allow the reverseFiltergraph method.
            // TODO: calculatedFrameBasedStartSeconds is way off and needs fixing.
            command1 = $"-y -hide_banner "
                + $"-ss {originalFrameBasedStartSeconds:0.############} -i \"{RelativePathToWorkingInputVideoFile}\" "
                + $"-filter_complex \"{interpolationFiltergraph1}; {audioFiltergraph1}\" "
                + $"-map [v] -map [a] -t {calculatedFrameBasedDuration:0.##########} -progress \"{videoFilename1}.progress\" -threads {{0}} {OutputInterimSettings} "
                + $"\"{videoFilename1}{OutputVideoInterimExtension}\" "
                + $"-map [SlowForwardV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.Last.png\"";

            // The second video is where the reversing happens. This was because I couldn't get the text overlay to work
            // while reversing the video.
            // In this Test Run Video, I will use a fully transparent black as the second input so as not to break this filtergraph.
            command2 = $"-y -hide_banner "
                + $"-i \"{videoFilename1}{OutputVideoInterimExtension}\" "
                + $"-f lavfi -i color=color=black:size={MainForm.HorizontalResolution}x{MainForm.VerticalResolution} -loop 1 "
                + $"-filter_complex \"[0:v] reverse [ReversedV]; "
                + $"[ReversedV] split [ReversedV1] [ReversedV2]; "
                + $"[ReversedV2] [0:v] overlay [v]; "
                + $"[0:a] areverse [a]\" "
                + $"-map [v] -map [a] -t {calculatedFrameBasedDuration:0.##########} -progress \"{videoFilename2}.progress\" -threads {{0}} {OutputInterimSettings} "
                + $"\"{videoFilename2}{OutputVideoInterimExtension}\" "
                + $"-map [ReversedV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.First.png\"";

            // Add the commands to the list.
            ffmpegCommands.Add(command1);
            ffmpegCommands.Add(command2);

            return true;
        }
    }
}
