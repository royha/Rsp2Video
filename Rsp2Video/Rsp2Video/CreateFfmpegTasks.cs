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
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Serialization;

namespace RSPro2Video
{
    // CreateTaskList might be a better name for this method.
    public partial class RSPro2VideoForm : Form
    {
        /// <summary>
        /// Creates the ffmpeg tasks that will generate the video clips.
        /// </summary>
        private void CreateFfmpegTasks()
        {
            InitMeltString();

            // Create all of the reversal tasks.
            CreateAllReverseVideoTasks();

            // If Forward and Reverse selected.
            if (ProjectSettings.BookmarkTypeFnR)
            {
                // Select the video contents for the output video.
                switch (ProjectSettings.VideoContents)
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
            if (ProjectSettings.BookmarkTypeOrphanedReversals)
            {
                // Select the video contents for the output video.
                switch (ProjectSettings.VideoContents)
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
            if (ProjectSettings.BookmarkTypeQuickCheck)
            {
                // Create the video from forward and reverse bookmarks only.
                AssembleClipListOrphanedReversalBookmarksOnly();
            }
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

                // Add all clipEntry files.
                bool firstFile = true;
                foreach (ClipEntry clipEntry in videoOutput.Clips)
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
                    // Add the clipEntry to the list.
                    fileList.Append('"');
                    fileList.Append(clipEntry.ClipFilename);
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
        //private bool AssembleMeltVideo()
        //{
        //    foreach (VideoOutput videoOutput in VideoOutputs)
        //    {
        //        // Update the user.
        //        Progress.Report("Working: Assembling " + videoOutput.Filenames);

        //        // Create the file list for this video.
        //        StringBuilder fileList = new StringBuilder();

        //        // Add all clipEntry files.
        //        foreach (String clipEntry in videoOutput.Clips)
        //        {
        //            // Add the clipEntry to the list.
        //            fileList.Append("file '");
        //            fileList.Append(clipEntry);
        //            fileList.Append("'");
        //            fileList.Append(Environment.NewLine);
        //        }

        //        // Create the Process to call the external program.
        //        Process process = new Process();

        //        // Create the arguments string.
        //        String arguments = String.Format("-consumer avformat:\"..\\{0}\" {1}{2}",
        //            videoOutput.Filenames,
        //            OutputFinalSettingsQmelt,
        //            fileList.ToString());

        //        // Configure the process using the StartInfo properties.
        //        process.StartInfo = new ProcessStartInfo
        //        {
        //            FileName = QmeltApp,
        //            Arguments = arguments,
        //            UseShellExecute = false,
        //            RedirectStandardError = true,
        //            CreateNoWindow = true,
        //            WindowStyle = ProcessWindowStyle.Maximized
        //        };

        //        // Log the ffmpeg command line options.
        //        File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

        //        // Start ffmpeg to extract the frames.
        //        process.Start();

        //        // Read the output of ffmpeg.
        //        String FfmpegOutput = process.StandardError.ReadToEnd();

        //        // Log the ffmpeg output.
        //        File.AppendAllText(LogFile, FfmpegOutput);

        //        // Wait here for the process to exit.
        //        process.WaitForExit();
        //        int ExitCode = process.ExitCode;
        //        process.Close();

        //        // Return success or failure.
        //        if (!(ExitCode == 0))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

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

                // Add all clipEntry files.
                foreach(ClipEntry clipEntry in videoOutput.Clips)
                {
                    // Follow the specific file format.
                    String line = String.Format("file '{0}'", clipEntry.ClipFilename);

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
        /// Stores the FFmpeg command as an FFmpegTask.
        /// </summary>
        /// <param name="Filename">The name of the output file without an extension.</param>
        /// <param name="FfmpegCommand">The ffmpeg commands.</param>
        /// <param name="SortOrder">The sort order for this ffmpeg task.</param>
        /// <param name="EstimatedDuration">The best guess for the clipEntry duration (for sorting purposes).</param>
        /// <param name="AddToVideoOutputs">Use true to add this video to VideoOutputs list of clips. Use false to not add this video to VideoOutputs.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateFfmpegTask(String Filename,
            String FfmpegCommand, FfmpegTaskSortOrder SortOrder, double EstimatedDuration, Boolean AddToVideoOutputs = true)
        {
            // Add the file to the list of clips to create.
            Boolean createFFmpegTask = AddToClips(Filename, EstimatedDuration, AddToVideoOutputs);

            // Add the task to the list of tasks
            if (createFFmpegTask)
            {
                FFmpegTasks.Add(new FFmpegTask(SortOrder, EstimatedDuration, Filename, FfmpegCommand));
            }

            return true;
        }

        /// <summary>
        /// Optionally adds the -progress strings. Adds -threads, OutputInterimSettings, and the output filename.
        /// </summary>
        /// <param name="BaseFfmpegCommand">The base ffmpeg command string.</param>
        /// <param name="Filename">The output filename.</param>
        /// <param name="AddProgress">Optional. Pass true to add the -progress string.
        /// Pass false to not add the -progress string. Default is true.</param>
        /// <returns>The updated ffmpeg command string.</returns>
        public String AddFfmpegOutputStrings(String BaseFfmpegCommand, String Filename, Boolean AddProgress = true)
        {
            StringBuilder newCommand = new StringBuilder();

            // Make sure overwrite file is yes, and hide the ffmpeg banner
            newCommand.Append("-y -hide_banner ");

            // Add the base command.
            newCommand.Append(BaseFfmpegCommand);

            // Add the progress parameters if requested.
            if (AddProgress)
            {
                newCommand.Append($" -progress \"{Filename}.progress\"");
            }

            // Add the rest of the ffmpeg commands.
            newCommand.Append($" -threads {{0}} {OutputInterimSettings} "
                + $"\"{Filename}{OutputVideoInterimExtension}\"");

            // Return completed string.
            return newCommand.ToString();
        }

        /// <summary>
        /// Stores the FFmpeg command as an FFmpegTask.
        /// </summary>
        /// <param name="Filenames">The name of the output file without an extension.</param>
        /// <param name="FfmpegCommands">The ffmpeg commands.</param>
        /// <param name="SortOrder">The sort order for this ffmpeg task.</param>
        /// <param name="EstimatedDuration">The best guess for the clipEntry duration (for sorting purposes).</param>
        /// <param name="AddToVideoOutputs">Use true to add this video to VideoOutputs list of clips. Use false to not add this video to VideoOutputs.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateFfmpegTask(List<String> Filenames,
            List<String> FfmpegCommands, FfmpegTaskSortOrder SortOrder, double EstimatedDuration, Boolean AddToVideoOutputs = true)
        {
            // If true, a FFmpegTask should be created for this clip.
            Boolean createFFmpegTask = true;

            // Add the file to the list of clips to create.
            if (SortOrder == FfmpegTaskSortOrder.ReverseVideoPass2)
            {
                // For reverse videos, Until I implement the .png reversal method (Filenames[1] and Filenames[2]), take only the first filename.
                createFFmpegTask = AddToClips(Filenames[0], EstimatedDuration, AddToVideoOutputs);
            }
            else if (SortOrder == FfmpegTaskSortOrder.TransitionVideo || SortOrder == FfmpegTaskSortOrder.ForwardBookmarkVideo)
            {
                // For transition videos that come to this overload (ie., they have an image overlay), use the [1]th filename.
                createFFmpegTask = AddToClips(Filenames[1], EstimatedDuration, AddToVideoOutputs);
            }
            else
            {
                // Put every filename into the list of clips to create.
                foreach (string filename in Filenames)
                {
                    Boolean retval = AddToClips(filename, EstimatedDuration, AddToVideoOutputs);

                    // If any of the files is a duplicate, assume they all are duplicates (or they will envetually create a duplicate).
                    if (retval == false)
                    {
                        createFFmpegTask = false;
                    }
                }
            }

            // If appropriate, add the task to the list of tasks.
            if (createFFmpegTask)
            {
                // Add the task to the list of tasks
                FFmpegTasks.Add(new FFmpegTask(SortOrder, EstimatedDuration, Filenames, FfmpegCommands));
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Filename">The name of the output file without an extension.</param>
        /// <param name="EstimatedDuration">The best guess for the clipEntry duration (for sorting purposes).</param>
        /// <param name="AddToVideoOutputs">Use true to add this video to VideoOutputs list of clips. Use false to not add this video to VideoOutputs.</param>
        /// <returns>Retuns true if the filename is new/unique and needs an FFmpegTask to create it; returns false 
        /// if this file will be a duplicate.</returns>
        private Boolean AddToClips (String Filename, double EstimatedDuration = 0.0d, Boolean AddToVideoOutputs = true)
        {
            Boolean retval = true;

            // Add the extension to the filename.
            String FilenameWithExtension = Filename + OutputVideoInterimExtension;

            // If this file is not in the list of clips to create, add it.
            if (!ClipsToCreate.ContainsKey(FilenameWithExtension))
            {
                // Add the filename and estimated duration to the list of created clips.
                ClipsToCreate.Add(FilenameWithExtension, EstimatedDuration);

                // Declare that this video needs an FFmpegTask.
                retval = true;
            }
            else
            {
                // Declare that this video does not need an FFmpegTask.
                retval = false;

                // Create the new filename.
                String newFilename;

                // If this file is not in the list of clips to duplicate, add it.
                if (!ClipsToDuplicate.ContainsKey(FilenameWithExtension))
                {
                    // Create the new filename.
                    newFilename = $"{Filename}(0){OutputVideoInterimExtension}";

                    // Add the new filename to the new list.
                    ClipsToDuplicate.Add(FilenameWithExtension, 
                        new List<string> { newFilename });
                }
                else
                {
                    // Create the new filename.
                    newFilename = $"{Filename}({ClipsToDuplicate[FilenameWithExtension].Count}){OutputVideoInterimExtension}";

                    // Add the new filename to the existing list.
                    ClipsToDuplicate[FilenameWithExtension].Add(newFilename);
                }

                // Store the new filename in VideoOutputs and FFmpegTasks.
                FilenameWithExtension = newFilename;
            }

            // If requested, add the filename to the list of clips to use to create this video.
            if (AddToVideoOutputs == true)
            {
                VideoOutputs[VideoOutputIndex].Clips.Add(new ClipEntry(FilenameWithExtension));
            }

            return retval;
        }

        /// <summary>
        /// Creates the clips for a full video with forward and reverse bookmarks.
        /// </summary>
        private void AssembleClipListFnRFullVideo()
        {
            // Add 1/2 second of black.
            AddBlack(0.5d);

            // Add the opening card.
            AddOpeningCard();

            // Fade in to the opening frame from black.
            AddTransitionFromBlack("v.First", ProjectSettings.TransitionLengthCard);
            
            // Add the starting video clipEntry.
            AddStartingVideoClip();

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // If there is a forward explanation.
                if (ProjectSettings.IncludeForwardExplanations && String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                {
                    // Add fade to black.
                    AddTransitionToBlack(forwardBookmark.Name + ".First", ProjectSettings.TransitionLengthCard);

                    // Add the forward explanation.
                    AddExplanationCard(forwardBookmark);

                    // Add fade from black.
                    AddTransitionFromBlack(forwardBookmark.Name + ".First", ProjectSettings.TransitionLengthCard);
                }

                // Add the forward video with text overlay.
                AddForwardBookmarkVideo(forwardBookmark);

                // Set the frame to transition from and initial transition length.
                TransitionFromFrame = forwardBookmark.Name + ".Last";
                double TransitionLength = ProjectSettings.TransitionLengthMajor;

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        String firstTransitionFromFrame = null;
                        String lastTransitionFromFrame = null;

                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = 
                                (ProjectSettings.ReversalRate1.ReversalSpeed == ProjectSettings.ReversalRate1.ReversalTone) ?
                                $"{reverseBookmark.Name}.1.{ProjectSettings.ReversalRate1.ReversalSpeed}.Last" :
                                $"{reverseBookmark.Name}.1.{ProjectSettings.ReversalRate1.ReversalSpeed}-"
                                + $"{ProjectSettings.ReversalRate1.ReversalTone}.Last";

                            if (firstTransitionFromFrame == null)
                            {
                                firstTransitionFromFrame = TransitionFromFrame;
                            }

                            lastTransitionFromFrame = TransitionFromFrame;
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame =
                                (ProjectSettings.ReversalRate2.ReversalSpeed == ProjectSettings.ReversalRate2.ReversalTone) ?
                                $"{reverseBookmark.Name}.2.{ProjectSettings.ReversalRate2.ReversalSpeed}.Last" :
                                $"{reverseBookmark.Name}.2.{ProjectSettings.ReversalRate2.ReversalSpeed}-"
                                + $"{ProjectSettings.ReversalRate2.ReversalTone}.Last";

                            if (firstTransitionFromFrame == null)
                            {
                                firstTransitionFromFrame = TransitionFromFrame;
                            }

                            lastTransitionFromFrame = TransitionFromFrame;
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame =
                                (ProjectSettings.ReversalRate3.ReversalSpeed == ProjectSettings.ReversalRate3.ReversalTone) ?
                                $"{reverseBookmark.Name}.3.{ProjectSettings.ReversalRate3.ReversalSpeed}.Last" :
                                $"{reverseBookmark.Name}.3.{ProjectSettings.ReversalRate3.ReversalSpeed}-"
                                + $"{ProjectSettings.ReversalRate3.ReversalTone}.Last";

                            if (firstTransitionFromFrame == null)
                            {
                                firstTransitionFromFrame = TransitionFromFrame;
                            }

                            lastTransitionFromFrame = TransitionFromFrame;
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 4, ProjectSettings.ReversalRate4, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame =
                                (ProjectSettings.ReversalRate4.ReversalSpeed == ProjectSettings.ReversalRate4.ReversalTone) ?
                                $"{reverseBookmark.Name}.4.{ProjectSettings.ReversalRate4.ReversalSpeed}.Last" :
                                $"{reverseBookmark.Name}.4.{ProjectSettings.ReversalRate4.ReversalSpeed}-"
                                + $"{ProjectSettings.ReversalRate4.ReversalTone}.Last";

                            if (firstTransitionFromFrame == null)
                            {
                                firstTransitionFromFrame = TransitionFromFrame;
                            }

                            lastTransitionFromFrame = TransitionFromFrame;
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = firstTransitionFromFrame;
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations)
                        {
                            if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                            {
                                // Add fade to black.
                                AddTransitionToBlack(lastTransitionFromFrame, ProjectSettings.TransitionLengthCard);

                                // Add the forward explanation.
                                AddExplanationCard(reverseBookmark);

                                // Prepare for the next transition.
                                TransitionFromFrame = "Black";
                                lastTransitionFromFrame = TransitionFromFrame;
                            }
                        }

                        // Reset the transition length for the next reversal set.
                        TransitionLength = ProjectSettings.TransitionLengthMajor;
                    }
                }

                // If the forward replay was requested, play it.
                //if (ProjectSettings.ReplayForwardVideo)
                //{
                //    // Transition to the first frame of this forward bookmark.
                //    AddTransitionFromFrameToForwardTransition(forwardBookmark);

                //    // Add the forward video again.
                //    AddForwardBookmarkVideo(forwardBookmark: forwardBookmark, HideTextOverlay: true);

                //    // Prepare for the next transition.
                //    TransitionFromFrame = forwardBookmark.ClipFilename + ".Last";
                //}

                //
                // All reversals for this forward forwardBookmark have been added. 
                //

                // Transition to normal video, unless replaying the forward video.
                if (ProjectSettings.ReplayForwardVideo == false)
                {
                    AddTransitionToNormalVideo(forwardBookmark.Name + ".Last", ProjectSettings.TransitionLengthMajor);
                }

                // Add the normal video between forward bookmarks.
                AddForwardVideo(ForwardBookmarks, i);
            }

            // Add fade to black.
            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);
            
            // Add the closing card.
            AddClosingCard();

            // Add 1/2 second of black.
            AddBlack(0.5d);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a video with forward and reverse bookmarks only.
        /// </summary>
        private void AssembleClipListFnRBookmarksOnly()
        {
            // Add 1/2 second of black.
            AddBlack(0.5d);

            // Add the opening card.
            AddOpeningCard();

            // Fade in from black only if there isn't a forward explanation on the first forward bookmark.
            if (ForwardBookmarks.Count > 0 && String.IsNullOrWhiteSpace(ForwardBookmarks[0].Explanation) == true)
            {
                // Fade in to the opening frame from black.
                AddTransitionFromBlack(ForwardBookmarks[0].Name + ".First", ProjectSettings.TransitionLengthCard);
            }

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // If there is a forward explanation.
                if (ProjectSettings.IncludeForwardExplanations && String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                {
                    // Add fade to black if this is not the first forward bookmark.
                    if (i > 0)
                    {
                        AddTransitionToBlack(forwardBookmark.Name + ".First", ProjectSettings.TransitionLengthCard);
                    }

                    AddExplanationCard(forwardBookmark);

                    // Add fade from black.
                    AddTransitionFromBlack(forwardBookmark.Name + ".First", ProjectSettings.TransitionLengthCard);
                }

                // Add the forward video with text overlay.
                AddForwardBookmarkVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";
                double TransitionLength = ProjectSettings.TransitionLengthMajor;

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations)
                        {
                            if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                            {
                                // Add fade to black.
                                AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

                                // Add the forward explanation.
                                AddExplanationCard(reverseBookmark);

                                // Prepare for the next transition.
                                TransitionFromFrame = "Black";
                            }
                        }
                    }
                }

