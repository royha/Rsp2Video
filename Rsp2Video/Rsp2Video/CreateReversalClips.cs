using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace RSPro2Video
{
    public partial class RSPro2VideoForm : Form
    {
        /// <summary>
        /// Creates all reversal clips and still frames.
        /// </summary>
        private void CreateReversalClips()
        {
            // Create all of the reversal audio files.
            CreateReversalAudioFiles();

            // Create all of the reversal video files.
            CreateReversalVideoFiles();
        }

        /// <summary>
        /// Creates the reversal .wav files.
        /// </summary>
        private void CreateReversalAudioFiles()
        {
            // Create the various reversals for each bookmarked reversal.
            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            {
                foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                {
                    // Update the user.
                    Progress.Report(String.Format("Working: Creating reverse audio for {0}: {1}", reverseBookmark.Name, reverseBookmark.Text));

                    // Create a .wav file for each selected reversal rate.
                    if (CreateReverseWav(reverseBookmark, 1, settings.ReversalRate1) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 2, settings.ReversalRate2) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 3, settings.ReversalRate3) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 4, settings.ReversalRate4) == false) { break; }
                }
            }
        }

        /// <summary>
        /// Creates a reversal .wav file based on the specified reversal rate.
        /// </summary>
        /// <param name="reverseBookmark"></param>
        /// <param name="reversalRate"></param>
        /// <returns></returns>
        private bool CreateReverseWav(Bookmark reverseBookmark, int reversalNumber, ReversalRate reversalRate)
        {
            // If this rate isn't selected, return.
            if (reversalRate.UseThisRate == false) { return true; }

            // Calculate start and length in seconds.
            float startSeconds = (float)reverseBookmark.SampleStart / (float)SampleRate;
            float lengthSeconds = ((float)reverseBookmark.SampleEnd / (float)SampleRate) - (float)startSeconds;

            // If speed and tone are the same, only one step is needed.
            if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
            {
                // Create a .wav file at the requested speed (ie., 70%) by using SoX "speed" feature which changes both length and tone.
                if (CallSoxSpeed(reverseBookmark.Name, reversalNumber, startSeconds, lengthSeconds, reversalRate.ReversalSpeed) == false) { return false; }
            }
            else
            // A two step process is needed.
            {
                // Change speed to achieve the desired tone (ReversalTone), then stretch to desired length.
                if (CallSoxStretch(reverseBookmark.Name, reversalNumber, startSeconds, lengthSeconds, reversalRate.ReversalSpeed, reversalRate.ReversalTone) == false) { return false; }
            }

            return true;
        }

        private bool CallSoxSpeed(string reversalName, int reversalNumber, float startSeconds, float lengthSeconds, int reversalSpeed)
        {
            Process process = new Process();

            String audioFilename = String.Format("{0}.{1}.{2}.wav", reversalName, reversalNumber, reversalSpeed);

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = SoxApp,
                Arguments = string.Format("\"{0}\" \"{1}\" trim {2:0.######} {3:0.######} reverse speed {4:0.######}",
                    settings.RspSoundFile, audioFilename, startSeconds, lengthSeconds, reversalSpeed / (float)100.0),
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Run SoX to create the .wav file.
            process.Start();

            // Read the error output of SoX, if there is any.
            String SoxOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            if (ExitCode != 0)
            {
                return false;
            }

            return true;
        }

        private bool CallSoxStretch(string reversalName, int reversalNumber, float startSeconds, float lengthSeconds, int reversalSpeed, int reversalTone)
        {
            Process process = new Process();

            //
            // Create the temp.wav file at the speed of the "tone" value.
            //

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = SoxApp,
                Arguments = string.Format("\"{0}\" _temp.wav trim {1:0.######} {2:0.######} reverse speed {3:0.######}",
                    settings.RspSoundFile, startSeconds, lengthSeconds, reversalTone / (float)100.0),
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Run SoX to create the .wav file.
            process.Start();

            // Read the error output of SoX, if there is any.
            String SoxOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            if (ExitCode != 0)
            {
                return false;
            }


            //
            // Create the final .wav file by stretching temp.wav to its final length.
            //

            String audioFilename = String.Format("{0}.{1}.{2}-{3}.wav", reversalName, reversalNumber, reversalSpeed, reversalTone);

            process.StartInfo.Arguments = string.Format("_temp.wav \"{0:0.######}\" stretch {1:0.######}",
                    audioFilename, (lengthSeconds / (reversalSpeed / (float)100.0)) / (lengthSeconds / (reversalTone / (float)100.0)));

            // Run SoX to create the .wav file.
            process.Start();

            // Read the error output of SoX, if there is any.
            SoxOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            ExitCode = process.ExitCode;
            process.Close();

            if (ExitCode != 0)
            {
                return false;
            }

            // Delete the temp file now that we're done with it.
            File.Delete("_temp.wav");

            return true;
        }

        /// <summary>
        /// Creates the reversal .wav files.
        /// </summary>
        private void CreateReversalVideoFiles()
        {
            // Create the various reversals for each bookmarked reversal.
            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            {
                foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                {
                    // Update the user.
                    Progress.Report(String.Format("Working: Creating video for {0}: {1}", reverseBookmark.Name, reverseBookmark.Text));

                    // Create a video file for each selected reversal rate.
                    if (CreateReverseVideo(reverseBookmark) == false) { break; }
                }
            }
        }

        /// <summary>
        /// Creates a reversal video file file based on the specified reversal rate.
        /// </summary>
        /// <param name="reversal"></param>
        /// <param name="reversalRate"></param>
        /// <returns></returns>
        private bool CreateReverseVideo(Bookmark reversal)
        {
            // Calculate start and length in seconds in the source video.
            float startSeconds = (float)reversal.SampleStart / (float)SampleRate;
            float lengthSeconds = ((float)reversal.SampleEnd / (float)SampleRate) - (float)startSeconds;

            // Adjust for the audio delay.
            startSeconds += (float)settings.AudioDelay / 1000f;
            startSeconds = startSeconds < 0 ? 0 : startSeconds;

            // Extract the individual frames in the source video.
            if (ExtractReverseVideoFrames(reversal.Name, startSeconds, lengthSeconds) == false) { return false; }

            // Rename the frames to be in reverse order.
            int frameCount = ReorderFrames(reversal.Name);
            if (frameCount == -1) { return false; }

            // Assemble the frames into a new video file.
            if (AssembleReverseVideoFrames(reversal, 1, settings.ReversalRate1, frameCount) == false) { return false; }
            if (AssembleReverseVideoFrames(reversal, 2, settings.ReversalRate2, frameCount) == false) { return false; }
            if (AssembleReverseVideoFrames(reversal, 3, settings.ReversalRate3, frameCount) == false) { return false; }
            if (AssembleReverseVideoFrames(reversal, 4, settings.ReversalRate4, frameCount) == false) { return false; }

            return true;
        }

        /// <summary>
        /// Extracts the frames for a reverse speech video clip as a numbered series of .png files.
        /// </summary>
        /// <param name="name">The name of the reversal forwardBookmark.</param>
        /// <param name="reversalNumber">The number of the reversal (first is often 100%, then 85%, ...).</param>
        /// <param name="startSeconds">The start location of the clip in seconds in SourceVideoFile.</param>
        /// <param name="lengthSeconds">The length of the clip in seconds at 100%.</param>
        /// <returns></returns>
        private bool ExtractReverseVideoFrames(string name, float startSeconds, float lengthSeconds)
        {
            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -ss {0:0.######} -i \"{1}\" -an -qscale 1 -t {2:0.######} \"{3}\\{4}.%06d.png\"",
                    startSeconds, WorkingInputVideoFile, lengthSeconds, FRAMES_DIR, name),
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

            if (!(ExitCode == 0))
            {
                return false;
            }

            return true;
        }

        private int ReorderFrames(string name)
        {
            // Change into the _tmp directory.
            Directory.SetCurrentDirectory(Path.Combine(WorkingDirectory, FRAMES_DIR));

            // Get a list of all the .png files (with path) from this clip.
            String[] frames = Directory.GetFiles(".", name + ".*.png");

            // Create an array of the .png files in reverse order with a slightly modified filename (adding "r" to the front of the filename).
            String[] newFrames = new string[frames.Length];

            int j = frames.Length - 1;
            int i = 0;
            for (; i < frames.Length; ++i, --j)
            {
                // Strip the path information.
                frames[j] = Path.GetFileName(frames[j]);

                // Add "r" and store in reverse order.
                newFrames[i] = "r" + frames[j];
            }

            // Copy first and last frame frame to working directory, and record their filenames in the list.
            String imageFilename = name + ".First.png";
            File.Copy(frames[frames.Length - 1], "..\\" + imageFilename, true);

            imageFilename = name + ".Last.png";
            File.Copy(frames[0], "..\\" + imageFilename, true);

            // Rename all of the files.
            for (i = 0; i < frames.Length; ++i)
            {
                if (File.Exists(newFrames[i]))
                {
                    File.Delete(newFrames[i]);
                }

                File.Move(frames[i], newFrames[i]);
            }

            // Change back to the working directory.
            Directory.SetCurrentDirectory(WorkingDirectory);

            return frames.Length;
        }

        private bool AssembleReverseVideoFrames(Bookmark reversal, int reversalNumber, ReversalRate reversalRate, int frameCount)
        {
            // If this rate isn't selected, return.
            if (reversalRate.UseThisRate == false) { return true; }

            Process process = new Process();

            // Generate the filename without the extension, since this will be used to access the .wav and .mkv files.
            String videoFilename;
            if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
            {
                videoFilename = String.Format("{0}.{1}.{2}", reversal.Name, reversalNumber, reversalRate.ReversalSpeed);
            }
            else
            {
                videoFilename = String.Format("{0}.{1}.{2}-{3}", reversal.Name, reversalNumber, reversalRate.ReversalSpeed, reversalRate.ReversalTone);
            }

            // Calculate the frames per second for this reversal.
            float reversalFps = FramesPerSecond * ((float)reversalRate.ReversalSpeed / (float)100);

            // Create the ffmpeg argument string.
            String arguments = String.Format("-y -framerate {0:0.######} -i \"{1}\\r{2}.%06d.png\" -i \"{3}{4}\" {6} \"{3}{5}\"",
                    FramesPerSecond * ((float)reversalRate.ReversalSpeed / 100f),
                    FRAMES_DIR,
                    reversal.Name,
                    videoFilename,
                    OutputAudioInterimExtension,
                    OutputVideoInterimExtension,
                    OutputImageSequenceSettings);

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

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            if (!(ExitCode == 0))
            {
                return false;
            }

            // Calculate the length, in seconds, of this clip. The slow reversal frame count, when adjusted back to the count of frames for the 
            // output video, can leave a fractional number of frames. This fractional amount will be counted as a full frame in the output video. 
            // Ie., 30 frames at 29fps for the reversal = 1.03448 seconds. 1.03448 * 30fps for the output video = 31.0344 frames, which then 
            // will generate 32 frames in the output video. 32 frames / 30fps = 1.066667 seconds.
            // float seconds = frameCount / reversalFps;
            // float outputFrameCount = seconds * FramesPerSecond;
            // if (outputFrameCount > Math.Floor(outputFrameCount)) { outputFrameCount = (float)Math.Floor(outputFrameCount + (float)1); }

            // Add the video clip to the list of clips.
            CreatedClipList.Add(videoFilename + ".mp4");

            return true;
        }

        /// <summary>
        /// Create the still images for the first and last frame of each forward video clip, 
        /// and opening and closing frames for the source video.
        /// </summary>
        private bool CreateForwardStills()
        {
            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            {
                if (CreateFirstAndLastImages(forwardBookmark) == false) { return false; }
            }

            CreateOpeningAndClosingImages();

            return true;
        }

        /// <summary>
        /// Creates .png files for the first and last frame of the specified forwardBookmark.
        /// </summary>
        /// <param name="bookmark">The forwardBookmark to use to create the first and last frame images.</param>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateFirstAndLastImages(Bookmark bookmark)
        {
            // Save the first frame.
            if (CreateStillImage(bookmark, bookmark.SampleStart, "First") == false) { return false; }

            // Save the last frame.
            if (CreateStillImage(bookmark, bookmark.SampleEnd, "Last") == false) { return false; }

            return true;
        }

        /// <summary>
        /// Creates a .png still image file from the specified location (measured in samples), using the specified filename suffix.
        /// </summary>
        /// <param name="bookmark">The forwardBookmark of the clip.</param>
        /// <param name="sample">The sample location in the source video file</param>
        /// <param name="suffix">The suffix to add to the filename (typically ".First" or ".Last").</param>
        /// <returns></returns>
        private bool CreateStillImage(Bookmark bookmark, int sample, string suffix)
        {
            Process process = new Process();
            float timeInSeconds = (float)sample / (float)SampleRate;
            float lengthInSeconds = (1f / FramesPerSecond) - (1f / 8192f);      // I subtract a small amount to make sure I get only one frame.

            // Adjust for the audio delay.
            timeInSeconds += (float)settings.AudioDelay / 1000f;
            timeInSeconds = timeInSeconds < 0 ? 0 : timeInSeconds;

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -ss {0:0.######} -i \"{1}\" -an -qscale 1 -t {2:0.######} -f image2 \"{3}.{4}.png\"",
                    timeInSeconds, WorkingInputVideoFile, lengthInSeconds, bookmark.Name, suffix),
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

            if (!(ExitCode == 0))
            {
                return false;
            }

            return true;
        }


        private bool CreateOpeningAndClosingImages()
        {
            CreateOpeningImage();

            CreateClosingImage();

            return true;
        }

        private bool CreateOpeningImage()
        {
            Process process = new Process();
            float lengthInSeconds = (1f / FramesPerSecond) - (1f / 8192f);      // I subtract a small amount to make sure I get only one frame.

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -ss 0.0 -i \"{0}\" -an -qscale 1 -t {1:0.######} -f image2 \"OpeningFrame.png\"",
                    WorkingInputVideoFile, lengthInSeconds),
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

            if (!(ExitCode == 0))
            {
                return false;
            }

            return true;
        }

        private bool CreateClosingImage()
        {
            Process process = new Process();
            float lengthInSeconds = (1f / FramesPerSecond) - (1f / 8192f);      // I subtract a small amount to make sure I get only one frame.
            int ExitCode = 0;
            bool frameCreated = false;

            // Loop through, starting from the reported end of the video, going back one hundredth of a second,
            // until we successfully get a frame.

            for (ClosingFrameTime = SourceVideoDuration; ClosingFrameTime > SourceVideoDuration - 2; ClosingFrameTime -= lengthInSeconds)
            {
                // Configure the process using the StartInfo properties.
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = FfmpegApp,
                    Arguments = String.Format("-y -ss {0:0.######} -i \"{1}\" -an -qscale 1 -t {2:0.######} -f image2 \"ClosingFrame.png\"",
                        ClosingFrameTime, WorkingInputVideoFile, lengthInSeconds),
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
                ExitCode = process.ExitCode;
                process.Close();

                if (!(ExitCode == 0))
                {
                    return false;
                }

                Match match = Regex.Match(FfmpegOutput, @"\r\nOutput file is empty,");
                if (!match.Success) { frameCreated = true; break; }
            }

            return frameCreated;
        }

        /// <summary>
        /// Creates the text files containing the text of the forward and backward bookmarks, and the 
        /// forward and backward explanations.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateTextImageFiles()
        {
            CreateOverlayPngFiles();
            CreateOpeningAndClosingCards();

            return true;
        }

        /// <summary>
        /// Creates .png files containing the overlay text.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateOverlayPngFiles()
        {
            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            {
                // For Forward and Reverse, create a text overly from the forward bookmark text.
                if (settings.BookmarkTypeFnR)
                {
                    // Write the text of the forward speech to a .png file.
                    CreateForwardTextOverlay(forwardBookmark.Name + ".Text.png", forwardBookmark.Text);
                }

                // if Orphaned Reversals or Quick Check, display the reverse bookmark name and text.
                if (settings.BookmarkTypeQuickCheck || settings.BookmarkTypeOrphanedReversals)
                {
                    // Get the first referenced bookmark.
                    Bookmark reverseBookmark = forwardBookmark.ReferencedBookmarks[0];

                    // Save to the forward name ".Text.png", the name and text of the reverse bookmark.
                    CreateForwardTextOverlay(forwardBookmark.Name + ".Text.png", reverseBookmark.Name + ": " + reverseBookmark.Text);
                }

                // Write the forward explanation to a .png file.
                if (String.IsNullOrWhiteSpace(forwardBookmark.Explanation) == false)
                {
                    CreateCard(forwardBookmark.Name + ".Explanation.png", forwardBookmark.Explanation);
                }

                for (int i = 0; i < forwardBookmark.ReferencedBookmarks.Count; ++i)
                {
                    Bookmark reverseBookmark = forwardBookmark.ReferencedBookmarks[i];

                    // Write the text of the reverse speech to a .png file.
                    if (settings.BookmarkTypeFnR)
                    {
                        CreateReverseTextOverlay(reverseBookmark.Name + ".Text.png", forwardBookmark.Text, reverseBookmark.Text, i);
                    }

                    if (settings.BookmarkTypeQuickCheck || settings.BookmarkTypeOrphanedReversals)
                    {
                        CreateForwardTextOverlay(reverseBookmark.Name + ".Text.png", reverseBookmark.Name + ": " + reverseBookmark.Text);
                    }
                    
                    // Write the reverse explanation to a .png file.
                    if (String.IsNullOrWhiteSpace(reverseBookmark.Explanation) == false)
                    {
                        CreateCard(reverseBookmark.Name + ".Explanation.png", reverseBookmark.Explanation);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the opening and closing cards.
        /// </summary>
        private bool CreateOpeningAndClosingCards()
        {
            // Get the opening and closing card text from the right source.
            if (settings.SourceBookmarkFile)
            {
                OpeningCard = BokOpeningCard;
                ClosingCard = BokClosingCard;
            }
            if (settings.SourceTranscriptFile)
            {
                OpeningCard = TxtOpeningCard;
                ClosingCard = TxtClosingCard;
            }

            if (String.IsNullOrWhiteSpace(OpeningCard) == false)
            {
                CreateCard("OpeningCard.png", OpeningCard);
            }

            if (String.IsNullOrWhiteSpace(ClosingCard) == false)
            {
                CreateCard("ClosingCard.png", ClosingCard);
            }

            return true;
        }

        /// <summary>
        /// Word wraps the string to the pixelWidth.
        /// </summary>
        /// <param name="sourceString">String to word wrap.</param>
        /// <returns>The word wrapped string.</returns>
        String WordWrap(String sourceString)
        {
            return WordWrap(sourceString, (int)(HorizontalResolution * 0.8f), FontForward);
        }

        /// <summary>
        /// Word wraps the string the pixelWidth.
        /// </summary>
        /// <param name="sourceString">tring to word wrap.</param>
        /// <param name="textOverlayFont">The font to use when measuring for word wrap.</param>
        /// <returns>The word wrapped string.</returns>
        String WordWrap(String sourceString, Font textOverlayFont)
        {
            return WordWrap(sourceString, (int)(HorizontalResolution * 0.8f), textOverlayFont);
        }

        /// <summary>
        /// Word wraps the string the pixelWidth.
        /// </summary>
        /// <param name="sourceString">String to word wrap.</param>
        /// <param name="pixelWidth">Width, in pixels, for the text.</param>
        /// <param name="textOverlayFont">The font to use when measuring for word wrap.</param>
        /// <returns>The word wrapped string.</returns>
        String WordWrap(String sourceString, int wrapPixelWidth, Font textOverlayFont)
        {
            StringBuilder wordWrapped = new StringBuilder();
            StringBuilder line = new StringBuilder();
            StringBuilder whitespaceSb = new StringBuilder();
            StringBuilder nonWhitespaceSb = new StringBuilder();
            bool inWhitespace = true;

            for (int i = 0; i < sourceString.Length; ++i)
            {
                Char c = sourceString[i];

                // Are we at the end of the line?
                if (c == '\r')
                {
                    // Remove the \n if it's there.
                    if (i + 1 < sourceString.Length && sourceString[i + 1] == '\n')
                    {
                        ++i;
                    }

                    // If this is not the first line, add \r\n to the word wrapped string.
                    if (wordWrapped.Length > 0)
                    {
                        wordWrapped.Append("\r\n");
                    }

                    // Add any pending characters to line.
                    if (inWhitespace == false)
                    {
                        line.Append(nonWhitespaceSb.ToString());
                        nonWhitespaceSb = new StringBuilder();
                    }
                    else
                    {
                        inWhitespace = false;
                        line.Append(whitespaceSb.ToString());
                        whitespaceSb = new StringBuilder();
                    }

                    // Add the line to the word wrapped string.
                    wordWrapped.Append(line.ToString());

                    // Empty line.
                    line = new StringBuilder();
                }

                // Is this a whitespace character?
                else if (Char.IsWhiteSpace(c))
                {
                    // Did we just transition into whitespace?
                    if (inWhitespace == false)
                    {
                        // If we did transition, add the non-whitespace string to the line. Then clear it.
                        inWhitespace = true;
                        line.Append(nonWhitespaceSb);
                        nonWhitespaceSb = new StringBuilder();
                    }

                    // Add this whitespace character to the whitespace string.
                    whitespaceSb.Append(c);
                }

                // This a non-whitespace character.
                else
                {
                    // Did we just transition out of whitespace?
                    if (inWhitespace == true)
                    {
                        inWhitespace = false;
                        line.Append(whitespaceSb);
                        whitespaceSb = new StringBuilder();
                    }

                    // Does this character put us over the line limit?
                    if (PixelWidth(line.ToString() + nonWhitespaceSb.ToString() + c, textOverlayFont) > wrapPixelWidth)
                    {
                        // If this is not the first line, add \r\n.
                        if (wordWrapped.Length > 0)
                        {
                            wordWrapped.Append("\r\n");
                        }

                        // If line is empty, then this is a bunch of unbroken characters. Break it up.
                        if (line.Length == 0)
                        {
                            wordWrapped.Append(nonWhitespaceSb.ToString());
                        }
                        else
                        {
                            wordWrapped.Append(line.ToString());
                            line = new StringBuilder();
                        }
                    }

                    // Add the character.
                    nonWhitespaceSb.Append(c);
                }
            }

            // The last character has been added. Add the last line.
            if (inWhitespace == false)
            {
                line.Append(nonWhitespaceSb.ToString());
            }

            // If this is not the first line, add \r\n before adding the final line.
            if (wordWrapped.Length > 0)
            {
                wordWrapped.Append("\r\n");
            }

            wordWrapped.Append(line.ToString());

            // Return the word wrapped string.
            return (wordWrapped.ToString());
        }

        /// <summary>
        /// Returns the pixel width of the specified string at the specified font size.
        /// </summary>
        /// <param name="line">The string to measure.</param>
        /// <param name="fontSize">The font size in points.</param>
        /// <returns></returns>
        int PixelWidth(String line, Font textOverlayFont)
        {
            Size textSize = TextRenderer.MeasureText(line, textOverlayFont);
            int pixelWidth = textSize.Width;
            return pixelWidth;
        }

        /// <summary>
        /// Creates a transparent image with the specified text near the bottom of the image, 
        /// and writes the image to the specified filename.
        /// </summary>
        /// <param name="filename">The filename to save the image.</param>
        /// <param name="text">The text to draw on the image.</param>
        private void CreateForwardTextOverlay(String filename, String text)
        {
            CreateReverseTextOverlay(filename, text, null, -1);
        }

        /// <summary>
        /// Creates the .png of the forward and reverse text to overlay onto video. Highlights the bracketed text in the
        /// forwardText as indicated by highlightBracketCount.
        /// </summary>
        /// <param name="forwardText">The forward text to display.</param>
        /// <param name="reverseText">The reverse text to display.</param>
        /// <param name="highlightBracketCount">The index of the forward text to be highlighted, ie., [zero] [one] [two].</param>
        private void CreateReverseTextOverlay(String filename, String forwardText, String reverseText, int highlightBracketCount)
        {
            // Word wrap the strings.
            String wwForwardText = WordWrap(forwardText);
            String wwReverseText = null;
            if (String.IsNullOrWhiteSpace(reverseText) == false) { wwReverseText = WordWrap(reverseText); }

            // Split the word wrapped string into separate strings, one per line.
            String[] forwardlines = wwForwardText.Split(new String[] { "\r\n" }, StringSplitOptions.None);

            // Create a new bitmap.
            Bitmap overlayBmp = new Bitmap(HorizontalResolution, VerticalResolution, PixelFormat.Format32bppArgb);

            // Create graphic object that will draw onto the bitmap.
            Graphics g = Graphics.FromImage(overlayBmp);

            // Initialize the bitmap with 100% alpha.
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 255, 255, 255)))
            {
                g.FillRectangle(brush, 0, 0, overlayBmp.Width, overlayBmp.Height);
            }

            // Set the alignment for the forward text.
            StringFormat formatForward = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Near,
                Trimming = StringTrimming.None,
            };

            // Measure the text sizes.
            SizeF textForwardSize = g.MeasureString(wwForwardText, FontForward, (int)(overlayBmp.Width * 0.8f), formatForward);
            Size textReverseSize = new Size(0, 0);
            if (String.IsNullOrWhiteSpace(reverseText) == false) { textReverseSize = TextRenderer.MeasureText(wwReverseText, FontReverse); }

            // Set the height of the text to the forward height plus the reverse height plus half a line.
            int textHeight;
            if (String.IsNullOrWhiteSpace(reverseText) == false)
            {
                textHeight = (int)(Math.Ceiling(textForwardSize.Height)) + textReverseSize.Height + FontForward.Height / 2;
            }
            else
            {
                textHeight = (int)(Math.Ceiling(textForwardSize.Height));
            }

            // Create a rectangle for the background.
            Rectangle rectBackground = new Rectangle(0, (int)(overlayBmp.Height * 0.9f - textHeight), overlayBmp.Width, textHeight + (int)((float)overlayBmp.Height / 80.0f));

            // Create a rectangle for the forward and reverse text.
            RectangleF rectForward = new RectangleF(0.0f, (float)overlayBmp.Height * 0.9f - textForwardSize.Height, (float)overlayBmp.Width + 32f, textForwardSize.Height);
            Rectangle rectReverse = new Rectangle(0, 0, 0, 0);
            if (String.IsNullOrWhiteSpace(reverseText) == false)
            {
                rectReverse = new Rectangle(0, (int)(overlayBmp.Height * 0.9f - textHeight), overlayBmp.Width + 32, textReverseSize.Height);
            }

            // Draw the text background (a transparent black rectangle).
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(BackgroundAlpha, 0, 0, 0)))
            {
                g.FillRectangle(brush, rectBackground);
            }

            // ------------------------------------------
            // Ensure the best possible quality rendering
            // ------------------------------------------
            // The smoothing mode specifies whether lines, curves, and the edges of filled areas use smoothing (also called antialiasing). 
            // One exception is that path gradient brushes do not obey the smoothing mode. 
            // Areas filled using a PathGradientBrush are rendered the same way (aliased) regardless of the SmoothingMode property.
            g.SmoothingMode = SmoothingMode.HighQuality;

            // The interpolation mode determines how intermediate values between two endpoints are calculated.
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Use this property to specify either higher quality, slower rendering, or lower quality, faster rendering of the contents of this Graphics object.
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // This one is important.
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Create string formatting options (used for alignment).
            StringFormat formatReverse = new StringFormat()
            {
                Alignment = StringAlignment.Center,
            };

            // Draw the reverse text onto the image.
            if (String.IsNullOrWhiteSpace(reverseText) == false)
            {
                g.DrawString(wwReverseText, FontReverse, Brushes.White, rectReverse, formatReverse);
            }

            // True if the highlight (underline) continues from the previous line.
            Boolean highlightUnderway = false;

            // Count the brackets so as to underline the correct bracketed text.
            int bracketCount = 0;

            // Add the forward text to the image.
            for (int i = 0; i < forwardlines.Length; ++i)
            {
                String searchString = forwardlines[i];

                // Get the full size of this line.
                SizeF lineSize = g.MeasureString(searchString, FontForward, overlayBmp.Width, formatForward);

                // Create the full size rectangle for this line.
                float x = ((float)overlayBmp.Width / 2.0f) - (lineSize.Width / 2.0f);
                float y = rectForward.Y + (float)(FontForward.Height * i);
                float width = lineSize.Width;
                float height = lineSize.Height;
                RectangleF rectLine = new RectangleF(x, y, width, height);

                // Set the starting point for rendering text.
                float textDrawPositionX = 0.0f;

                // Search through the line.
                int searchIndex = 0;
                while (searchIndex < searchString.Length)
                {
                    int bracketPos = 0;

                    // Are we continuing a highlight from the previous line?
                    if (highlightUnderway == true)
                    {
                        // Search for the closing square bracket.
                        bracketPos = searchString.IndexOf(']', searchIndex);

                        // If the bracket was not found
                        if (bracketPos == -1)
                        {
                            // Since we're currently underlining, draw underlined searchString from searchIndex to end of line.
                            //
                            // Get text draw size.
                            String textToDraw = searchString.Substring(searchIndex);
                            SizeF textDrawSize = g.MeasureString(textToDraw, FontForwardUnderline, overlayBmp.Width, formatForward);

                            // Create the text rectangle.
                            RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForward.Height);

                            // Draw the text onto the rectangle.
                            g.DrawString(textToDraw, FontForwardUnderline, Brushes.White, rectDrawText, formatForward);

                            // We're done with this line, so break out of the loop.
                            break;
                        }
                        // The closing bracket was found.
                        else
                        {
                            // Draw underline searchString from searchIndex to (searchIndex + bracketPos - 1)
                            //
                            // Get text draw size.
                            String textToDraw = searchString.Substring(searchIndex, bracketPos - searchIndex);
                            SizeF textDrawSize = g.MeasureString(textToDraw, FontForwardUnderline, overlayBmp.Width, formatForward);

                            // Create the text rectangle.
                            RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForward.Height);

                            // Draw the text onto the rectangle.
                            g.DrawString(textToDraw, FontForwardUnderline, Brushes.White, rectDrawText, formatForward);

                            // Update the text position for the next.
                            textDrawPositionX += textDrawSize.Width;

                            // Update the searchIndex.
                            searchIndex = bracketPos;

                            // Indicate that the highlight is no longer underway.
                            highlightUnderway = false;
                        }
                    }
                    // No highlight from the previous line.
                    else
                    {
                        // Search for the opening square bracket.
                        bracketPos = searchString.IndexOf('[', searchIndex);

                        // If the bracket was not found.
                        if (bracketPos == -1)
                        {
                            // Draw non-underlined searchString from searchIndex to end of line.
                            //
                            // Get text draw size.
                            String textToDraw = searchString.Substring(searchIndex);
                            SizeF textDrawSize = g.MeasureString(textToDraw, FontForward, overlayBmp.Width, formatForward);

                            // Create the text rectangle.
                            RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForward.Height);

                            // Draw the text onto the line.
                            g.DrawString(textToDraw, FontForward, Brushes.White, rectDrawText, formatForward);

                            // break out of the loop.
                            break;
                        }
                        // The opening bracket was found.
                        else
                        {
                            // Display the text before the [, and the [.
                            //
                            // Get text draw size.
                            String textToDrawPre = searchString.Substring(searchIndex, bracketPos - searchIndex + 1);
                            SizeF textDrawPreSize = g.MeasureString(textToDrawPre, FontForward, overlayBmp.Width, formatForward);

                            // Create the text rectangle.
                            RectangleF rectDrawPreText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawPreSize.Width, FontForward.Height);

                            // Draw the text onto the rectangle.
                            g.DrawString(textToDrawPre, FontForward, Brushes.White, rectDrawPreText, formatForward);

                            // Update the text position for the next.
                            textDrawPositionX += textDrawPreSize.Width;

                            // Adjust to skip over the opening square bracket (being aware that the bracket 
                            // may be at the end of the string, or the string may be zero length).
                            searchIndex = bracketPos + 1;
                            if (searchIndex >= searchString.Length)
                            {
                                searchIndex = searchString.Length > 0 ? searchString.Length - 1 : 0;
                            }

                            // Is this the bracket we're searching for?
                            if (bracketCount == highlightBracketCount)
                            {
                                // Increment the bracket count.
                                ++bracketCount;

                                // Search for the closing square bracket.
                                bracketPos = searchString.IndexOf(']', searchIndex);

                                // If the closing square bracket was found.
                                if (bracketPos > -1)
                                {
                                    // Draw underlined searchString.
                                    //
                                    // Get text draw size.
                                    String textToDraw = searchString.Substring(searchIndex, bracketPos - searchIndex);
                                    SizeF textDrawSize = g.MeasureString(textToDraw, FontForwardUnderline, overlayBmp.Width, formatForward);

                                    // Create the text rectangle.
                                    RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForwardUnderline.Height);

                                    // Draw the text onto the rectangle.
                                    g.DrawString(textToDraw, FontForwardUnderline, Brushes.White, rectDrawText, formatForward);

                                    // Update the text position for the next.
                                    textDrawPositionX += textDrawSize.Width;

                                    // Update the searchIndex.
                                    searchIndex = bracketPos;
                                }
                                else
                                {
                                    // Can the closing bracket be found on a subsequent line?
                                    int closingBracketPos = -1;
                                    for (int j = i + 1; j < forwardlines.Length; ++j)
                                    {
                                        closingBracketPos = forwardlines[j].IndexOf(']');
                                        if (closingBracketPos > -1) { break; }
                                    }

                                    // If the closing bracket was found.
                                    if (closingBracketPos > -1)
                                    {
                                        // Draw underlined searchString from searchIndex to end of line.
                                        //
                                        // Get text draw size.
                                        String textToDraw = searchString.Substring(searchIndex);
                                        SizeF textDrawSize = g.MeasureString(textToDraw, FontForwardUnderline, overlayBmp.Width, formatForward);

                                        // Create the text rectangle.
                                        RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForwardUnderline.Height);

                                        // Draw the text onto the rectangle.
                                        g.DrawString(textToDraw, FontForwardUnderline, Brushes.White, rectDrawText, formatForward);

                                        // Indicate a multiline highlight is underway
                                        highlightUnderway = true;

                                        // Break out of the loop.
                                        break;
                                    }
                                    // The closing bracket was not found, so this will not be highlighted.
                                    else
                                    {
                                        // Draw non-underlined searchString from searchIndex to end of line.
                                        //
                                        // Get text draw size.
                                        String textToDraw = searchString.Substring(searchIndex);
                                        SizeF textDrawSize = g.MeasureString(textToDraw, FontForward, overlayBmp.Width, formatForward);

                                        // Create the text rectangle.
                                        RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForward.Height);

                                        // Draw the text onto the rectangle.
                                        g.DrawString(searchString.Substring(searchIndex), FontForward, Brushes.White, rectDrawText, formatForward);

                                        // Break out of the loop.
                                        break;
                                    }
                                }

                            }
                            // This was not the bracket we're searching for. Display the non-underlined text after the bracket.
                            else
                            {
                                String textToDraw;

                                // Increment the bracket count.
                                ++bracketCount;

                                // Search for the closing square bracket.
                                bracketPos = searchString.IndexOf(']', searchIndex);

                                // If the closing square bracket was found.
                                if (bracketPos > -1)
                                {
                                    // Get text draw size up to closing square bracket.
                                    textToDraw = searchString.Substring(searchIndex, bracketPos - searchIndex);
                                }
                                else
                                {
                                    // Get text draw size to end of string.
                                    textToDraw = searchString.Substring(searchIndex);
                                }

                                SizeF textDrawSize = g.MeasureString(textToDraw, FontForward, overlayBmp.Width, formatForward);

                                // Create the text rectangle.
                                RectangleF rectDrawText = new RectangleF(rectLine.X + textDrawPositionX, rectLine.Y, textDrawSize.Width, FontForward.Height);

                                // Draw the text onto the rectangle.
                                g.DrawString(textToDraw, FontForward, Brushes.White, rectDrawText, formatForward);

                                // Update the text position for the next string.
                                textDrawPositionX += textDrawSize.Width;

                                // Update the searchIndex.
                                if (bracketPos > -1)
                                {
                                    searchIndex = bracketPos;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Flush all graphics changes to the bitmap.
            g.Flush();

            // Save the text overlay as a .png file.
            overlayBmp.Save(filename, ImageFormat.Png);

            // Dispose of the bitmap and graphics object.
            overlayBmp.Dispose();
            g.Dispose();
        }

        /// <summary>
        /// Creates a text card image with the specified text, 
        /// and writes the image to the specified filename.
        /// </summary>
        /// <param name="filename">The filename to save the image.</param>
        /// <param name="text">The text to draw on the image.</param>
        private void CreateCard(String filename, String text)
        {
            // Determine if this is a centered card.
            Boolean cardCentered = text[0] == '^' ? true : false;

            if (cardCentered)
            {
                // Remove the caret.
                text = text.Substring(1);
            }

            if (String.IsNullOrWhiteSpace(text)) { return; }

            // Word wrap the string.
            // String wordWrapped = WordWrap(text, FontForward);
            String wordWrapped = text;

            // Create new bitmap the same size as the image.
            Bitmap cardBmp = new Bitmap(HorizontalResolution, VerticalResolution, PixelFormat.Format32bppArgb);

            // Create graphic object that will draw onto the bitmap.
            Graphics g = Graphics.FromImage(cardBmp);

            // Initialize the bitmap with black.
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, Color.Black)))
            {
                g.FillRectangle(brush, 0, 0, cardBmp.Width, cardBmp.Height);
            }

            // Measure the text size.
            Size textSize = TextRenderer.MeasureText(wordWrapped, FontForward);

            // Create a rectangle for the text display.
            Rectangle rect = new Rectangle((int)((float)cardBmp.Width * 0.1f), (int)((float)cardBmp.Height * 0.1f), (int)(cardBmp.Width * 0.8f), (int)(cardBmp.Height * 0.8f));

            // ------------------------------------------
            // Ensure the best possible quality rendering
            // ------------------------------------------
            // The smoothing mode specifies whether lines, curves, and the edges of filled areas use smoothing (also called antialiasing). 
            // One exception is that path gradient brushes do not obey the smoothing mode. 
            // Areas filled using a PathGradientBrush are rendered the same way (aliased) regardless of the SmoothingMode property.
            g.SmoothingMode = SmoothingMode.HighQuality;

            // The interpolation mode determines how intermediate values between two endpoints are calculated.
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Use this property to specify either higher quality, slower rendering, or lower quality, faster rendering of the contents of this Graphics object.
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // This one is important.
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Create string formatting options (used for alignment).
            StringFormat format = new StringFormat()
            {
                Alignment = cardCentered ? StringAlignment.Center : StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };

            // Draw the text onto the image.
            g.DrawString(wordWrapped, FontForward, Brushes.White, rect, format);

            // Flush all graphics changes to the bitmap.
            g.Flush();

            // Now overlayBmp is ready to save out as a text overlay .png file.
            // pictureBox1.Image = overlayBmp;

            // Save the text overlay as a .png file.
            cardBmp.Save(filename, ImageFormat.Png);

            // Dispose of the bitmap and graphics object.
            cardBmp.Dispose();
            g.Dispose();
        }
    }
}