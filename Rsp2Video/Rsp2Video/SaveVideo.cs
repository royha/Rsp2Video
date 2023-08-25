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
        /// <summary>
        /// Saves the video or video project to disk.
        /// </summary>
        private void SaveVideo()
        {
            // If Forward and Reverse selected.
            if (settings.BookmarkTypeFnR)
            {
                // Select the video contents for the output video.
                switch (settings.VideoContents)
                {
                    case VideoContents.FullVideo:

                        // Create the full video.
                        AssembleClipListFnRFullVideo();
                        break;

                    case VideoContents.BookmarksOnly:

                        // Create the video from forward and reverse bookmarks only.
                        AssembleClipListFnRBookmarksOnly();
                        break;

                    case VideoContents.SeparateVideos:

                        // Create separate videos for each reverse bookmarks.
                        AssembleClipListFnRSeparateVideos();
                        break;
                }
            }

            // If Orphaned Reversals is selected.
            if (settings.BookmarkTypeOrphanedReversals)
            {
                // Select the video contents for the output video.
                switch (settings.VideoContents)
                {
                    case VideoContents.FullVideo:

                        // Create the full video.
                        AssembleClipListOrphanedReversalsFullVideo();
                        break;

                    case VideoContents.BookmarksOnly:

                        // Create the video from forward and reverse bookmarks only.
                        AssembleClipListOrphanedReversalBookmarksOnly();
                        break;

                    case VideoContents.SeparateVideos:

                        // Create separate videos for each reverse bookmarks.
                        AssembleClipListOrphanedReversalsSeparateVideos();
                        break;
                }
            }

            // If Quick Check is selected.
            if (settings.BookmarkTypeQuickCheck)
            {
                // Create the video from forward and reverse bookmarks only.
                AssembleClipListOrphanedReversalBookmarksOnly();
            }

            // Assemble the clips into a video (or videos).
            AssembleVideo();
        }

        /// <summary>
        /// Assembles the final video from the list of video clips.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AssembleVideo()
        {
            return AssembleFfmpegVideo();
        }

        /// <summary>
        /// Assembles the final video from the list of video clips by using the command line Copy command.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AssembleCopyVideo()
        {
            foreach (VideoOutput videoOutput in VideoOutputs)
            {
                // Update the user.
                Progress.Report("Working: Assembling " + videoOutput.Filename);

                // Create the file list for this video.
                StringBuilder fileList = new StringBuilder();

                // Add all clip files.
                bool firstFile = true;
                foreach (String clip in videoOutput.Clips)
                {
                    // Add a "+" if this is not the first file.
                    if (firstFile)
                    {
                        firstFile = false;
                    }
                    else
                    {
                        fileList.Append("+");
                    }
                    // Add the clip to the list.
                    fileList.Append('"');
                    fileList.Append(clip);
                    fileList.Append('"');
                }

                // Create the Process to call the external program.
                Process process = new Process();

                // Create the arguments string.
                String arguments = String.Format("/c copy /y /b {0} \"..\\{1}\"",
                    fileList.ToString(),
                    videoOutput.Filename);

                // Configure the process using the StartInfo properties.
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Maximized
                };

                // Log the command line options.
                File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

                // Start the copy command to combine the clips.
                process.Start();

                // Read the output of ffmpeg.
                String cmdOutput = process.StandardError.ReadToEnd();

                // Log the output.
                File.AppendAllText(LogFile, cmdOutput);

                // Wait here for the process to exit.
                process.WaitForExit();
                int ExitCode = process.ExitCode;
                process.Close();

                // Return success or failure.
                if (!(ExitCode == 0))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Assembles the final video from the list of video clips by using qmelt.exe.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AssembleMeltVideo()
        {
            foreach (VideoOutput videoOutput in VideoOutputs)
            {
                // Update the user.
                Progress.Report("Working: Assembling " + videoOutput.Filename);

                // Create the file list for this video.
                StringBuilder fileList = new StringBuilder();

                // Add all clip files.
                foreach (String clip in videoOutput.Clips)
                {
                    // Add the clip to the list.
                    fileList.Append("file '");
                    fileList.Append(clip);
                    fileList.Append("'");
                    fileList.Append(Environment.NewLine);
                }

                // Create the Process to call the external program.
                Process process = new Process();

                // Create the arguments string.
                String arguments = String.Format("-consumer avformat:\"..\\{0}\" {1}{2}",
                    videoOutput.Filename,
                    OutputFinalSettingsQmelt,
                    fileList.ToString());

                // Configure the process using the StartInfo properties.
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = QmeltApp,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Maximized
                };

                // Log the ffmpeg command line options.
                File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

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
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Assembles the final video from the list of video clips by using ffmpeg.exe.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AssembleFfmpegVideo()
        {
            foreach (VideoOutput videoOutput in VideoOutputs)
            {
                // Update the user.
                Progress.Report("Working: Assembling " + videoOutput.Filename);

                // Create the file list for this video.
                List<String> fileList = new List<string>();

                // Add all clip files.
                foreach(String clip in videoOutput.Clips)
                {
                    // Follow the specific file format.
                    String line = String.Format("file '{0}'", clip);

                    // Add the line to the list.
                    fileList.Add(line);
                }

                // Write the text file.
                File.WriteAllLines("filelist.txt", fileList);

                // Create the Process to call the external program.
                Process process = new Process();

                // Create the arguments string.
                String arguments = String.Format("-y -hide_banner -r {0:0.######} -f concat -safe 0 -i filelist.txt {1} \"..\\{2}\"",
                    FramesPerSecond,
                    OutputFinalSettings,
                    videoOutput.Filename);

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

                // Log the ffmpeg command line options.
                File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

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
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Runs the ffmpeg program with the specified commands to produce the output file.
        /// </summary>
        /// <param name="filename">The name of the output file without an extension.</param>
        /// <param name="command">The ffmpeg commands.</param>
        /// <param name="AddToVideo">Use true to add this video to VideoOutputs list of clips. Use false to not add this video to VideoOutputs.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RunFfmpeg(String filename, String command, Boolean AddToVideoOutputs = true)
        {
            bool retval = true;

            // Add the extension to the filename.
            filename += OutputVideoInterimExtension;

            // If this file is not in the list of created clips, create it.
            if (CreatedClipList.IndexOf(filename) == -1)
            {
                // Create the Process to call the external program.
                Process process = new Process();

                // Create the arguments string.
                String arguments = String.Format("-y -hide_banner {0} {1} \"{2}\"",
                    command,
                    OutputInterimSettings,
                    filename);

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

                // Log the ffmpeg command line options.
                File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

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
                    return false;
                }

                // Add the filename to the list of created clips.
                CreatedClipList.Add(filename);
            }

            // If requested, add the filename to the list of clips to use to create this video.
            if (AddToVideoOutputs == true)
            {
                VideoOutputs[VideoOutputIndex].Clips.Add(filename);
            }

            return retval;
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a fill video with forward and reverse bookmarks.
        /// </summary>
        private void AssembleClipListFnRFullVideo()
        {
            InitMeltString();

            // Add 1/2 second of black.
            AddBlack(0.5f);

            // Add the video offset.
            AddVideoOffset();

            // Add the opening card.
            AddOpeningCard();

            // Fade in to the opening frame from black.
            AddTransitionFromBlack("OpeningFrame");
            
            // Add the starting video clip.
            AddStartingVideoClip();

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // If there is a forward explanation.
                if (String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                {
                    // Add fade to black.
                    AddTransitionToBlack(forwardBookmark.Name + ".First");

                    // Add the forward explanation.
                    AddExplanationCard(forwardBookmark);

                    // Add fade from black.
                    AddTransitionFromBlack(forwardBookmark.Name + ".First");
                }

                // Add the forward video with text overlay.
                AddForwardVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (settings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame);

                            // Add the forward explanation.
                            AddExplanationCard(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = "Black";
                        }
                    }
                }

                // If the forward replay was requested, play it.
                if (settings.ReplayForwardVideo)
                {
                    // Transition to the first frame of this forward bookmark.
                    AddTransitionFromFrameToForwardTransition(forwardBookmark);

                    // Add the forward video again.
                    AddForwardVideo(forwardBookmark: forwardBookmark, HideTextOverlay: true);

                    // Prepare for the next transition.
                    TransitionFromFrame = forwardBookmark.Name + ".Last";
                }

                //
                // All reversals for this forward forwardBookmark have been added. 
                //

                // Transition to normal video, unless replaying the forward video.
                if (settings.ReplayForwardVideo == false)
                {
                    AddTransitionToNormalVideo(forwardBookmark.Name + ".Last");
                }

                // Add the normal video between forward bookmarks.
                AddNormalVideo(ForwardBookmarks, i);
            }

            // Add fade to black.
            AddTransitionToBlack(TransitionFromFrame);
            
            // Add the closing card.
            AddClosingCard();

            // Add 1/2 second of black.
            AddBlack(0.5f);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a video with forward and reverse bookmarks only.
        /// </summary>
        private void AssembleClipListFnRBookmarksOnly()
        {
            InitMeltString();

            // Add 1/2 second of black.
            AddBlack(0.5f);

            // Add the video offset.
            AddVideoOffset();

            // Add the opening card.
            AddOpeningCard();

            // Fade in from black only if there isn't a forward explanation on the first forward bookmark.
            if (ForwardBookmarks.Count > 0 && String.IsNullOrWhiteSpace(ForwardBookmarks[0].Explanation) == true)
            {
                // Fade in to the opening frame from black.
                AddTransitionFromBlack(ForwardBookmarks[0].Name + ".First");
            }

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // If there is a forward explanation.
                if (String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                {
                    // Add fade to black if this is not the first forward bookmark.
                    if (i > 0)
                    {
                        AddTransitionToBlack(forwardBookmark.Name + ".First");
                    }

                    // Add the forward explanation.
                    AddExplanationCard(forwardBookmark);

                    // Add fade from black.
                    AddTransitionFromBlack(forwardBookmark.Name + ".First");
                }

                // Add the forward video with text overlay.
                AddForwardVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (settings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame);

                            // Add the forward explanation.
                            AddExplanationCard(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = "Black";
                        }
                    }
                }

                // If the forward replay was requested, play it.
                if (settings.ReplayForwardVideo)
                {
                    // Transition to the first frame of this forward bookmark.
                    AddTransitionFromFrameToForwardTransition(forwardBookmark);

                    // Add the forward video again.
                    AddForwardVideo(forwardBookmark: forwardBookmark, HideTextOverlay: true);

                    // Prepare for the next transition.
                    TransitionFromFrame = forwardBookmark.Name + ".Last";
                }

                // Transition from the end of this reversal to the beginning of the next.
                AddTransitionToNextForward(i);
            }

            // Add fade to black.
            AddTransitionToBlack(TransitionFromFrame);

            // Add the closing card.
            AddClosingCard();

            // Add 1/2 second of black.
            AddBlack(0.5f);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt into separate videos, one for each reverse bookmark.
        /// </summary>
        private void AssembleClipListFnRSeparateVideos()
        {
            VideoOutputIndex = -1;

            // Set the path to the source video file.
            RelativePathToWorkingInputVideoFile = Path.GetFileName(WorkingInputVideoFile);

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Only make a videos if there is a reverse bookmarks in this forward bookmark.
                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    // Loop through all reverse bookmarks.
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Sanitize the bookmark text since it will be used as a filename.
                        String sanitizedText = Regex.Replace(reverseBookmark.Text, @"[/:*\?<>\|*$=]", "");  // Remove characters not allowed in filenames.
                        sanitizedText = (sanitizedText.IndexOf('\r') > -1) ? sanitizedText.Substring(sanitizedText.IndexOf('\r')) : sanitizedText;
                        sanitizedText = (sanitizedText.IndexOf('\n') > -1) ? sanitizedText.Substring(sanitizedText.IndexOf('\n')) : sanitizedText;

                        // Create the output video filename.
                        String outputVideoFilename = String.Format("{0}{1} {2}{3}",
                            textBoxOutputFile.Text,
                            reverseBookmark.Name,
                            sanitizedText,
                            OutputVideoFinalExtension);

                        // Creates the list of clips for this video.
                        VideoOutputs.Add(new VideoOutput(outputVideoFilename));

                        // Update the index.
                        ++VideoOutputIndex;

                        // Update the user.
                        Progress.Report(String.Format("Working: Creating {0}", outputVideoFilename));

                        // Add 1/2 second of black.
                        AddBlack(0.5f);

                        // Add the video offset.
                        AddVideoOffset();

                        // If there is a forward explanation.
                        if (String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                        {
                            // Add the forward explanation.
                            AddExplanationCard(forwardBookmark);
                        }

                        // Add fade from black.
                        AddTransitionFromBlack(forwardBookmark.Name + ".First");

                        // Add the forward video with text overlay.
                        AddForwardVideo(forwardBookmark);

                        // Set the frame to transition from.
                        TransitionFromFrame = forwardBookmark.Name + ".Last";

                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (settings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame);

                            // Add the forward explanation.
                            AddExplanationCard(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = "Black";
                        }
                        // There was no reverse explanation, so a fade to black is in order.
                        else
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame);
                        }

                        // Add 1/2 second of black.
                        AddBlack(0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a fill video with forward and reverse bookmarks.
        /// </summary>
        private void AssembleClipListOrphanedReversalsFullVideo()
        {
            InitMeltString();

            // Add 1/2 second of black.
            AddBlack(0.5f);

            // Add the video offset.
            AddVideoOffset();

            // Add the starting video clip.
            AddStartingVideoClip();

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // Add the forward video with text overlay.
                AddForwardVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (settings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }
                    }
                }

                //
                // All reversals for this forward forwardBookmark have been added. 
                //

                // Transition to normal video.
                AddTransitionToNormalVideo(forwardBookmark.Name + ".Last");

                // Add the normal video between forward bookmarks.
                AddNormalVideo(ForwardBookmarks, i);
            }

            // Add 1/2 second of black.
            AddBlack(0.5f);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a video with forward and reverse bookmarks only.
        /// </summary>
        private void AssembleClipListOrphanedReversalBookmarksOnly()
        {
            InitMeltString();

            // Add 1/2 second of black.
            AddBlack(0.2f);

            // Add the video offset.
            AddVideoOffset();

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // Add the forward video with text overlay.
                AddForwardVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (settings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }
                    }
                }

                // Transition from the end of this reversal to the beginning of the next.
                AddTransitionToNextForward(i);
            }

            // Add 1/2 second of black.
            AddBlack(0.5f);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt into separate videos, one for each reverse bookmark.
        /// </summary>
        private void AssembleClipListOrphanedReversalsSeparateVideos()
        {
            VideoOutputIndex = -1;

            // Set the path to the source video file.
            RelativePathToWorkingInputVideoFile = Path.GetFileName(WorkingInputVideoFile);

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Only make a videos if there is a reverse bookmarks in this forward bookmark.
                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    // Loop through all reverse bookmarks.
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Sanitize the bookmark text since it will be used as a filename.
                        String sanitizedText = Regex.Replace(reverseBookmark.Text, @"[/:*\?<>\|*$=]", "");  // Remove characters not allowed in filenames.
                        int s = sanitizedText.IndexOf('\r');
                        sanitizedText = (sanitizedText.IndexOf('\r') > -1) ? sanitizedText.Substring(0, sanitizedText.IndexOf('\r')) : sanitizedText;
                        sanitizedText = (sanitizedText.IndexOf('\n') > -1) ? sanitizedText.Substring(0, sanitizedText.IndexOf('\n')) : sanitizedText;

                        // Create the output video filename.
                        String outputVideoFilename = String.Format("{0}{1} {2}{3}",
                            textBoxOutputFile.Text,
                            reverseBookmark.Name,
                            sanitizedText,
                            OutputVideoFinalExtension);

                        // Creates the list of clips for this video.
                        VideoOutputs.Add(new VideoOutput(outputVideoFilename));

                        // Update the index.
                        ++VideoOutputIndex;

                        // Update the user.
                        Progress.Report(String.Format("Working: Creating {0}", outputVideoFilename));

                        // Add 1/2 second of black.
                        AddBlack(0.5f);

                        // Add the video offset.
                        AddVideoOffset();

                        // Add the forward video with text overlay.
                        AddForwardVideo(forwardBookmark);

                        // Set the frame to transition from.
                        TransitionFromFrame = forwardBookmark.Name + ".Last";

                        // Add the four reversal rates.
                        if (settings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, settings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, settings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, settings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (settings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark);

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, settings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // Add 1/2 second of black.
                        AddBlack(0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the process for creating the MELT command line string.
        /// </summary>
        private void InitMeltString()
        {
            VideoOutputIndex = 0;   // Creating the first video.

            // Creates the list of clips for the first video.
            VideoOutputs.Add(new VideoOutput(textBoxOutputFile.Text));

            // Set the path to the source video file.
            RelativePathToWorkingInputVideoFile = Path.GetFileName(WorkingInputVideoFile);
        }

        /// <summary>
        /// Creates a video clip of the specified number of seconds of black.
        /// </summary>
        /// <remarks>
        /// The final output video is created with qmelt.exe. The qmelt.exe program won't output
        /// the correct video resolution unless the first frame or clip is at the correct video
        /// resolution.
        /// 
        /// This is the first clip added to the video. I create it from the first frame of the
        /// source video, make it black, and repeat that frame for the specified duration. This
        /// results in the correct video resolution in the output video.
        /// </remarks>
        /// <param name="duration">The length of time for the video of black.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddBlack(float duration)
        {
            // Create the filename for this video clip.
            String filename = String.Format("Black.{0:0.######}", duration);

            // Create the inner commands for ffmpeg. 
            String command = String.Format("-i \"{0}\" -f lavfi -i color=size={2}x{3}:c=black -loop 1 -t {1:0.######} -filter_complex \"[0:a]volume = 0.0[a];[0:v][1:v]overlay[v]\" -map [v] -map [a] -t {1:0.######}",
                RelativePathToWorkingInputVideoFile,
                duration,
                HorizontalResolution,
                VerticalResolution);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

            /// <summary>
            /// Adds the video offset.
            /// </summary>
            /// <returns></returns>
            private bool AddVideoOffset()
        {
            return true;
        }


        /// <summary>
        /// Adds the opening card, if there is one.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddOpeningCard()
        {
            bool retval = true;

            // If There is an opening card, add it.
            if (String.IsNullOrWhiteSpace(OpeningCard) == false)
            {
                // Create the filename for this video clip.
                String filename = "OpeningCard";

                // Calculate the length of time to display the card.
                float displayLength = (float)OpeningCard.Length / (float)ReadingCharactersPerSecond;

                // Create the inner commands for ffmpeg. 
                String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                    FramesPerSecond,
                    filename,
                    displayLength,
                    RelativePathToWorkingInputVideoFile);

                // Call ffmpeg.
                retval = RunFfmpeg(filename, command);
            }

            return retval;
        }

        /// <summary>
        /// Adds the closing card.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddClosingCard()
        {
            bool retval = true;

            // If There is a closing card, add it.
            if (String.IsNullOrWhiteSpace(ClosingCard) == false)
            {
                // Create the filename for this video clip.
                String filename = "ClosingCard";

                // Calculate the length of time to display the card.
                float displayLength = (float)ClosingCard.Length / (float)ReadingCharactersPerSecond;

                // Create the inner commands for ffmpeg. 
                String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                    FramesPerSecond,
                    filename,
                    displayLength,
                    RelativePathToWorkingInputVideoFile);

                // Call ffmpeg.
                retval = RunFfmpeg(filename, command);
            }

            return retval;
        }

        /// <summary>
        /// Adds the initial video clip before the first forward forwardBookmark.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddStartingVideoClip()
        {
            // Calculate the start time and duration.
            float startTime = 0.0f;
            float duration = ((float)ForwardBookmarks[0].SampleStart / (float)SampleRate);

            // Adjust for the audio delay.
            startTime += (float)settings.AudioDelay / 1000f;
            if (startTime < 0)
            {
                // If the audio delay is negative, reduce the duration by the audio delay.
                duration += startTime;
                startTime = 0;
            }

            // Create the filename for this clip.
            String filename = String.Format("v{0:0.######}-{1:0.######}", startTime, duration);

            // Create the inner commands for ffmpeg.
            String command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                startTime,
                RelativePathToWorkingInputVideoFile,
                duration);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        /// <summary>
        /// Adds the explanation card from the specified bookmark.
        /// </summary>
        /// <param name="bookmark">The forward bookmark.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddExplanationCard(Bookmark bookmark)
        {
            // Create the filename for this video clip.
            String filename = bookmark.Name + ".Explanation";

            // Calculate the length of time to display the card.
            float displayLength = (float)bookmark.Explanation.Length / (float)ReadingCharactersPerSecond;

            // Create the inner commands for ffmpeg. 
            String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                FramesPerSecond,
                filename,
                displayLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        /// <summary>
        /// Adds a transition from black to the specified image.
        /// </summary>
        /// <param name="ImageName">The image to fade to, without an extension.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionFromBlack(String ImageName)
        {
            // Set the video output filename.
            String filename = "Black-" + ImageName;

            // Calculate the length of time to display the card.
            float fadeLength = 1.0f;

            // Create the inner commands for ffmpeg.
            // -r {0}                       # Frame rate.
            // -loop 1 - i \"{1}.png\"      # Loop this input stream.
            // -i \"{3}\"                   # Use this input stream (will be used to create silent audio channel).
            // -filter_complex              # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                        # From the third input, select the audio channel.
            // volume=0.0,                  # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}  # Set the audio duration to {3}.
            // [a];                         # Name this stream "a" for audio.
            // [0:v]                        # From the first input, select the video channel.
            // fade=t=in:                   # Fade, type in (fade in).
            // d={2}                        # Duration of the fade.
            // [v]\"                        # Name this stream "v" for video.
            // -map \"[v]\"                 # Put the video stream into stream 0:0.
            // -map \"[a]\"                 # Put the audio stream into stream 0:1.
            // -t {2:0.######}              # Set the duration of this clip (may be redundant since the audio duration and fade duration are identical).
            String command = String.Format("-r {0} -loop 1 -i \"{1}.png\" -i \"{3}\" -filter_complex \"[1:a]volume=0.0,atrim=duration={2:0.######}[a];[0:v]fade=t=in:d={2:0.######}[v]\" -map \"[v]\" -map \"[a]\" -t {2:0.######}",
                FramesPerSecond,
                ImageName,
                fadeLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        /// <summary>
        /// Adds a transition from the specified image to black.
        /// </summary>
        /// <param name="ImageName">The image to fade to, without an extension.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionToBlack(String ImageName)
        {
            // Set the video output filename.
            String filename = ImageName + "-Black";

            // Calculate the length of time to display the card.
            float fadeLength = 1.0f;

            // Create the inner commands for ffmpeg.
            // -r {0}                       # Frame rate.
            // -loop 1 - i \"{1}.png\"      # Loop this input stream.
            // -i \"{3}\"                   # Use this input stream (will be used to create silent audio channel).
            // -filter_complex              # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                        # From the third input, select the audio channel.
            // volume=0.0,                  # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}  # Set the audio duration to {3}.
            // [a];                         # Name this stream "a" for audio.
            // [0:v]                        # From the first input, select the video channel.
            // fade=t=out:                  # Fade, type out (fade out).
            // d={2}                        # Duration of the fade.
            // [v]\"                        # Name this stream "v" for video.
            // -map \"[v]\"                 # Put the video stream into stream 0:0.
            // -map \"[a]\"                 # Put the audio stream into stream 0:1.
            // -t {2:0.######}              # Set the duration of this clip (may be redundant since the audio duration and fade duration are identical).
            String command = String.Format("-r {0} -loop 1 -i \"{1}.png\" -i \"{3}\" -t {2:0.######} -filter_complex \"[1:a]volume=0.0,atrim=duration={2:0.######}[a];[0:v]fade=t=out:d={2:0.######}[v]\" -map \"[v]\" -map \"[a]\" -t {2:0.######}",
                FramesPerSecond,
                ImageName,
                fadeLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        /// <summary>
        /// Adds the specified forward video clip with the forward text overlay image added.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddForwardVideo(Bookmark forwardBookmark, bool HideTextOverlay = false)
        {
            bool retval = true;
            //
            // Extract the forward clip from the source video.
            //

            // Calculate the start time and duration.
            float startTime = (float)forwardBookmark.SampleStart / (float)SampleRate;
            float duration = ((float)forwardBookmark.SampleEnd / (float)SampleRate) - startTime;

            // Adjust for the audio delay.
            startTime += (float)settings.AudioDelay / 1000f;
            startTime = startTime < 0 ? 0 : startTime;

            // Create the filename for this clip.
            String filename1 = String.Format("v{0:0.######}-{1:0.######}", startTime, duration);

            // Create the inner commands for ffmpeg.
            String command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                startTime,
                RelativePathToWorkingInputVideoFile,
                duration);

            // Call ffmpeg. Since this clip does not have the text overlay, do not add this to the list of clips to assemble.
            retval = RunFfmpeg(filename: filename1, command: command, AddToVideoOutputs: HideTextOverlay);

            // Return if there was an error.
            if (retval == false)
            {
                return false;
            }

            // If we are not displaying the text overlay (when replaying the forward video), return.
            if (HideTextOverlay)
            {
                return true;
            }

            //
            // Overlay the forward text overlay image.
            //

            // Create the filename for this clip.
            String filename2 = forwardBookmark.Name;

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -t {3} -filter_complex \"[0:v][1:v]overlay\"",
                filename1,
                OutputVideoInterimExtension,
                forwardBookmark.Name,
                duration);

            // Call ffmpeg.
            return RunFfmpeg(filename: filename2, command: command, AddToVideoOutputs: true);
        }

        /// <summary>
        /// Creates a transition with text overlay from TransitionFromFrame to the first frame of the reverse clip.
        /// </summary>
        /// <param name="reverseBookmark">The reverse bookmark of this transition.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionFromFrameToReverseTransition(Bookmark reverseBookmark)
        {
            bool retval = true;

            //
            // Create the transition clip.
            //
            
            // TransitionFromFrame is already set. Now set the TransitionToFrame.
            TransitionToFrame = reverseBookmark.Name + ".First";

            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(TransitionToFrame);
            }
            
            // Set the video output filename.
            String filename1 = TransitionFromFrame + "-" + TransitionToFrame;

            // Calculate the length of time to display the card.
            float transitionLength = 1.0f;

            // Create the inner commands for ffmpeg.
            // -r {0}                                   # Frame rate.
            // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
            // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
            // -i \"{3}\"                               # Use this input stream (will be used to create silent audio channel).
            // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                                    # From the third input, select the audio channel.
            // volume=0.0,                              # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}              # Set the audio duration to {3}.
            // [a];                                     # Name this stream "a" for audio.
            // [1]format=yuva444p,                      # Format the first image as yuva444p.
            // fade=d={3:0.######}:                     # Set the fade duration.
            // t=in:                                    # Set the fade type to "fade in".
            // alpha=1,                                 # Fade only the alpha channel.
            // setpts=                                  # Set the presentation timestamp
            // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
            //                                          #   STARTPTS is the PTS of the first frame.
            //                                          #   TB is the timebase of the input timestamps.
            // [f0];                                    # Name this stream "f0".
            // [0][f0]overlay,                          # Overlay stream 0 and f0.
            // format=yuv420p                           # Format the overlayed video as yuv420p
            // [v]\"                                    # Name the overlayed video "v" for video.
            // -map \"[v]\"                             # Put the video stream into stream 0:0.
            // -map \"[a]\"                             # Put the audio stream into stream 0:1.
            String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\" -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                FramesPerSecond,
                TransitionFromFrame,
                TransitionToFrame,
                transitionLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            retval = RunFfmpeg(filename: filename1, command: command, AddToVideoOutputs: false);

            // Return if there was an error.
            if (retval == false)
            {
                return false;
            }

            //
            // Overlay the reverse text overlay image.
            //

            // Create the filename for this clip.
            String filename2 = filename1 + ".Text";

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                filename1,
                OutputVideoInterimExtension,
                reverseBookmark.Name);

            // Call ffmpeg.
            return RunFfmpeg(filename: filename2, command: command, AddToVideoOutputs: true);
        }

        /// <summary>
        /// Adds a transition from TransitionFromFrame to the first frame of the forward bookmark.
        /// </summary>
        /// <param name="forwardBookmark">The forward bookmark.</param>
        /// <returns></returns>
        private bool AddTransitionFromFrameToForwardTransition(Bookmark forwardBookmark)
        {
            bool retval = true;

            //
            // Create the transition clip.
            //

            // TransitionFromFrame is already set. Now set the TransitionToFrame.
            TransitionToFrame = forwardBookmark.Name + ".First";

            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(TransitionToFrame);
            }

            // Set the video output filename.
            String filename1 = TransitionFromFrame + "-" + TransitionToFrame;

            // Calculate the length of time to display the card.
            float transitionLength = 1.0f;

            // Create the inner commands for ffmpeg.
            // -r {0}                                   # Frame rate.
            // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
            // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
            // -i \"{3}\"                               # Use this input stream (will be used to create silent audio channel).
            // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                                    # From the third input, select the audio channel.
            // volume=0.0,                              # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}              # Set the audio duration to {3}.
            // [a];                                     # Name this stream "a" for audio.
            // [1]format=yuva444p,                      # Format the first image as yuva444p.
            // fade=d={3:0.######}:                     # Set the fade duration.
            // t=in:                                    # Set the fade type to "fade in".
            // alpha=1,                                 # Fade only the alpha channel.
            // setpts=                                  # Set the presentation timestamp
            // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
            //                                          #   STARTPTS is the PTS of the first frame.
            //                                          #   TB is the timebase of the input timestamps.
            // [f0];                                    # Name this stream "f0".
            // [0][f0]overlay,                          # Overlay stream 0 and f0.
            // format=yuv420p                           # Format the overlayed video as yuv420p
            // [v]\"                                    # Name the overlayed video "v" for video.
            // -map \"[v]\"                             # Put the video stream into stream 0:0.
            // -map \"[a]\"                             # Put the audio stream into stream 0:1.
            String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\" -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                FramesPerSecond,
                TransitionFromFrame,
                TransitionToFrame,
                transitionLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            retval = RunFfmpeg(filename: filename1, command: command, AddToVideoOutputs: false);

            // Return if there was an error.
            if (retval == false)
            {
                return false;
            }

            //
            // Overlay the reverse text overlay image.
            //

            // Create the filename for this clip.
            String filename2 = filename1 + ".Text";

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                filename1,
                OutputVideoInterimExtension,
                forwardBookmark.Name);

            // Call ffmpeg.
            return RunFfmpeg(filename: filename2, command: command, AddToVideoOutputs: true);
        }

        /// <summary>
        /// Overlays the reverse bookmark text image over the specified reverse video.
        /// </summary>
        /// <param name="reverseBookmark">The reverse bookmark source this reverse video.</param>
        /// <param name="reversalNumber">The reversal number (ie., 100% would be 1, 85% would be 2, etc).</param>
        /// <param name="reversalRate">The reversal speed and tone.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddReverseVideo(Bookmark reverseBookmark, int reversalNumber, ReversalRate reversalRate)
        {
            // Find the specific reverse video clip.
            String reverseVideoFilename;
            if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
            {
                reverseVideoFilename = String.Format("{0}.{1}.{2}", 
                    reverseBookmark.Name, 
                    reversalNumber, 
                    reversalRate.ReversalSpeed);
            }
            else
            {
                reverseVideoFilename = String.Format("{0}.{1}.{2}-{3}", 
                    reverseBookmark.Name, 
                    reversalNumber, 
                    reversalRate.ReversalSpeed, 
                    reversalRate.ReversalTone);
            }

            // Create the filename for this clip.
            String filename = reverseVideoFilename + ".Text";

            // Create the inner commands for ffmpeg.
            String command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                reverseVideoFilename,
                OutputVideoInterimExtension,
                reverseBookmark.Name);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        private bool AddBackAndForthTransition(Bookmark reverseBookmark)
        {
            bool retval = true;

            //
            // Create the transition clip.
            //

            // Create the filename for this video clip.
            String filename1 = TransitionFromFrame;

            // Calculate the length of time to display the card.
            float displayLength = 1.0f;

            // Create the inner commands for ffmpeg. 
            String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######} ",
                FramesPerSecond,
                filename1,
                displayLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            retval = RunFfmpeg(filename: filename1, command: command, AddToVideoOutputs: false);

            // Return if there was an error.
            if (retval == false)
            {
                return false;
            }

            //
            // Overlay the reverse text overlay image.
            //

            // Create the filename for this clip.
            String filename2 = filename1 + ".Text";

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                filename1,
                OutputVideoInterimExtension,
                reverseBookmark.Name);

            // Call ffmpeg.
            return RunFfmpeg(filename: filename2, command: command, AddToVideoOutputs: true);
        }

        private bool AddBackAndForth(Bookmark reverseBookmark)
        {
            // Find the first selected reverse video.
            ReversalRate reversalRate;
            int reversalNumber;
            if (settings.ReversalRate1.UseThisRate)
            {
                reversalRate = settings.ReversalRate1;
                reversalNumber = 1;
            }
            else if (settings.ReversalRate2.UseThisRate)
            {
                reversalRate = settings.ReversalRate2;
                reversalNumber = 2;
            }
            else if (settings.ReversalRate3.UseThisRate)
            {
                reversalRate = settings.ReversalRate3;
                reversalNumber = 3;
            }
            else if (settings.ReversalRate4.UseThisRate)
            {
                reversalRate = settings.ReversalRate4;
                reversalNumber = 4;
            }
            else
            {
                return true;
            }

            bool retval = true;

            //
            // Extract the forward clip from the source video.
            //

            // Calculate the start time and duration.
            float startTime = (float)reverseBookmark.SampleStart / (float)SampleRate;
            float duration = ((float)reverseBookmark.SampleEnd / (float)SampleRate) - startTime;

            // Adjust for the audio delay.
            startTime += (float)settings.AudioDelay / 1000f;
            startTime = startTime < 0 ? 0 : startTime;

            // Create the filename for this clip.
            String filename1 = String.Format("v{0:0.######}-{1:0.######}", startTime, duration);

            // Create the inner commands for ffmpeg.
            String command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                startTime,
                RelativePathToWorkingInputVideoFile,
                duration);

            // Call ffmpeg. Since this clip does not have the text overlay, do not add this to the list of clips to assemble.
            retval = RunFfmpeg(filename: filename1, command: command, AddToVideoOutputs: false);

            // Return if there was an error.
            if (retval == false)
            {
                return false;
            }

            //
            // Overlay the forward text overlay image.
            //

            // Create the filename for this clip.
            String filename2 = reverseBookmark.Name + ".Forward.Text";

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                filename1,
                OutputVideoInterimExtension,
                reverseBookmark.Name);

            // Call ffmpeg.
            retval = RunFfmpeg(filename: filename2, command: command, AddToVideoOutputs: true);

            // Add the reverse clip.
            return AddReverseVideo(reverseBookmark, reversalNumber, reversalRate);
        }

        /// <summary>
        /// Creates a transition from TransitionFromFrame to transitionToFrame.
        /// </summary>
        /// <param name="transitionToFrame">The frame to transition to.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionToNormalVideo(String transitionToFrame)
        {
            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(transitionToFrame);
            }

            // Set the video output filename.
            String filename = TransitionFromFrame + "-" + transitionToFrame;

            // Calculate the length of time to display the card.
            float transitionLength = 1.0f;

            // Create the inner commands for ffmpeg.
            // -r {0}                                   # Frame rate.
            // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
            // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
            // -i \"{4}\"                               # Use this input stream (will be used to create silent audio channel).
            // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                                    # From the third input, select the audio channel.
            // volume=0.0,                              # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}              # Set the audio duration to {3}.
            // [a];                                     # Name this stream "a" for audio.
            // [1]format=yuva444p,                      # Format the first image as yuva444p.
            // fade=d={3:0.######}:                     # Set the fade duration.
            // t=in:                                    # Set the fade type to "fade in".
            // alpha=1,                                 # Fade only the alpha channel.
            // setpts=                                  # Set the presentation timestamp
            // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
            //                                          #   STARTPTS is the PTS of the first frame.
            //                                          #   TB is the timebase of the input timestamps.
            // [f0];                                    # Name this stream "f0".
            // [0][f0]overlay,                          # Overlay stream 0 and f0.
            // format=yuv420p                           # Format the overlayed video as yuv420p
            // [v]\"                                    # Name the overlayed video "v" for video.
            // -map \"[v]\"                             # Put the video stream into stream 0:0.
            // -map \"[a]\"                             # Put the audio stream into stream 0:1.
            String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\" -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                FramesPerSecond,
                TransitionFromFrame,
                transitionToFrame,
                transitionLength,
                RelativePathToWorkingInputVideoFile);

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }

        private bool AddTransitionToNextForward(int index)
        {
            bool retval = true;

            // If this is not the last bookmark
            if (index < ForwardBookmarks.Count - 1)
            {
                // Determine the transition to frame.
                TransitionToFrame = ForwardBookmarks[index + 1].Name + ".First";

                // If we are transitioning from black.
                if (TransitionFromFrame == "Black")
                {
                    // Use the method that performs this function.
                    return AddTransitionFromBlack(TransitionToFrame);
                }
                
                // Set the video output filename.
                String filename = TransitionFromFrame + "-" + TransitionToFrame;

                // Calculate the length of time to display the card.
                float transitionLength = 1.0f;

                // Create the inner commands for ffmpeg.
                // -r {0}                                   # Frame rate.
                // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
                // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
                // -i \"{4}\"                               # Use this input stream (will be used to create silent audio channel).
                // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
                // [2:a]                                    # From the third input, select the audio channel.
                // volume=0.0,                              # Multiply the audio value by 0 (silence).
                // atrim=duration={2:0.######}              # Set the audio duration to {3}.
                // [a];                                     # Name this stream "a" for audio.
                // [1]format=yuva444p,                      # Format the first image as yuva444p.
                // fade=d={3:0.######}:                     # Set the fade duration.
                // t=in:                                    # Set the fade type to "fade in".
                // alpha=1,                                 # Fade only the alpha channel.
                // setpts=                                  # Set the presentation timestamp
                // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
                //                                          #   STARTPTS is the PTS of the first frame.
                //                                          #   TB is the timebase of the input timestamps.
                // [f0];                                    # Name this stream "f0".
                // [0][f0]overlay,                          # Overlay stream 0 and f0.
                // format=yuv420p                           # Format the overlayed video as yuv420p
                // [v]\"                                    # Name the overlayed video "v" for video.
                // -map \"[v]\"                             # Put the video stream into stream 0:0.
                // -map \"[a]\"                             # Put the audio stream into stream 0:1.
                String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\"  -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                    FramesPerSecond,
                    TransitionFromFrame,
                    TransitionToFrame,
                    transitionLength,
                    RelativePathToWorkingInputVideoFile);

                // Call ffmpeg.
                retval = RunFfmpeg(filename, command);
            }

            return retval;
        }

        private bool AddNormalVideo(List<Bookmark> ForwardBookmarks, int index)
        {
            String command;
            String filename;

            // Calculate the end time.
            float endTime = (float)ForwardBookmarks[index].SampleEnd / (float)SampleRate;
            float startTimeOfNextBookmark = 864000.0f;

            // Adjust for the audio delay.
            endTime += (float)settings.AudioDelay / 1000f;
            endTime = endTime < 0 ? 0 : endTime;

            // If this is not the last forward bookmark.
            if (index < ForwardBookmarks.Count - 1)
            {
                // Calculate the next start time.
                startTimeOfNextBookmark = ForwardBookmarks[index + 1].SampleStart / (float)SampleRate;

                // Adjust for the audio delay.
                startTimeOfNextBookmark += (float)settings.AudioDelay / 1000f;
                startTimeOfNextBookmark = startTimeOfNextBookmark < 0 ? 0 : startTimeOfNextBookmark;

                // If the end time of this bookmark is beyond the start time of the next bookmark, this clip is not necessary.
                if (endTime >= startTimeOfNextBookmark)
                {
                    return true;
                }

                // Calculate the duration.
                float duration = startTimeOfNextBookmark - (((float)ForwardBookmarks[index].SampleEnd / (float)SampleRate) + (float)settings.AudioDelay / 1000f);

                // Create the inner commands for ffmpeg.
                command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                    endTime,
                    RelativePathToWorkingInputVideoFile,
                    duration);

                // Create the filename for the video that comes after this clip. The value of endTime is the end of this bookmark.
                filename = String.Format("v{0:0.######}-{1:0.######}", endTime, duration);
            }
            // This is the last forward bookmark.
            else
            {
                // There is no need to add a time, since we are taking the video file from endTime of this bookmark to the end.
                command = String.Format("-ss {0:0.######} -i \"{1}\"",
                endTime,
                RelativePathToWorkingInputVideoFile);

                // Create the filename for the video that comes after this clip. The value of endTime is the end of this bookmark.
                filename = String.Format("v{0:0.######}-End", endTime);
            }

            // Call ffmpeg.
            return RunFfmpeg(filename, command);
        }
    }
}