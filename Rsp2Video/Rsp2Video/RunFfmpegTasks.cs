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
    // Runs the ffmpeg tasks to generate the clips for this vide.
    public partial class RSPro2VideoForm : Form
    {
        /// <summary>
        /// Executes all FFmpeg tasks.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunAllFfmpegTasks()
        {
            Boolean retval = true;
            int ffmpegThreads = 0;

            // Get a list of Phase 1 tasks, ordered by SortOrder, followed by EstimatedDuration in decending order.
            List<FFmpegTask> phaseOneTasks = FFmpegTasks
                .FindAll(f => f.Phase == FfmpegPhase.PhaseOne)
                .OrderBy(o => o.SortOrder)
                .ThenByDescending(t => t.EstimatedDuration)
                .ToList();

            // Run all of the Phase 1 tasks in order.
            foreach (FFmpegTask ffmpegTask in phaseOneTasks)
            {
                switch (ffmpegTask.SortOrder)
                {
                    case FfmpegTaskSortOrder.ReverseMinterpolateVideo:
                        retval = RunReverseMinterpolateVideoTask(ffmpegThreads, ffmpegTask);
                        break;
                }

                if (retval == false)
                {
                    return false;
                }
            }

            // Get a list of Phase 2 tasks, ordered by SortOrder, followed by EstimatedDuration in decending order.
            List<FFmpegTask> phaseTwoTasks = FFmpegTasks
                .FindAll(f => f.Phase == FfmpegPhase.PhaseTwo)
                .OrderBy(o => o.SortOrder)
                .ThenByDescending(t => t.EstimatedDuration)
                .ToList();

            // Run all of the Phase 2 tasks in order.
            foreach (FFmpegTask ffmpegTask in phaseTwoTasks)
            {
                switch (ffmpegTask.SortOrder)
                {
                    case FfmpegTaskSortOrder.ReverseVideo:
                        retval = RunReverseVideoTask(ffmpegThreads, ffmpegTask);
                        break;

                    case FfmpegTaskSortOrder.ForwardBookmarkVideo:
                        retval = RunForwardBookmarkVideoTask(ffmpegThreads, ffmpegTask);
                        break;

                    case FfmpegTaskSortOrder.ForwardVideo:
                        retval = RunForwardVideoTask(ffmpegThreads, ffmpegTask);
                        break;
                }

                if (retval == false)
                {
                    return false;
                }
            }

            // Get a list of Phase 3 tasks, ordered by SortOrder, followed by EstimatedDuration in decending order.
            List<FFmpegTask> phaseThreeTasks = FFmpegTasks
                .FindAll(f => f.Phase == FfmpegPhase.PhaseThree)
                .OrderBy(o => o.SortOrder)
                .ThenByDescending(t => t.EstimatedDuration)
                .ToList();

            // Run all of the Phase 3 tasks in order.
            foreach (FFmpegTask ffmpegTask in phaseTwoTasks)
            {
                switch (ffmpegTask.SortOrder)
                {
                    case FfmpegTaskSortOrder.CardVideo:
                        retval = RunCardVideoTask(ffmpegThreads, ffmpegTask);
                        break;

                    case FfmpegTaskSortOrder.TransitionVideo:
                        retval = RunTransitionVideoTask(ffmpegThreads, ffmpegTask);
                        break;
                }

                if (retval == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a minterpolated reverse bookmark video clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        /// <remarks>Used when available memory is sufficient to use the filtergraph reverse method.</remarks>
        private Boolean RunReverseMinterpolateVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            String videoFilename = ffmpegTask.VideoFilenames[0];

            // Run the command.
            if (RunFfmpeg(videoFilename, ffmpegTask.FFmpegCommands[0], ffmpegThreads) == false)
            {
                return false;
            }

            // Get the duration.
            double duration = GetProgressDuration(ffmpegTask.VideoFilenames[0]);
            if (duration < 0.0d)
            {
                return false;
            }

            // Add the file and duration to the list.
            if (ClipDuration.TryAdd($"{videoFilename}{OutputVideoInterimExtension}", duration) == false)
            {
                return false;
            }

            // Create the .First.png file.
            // *** Trying -vframes 1 instead of -update 1.
            String getFirst = $"-i \"{videoFilename}{OutputVideoInterimExtension}\" "
                + $"-pix_fmt rgb48 -an -filter:v \"select=eq(n\\,0)\" -f image2 -vframes 1 \"{videoFilename}.First.png\"";

            if (RunFfmpegRaw(getFirst) == false)
            {
                return false;
            }

            // Find the last frame.
            int lastFrame = (int)Math.Round(duration / FramesPerSecond);

            // Create the .Last.png file.
            String getLast = $"-i \"{videoFilename}{OutputVideoInterimExtension}\" "
                + $"-pix_fmt rgb48 -an -filter:v \"select=eq(n\\,{lastFrame})\" -f image2 -update 1 \"{videoFilename}.Last.png\"";

            if (RunFfmpegRaw(getLast) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a reverse bookmark video clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunReverseVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            // Available memory is sufficient to use the filtergraph reverse method.
            // if (RunReverseVideoTaskReverseMethod(ffmpegThreads, ffmpegTask) == false) { return false; }

            // Available memory is insufficent to use the filtergraph reverse method. Use the .png method instead.
            if (RunReverseVideoTaskPngMethod(ffmpegThreads, ffmpegTask) == false) { return false; }

            return true;
        }

        /// <summary>
        /// Creates a forward bookmark video clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunForwardBookmarkVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            return true;
        }

        /// <summary>
        /// Creates a forward video clip (video between bookmarks) based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunForwardVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            return true;
        }

        /// <summary>
        /// Creates a text card clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunCardVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            return true;
        }

        /// <summary>
        /// Creates a transition clip based on the specified FFmpegTask.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunTransitionVideoTask(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            return true;
        }

        /// <summary>
        /// Creates a reverse video clip based on the specified FFmpegTask. Uses filter_complex filtergraph to reverse the video.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean RunReverseVideoTaskReverseMethod(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            // Execute the ffmpeg FfmpegCommand to create the reverse video.
            if (RunFfmpegRaw(String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads)) == false) { return false; }

            // Get and store the clip duration.
            if (GetProgressDuration(ffmpegTask.VideoFilenames[0]) < 0) { return false; }

            return true;
        }


        private double GetProgressDuration(string videoFilename)
        {
            String progressFile = videoFilename + ".progress";
            String progressFileContents = String.Empty;
            String searchString = "frame=";

            // Read in the .progress file.
            try { progressFileContents = File.ReadAllText(progressFile); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error opening {progressFile}. {e.Message}\r\n\r\n");
                return -1d;
            }

            // Log the .progress file.
            File.AppendAllText(LogFile, $"{progressFile}:\r\n");
            File.AppendAllText(LogFile, progressFileContents);
            File.AppendAllText(LogFile, "\r\n");

            // Find the last frame count.
            int i = progressFileContents.LastIndexOf(searchString);
            if (i == -1)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to find frame count in {progressFile}. {progressFileContents}\r\n\r\n");
                return -1d;
            }

            // Parse the last frame count.
            String frameCountString = progressFileContents.Substring(i + searchString.Length);
            frameCountString = frameCountString.Substring(0, frameCountString.IndexOf('\n'));
            if (Int32.TryParse(frameCountString, out int frameCount) == false)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Error parsing frame count in {progressFile}. {progressFileContents}\r\n\r\n");
                return -1d;
            }
            double duration = Math.Round((double)frameCount / FramesPerSecond, 15);

            // Store the reverse video clip duration.
            if (ClipDuration.TryAdd($"{videoFilename}{OutputVideoInterimExtension}", duration) == false)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Video file {progressFile} already exists in ClipDuration.\r\n\r\n");
                return -1d;
            }

            File.Delete(progressFile);

            return duration;
        }

        /// <summary>
        /// Creates a reverse video clip based on the specified FFmpegTask. Exports and imports .png files to reverse the video.
        /// </summary>
        /// <param name="ffmpegThreads">The value for the ffmpeg -threads FfmpegCommand.</param>
        /// <param name="ffmpegTask">The FFmpegTask that contains the data to create the clip.</param>
        /// <returns>Returns true if successful; otherwise, false.</returns>

        private Boolean RunReverseVideoTaskPngMethod(int ffmpegThreads, FFmpegTask ffmpegTask)
        {
            // If the memory requirements for this reversal video are too great to use reverseFiltergraph method, output a
            // series of .png files for the video.

            DirectoryInfo diPngDirectory;

            // Create a directory to store the .png files.
            String PngDirectory = Path.Combine(WorkingDirectory, ffmpegTask.VideoFilenames[0]);

            // Create the directory for the .png files.
            try { diPngDirectory = Directory.CreateDirectory(PngDirectory); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to create directory {PngDirectory}, {e.Message}\r\n\r\n");
                return false;
            }

            // Output the clip as a series of .png files.
            if (RunFfmpegRaw(String.Format(ffmpegTask.FFmpegCommands[1], ffmpegThreads)) == false) { return false; }

            // Rename the .png files to reverse their order.
            String[] newFrames = ReorderFrames(ffmpegTask.VideoFilenames[0]);

            // Assemble the reversed .png files and add the reversed audio to create the reversal clip.
            if (RunFfmpegRaw(String.Format(ffmpegTask.FFmpegCommands[2], ffmpegThreads)) == false) { return false; }

            // Move and rename the last .png file to the VideoFilenames.First.png.
            String imageFilename = Path.Combine(WorkingDirectory, $"{ffmpegTask.VideoFilenames[0]}.First.png");

            if (File.Exists(imageFilename))
            {
                try { File.Delete(imageFilename); }
                catch (Exception e)
                {
                    File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {imageFilename}, {e.Message}\r\n\r\n");
                    return false;
                }
            }

            try { File.Move(Path.Combine(PngDirectory, newFrames[newFrames.Length - 1]), imageFilename); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {imageFilename}, {e.Message}\r\n\r\n");
                return false;
            }

            // Move and rename the first .png file to the VideoFilenames.Last.png
            imageFilename = Path.Combine(WorkingDirectory, $"{ffmpegTask.VideoFilenames}.Last.png");

            if (File.Exists(imageFilename))
            {
                try { File.Delete(imageFilename); }
                catch (Exception e)
                {
                    File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {imageFilename}, {e.Message}\r\n\r\n");
                    return false;
                }
            }

            try { File.Move(Path.Combine(PngDirectory, newFrames[0]), imageFilename); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {imageFilename}, {e.Message}\r\n\r\n");
                return false;
            }

            // Delete the directory for the .png files.
            try { diPngDirectory.Delete(true); }
            catch
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete directory {PngDirectory}\r\n\r\n");
                // return false; 
            }

            // Store the reverse video clip duration.
            if (ClipDuration.TryAdd($"{ffmpegTask.VideoFilenames}{OutputVideoInterimExtension}", (double)newFrames.Length * FramesPerSecond) == false)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Video file {ffmpegTask.VideoFilenames} already exists in ClipDuration.\r\n\r\n");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Runs the ffmpeg program with the specified commands to produce the output file.
        /// </summary>
        /// <param name="Filename">The name of the output file without an extension.</param>
        /// <param name="FfmpegCommand">The ffmpeg commands.</param>
        /// <param name="ffmpegThreads">The number of ffmpeg threads to use for this task.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RunFfmpeg(String Filename, String FfmpegCommand, int ffmpegThreads)
        {
            // Create the Process to call the external program.
            Process process = new Process();

            // Create the arguments string.
            String arguments = $"-y -hide_banner {FfmpegCommand} {OutputInterimSettings} -progress {Filename}.progress \"{Filename}{OutputVideoInterimExtension}\"";

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Log the ffmpeg FfmpegCommand line options.
            File.AppendAllText(LogFile, $"\r\n\r\n***Command line: {process.StartInfo.FileName} {process.StartInfo.Arguments}\r\n\r\n");

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Log the ffmpeg output.
            File.AppendAllText(LogFile, FfmpegOutput);

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            // Return success or failure.
            if (!(ExitCode == 0))
            {
                File.AppendAllText(LogFile, $"Error: ffmpeg exit code {ExitCode}\r\n");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Runs the ffmpeg program with the specified argument string without adding the Filename to CreatedClipList or to
        /// VideoOutputs.
        /// </summary>
        /// <param name="arguments">The ffmpeg commands.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RunFfmpegRaw(String arguments)
        {
            // Create the Process to call the external program.
            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Log the ffmpeg FfmpegCommand line options.
            File.AppendAllText(LogFile, $"\r\n\r\n***Command line: {process.StartInfo.FileName} {process.StartInfo.Arguments}\r\n\r\n");

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Log the ffmpeg output.
            File.AppendAllText(LogFile, FfmpegOutput);

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            // Return success or failure.
            if (!(ExitCode == 0))
            {
                File.AppendAllText(LogFile, $"Error: ffmpeg exit code {ExitCode}\r\n");
                return false;
            }

            return true;
        }
    }
}