                // If the forward replay was requested, play it.
                //if (ProjectSettings.ReplayForwardVideo)
                //{
                //    // Transition to the first frame of this forward bookmark.
                //    AddTransitionFromFrameToForwardTransition(forwardBookmark);

                //    // Add the forward video again.
                //    AddForwardBookmarkVideo(forwardBookmark: forwardBookmark, HideTextOverlay: true);

                //    // Prepare for the next transition.
                //    TransitionFromFrame = forwardBookmark.ClipFilename + ".Last";
                //}

                // Transition from the end of this reversal to the beginning of the next.
                AddTransitionToNextForward(i, ProjectSettings.TransitionLengthMajor);
            }

            // Add fade to black.
            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

            // Add the closing card.
            AddClosingCard();

            // Add 1/2 second of black.
            AddBlack(0.5d);
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
                        AddBlack(0.5d);

                        // If there is a forward explanation.
                        if (ProjectSettings.IncludeForwardExplanations && String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                        {
                            // Add the forward explanation.
                            AddExplanationCard(forwardBookmark);
                        }

                        // Add fade from black.
                        AddTransitionFromBlack(forwardBookmark.Name + ".First", ProjectSettings.TransitionLengthCard);

                        // Add the forward video with text overlay.
                        AddForwardBookmarkVideo(forwardBookmark);

                        // Set the frame to transition from.
                        TransitionFromFrame = forwardBookmark.Name + ".Last";
                        double TransitionLength = ProjectSettings.TransitionLengthMajor;

                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 4, ProjectSettings.ReversalRate4, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations && String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

                            // Add the forward explanation.
                            AddExplanationCard(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = "Black";
                        }
                        // There was no reverse explanation, so a fade to black is in order.
                        else
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);
                        }

                        // Add 1/2 second of black.
                        AddBlack(0.5d);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a fill video with forward and reverse bookmarks.
        /// </summary>
        private void AssembleClipListOrphanedReversalsFullVideo()
        {
            // Add 1/2 second of black.
            AddBlack(0.5d);

            // Add the starting video clipEntry.
            AddStartingVideoClip();

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // Add the forward video with text overlay.
                AddForwardBookmarkVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";
                double TransitionLength = ProjectSettings.TransitionLengthMajor;

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 4, ProjectSettings.ReversalRate4, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations)
                        {
                            if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                            {
                                // Add fade to black.
                                AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

                                // Add the forward explanation.
                                AddExplanationCard(reverseBookmark);

                                // Prepare for the next transition.
                                TransitionFromFrame = "Black";
                            }
                        }
                    }
                }

                //
                // All reversals for this forward forwardBookmark have been added. 
                //

                // Transition to normal video.
                AddTransitionToNormalVideo(forwardBookmark.Name + ".Last", ProjectSettings.TransitionLengthMajor);

                // Add the normal video between forward bookmarks.
                AddForwardVideo(ForwardBookmarks, i);
            }

            // Add 1/2 second of black.
            AddBlack(0.5d);
        }

        /// <summary>
        /// Creates the clips to be assembled by qmelt for a video with forward and reverse bookmarks only.
        /// </summary>
        private void AssembleClipListOrphanedReversalBookmarksOnly()
        {
            // Add 1/2 second of black.
            AddBlack(0.2f);

            // Loop through all of the forward bookmarks.
            for (int i = 0; i < ForwardBookmarks.Count; ++i)
            {
                Bookmark forwardBookmark = ForwardBookmarks[i];

                // Update the user.
                Progress.Report(String.Format("Working: Creating video for {0}: {1}", forwardBookmark.Name, forwardBookmark.Text));

                // Add the forward video with text overlay.
                AddForwardBookmarkVideo(forwardBookmark);

                // Set the frame to transition from.
                TransitionFromFrame = forwardBookmark.Name + ".Last";
                double TransitionLength = ProjectSettings.TransitionLengthMajor;

                if (forwardBookmark.ReferencedBookmarks.Count > 0)
                {
                    foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                    {
                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 4, ProjectSettings.ReversalRate4, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations)
                        {
                            if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                            {
                                // Add fade to black.
                                AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

                                // Add the forward explanation.
                                AddExplanationCard(reverseBookmark);

                                // Prepare for the next transition.
                                TransitionFromFrame = "Black";
                            }
                        }
                    }
                }

                // Transition from the end of this reversal to the beginning of the next.
                AddTransitionToNextForward(i, ProjectSettings.TransitionLengthMajor);
            }

            // Add 1/2 second of black.
            AddBlack(0.5d);
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
                        AddBlack(0.5d);

                        // Add the forward video with text overlay.
                        AddForwardBookmarkVideo(forwardBookmark);

                        // Set the frame to transition from.
                        TransitionFromFrame = forwardBookmark.Name + ".Last";
                        double TransitionLength = ProjectSettings.TransitionLengthMajor;

                        // Add the four reversal rates.
                        if (ProjectSettings.ReversalRate1.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 1, ProjectSettings.ReversalRate1, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 1, ProjectSettings.ReversalRate1);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate2.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 2, ProjectSettings.ReversalRate2, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 2, ProjectSettings.ReversalRate2);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate3.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 3, ProjectSettings.ReversalRate3, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 3, ProjectSettings.ReversalRate3);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        if (ProjectSettings.ReversalRate4.UseThisRate == true)
                        {
                            // Transition into the reverse bookmark.
                            AddTransitionFromFrameToReverseTransition(reverseBookmark, 4, ProjectSettings.ReversalRate4, TransitionLength);
                            TransitionLength = ProjectSettings.TransitionLengthMinor;

                            // Play the reverse bookmark.
                            AddReverseVideo(reverseBookmark, 4, ProjectSettings.ReversalRate4);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If a back and forth was requested, add it.
                        if (ProjectSettings.IncludeBackAndForth)
                        {
                            // Add the transition to the back and forth.
                            AddBackAndForthTransition(reverseBookmark, ProjectSettings.TransitionLengthMajor);

                            // Add the back and forth.
                            AddBackAndForth(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = reverseBookmark.Name + ".Last";
                        }

                        // If there is a reverse explanation.
                        if (ProjectSettings.IncludeReverseExplanations && String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);

                            // Add the forward explanation.
                            AddExplanationCard(reverseBookmark);

                            // Prepare for the next transition.
                            TransitionFromFrame = "Black";
                        }
                        // There was no reverse explanation, so a fade to black is in order.
                        else
                        {
                            // Add fade to black.
                            AddTransitionToBlack(TransitionFromFrame, ProjectSettings.TransitionLengthCard);
                        }

                        // Add 1/2 second of black.
                        AddBlack(0.5d);
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
        /// Creates a video clipEntry of the specified number of seconds of black.
        /// </summary>
        /// <remarks>
        /// This is the first clipEntry added to the video. I create it from the first frame of the
        /// source video, make it black, and repeat that frame for the specified duration. This
        /// results in the correct video resolution in the output video.
        /// </remarks>
        /// <param name="duration">The length of time for the video of black.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddBlack(double duration)
        {
            // Create the filename for this video clipEntry.
            String filename = $"Black.{duration:0.######}";
            String FilenameWithExtension = filename + OutputVideoInterimExtension;

            // Create the inner commands for ffmpeg. 
            String command = $"-i \"{RelativePathToWorkingInputVideoFile}\" "
            + $"-f lavfi -i color=size={HorizontalResolution}x{VerticalResolution}:c=black -loop 1 "
            + $"-filter_complex \"[0:a]volume = 0.0[a];[:v][1:v]overlay[v]\" "
            + $"-map [v] -map [a] -t {duration:0.######}";

            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename,
                command,
                FfmpegTaskSortOrder.ForwardVideo,
                duration);
        }


        /// <summary>
        /// Adds the opening card, if there is one.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddOpeningCard()
        {
            bool retval = true;

            // Skip if OpeningCard isn't included.
            if (ProjectSettings.IncludeOpeningCard)
            {
                // If There is an opening card, add it.
                if (String.IsNullOrWhiteSpace(OpeningCard) == false)
                {
                    // Create the filename for this video clipEntry.
                    String filename = "OpeningCard";

                    // Calculate the length of time to display the card.
                    double duration = (double)OpeningCard.Length / (double)ProjectSettings.ReadingCharactersPerSecond;

                    // Create the inner commands for ffmpeg. 
                    String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                        FramesPerSecond,
                        filename,
                        duration,
                        RelativePathToWorkingInputVideoFile);
                    command = AddFfmpegOutputStrings(command, filename);

                    // Create the ffmpeg task for this clipEntry.
                    retval = CreateFfmpegTask(filename, 
                        command, 
                        FfmpegTaskSortOrder.CardVideo, 
                        duration);
                }
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

            // Skip if ClosingCard isn't included.
            if (ProjectSettings.IncludeClosingCard)
            {
                // If There is a closing card, add it.
                if (String.IsNullOrWhiteSpace(ClosingCard) == false)
                {
                    // Create the filename for this video clipEntry.
                    String filename = "ClosingCard";

                    // Calculate the length of time to display the card.
                    double duration = (double)ClosingCard.Length / (double)ProjectSettings.ReadingCharactersPerSecond;

                    // Create the inner commands for ffmpeg. 
                    String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                        FramesPerSecond,
                        filename,
                        duration,
                        RelativePathToWorkingInputVideoFile);
                    command = AddFfmpegOutputStrings(command, filename);

                    // Create the ffmpeg task for this clipEntry.
                    retval = CreateFfmpegTask(filename, 
                        command, 
                        FfmpegTaskSortOrder.CardVideo, 
                        duration);
                }
            }

            return retval;
        }

        /// <summary>
        /// Adds the initial video clipEntry before the first forward forwardBookmark.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddStartingVideoClip()
        {
            // Calculate the start time and duration.
            double startTime = 0.0d;
            double duration = ((double)ForwardBookmarks[0].SampleStart / (double)SampleRate);

            // Create the filename for this clipEntry.
            String filename = String.Format("v{0:0.######}-{1:0.######}", startTime, duration);

            // Create the inner commands for ffmpeg.
            String command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                startTime,
                RelativePathToWorkingInputVideoFile,
                duration);

            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, 
                command, 
                FfmpegTaskSortOrder.ForwardVideo, 
                duration);
        }

        /// <summary>
        /// Adds the explanation card from the specified bookmark.
        /// </summary>
        /// <param name="bookmark">The forward bookmark.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddExplanationCard(Bookmark bookmark)
        {
            // Create the filename for this video clipEntry.
            String filename = bookmark.Name + ".Explanation";

            // Calculate the length of time to display the card.
            double duration = (double)bookmark.Explanation.Length / (double)ProjectSettings.ReadingCharactersPerSecond;

            // Create the inner commands for ffmpeg. 
            String command = String.Format("-r {0:0.######} -loop 1 -i \"{1}.png\" -t {2:0.######} -i \"{3}\" -af volume=0.0 -t {2:0.######}",
                FramesPerSecond,
                filename,
                duration,
                RelativePathToWorkingInputVideoFile);
            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename,
                command, 
                FfmpegTaskSortOrder.CardVideo, 
                duration);
        }

        /// <summary>
        /// Adds a transition from black to the specified image.
        /// </summary>
        /// <param name="ImageName">The image to fade to, without an extension.</param>
        /// <param name="duration">Length in time for the transition.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionFromBlack(String ImageName, Double duration)
        {
            // If the transition length is less than the time of a single frame, return.
            if (duration < 1.0d / FramesPerSecond)
            {
                return true;
            }

            // Set the video output filename.
            String filename = "Black-" + ImageName;

            // Create the inner commands for ffmpeg.
            // -r {0}                       # Frame rate.
            // -loop 1 - i \"{1}.png\"      # Make a video input stream from a loop of the specified image.
            // -i \"{3}\"                   # Use this input stream (will be used to create silent audio channel).
            // -filter_complex              # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                        # From the third input, select the audio channel.
            // volume=0.0,                  # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}  # Set the audio duration to {3}.
            // [a];                         # ClipFilename this stream "a" for audio.
            // [0:v]                        # From the first input, select the video channel.
            // fade=t=in:                   # Fade, type in (fade in).
            // d={2}                        # Duration of the fade.
            // [v]\"                        # ClipFilename this stream "v" for video.
            // -map \"[v]\"                 # Put the video stream into stream 0:0.
            // -map \"[a]\"                 # Put the audio stream into stream 0:1.
            // -t {2:0.######}              # Set the duration of this clipEntry (may be redundant since the audio duration and fade duration are identical).
            String command = String.Format("-r {0} -loop 1 -i \"{1}.png\" -i \"{3}\" -filter_complex \"[1:a]volume=0.0,atrim=duration={2:0.######}[a];[0:v]fade=t=in:d={2:0.######}[v]\" -map \"[v]\" -map \"[a]\" -t {2:0.######}",
                FramesPerSecond,
                ImageName,
                duration,
                RelativePathToWorkingInputVideoFile);
            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, 
                command, 
                FfmpegTaskSortOrder.TransitionVideo, 
                duration);
        }

        /// <summary>
        /// Adds a transition from the specified image to black.
        /// </summary>
        /// <param name="ImageName">The image to fade to, without an extension.</param>
        /// <param name="duration">Length in time for the transition.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionToBlack(String ImageName, Double duration)
        {
            // If the transition length is less than the time of a single frame, return.
            if (duration < 1.0d / FramesPerSecond)
            {
                return true;
            }

            // Set the video output filename.
            String filename = ImageName + "-Black";

            // Create the inner commands for ffmpeg.
            // -r {0}                       # Frame rate.
            // -loop 1 - i \"{1}.png\"      # Make a video input stream from a loop of the specified image.
            // -i \"{3}\"                   # Use this input stream (will be used to create silent audio channel).
            // -filter_complex              # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                        # From the third input, select the audio channel.
            // volume=0.0,                  # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}  # Set the audio duration to {3}.
            // [a];                         # ClipFilename this stream "a" for audio.
            // [0:v]                        # From the first input, select the video channel.
            // fade=t=out:                  # Fade, type out (fade out).
            // d={2}                        # Duration of the fade.
            // [v]\"                        # ClipFilename this stream "v" for video.
            // -map \"[v]\"                 # Put the video stream into stream 0:0.
            // -map \"[a]\"                 # Put the audio stream into stream 0:1.
            // -t {2:0.######}              # Set the duration of this clipEntry (may be redundant since the audio duration and fade duration are identical).
            String command = String.Format("-r {0} -loop 1 -i \"{1}.png\" -i \"{3}\" -t {2:0.######} -filter_complex \"[1:a]volume=0.0,atrim=duration={2:0.######}[a];[0:v]fade=t=out:d={2:0.######}[v]\" -map \"[v]\" -map \"[a]\" -t {2:0.######}",
                FramesPerSecond,
                ImageName,
                duration,
                RelativePathToWorkingInputVideoFile);
            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, 
                command, 
                FfmpegTaskSortOrder.TransitionVideo, 
                duration);
        }

        /// <summary>
        /// Adds the specified forward video clipEntry with the forward text overlay image added.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddForwardBookmarkVideo(Bookmark forwardBookmark, bool HideTextOverlay = false)
        {
            List<String> FfmpegCommands = new List<String>();
            List<String> Filenames = new List<String>();

            //
            // Extract the forward clipEntry from the source video.
            //

            // Calculate the start time and duration.
            double startTime = (double)forwardBookmark.SampleStart / (double)SampleRate;
            double duration = ((double)forwardBookmark.SampleEnd / (double)SampleRate) - startTime;

            // Create the command to output both the .Last and .First non-overlay images.
            String filename = String.Empty;
            Filenames.Add(filename);
            String command = $"-y -hide_banner -threads {{0}} "
                + $"-i \"{RelativePathToWorkingInputVideoFile}\" -ss {startTime + duration:0.######} "
                + $"-i \"{RelativePathToWorkingInputVideoFile}\" -ss {startTime:0.######} "
                + $"-map 0:v -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{forwardBookmark.Name + ".Last.png"}\" "
                + $"-map 1:v -pix_fmt rgb48 -an -q:v 1 -frames:v 1 \"{forwardBookmark.Name + ".First.png"}\"";
            FfmpegCommands.Add(command);

            // If we are not displaying the text overlay, do a simple clip.
            if (HideTextOverlay)
            {
                // Create the filename for this clipEntry.
                filename = $"v{startTime:0.######}-{duration:0.######}";
                Filenames.Add(filename);

                // Create the inner commands for ffmpeg.
                command = $"-ss {startTime:0.######} -i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-t {duration:0.######}";
                command = AddFfmpegOutputStrings(command, filename, AddProgress: false);

                // Add this command to the list of commands.
                FfmpegCommands.Add(command);
            }
            else
            {
                //
                // Overlay the forward text overlay image.
                //

                // Create the filename for this clipEntry.
                filename = forwardBookmark.Name + ".Text";
                Filenames.Add(filename);

                // Create the inner commands for ffmpeg.
                command = $"-ss {startTime:0.######} -i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-i \"{filename}.png\" -t {duration} -filter_complex \"[0:v][1:v]overlay\"";
                command = AddFfmpegOutputStrings(command, filename);

                // Add this command to the list of commands.
                FfmpegCommands.Add(command);
            }

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(Filenames: Filenames,
                FfmpegCommands: FfmpegCommands,
                SortOrder: FfmpegTaskSortOrder.ForwardBookmarkVideo,
                EstimatedDuration: duration);
        }

        /// <summary>
        /// Creates a transition with text overlay from TransitionFromFrame to the first frame of the reverse clipEntry.
        /// </summary>
        /// <param name="reverseBookmark">The reverse bookmark of this transition.</param>
        /// <param name="reversalNumber">The number of the reversal (1=first, 2=second, ...).</param>
        /// <param name="reversalRate">The reversal rate of the reversal.</param>
        /// <param name="TransitionLength">Length in time for the transition.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionFromFrameToReverseTransition(Bookmark reverseBookmark, int reversalNumber, ReversalRate reversalRate, Double TransitionLength)
        {
            String command = String.Empty;

            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / FramesPerSecond)
            {
                return true;
            }

            // TransitionFromFrame is already set. Now set the TransitionToFrame.
            TransitionToFrame = (reversalRate.ReversalSpeed == reversalRate.ReversalTone) ?
                $"{reverseBookmark.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}.First" :
                $"{reverseBookmark.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}-{reversalRate.ReversalTone}.First";

            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(TransitionToFrame, TransitionLength);
            }

            // Set the video output filename.
            String filename = TransitionFromFrame + "-" + TransitionToFrame;

            if (ProjectSettings.TransitionType == TransitionType.XFade)
            {
                command = $"-r {FramesPerSecond} "
                    + $"-i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionToFrame}.png\" "
                    + $"-i \"{reverseBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] volume=0.0,atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v]xfade=transition={ProjectSettings.XFadeTransitionType}:duration={TransitionLength:0.######} [xsition]; "
                    + $"[xsition][3:v] overlay [v]\" -map [v] -map [a] ";
            }
            else if (ProjectSettings.TransitionType == TransitionType.HoldLastFrame)
            {
                command = $"-r {FramesPerSecond} "
                    + $"-i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                    + $"-i \"{reverseBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] volume=0.0,atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v] overlay [v]\" -map [v] -map [a] ";
            }
            else
            {
                return true;
            }

            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, command, FfmpegTaskSortOrder.TransitionVideo, TransitionLength);
        }

        /// <summary>
        /// Adds a transition from TransitionFromFrame to the first frame of the forward bookmark.
        /// </summary>
        /// <param name="forwardBookmark">The forward bookmark.</param>
        /// <param name="TransitionLength">Length in time for the transition.</param>
        /// <returns></returns>
        private bool AddTransitionFromFrameToForwardTransition(Bookmark forwardBookmark, Double TransitionLength)
        {
            String command = String.Empty;

            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / FramesPerSecond)
            {
                return true;
            }

            //
            // Create the transition clipEntry.
            //

            // TransitionFromFrame is already set. Now set the TransitionToFrame.
            TransitionToFrame = forwardBookmark.Name + ".First";

            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(TransitionToFrame, TransitionLength);
            }

            // Set the video output filename.
            String filename = TransitionFromFrame + "-" + TransitionToFrame;

            if (ProjectSettings.XFadeTransitionType == TransitionType.XFade.ToString())
            {
                command = $"-r {FramesPerSecond} "
                    + $"-i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionToFrame}.png\" "
                    + $"-i \"{forwardBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] volume=0.0,atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v]xfade=transition={ProjectSettings.XFadeTransitionType}:duration={TransitionLength:0.######} [xsition]; "
                    + $"[xsition][3:v] overlay [v]\" -map [v] -map [a] ";
            }
            else if (ProjectSettings.XFadeTransitionType == TransitionType.HoldLastFrame.ToString())
            {
                command = $"-r {FramesPerSecond} "
                    + $"-i \"{RelativePathToWorkingInputVideoFile}\" "
                    + $"-loop 1 -t {TransitionLength:0.######} -i \"{TransitionFromFrame}.png\" "
                    + $"-i \"{forwardBookmark.Name}.Text.png\" "
                    + $"-filter_complex \"[0:a] volume=0.0,atrim=duration={TransitionLength:0.######} [a]; "
                    + $"[1:v][2:v] overlay [v]\" -map [v] -map [a] ";
            }

            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, command, FfmpegTaskSortOrder.TransitionVideo, TransitionLength);
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
            // Find the specific reverse video clipEntry.
            String reverseVideoFilename = (reversalRate.ReversalSpeed == reversalRate.ReversalTone) ?
                $"{reverseBookmark.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}" :
                $"{reverseBookmark.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}-{reversalRate.ReversalTone}";

            // Create the filename for this clipEntry.
            String filename = reverseVideoFilename + ".Text";

            // Create the inner commands for ffmpeg.
            String command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                filename,
                OutputVideoInterimExtension,
                reverseBookmark.Name);

            // Get the clipEntry duration.
            double duration = 0.0d;
            if (ClipsToCreate.ContainsKey(filename + OutputVideoInterimExtension))
            {
                duration = ClipsToCreate[filename + OutputVideoInterimExtension];
            }
            else
            {
                File.AppendAllText(LogFile, $"\r\n\r\n***File not in ClipsToCreate: {filename}. (AddReverseVideo)\r\n\r\n");
                return false;
            }

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, command, FfmpegTaskSortOrder.ReverseVideoPass2, duration);
        }

        private bool AddBackAndForthTransition(Bookmark reverseBookmark, Double TransitionLength)
        {
            List<String> FfmpegCommands = new List<String>();
            List<String> Filenames = new List<String>();

            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / FramesPerSecond)
            {
                return true;
            }

            //
            // Create the transition clipEntry.
            //

            // Create the filename for this video clipEntry.
            String filename = TransitionFromFrame;
            Filenames.Add(filename);

            // Create the inner commands for ffmpeg. 
            String command = String.Format("-r {0:0.###############} -loop 1 -i \"{1}.png\" -t {2:0.###############} -i \"{3}\" -af volume=0.0 -t {2:0.###############} ",
                FramesPerSecond,
                Filenames[0],
                TransitionLength,
                RelativePathToWorkingInputVideoFile);
            command = AddFfmpegOutputStrings(command, filename);

            // Add this command to the list of commands.
            FfmpegCommands.Add(command);

            //
            // Overlay the reverse text overlay image.
            //

            // Create the filename for this clipEntry.
            filename = Filenames[0] + ".Text";
            Filenames.Add(filename);

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                Filenames[0],
                OutputVideoInterimExtension,
                reverseBookmark.Name);
            command = AddFfmpegOutputStrings(command, filename);

            // Add this command to the list of commands.
            FfmpegCommands.Add(command);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(Filenames: Filenames,
                FfmpegCommands: FfmpegCommands,
                SortOrder: FfmpegTaskSortOrder.TransitionVideo,
                EstimatedDuration: TransitionLength);
        }

        private bool AddBackAndForth(Bookmark reverseBookmark)
        {
            // TODO: Make this happen in one ffmpeg command.

            List<String> FfmpegCommands = new List<String>();
            List<String> Filenames = new List<String>();
            
            // Find the first selected reverse video.
            ReversalRate reversalRate;
            int reversalNumber;
            if (ProjectSettings.ReversalRate1.UseThisRate)
            {
                reversalRate = ProjectSettings.ReversalRate1;
                reversalNumber = 1;
            }
            else if (ProjectSettings.ReversalRate2.UseThisRate)
            {
                reversalRate = ProjectSettings.ReversalRate2;
                reversalNumber = 2;
            }
            else if (ProjectSettings.ReversalRate3.UseThisRate)
            {
                reversalRate = ProjectSettings.ReversalRate3;
                reversalNumber = 3;
            }
            else if (ProjectSettings.ReversalRate4.UseThisRate)
            {
                reversalRate = ProjectSettings.ReversalRate4;
                reversalNumber = 4;
            }
            else
            {
                return true;
            }

            //
            // Extract the forward clipEntry from the source video.
            //

            // Calculate the start time and duration.
            double startTime = (double)reverseBookmark.SampleStart / (double)SampleRate;
            double duration = ((double)reverseBookmark.SampleEnd / (double)SampleRate) - startTime;

            // Create the filename for this clipEntry.
            String filename = String.Format("v{0:0.######}-{1:0.######}", startTime, duration);
            Filenames.Add(filename);

            // Create the inner commands for ffmpeg.
            String command = String.Format("-ss {0:0.######} -i \"{1}\" -t {2:0.######}",
                startTime,
                RelativePathToWorkingInputVideoFile,
                duration);
            command = AddFfmpegOutputStrings(command, filename);

            // Add this command to the list of commands.
            FfmpegCommands.Add(command);

            //
            // Overlay the forward text overlay image.
            //

            // Create the filename for this clipEntry.
            filename = reverseBookmark.Name + ".Forward.Text";
            Filenames.Add(filename);

            // Create the inner commands for ffmpeg.
            command = String.Format("-i \"{0}{1}\" -i \"{2}.Text.png\" -filter_complex \"[0:v][1:v]overlay\"",
                Filenames[0],
                OutputVideoInterimExtension,
                reverseBookmark.Name);
            command = AddFfmpegOutputStrings(command, filename);

            // Add this command to the list of commands.
            FfmpegCommands.Add(command);

            // Create the ffmpeg task for this clipEntry.
            CreateFfmpegTask(Filenames: Filenames,
                FfmpegCommands: FfmpegCommands,
                SortOrder: FfmpegTaskSortOrder.TransitionVideo,
                EstimatedDuration: duration);

            // Add the reverse clipEntry.
            return AddReverseVideo(reverseBookmark, reversalNumber, reversalRate);
        }

        /// <summary>
        /// Creates a transition from TransitionFromFrame to transitionToFrame.
        /// </summary>
        /// <param name="transitionToFrame">The frame to transition to.</param>
        /// <param name="TransitionLength">Length in time for the transition.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool AddTransitionToNormalVideo(String transitionToFrame, Double TransitionLength)
        {
            List<String> FfmpegCommands = new List<String>();

            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / FramesPerSecond)
            {
                return true;
            }

            // If we are transitioning from black.
            if (TransitionFromFrame == "Black")
            {
                // Use the method that performs this function.
                return AddTransitionFromBlack(transitionToFrame, TransitionLength);
            }

            // Set the video output filename.
            String filename = TransitionFromFrame + "-" + transitionToFrame;

            // Create the inner commands for ffmpeg.
            // -r {0}                                   # Frame rate.
            // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
            // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
            // -i \"{4}\"                               # Use this input stream (will be used to create silent audio channel).
            // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
            // [2:a]                                    # From the third input, select the audio channel.
            // volume=0.0,                              # Multiply the audio value by 0 (silence).
            // atrim=duration={2:0.######}              # Set the audio duration to {3}.
            // [a];                                     # ClipFilename this stream "a" for audio.
            // [1]format=yuva444p,                      # Format the first image as yuva444p.
            // fade=d={3:0.######}:                     # Set the fade duration.
            // t=in:                                    # Set the fade type to "fade in".
            // alpha=1,                                 # Fade only the alpha channel.
            // setpts=                                  # Set the presentation timestamp
            // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
            //                                          #   STARTPTS is the PTS of the first frame.
            //                                          #   TB is the timebase of the input timestamps.
            // [f0];                                    # ClipFilename this stream "f0".
            // [0][f0]overlay,                          # Overlay stream 0 and f0.
            // format=yuv420p                           # Format the overlayed video as yuv420p
            // [v]\"                                    # ClipFilename the overlayed video "v" for video.
            // -map \"[v]\"                             # Put the video stream into stream 0:0.
            // -map \"[a]\"                             # Put the audio stream into stream 0:1.
            String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\" -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                FramesPerSecond,
                TransitionFromFrame,
                transitionToFrame,
                TransitionLength,
                RelativePathToWorkingInputVideoFile);
            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename,
                command,
                FfmpegTaskSortOrder.TransitionVideo,
                TransitionLength);
        }

        private bool AddTransitionToNextForward(int index, double TransitionLength)
        {
            // If the transition length is less than the time of a single frame, return.
            if (TransitionLength < 1.0d / FramesPerSecond)
            {
                return true;
            }

            // If this is not the last bookmark
            if (index < ForwardBookmarks.Count - 1)
            {
                // Determine the transition to frame.
                TransitionToFrame = ForwardBookmarks[index + 1].Name + ".First";

                // If we are transitioning from black.
                if (TransitionFromFrame == "Black")
                {
                    // Use the method that performs this function.
                    return AddTransitionFromBlack(TransitionToFrame, TransitionLength);
                }
                
                // Set the video output filename.
                String filename = TransitionFromFrame + "-" + TransitionToFrame;

                // Create the inner commands for ffmpeg.
                // -r {0}                                   # Frame rate.
                // -loop 1 -t {3:0.######} -i \"{1}.png\"   # Loop this first input image for {3} seconds.
                // -loop 1 -t {3:0.######} -i \"{2}.png\"   # Loop this second input iamge for {3} seconds.
                // -i \"{4}\"                               # Use this input stream (will be used to create silent audio channel).
                // -filter_complex                          # Pass the following string to the ffmpeg filter_complex.
                // [2:a]                                    # From the third input, select the audio channel.
                // volume=0.0,                              # Multiply the audio value by 0 (silence).
                // atrim=duration={2:0.######}              # Set the audio duration to {3}.
                // [a];                                     # ClipFilename this stream "a" for audio.
                // [1]format=yuva444p,                      # Format the first image as yuva444p.
                // fade=d={3:0.######}:                     # Set the fade duration.
                // t=in:                                    # Set the fade type to "fade in".
                // alpha=1,                                 # Fade only the alpha channel.
                // setpts=                                  # Set the presentation timestamp
                // PTS-STARTPTS/TB                          # PTS is he presentation timestamp in input
                //                                          #   STARTPTS is the PTS of the first frame.
                //                                          #   TB is the timebase of the input timestamps.
                // [f0];                                    # ClipFilename this stream "f0".
                // [0][f0]overlay,                          # Overlay stream 0 and f0.
                // format=yuv420p                           # Format the overlayed video as yuv420p
                // [v]\"                                    # ClipFilename the overlayed video "v" for video.
                // -map \"[v]\"                             # Put the video stream into stream 0:0.
                // -map \"[a]\"                             # Put the audio stream into stream 0:1.
                String command = String.Format("-r {0} -loop 1 -t {3:0.######} -i \"{1}.png\" -loop 1 -t {3:0.######} -i \"{2}.png\" -i \"{4}\"  -filter_complex \"[2:a]volume=0.0,atrim=duration={3:0.######}[a];[1]format=yuva444p,fade=d={3:0.######}:t=in:alpha=1,setpts=PTS-STARTPTS/TB[f0]; [0][f0]overlay,format=yuv420p[v]\" -map \"[v]\" -map \"[a]\"",
                    FramesPerSecond,
                    TransitionFromFrame,
                    TransitionToFrame,
                    TransitionLength,
                    RelativePathToWorkingInputVideoFile);
                command = AddFfmpegOutputStrings(command, filename);

                // Create the ffmpeg task for this clipEntry.
                CreateFfmpegTask(filename,
                    command, 
                    FfmpegTaskSortOrder.TransitionVideo, 
                    TransitionLength);
            }

            return true;
        }

        /// <summary>
        /// Adds the forward, non-bookmarked, video.
        /// </summary>
        /// <param name="ForwardBookmarks"></param>
        /// <param name="index"></param>
        /// <returns>The ffmpeg task object with the commands to create this clipEntry.</returns>
        private bool AddForwardVideo(List<Bookmark> ForwardBookmarks, int index)
        {
            String command;
            String filename;

            // Calculate the end time.
            double endTime = (double)ForwardBookmarks[index].SampleEnd / (double)SampleRate;
            double startTimeOfNextBookmark = 864000.0d;
            double duration = 0.0d;

            // If this is not the last forward bookmark.
            if (index < ForwardBookmarks.Count - 1)
            {
                // Calculate the next start time.
                startTimeOfNextBookmark = ForwardBookmarks[index + 1].SampleStart / (double)SampleRate;

                // Adjust for the audio delay.
                //startTimeOfNextBookmark += (double)ProjectSettings.VideoDelay / 1000d;
                //startTimeOfNextBookmark = startTimeOfNextBookmark < 0 ? 0 : startTimeOfNextBookmark;

                // If the end time of this bookmark is beyond the start time of the next bookmark, this clipEntry is not necessary.
                if (endTime >= startTimeOfNextBookmark)
                {
                    return true;
                }

                // Calculate the duration.
                //double duration = startTimeOfNextBookmark - (((double)ForwardBookmarks[index].SampleEnd / (double)SampleRate) + (double)ProjectSettings.VideoDelay / 1000d);
                duration = startTimeOfNextBookmark - ((double)ForwardBookmarks[index].SampleEnd / (double)SampleRate);

                // Create the inner commands for ffmpeg.
                command = $"-ss {endTime:0.######} -i \"{RelativePathToWorkingInputVideoFile}\" -t {duration:0.######}";

                // Create the filename for the video that comes after this clipEntry. The value of endTime is the end of this bookmark.
                filename = $"v{endTime:0.######}-{duration:0.######}";
            }
            // This is the last forward bookmark.
            else
            {
                // There is no need to add a time, since we are taking the video file from endTime of this bookmark to the end.
                command = $"-ss {endTime:0.######} -i \"{RelativePathToWorkingInputVideoFile}\"";

                // Create the filename for the video that comes after this clipEntry. The value of endTime is the end of this bookmark.
                filename = $"v{endTime:0.######}-End";
            }

            // Add progress, output settings, and output filename.
            command = AddFfmpegOutputStrings(command, filename);

            // Create the ffmpeg task for this clipEntry.
            return CreateFfmpegTask(filename, 
                command, 
                FfmpegTaskSortOrder.ForwardVideo, 
                duration);
        }
    }
}