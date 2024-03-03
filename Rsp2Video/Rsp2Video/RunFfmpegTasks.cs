﻿using System;
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
                Boolean retval = true;

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
                Boolean retval = true;

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
            foreach (FFmpegTask ffmpegTask in phaseThreeTasks)
            {
                Boolean retval = true;

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
                    // return false;
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

            // Set the value for the ffmpeg "-threads" parameter.
            String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads);

            // Run the command.
            if (RunFfmpegRaw(ffmpegCommand) == false)
            {
                return false;
            }

            // Get the clip duration and set that duration in the ClipDuration dictionary.
            ClipDuration clipDuration = GetProgressDuration(videoFilename);
            if (clipDuration.FrameCount < 0)
            {
                return false;
            }

            // Get the first and last frame of the clip.
            if (CreateFirstAndLastFrameFromClip(videoFilename, clipDuration.Duration) == false)
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
            String videoFilename = ffmpegTask.VideoFilenames[0];

            // Set the value for the ffmpeg "-threads" parameter.
            String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads);

            // Run the command.
            if (RunFfmpegRaw(ffmpegCommand) == false)
            {
                return false;
            }

            // Get the clip duration and set that duration in the ClipDuration dictionary.
            ClipDuration clipDuration = GetProgressDuration(videoFilename);
            if (clipDuration.FrameCount < 0)
            {
                return false;
            }

            // Get the first and last frame of the clip.
            if (CreateFirstAndLastFrameFromClip(videoFilename, clipDuration.Duration) == false)
            {
                return false;
            }

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
            String videoFilename = String.Empty;

            // Loop through the ffmpeg commands.
            for (int i = 0; i < ffmpegTask.FFmpegCommands.Count; ++i)
            {
                // Get the filename and the ffmpeg command for this iteration.
                videoFilename = ffmpegTask.VideoFilenames[i];
                String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[i], ffmpegThreads);

                // Run the command.
                if (RunFfmpegRaw(ffmpegCommand) == false)
                {
                    return false;
                }
            }

            // Get the clip duration of the final filename and set that duration in the ClipDuration dictionary.
            ClipDuration clipDuration = GetProgressDuration(videoFilename);
            if (clipDuration.FrameCount < 0)
            {
                return false;
            }

            // Get the first and last frame of the clip.
            if (CreateFirstAndLastFrameFromClip(videoFilename, clipDuration.Duration) == false)
            {
                return false;
            }

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
            String videoFilename = ffmpegTask.VideoFilenames[0];
            String FilenameWithExtension = videoFilename + OutputVideoInterimExtension;

            // Set the value for the ffmpeg "-threads" parameter.
            String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads);

            // Because separate threads could want to write to a file ("Black.0.5.mp4" for example), I lock the
            // filename until everything is done. When the locked thread gets access, it will discover the 
            // file already exists and will not attempt to create the file.
            lock (FilenameWithExtension)
            {
                // If the file exists, then I don't need to create it.
                if (File.Exists(FilenameWithExtension))
                {
                    return true;
                }

                // Run the command.
                if (RunFfmpegRaw(ffmpegCommand) == false)
                {
                    return false;
                }

                // Get the clip duration and set that duration in the ClipDuration dictionary.
                GetProgressDuration(videoFilename);
            }

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
            String videoFilename = ffmpegTask.VideoFilenames[0];
            String FilenameWithExtension = videoFilename + OutputVideoInterimExtension;

            // Set the value for the ffmpeg "-threads" parameter.
            String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads);

            // Because separate threads could want to write to a file ("Black.0.5.mp4" for example), I lock the
            // filename until everything is done. When the locked thread gets access, it will discover the 
            // file already exists and will not attempt to create the file.
            lock (FilenameWithExtension)
            {
                // If the file exists, then I don't need to create it.
                if (File.Exists(FilenameWithExtension))
                {
                    return true;
                }

                // Run the command.
                if (RunFfmpegRaw(ffmpegCommand) == false)
                {
                    return false;
                }

                // Get the clip duration and set that duration in the ClipDuration dictionary.
                GetProgressDuration(videoFilename);
            }

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
            String videoFilename = String.Empty;
            String FilenameWithExtension = String.Empty;

            // Presently, there are two types of transition videos. Single step and two-step.
            if (ffmpegTask.VideoFilenames.Count == 1)
            {
                videoFilename = ffmpegTask.VideoFilenames[0];
                FilenameWithExtension = videoFilename + OutputVideoInterimExtension;

                // Set the value for the ffmpeg "-threads" parameter.
                String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[0], ffmpegThreads);

                // Because separate threads could want to write to a file ("Black.0.5.mp4" for example), I lock the
                // filename until everything is done. When the locked thread gets access, it will discover the 
                // file already exists and will not attempt to create the file.
                lock (FilenameWithExtension)
                {
                    // If the file exists, then I don't need to create it.
                    if (File.Exists(FilenameWithExtension))
                    {
                        return true;
                    }

                    // Run the command.
                    if (RunFfmpegRaw(ffmpegCommand) == false)
                    {
                        return false;
                    }

                    // Get the clip duration and set that duration in the ClipDuration dictionary.
                    GetProgressDuration(videoFilename);
                }
            }
            else
            {
                // Loop through the ffmpeg commands.
                for (int i = 0; i < ffmpegTask.FFmpegCommands.Count; ++i)
                {
                    // Get the filename and the ffmpeg command for this iteration.
                    videoFilename = ffmpegTask.VideoFilenames[i];
                    String ffmpegCommand = String.Format(ffmpegTask.FFmpegCommands[i], ffmpegThreads);

                    // Run the command.
                    if (RunFfmpegRaw(ffmpegCommand) == false)
                    {
                        return false;
                    }
                }

                // Get the clip duration of the final filename and set that duration in the ClipDuration dictionary.
                GetProgressDuration(videoFilename);
            }

            return true;
        }


        private ClipDuration GetProgressDuration(string videoFilename)
        {
            String progressFile = videoFilename + ".progress";
            String progressFileContents = String.Empty;
            String searchString = "frame=";

            // Read in the .progress file.
            try { progressFileContents = File.ReadAllText(progressFile); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error opening {progressFile}. {e.Message}\r\n\r\n");
                return new ClipDuration(-1, -1.0d);
            }

            // Log the .progress file.
            File.AppendAllText(LogFile, $"{progressFile}:\r\n");
            File.AppendAllText(LogFile, progressFileContents);
            File.AppendAllText(LogFile, "\r\n");

            // Find the last frame count.
            int i = progressFileContents.LastIndexOf(searchString);
            if (i < 0)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to find frame count in {progressFile}. {progressFileContents}\r\n\r\n");
                return new ClipDuration(-1, -1.0d);
            }

            // Parse the last frame count.
            String frameCountString = progressFileContents.Substring(i + searchString.Length);
            frameCountString = frameCountString.Substring(0, frameCountString.IndexOf('\n'));
            if (Int32.TryParse(frameCountString, out int frameCount) == false)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Error parsing frame count in {progressFile}. {progressFileContents}\r\n\r\n");
                return new ClipDuration(-1, -1.0d);
            }
            double duration = Math.Round((double)frameCount / FramesPerSecond, 15);

            // Store the reverse video clip duration.
            if (ClipDuration.TryAdd($"{videoFilename}{OutputVideoInterimExtension}", duration) == false)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Video file {progressFile} already exists in ClipDuration.\r\n\r\n");
                return new ClipDuration(-1, -1.0d);
            }

            File.Delete(progressFile);

            return new ClipDuration(frameCount, duration);
        }

        /// <summary>
        /// Creates the {videoFilename}.First.png and {videoFilename}.Last.png for the specified video clip.
        /// </summary>
        /// <param name="videoFilename"></param>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private Boolean CreateFirstAndLastFrameFromClip(string videoFilename, double duration)
        {
            String videoFilenameWithExtension = videoFilename + OutputVideoInterimExtension;
            Boolean retval = false;


            // Create the .Last.png file.

            // Create a temp directory to store the last imageFiles of the clip.
            String frameStorageDirectory = Path.Combine(WorkingDirectory, videoFilename);
            DirectoryInfo diFrameStorageDirectory;
            try { diFrameStorageDirectory = Directory.CreateDirectory(frameStorageDirectory); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to create directory {frameStorageDirectory}, {e.Message}\r\n\r\n");
                return false;
            }

            // Output the last few imageFiles of the clip in the temp directory.

            String fileOutputString = Path.Combine(frameStorageDirectory, videoFilename);
            for (int i = 0; retval == false; ++i)
            {
                // Get the time to see back from EOF.
                double sseofValue = LastFrameSeekBack[i];

                // If the clip is longer than the seek back value.
                if (duration >= sseofValue)
                {
                    // Create the ffmpeg command to write the final imageFiles of the clip.
                    String getLast = $"-y -hide_banner -sseof -{sseofValue} -i \"{videoFilenameWithExtension}\" "
                        + $"-pix_fmt rgb48 -an \"{fileOutputString + ".Last.%05d.png"}\"";

                    // Run the ffmpeg command.
                    retval = RunFfmpegRaw(getLast);
                }
                else
                {
                    // The clip is not longer than the seek back value.
                    // Create the ffmpeg command to write all imageFiles from the clip.
                    String getLast = $"-y -hide_banner -i \"{videoFilenameWithExtension}\" "
                        + $"-pix_fmt rgb48 -an -filter:v \"{fileOutputString + ".Last.%05d.png"}\"";

                    // Run the ffmpeg command.
                    retval = RunFfmpegRaw(getLast);
                    break;
                }
            }

            // Return false if all tries failed.
            if (retval == false)
            {
                return false;
            }

            // Create the ffmpeg command to write the final imageFiles of the clip.

            // Move and rename the last .png file to the VideoFilenames.First.png.

            // Get a list of the files that match.
            string[] imageFiles = Directory.GetFiles(frameStorageDirectory, $"{videoFilename}.Last.*.png");

            // The File.Move method won't overwrite a file. It must be deleted before a rename/move can succeed.
            String lastImageFilename = Path.Combine(WorkingDirectory, $"{videoFilename}.Last.png");
            if (File.Exists(lastImageFilename))
            {
                try { File.Delete(lastImageFilename); }
                catch (Exception e)
                {
                    File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {lastImageFilename}, {e.Message}\r\n\r\n");
                    return false;
                }
            }

            // Move and rename the last frame.
            string renameFrom = Path.Combine(frameStorageDirectory, imageFiles[imageFiles.Length - 1]);
            try { File.Move(Path.Combine(frameStorageDirectory, imageFiles[imageFiles.Length - 1]), lastImageFilename); }
            catch (Exception e)
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete {lastImageFilename}, {e.Message}\r\n\r\n");
                return false;
            }

            // Delete the directory for the .png files.
            try { diFrameStorageDirectory.Delete(true); }
            catch
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***Error: Unable to delete directory {frameStorageDirectory}\r\n\r\n");
                return false;
            }


            // Create the .First.png file.

            // Create the ffmpeg command to write the .First.png file.
            // TODO: Single first frame:
            // ffmpeg -y -hide_banner -i "v.mp4" -pix_fmt rgb48 -an -q:v 1 -imageFiles:v 1 "output.First.png" 
            String getFirst = $"-y -hide_banner -i \"{videoFilenameWithExtension}\" "
                + $"-pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{videoFilename}.First.png\"";

            // Run the ffmpeg command.
            if (RunFfmpegRaw(getFirst) == false)
            {
                return false;
            }

            return true;
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

            // Start ffmpeg to extract the imageFiles.
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
            File.AppendAllText(LogFile, $"\r\n\r\n***Command line: {process.StartInfo.FileName} " 
                + "{process.StartInfo.Arguments}\r\n\r\n");

            // Start ffmpeg to extract the imageFiles.
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