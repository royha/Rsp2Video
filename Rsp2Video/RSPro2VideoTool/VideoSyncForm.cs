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
using System.Text.RegularExpressions;
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
        double VideoOffset;                                             // The video offset, in frames, to align the video with the audio.
        Double VideoDelayInSeconds;                                              // The video offset, in seconds, to align the video with the audio.
        List<String> ffmpegCommands1;                                   // The list of ffmpeg command strings to create the test run video, phase 1.
        List<String> ffmpegCommands2;                                   // The list of ffmpeg command strings to create the test run video, phase 2.
        List<String> ffmpegCommands3;                                   // The list of ffmpeg command strings to create the test run video, phase 3.
        double ClipStartTime;                                           // The start time of the test run clip.
        double ClipEndTime;                                             // The end time of the test run clip.
        double ClipDuration;                                            // The ClipDuration of the test run clip.
        String TestRunFile = "TestRun";
        String SyncedVideoFilename = String.Empty;
        String OutputVideoInterimExtension = ".mkv";
        String OutputVideoFinalExtension = ".mp4";
        String OutputInterimSettings = "-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 18 -c:a aac -q:a 1 -movflags +faststart";
        String OutputHighSettings = "-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 16 -c:a aac -q:a 1 -movflags +faststart";

        public VideoSyncForm()
        {
            InitializeComponent();
        }

        public VideoSyncForm(Form callingForm)
        {
            MainForm = callingForm as RSPro2VideoToolForm;
            InitializeComponent();

            // Show the source video filename.
            labelVideoNameValue.Text = Path.GetFileName(MainForm.SourceVideoFile);

            trackBarVideoStartTime.Maximum = (int)MainForm.SourceVideoDuration;
            trackBarVideoStartTime.Value = 1;
            trackBarVideoStartTime.Value = 0;
            trackBarVideoStartTime.Value = (int)(MainForm.SourceVideoDuration / 10.0d);
            trackBarVideoDuration.Value = 3;

            // Create the _tmp directory.
            CreateDirectories();
        }

        private String FormatTimeSpan (double duration)
        {
            // Choose a different format string if this duration is greater than one hour.
            String formatString = (duration < 3600.0d) ? @"mm\:ss\.ffff" : @"hh\:mm\:ss\.ffff";

            TimeSpan durationTimeSpan = TimeSpan.FromSeconds(duration);

            // Convert to string with full precision
            string timeSpanString = durationTimeSpan.ToString(formatString);

            // Use regular expression to remove trailing zeros
            string truncatedTimeSpan = Regex.Replace(timeSpanString, @"(\.\d*?)0+$", "$1");

            // Remove the decimal point if no fractional part remains
            truncatedTimeSpan = Regex.Replace(truncatedTimeSpan, @"\.$", "");

            return truncatedTimeSpan;
        }

        private void buttonMakeFinalVideo_Click(object sender, EventArgs e)
        {
            // Let the user select the output outputFilename.
            SyncedVideoFilename = MainForm.SaveVideoFileDialog(VideoOutputType.Sync);
            if (SyncedVideoFilename == null) { return; }

            String syncedVideoFilenameWithoutExtension = Path.GetFileNameWithoutExtension(SyncedVideoFilename);
            String syncedVideoPath = Path.GetDirectoryName(SyncedVideoFilename);
            String syncedVideoFilenameWithoutExtensionWithPath = Path.Combine(syncedVideoPath,
                syncedVideoFilenameWithoutExtension);

            // Set the log file location.
            MainForm.SetLogFileLocation(syncedVideoFilenameWithoutExtension);

            // Output the synchronized video file.
            Process process = new Process();

            String arguments = $"-y -hide_banner -i \"{MainForm.SourceVideoFile}\" "
                + $"-itsoffset {VideoDelayInSeconds:0.#######} "
                + $"-i \"{MainForm.SourceVideoFile}\" -map 1:v -map 0:a -c copy "
                + $"\"{SyncedVideoFilename}\"";

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
                MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nComment: Sync audio and video for final output.\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n***Error: Exit code {ExitCode}\r\n\r\n{FfmpegOutput}\r\n");

                MessageBox.Show("The file did not save correctly.", "Error writing synced file");

                this.DialogResult = DialogResult.Abort;

                return;
            }

            // Log the ffmpeg command line options and the ffmpeg output.
            MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nComment: Sync audio and video for final output.\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n{FfmpegOutput}\r\n");

            // Remove the _tmp directory and its contents.
            RemoveTemp_DirDirectory();

            // Close this dialog box.
            this.DialogResult = DialogResult.OK;
        }

        private void buttonMakeTestRun_Click(object sender, EventArgs e)
        {
            labelVideoSyncStatus.Text = "Creating test run video ...";

            groupBoxVideoSync.Enabled = false;

            // Change the mouse pointer to an hourglass.
            Application.UseWaitCursor = true;

            Application.DoEvents();

            if (InitializeTestRunVideo() == false) { return; }

            // Run the synchronization process asynchronously.
            Task task = Task.Run(() => MakeTestRunVideo());
            task.Wait();

            // Restore the mouse pointer to the normal arrow.
            Application.UseWaitCursor = false;

            labelVideoSyncStatus.Text = String.Empty;

            // Launch the video player.
            try
            {
                System.Diagnostics.Process.Start(Path.Combine(WorkingDirectory, TestRunFile + OutputVideoFinalExtension));
            }
            catch { }
            
            groupBoxVideoSync.Enabled = true;
        }

        private void MakeTestRunVideo()
        {
            MakeForwardClipString();

            MakeReverseClipStrings();

            // MakeTransitionClipStrings();

            MakeClipsFromClipStrings();

            AssembleTestRunVideo();

            //// Delete the log file.
            //if (MainForm.checkBoxDeleteLogfile.Checked)
            //{
            //    MainForm.DeleteLogFile();
            //}
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Cancel out of this dialog box.
            this.DialogResult = DialogResult.Cancel;

            // Remove the _tmp directory.
            RemoveTemp_DirDirectory();
        }

        private void trackBarVideoStartTime_ValueChanged(object sender, EventArgs e)
        {
            labelStartTimeValue.Text = FormatTimeSpan(trackBarVideoStartTime.Value);
        }

        private void trackBarVideoDuration_ValueChanged(object sender, EventArgs e)
        {
            // Update the display.
            labelDurationValue.Text = $"{trackBarVideoDuration.Value} seconds";
        }

        private Boolean InitializeTestRunVideo()
        {
            // Set the video offset/delay.
            VideoOffset = (double)numericOffset.Value;
            VideoDelayInSeconds = VideoOffset / MainForm.FramesPerSecond;

            // Initialize the lists of ffmpeg command strings.
            ffmpegCommands1 = new List<String>();
            ffmpegCommands2 = new List<String>();
            ffmpegCommands3 = new List<String>();

            // Set the log file location.
            MainForm.SetLogFileLocation(Path.Combine(WorkingDirectory, TestRunFile + OutputVideoFinalExtension));

            // Copy and sync the source video into the _tmp directory as v.mp4.
            CopySourceVideoToWorkingDirectory();

            // Calculate the start and end points of the clip.
            ClipStartTime = trackBarVideoStartTime.Value;
            ClipDuration = trackBarVideoDuration.Value;

            // Is there enough time for the requested duration?
            if (MainForm.SourceVideoDuration - ClipStartTime < ClipDuration)
            {
                MessageBox.Show("The Start time does not leave enough time for your chosen Duration.\r\nPlease change the Start Time or the Duration and try again.",
                    "Time and Duration error");

                return false;
            }

            ClipEndTime = ClipStartTime + ClipDuration;

            return true;
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

        private void MakeTransitionClipStrings()
        {
            AddTransitionFromFrames("F1.First", "R1.100.First", 1.0d);
            AddTransitionFromFrames("R1.100.Last", "R1.85.First", 1.0d);
            AddTransitionFromFrames("R1.85.Last", "R1.70.First", 1.0d);
        }

        private void MakeClipsFromClipStrings()
        {
            // Calculate the maximum threads to use, based on 4 simultaneous processes.
            List<int> ThreadsMax = CalculateMaxThreads(4);
            int tplThreads = ThreadsMax[0];
            int ffmpegThreads = ThreadsMax[1];

            // Run all of the phase 1 tasks in order.
            //foreach (String ffmpegCommand in ffmpegCommands1)
            Parallel.ForEach(ffmpegCommands1, new ParallelOptions { MaxDegreeOfParallelism = tplThreads }, ffmpegCommand =>
            {
                RunFfmpegTask(ffmpegThreads, ffmpegCommand);
            //}
            });

            // Calculate the maximum threads to use, based on 3 simultaneous processes.
            ThreadsMax = CalculateMaxThreads(3);
            tplThreads = ThreadsMax[0];
            ffmpegThreads = ThreadsMax[1];

            // Run all of the phase 2 tasks in order.
            //foreach (String ffmpegCommand in ffmpegCommands2)
            Parallel.ForEach(ffmpegCommands2, new ParallelOptions { MaxDegreeOfParallelism = tplThreads }, ffmpegCommand =>
            {
                RunFfmpegTask(ffmpegThreads, ffmpegCommand);
            //}
            });

            // Run all of the phase 3 tasks in order.
            //foreach (String ffmpegCommand in ffmpegCommands3)
            //Parallel.ForEach(ffmpegCommands3, new ParallelOptions { MaxDegreeOfParallelism = tplThreads }, ffmpegCommand =>
            //{
            //    RunFfmpegTask(ffmpegThreads, ffmpegCommand);
            //}
            //});
        }

        /// <summary>
        /// Calculates the maximum TPL threads and ffmpeg threads based on the specified max processes to run simultaneously.
        /// </summary>
        /// <param name="MaxProcesses">The maximum number of processes to run simultaneously.</param>
        /// <returns>A list of int values with tplThreads as element 0, and ffmpegThreads as element 1.</returns>
        private List<int> CalculateMaxThreads(int MaxProcesses)
        {
            int tplThreads, ffmpegThreads;

            // Calculate how many .NET TPL threads to use, and how many ffmpeg threads to use within each TPL thread.
            if (Environment.ProcessorCount < MaxProcesses)
            {
                // If 8 or fewer processors are available, use one TPL thread for each processor,
                // and use one thread per ffmpeg task.
                tplThreads = Environment.ProcessorCount; ffmpegThreads = 1;
            }
            else
            {
                // Max out at 4 threads so as not to overwhelm disk I/O.
                int maxTplThreads = MaxProcesses;

                // ffmpeg defaults to 1.5 * the processor count, so I will follow that paradigm.
                int maxFfmpegThreads = (int)(Environment.ProcessorCount * 1.5d);

                // Special case to round down for a ratioRemainder that ends in .5
                // (4 TPL * 1.5 = 6 ffmpeg threads, = 1.5 ffmpeg/tpl, works better with 4, 1 than 4, 2.
                Double ratioRemainder = ((Double)maxFfmpegThreads % (Double)maxTplThreads) / (Double)maxTplThreads;
                if (ratioRemainder <= 0.5d)
                {
                    // Round down.
                    tplThreads = maxTplThreads; ffmpegThreads = (int)(Math.Floor((Double)maxFfmpegThreads / (Double)maxTplThreads));
                }
                else
                {
                    // Round up.
                    tplThreads = maxTplThreads; ffmpegThreads = (int)(Math.Ceiling((Double)maxFfmpegThreads / (Double)maxTplThreads));
                }
            }

            return new List<int> { tplThreads, ffmpegThreads };
        }

        private void AssembleTestRunVideo()
        {
            // Create the file list for this video.
            List<String> fileList = new List<string>
            {
                "file F1.mkv",
                // "file F1.First-R1.100.First.mkv",
                "file R1.100.Text.mkv",
                // "file R1.100.Last-R1.85.First.mkv",
                "file R1.85.Text.mkv",
                // "file R1.85.Last-R1.70.First.mkv",
                "file R1.70.Text.mkv"
            };

            // Write the text file.
            File.WriteAllLines("filelist.txt", fileList);

            // Create the Process to call the external program.
            Process process = new Process();

            // Create the arguments string.
            String arguments = $"-y -hide_banner -f concat -safe 0 -i filelist.txt -c copy "
                + $"\"{TestRunFile}{OutputVideoFinalExtension}\"";

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

            // Log the ffmpeg command line options.
            MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n");

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Log the ffmpeg output.
            MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, FfmpegOutput);

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            return;
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
                + $"-itsoffset {VideoDelayInSeconds:0.#######} "
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
            ffmpegCommands1.Add(command);

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
            //command1 = $"-y -hide_banner "
            //    + $"-ss {originalFrameBasedStartSeconds:0.############} -i \"{RelativePathToWorkingInputVideoFile}\" -t {originalFrameBasedDuration:0.##########} "
            //    + $"-filter_complex \"{interpolationFiltergraph1}; {audioFiltergraph1}\" "
            //    + $"-map [v] -map [a] -t {originalFrameBasedDuration:0.##########} -progress \"{videoFilename1}.progress\" -threads {{0}} {OutputInterimSettings} "
            //    + $"\"{videoFilename1}{OutputVideoInterimExtension}\" "
            //    + $"-map [SlowForwardV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.Last.png\"";

            // Adding the "-t {originalFrameBasedDuration:0.############} " solved the "this made way too long of a clip" issue.
            command1 = $"-y -hide_banner "
                + $"-ss {originalFrameBasedStartSeconds:0.############} -t {originalFrameBasedDuration:0.############} -i \"{RelativePathToWorkingInputVideoFile}\" "
                + $"-filter_complex \"{interpolationFiltergraph1}; {audioFiltergraph1}\" "
                + $"-map [v] -map [a] -progress \"{videoFilename1}.progress\" -threads {{0}} {OutputHighSettings} "
                + $"\"{videoFilename1}{OutputVideoInterimExtension}\" "
                + $"-map [SlowForwardV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.Last.png\"";

            // The second video is where the reversing happens. This was because I couldn't get the text overlay to work
            // while reversing the video.
            // In this Test Run Video, I will use a fully transparent black as the second input so as not to break this filtergraph.
            //command2 = $"-y -hide_banner "
            //    + $"-i \"{videoFilename1}{OutputVideoInterimExtension}\" "
            //    + $"-f lavfi -i color=color=black:size={MainForm.HorizontalResolution}x{MainForm.VerticalResolution} -loop 1 -t {calculatedFrameBasedDuration:0.##########} "
            //    + $"-filter_complex \"[0:v] reverse [ReversedV]; "
            //    + $"[ReversedV] split [ReversedV1] [ReversedV2]; "
            //    + $"[ReversedV2] [0:v] overlay [v]; "
            //    + $"[0:a] areverse [a]\" "
            //    + $"-map [v] -map [a] -progress \"{videoFilename2}.progress\" -threads {{0}} {OutputInterimSettings} "
            //    + $"\"{videoFilename2}{OutputVideoInterimExtension}\" "
            //    + $"-map [ReversedV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.First.png\"";

            command2 = $"-y -hide_banner "
                + $"-i \"{videoFilename1}{OutputVideoInterimExtension}\" "
                + $"-f lavfi -i color=color=black:size={MainForm.HorizontalResolution}x{MainForm.VerticalResolution} -loop 1 -t {calculatedFrameBasedDuration:0.##########} "
                + $"-filter_complex \"[0:v] reverse [ReversedV]; "
                + $"[ReversedV] split [ReversedV1] [ReversedV2]; "
                + $"[0:v] [ReversedV2] overlay [v]; "
                + $"[0:a] areverse [a]\" "
                + $"-map [v] -map [a] -progress \"{videoFilename2}.progress\" -threads {{0}} {OutputInterimSettings} -shortest "
                + $"\"{videoFilename2}{OutputVideoInterimExtension}\" "
                + $"-map [ReversedV1] -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename1}.First.png\"";

            // Add the commands to the list.
            ffmpegCommands1.Add(command1);
            ffmpegCommands2.Add(command2);

            return true;
        }

        private void AddTransitionFromFrames(String TransitionFromFrame, 
            String TransitionToFrame, 
            Double TransitionLength,
            TransitionType TransitionType = TransitionType.XFade)
        {
            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / MainForm.FramesPerSecond)
            {
                return;
            }

            String command = String.Empty;

            // Set the video output filename.
            String filename = TransitionFromFrame + "-" + TransitionToFrame;

            if (TransitionType == TransitionType.XFade)
            {
                command = $"-y -hide_banner -i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-framerate {MainForm.FramesPerSecond:0.###############} -loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                    + $"-framerate {MainForm.FramesPerSecond:0.###############} -loop 1 -t {TransitionLength:0.######} -i \"{TransitionToFrame}.png\" "
                  //+ $"-i \"{reverseBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] asetpts=PTS-STARTPTS, volume=0.0, atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v]xfade=transition=fade:duration={TransitionLength:0.######}, setpts=PTS-STARTPTS [v]\" "
                    + $"-map [v] -map [a] "
                    + $" -threads {{0}} {OutputInterimSettings} "
                    + $"\"{filename}{OutputVideoInterimExtension}\"";
            }
            else if (TransitionType == TransitionType.HoldLastFrame)
            {
                command = $"-y -hide_banner -i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-framerate {MainForm.FramesPerSecond:0.###############} -loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                  //+ $"-i \"{reverseBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] asetpts=PTS-STARTPTS, volume=0.0,atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v] overlay, setpts=PTS-STARTPTS [v]\" -map [v] -map [a] "
                    + $" -threads {{0}} {OutputInterimSettings} "
                    + $"\"{filename}{OutputVideoInterimExtension}\"";
            }

            // Create the ffmpeg task for this clipEntry.
            ffmpegCommands3.Add(command);

            return;
        }


        /// <summary>
        /// Creates a minterpolated reverse bookmark video clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the ffmpeg command line string to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        /// <remarks>Used when available memory is sufficient to use the filtergraph reverse method.</remarks>
        private Boolean RunFfmpegTask(int ffmpegThreads, String ffmpegTask)
        {
            // Set the value for the ffmpeg "-threads" parameter.
            String ffmpegCommand = String.Format(ffmpegTask, ffmpegThreads);

            // Run the command.
            return RunFfmpegTaskRaw(ffmpegCommand);
        }

        /// <summary>
        /// Runs the ffmpeg program with the specified argument string without adding the Filename to CreatedClipList or to
        /// VideoOutputs. Also includes the name of the method that created this ffmpeg task in the logfile.
        /// </summary>
        /// <param name="arguments">The ffmpeg commands.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RunFfmpegTaskRaw(String arguments, String comment = null)
        {
            // Create the Process to call the external program.
            Process process = new Process();

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

            // Start ffmpeg to extract the imageFiles.
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
                // Log the task, the exit code, and the output.
                if (comment == null)
                {
                    MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"Command line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n*** Error: ffmpeg exit code = {ExitCode}\r\n\r\nffmpeg output:\r\n{FfmpegOutput}");
                }
                else
                {
                    MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"Comment: {comment}\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n*** Error: ffmpeg exit code = {ExitCode}\r\n\r\nffmpeg output:\r\n{FfmpegOutput}");
                }
                return false;
            }

            // Log the ffmpeg task and the output.
            if (comment == null)
            {
                MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"Command line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\nffmpeg output: \r\n{FfmpegOutput}");
            }
            else
            {
                MainForm.WriteLog(MethodBase.GetCurrentMethod().Name, $"Comment: {comment}\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\nffmpeg output: \r\n{FfmpegOutput}");
            }

            return true;
        }
    }
}
