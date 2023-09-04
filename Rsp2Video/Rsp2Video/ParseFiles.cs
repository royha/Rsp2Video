using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace RSPro2Video
{
    public partial class RSPro2VideoForm : Form
    {
        /// <summary>
        /// Parses the files for important values like audio sample rate, frame rate, and 
        /// forward and reverse sections.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ValidateAndParseFiles()
        {
            // Clear the error messages.
            //labelSourceVideoFileError.Visible = false;
            //labelSoundFileError.Visible = false;
            //labelTranscriptFileError.Visible = false;
            labelOutputVideoFileError.Visible = false;
            labelVideoOffsetError.Visible = false;
            labelMainFileError.Visible = false;

            // Find and set the video file and set the project file.
            if (FindFilesFromMainFile() == false)
            {
                return false;
            }

            // Set logfile location.
            if (SetLogFileLocation() == false)
            {
                return false;
            }

            // Validate and parse the video file.
            if (ValidateAndParseVideo() == false)
            {
                return false;
            }

            // Validate and parse the audio.
            //if (ValidateAndParseAudio() == false)
            //{
            //    return false;
            //}

            // Validate and parse the bookmark file.
            if (ValidateAndParseBookmarks() == false)
            {
                return false;
            }

            // Validate and parse the transcript file.
            //if (ValidateAndParseTranscript() == false)
            //{
            //    return false;
            //}

            // Validate ouptut video file.
            if (ValidateOutputVideoFile() == false)
            {
                return false;
            }

            // Validate video offset.
            if (ValidateVideoOffset() == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds and stores the source video filename and the project filename based on 
        /// the bookmark filename and location.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool FindFilesFromMainFile()
        {
            String mainFile = String.Empty;

            try
            {
                mainFile = Path.GetFullPath(textBoxMainFile.Text).Trim();
            }
            catch (ArgumentNullException)
            {
                labelMainFileError.Text = "You must specify a bookmark or project file.";
                labelMainFileError.Visible = true;
                return false;
            }
            catch (NotSupportedException)
            {
                labelMainFileError.Text = "The path is not valid.";
                labelMainFileError.Visible = true;
                return false;
            }
            catch (PathTooLongException)
            {
                labelMainFileError.Text = "The path is not valid.";
                labelMainFileError.Visible = true;
                return false;
            }
            catch (SecurityException)
            {
                labelMainFileError.Text = "You do not have permissions to access this file.";
                labelMainFileError.Visible = true;
                return false;
            }
            catch (ArgumentException)
            {
                labelMainFileError.Text = "The path is not valid.";
                labelMainFileError.Visible = true;
                return false;
            }
            catch (Exception e)
            {
                // Update the error label text with an appropriate error message.
                if (String.IsNullOrWhiteSpace(textBoxMainFile.Text) == true)
                {
                    labelMainFileError.Text = "You must specify a bookmark or project file.";
                    labelMainFileError.Visible = true;
                }
                else
                {
                    labelMainFileError.Text = "The path is not valid.";
                    labelMainFileError.Visible = true;
                }

                return false; 
            }

            // Get the file extension of the bookmark file.
            String mainFileExtension = Path.GetExtension(mainFile).Trim().ToLower();

            switch (mainFileExtension)
            {
                // RSPro bookmark file.
                case ".fmbok":
                case ".bok":
                    // Verify the bookmark file exists.
                    if (File.Exists(mainFile) == false)
                    {
                        return false;
                    }

                    // Set the bookmark file.
                    ProgramSettings.BookmarkFile = mainFile;

                    // Set the bookmark file type.
                    if (mainFileExtension == ".fmbok")
                    {
                        ProgramSettings.BookmarkFileType = BookmarkFileType.FmBok;
                    }
                    else if (mainFileExtension == ".bok")
                    {
                        ProgramSettings.BookmarkFileType = BookmarkFileType.bok;
                    }


                    // Find the first video file to match.
                    foreach (String extension in new String[] { ".mp4", ".webm", ".avi", ".mov", ".mkv", ".mpg", ".mpeg", ".wmv" })
                    {
                        String videoFilepath = Path.ChangeExtension(ProgramSettings.BookmarkFile, extension);
                        if (File.Exists(videoFilepath) == true)
                        {
                            // Load the project file or create it if it doesn't exist.
                            String projectFilepath = videoFilepath + ".RSPro2Video";
                            LoadProjectSettings(projectFilepath);

                            // Store the project file.
                            ProgramSettings.ProjectFile = projectFilepath;

                            // Store the video file in the project settings.
                            ProjectSettings.SourceVideoFile = videoFilepath;

                            return true;
                        }
                    }

                    // No matching video file was found.
                    return false;

                // RSVideo bookmark file.
                case ".rsvideo":
                    // Verify the bookmark file exists.
                    if (File.Exists(mainFile) == false)
                    {
                        return false;
                    }

                    // Set the bookmark file and file type.
                    ProgramSettings.BookmarkFile = mainFile;
                    ProgramSettings.BookmarkFileType = BookmarkFileType.RSVideo;

                    // RSVideo bookmark file. Remove ".rsvideo" from the end of the filepath to get the video file path.
                    String rsVideoFilepath = mainFile.Substring(0, mainFile.Length - 8);

                    // Verify the video file exists.
                    if (File.Exists(rsVideoFilepath) == true)
                    {
                        // Load the project file or create it if it doesn't exist.
                        String projectFilepath = rsVideoFilepath + ".RSPro2Video";
                        LoadProjectSettings(projectFilepath);

                        // Store the project file.
                        ProgramSettings.ProjectFile = projectFilepath;

                        // Store the video file in the project settings.
                        ProjectSettings.SourceVideoFile = rsVideoFilepath;
                    }
                    else
                    {
                        // The video file was not found.
                        labelMainFileError.Text = String.Format("The video file was not found: {0}", Path.GetFileName(rsVideoFilepath));
                        labelMainFileError.Visible = true;
                        return false;
                    }
                    break;

                default:
                    labelMainFileError.Text = "This is not a recognized bookmark or project file type.";
                    labelMainFileError.Visible = true;
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a log file using the source video filename + ".log"
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool SetLogFileLocation()
        {
            // Add ".log" to the end of the full path and filename of the source video file, just like Kdenlive.
            LogFile = Path.GetFullPath(ProjectSettings.SourceVideoFile) + ".log";

            // Delete the log file.
            try
            {
                File.Delete(LogFile);
            }
            catch { }

            // Write the initial log entry.
            String LogEntry = String.Format("***Log start time: {0}\r\nFilename: {1}\r\n\r\n",
                DateTime.Now.ToString(), ProjectSettings.SourceVideoFile);

            File.AppendAllText(LogFile, LogEntry);

            return true;
        }

        /// <summary>
        /// Parses the audio file to determine the sample rate of the file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        /// &&& This must be replaced with ffmpeg.
        /// ffprobe -v error -select_streams a -of default=noprint_wrappers=1:nokey=1 -show_entries stream=sample_rate
        private bool ValidateAndParseAudio()
        {
            Process process = new Process();

            // This ffprobe string returns the sample rate in text, and only the sample rate in text.
            String arguments = String.Format("-v error -select_streams a -of default=noprint_wrappers=1:nokey=1 -show_entries stream=sample_rate \"{0}\"",
                ProjectSettings.SourceVideoFile);

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmprobeApp,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Start ffprobe to get the audio file information.
            process.Start();

            // Read the output of ffprobe.
            String FfprobeOutput = process.StandardOutput.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            if (ExitCode != 0)
            {
                MessageBox.Show("There was an error reading " + ProjectSettings.SourceVideoFile + ":" + Environment.NewLine + Environment.NewLine + "Error message: " + FfprobeOutput,
                    "Error reading Reverse Speech Pro sound file");
                return false;
            }

            // Parse for the sample rate.
            if (Int32.TryParse(FfprobeOutput, out int rate))
            {
                SampleRate = rate;
            }
            else
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses a bookmark file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool ValidateAndParseBookmarks()
        {
            Boolean returnValue = true;

            if (ProgramSettings.BookmarkFileType == BookmarkFileType.bok || ProgramSettings.BookmarkFileType == BookmarkFileType.FmBok)
            {
                returnValue = ValidateAndParseBokBookmarks();
            }
            else if (ProgramSettings.BookmarkFileType == BookmarkFileType.RSVideo)
            {
                returnValue = ValidateAndParseRSVideoBookmarks();
            }

            return returnValue;
        }

        /// <summary>
        /// Loads and parses all bookmark data from the .FmBok/.bok file associated with the Reverse Speech Pro sound file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool ValidateAndParseBokBookmarks()
        {
            if (ReadBokBookmarkFile() == false)
            {
                return false;
            }

            if (ExtractBokBookmarkData() == false)
            {
                return false;
            }

            AssembleBokBookmarks();

            return true;
        }

        /// <summary>
        /// Loads and parses all bookmark data from the .RSVideo file associated with the Reverse Speech Video file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool ValidateAndParseRSVideoBookmarks()
        {
            if (ReadRSVideoBookmarkFile() == false)
            {
                return false;
            }

            if (ExtractRSVideoBookmarkData() == false)
            {
                return false;
            }

            HandleOverlappingRSVideoForwardBookmarks();

            BokForwardBookmarks = RsvForwardBookmarks;
            BokReverseBookmarks = RsvReverseBookmarks;

            ConnectFnRBookmarks();

            AssembleOrphanReverseBookmarks();

            return true;
        }

        /// <summary>
        /// Read the .RSVideo file into a ReversalDefinition object.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool ReadRSVideoBookmarkFile()
        {
            RSVideoReversalDefinition = DeSerializeObject<ReversalDefinition>(ProgramSettings.BookmarkFile);

            return true;
        }

        /// <summary>
        /// Parses the .RSVideo file data to create forward and reverse Bookmark data.
        /// </summary>
        /// <returns>Returns true if successful; otherwise, false.</returns>
        private bool ExtractRSVideoBookmarkData()
        {
            String doubleNewLine = Environment.NewLine + Environment.NewLine;

            // Set the number of leading zeros for the bookmark names.
            String forwardFormatString = "F{0:00}";
            String reverseFormatString = "R{0:00}";

            if (RSVideoReversalDefinition.Forward.Count >= 100)
            {
                forwardFormatString = "F{0:000}";
                reverseFormatString = "R{0:000}";
            }

            // Clear out the bookmark lists.
            RsvForwardBookmarks = new List<Bookmark>();
            RsvReverseBookmarks = new List<Bookmark>();

            // Extract forward bookmarks.
            for (int i = 0; i < RSVideoReversalDefinition.Forward.Count; ++i)
            {
                // Skip this bookmark if it is null, empty, or if the starting or ending sample is -1.
                if (RSVideoReversalDefinition.Forward[i].Text == null)
                {
                    continue;
                }

                // Trim the string and change all occurances of {CrLf} to \r\n.
                String text = Regex.Replace(RSVideoReversalDefinition.Forward[i].Text.Trim(), "{CrLf}", Environment.NewLine);

                if (text == String.Empty || text.IndexOf(doubleNewLine) == 0 ||
                    RSVideoReversalDefinition.Forward[i].ForwardStartSampleNo == -1 ||
                    RSVideoReversalDefinition.Forward[i].ForwardEndSampleNo == -1)
                {
                    continue;
                }

                Bookmark newBookmark = new Bookmark();

                // Create bookmark name.
                newBookmark.Name = String.Format(forwardFormatString, i + 1);

                // Determine if there is an explanation.
                int idx = text.IndexOf(doubleNewLine);

                if (idx >= 0)
                {
                    newBookmark.Text = text.Substring(0, idx).Trim();
                    newBookmark.Explanation = text.Substring(idx + doubleNewLine.Length).Trim();
                }
                else
                {
                    newBookmark.Text = text;
                    newBookmark.Explanation = String.Empty;
                }

                // Identify OpeningCard and ClosingCard.
                switch (newBookmark.Text.ToLower())
                {
                    case "openingcard":
                    case "opening card":
                        BokOpeningCard = newBookmark.Explanation;
                        continue;

                    case "closingcard":
                    case "closing card":
                        BokClosingCard = newBookmark.Explanation;
                        continue;
                }

                newBookmark.SampleStart = RSVideoReversalDefinition.Forward[i].ForwardStartSampleNo;
                newBookmark.SampleEnd = RSVideoReversalDefinition.Forward[i].ForwardEndSampleNo;

                RsvForwardBookmarks.Add(newBookmark);
            }

            // Extract reverse bookmarks.
            for (int i = 0; i < RSVideoReversalDefinition.Reversal.Count; ++i)
            {
                // Skip this bookmark if it is null, empty, or if the starting or ending sample is -1.
                if (RSVideoReversalDefinition.Reversal[i].Text == null)
                {
                    continue;
                }

                // Trim the string and change all occurances of {CrLf} to \r\n.
                String text = Regex.Replace(RSVideoReversalDefinition.Reversal[i].Text.Trim(), "{CrLf}", Environment.NewLine);

                if (text == String.Empty || text.IndexOf(doubleNewLine) == 0 ||
                    RSVideoReversalDefinition.Reversal[i].ReverseStartSampleNo == -1 ||
                    RSVideoReversalDefinition.Reversal[i].ReverseEndSampleNo == -1)
                {
                    continue;
                }

                Bookmark newBookmark = new Bookmark();

                // Create bookmark name.
                newBookmark.Name = String.Format(reverseFormatString, i + 1);

                // Determine if there is an explanation.
                int idx = text.IndexOf(doubleNewLine);

                if (idx >= 0)
                {
                    newBookmark.Text = text.Substring(0, idx).Trim();
                    newBookmark.Explanation = text.Substring(idx + doubleNewLine.Length).Trim();
                }
                else
                {
                    newBookmark.Text = text;
                    newBookmark.Explanation = String.Empty;
                }

                // Identify OpeningCard and ClosingCard.
                switch (newBookmark.Text.ToLower())
                {
                    case "openingcard":
                    case "opening card":
                        BokOpeningCard = newBookmark.Explanation;
                        continue;

                    case "closingcard":
                    case "closing card":
                        BokClosingCard = newBookmark.Explanation;
                        continue;
                }

                newBookmark.SampleStart = RSVideoReversalDefinition.Reversal[i].ReverseStartSampleNo;
                newBookmark.SampleEnd = RSVideoReversalDefinition.Reversal[i].ReverseEndSampleNo;

                RsvReverseBookmarks.Add(newBookmark);
            }

            return true;
        }

        /// <summary>
        /// Finds overlapping forward bookmarks to combine.
        /// </summary>
        private void HandleOverlappingRSVideoForwardBookmarks()
        {
            // Check this bookmark ...
            foreach (Bookmark currentBookmark in RsvForwardBookmarks)
            {
                // Skip if the bookmark is already marked for removal.
                if (currentBookmark.Name == "Remove")
                {
                    continue;
                }

                // ... against this bookmark.
                foreach (Bookmark otherBookmark in RsvForwardBookmarks)
                {
                    // Skip comparing the bookmark to itself.
                    if (otherBookmark == currentBookmark)
                    {
                        continue;
                    }

                    // Skip if the bookmark is already marked for removal.
                    if (otherBookmark.Name == "Remove")
                    {
                        continue;
                    }

                    // OOOOOOOOOOOOOOOOOOOOOOO
                    //   CCCCCCCCCCCCCCCCCCC
                    // Is the current bookmark fully contained in the other bookmark?
                    if (currentBookmark.SampleStart >= otherBookmark.SampleStart &&
                        currentBookmark.SampleEnd <= otherBookmark.SampleEnd)
                    {
                        CombineBookmarks(currentBookmark, otherBookmark);
                    }

                    //   OOOOOOOOOOOOOOOOOOOOOOO
                    // CCCCCCCCCCCCCCCCCCCCCCC
                    // Is the start of the other inside the current?
                    else if (otherBookmark.SampleStart >= currentBookmark.SampleStart &&
                        otherBookmark.SampleStart <= currentBookmark.SampleEnd)
                    {
                        // Is the overlap is greater than MinBookmarkOverlap?
                        if (currentBookmark.SampleEnd - otherBookmark.SampleStart >=
                            (currentBookmark.SampleEnd - otherBookmark.SampleStart) * MinBookmarkOverlap)
                        {
                            CombineBookmarks(currentBookmark, otherBookmark);
                        }
                    }

                    // OOOOOOOOOOOOOOOOOOOOOOO
                    //   CCCCCCCCCCCCCCCCCCCCCCC
                    // Is the end of the other inside the current?
                    else if (otherBookmark.SampleEnd >= currentBookmark.SampleStart &&
                        otherBookmark.SampleEnd <= currentBookmark.SampleEnd)
                    {
                        if (otherBookmark.SampleEnd - currentBookmark.SampleStart >=
                            (otherBookmark.SampleEnd - currentBookmark.SampleStart) * MinBookmarkOverlap)
                        {
                            CombineBookmarks(currentBookmark, otherBookmark);
                        }
                    }
                }
            }

            // Remove all bookmarks that are marked for removeal.
            RsvForwardBookmarks.RemoveAll(x => x.Name == "Remove");
        }

        /// <summary>
        /// Combines forward bookmarks in .RSVideo file data.
        /// </summary>
        /// <remarks>If .Text strings (minus any square brackets) are not identical in length, 
        /// chooses the bookmark with the longest .Text string, and deletes <param name="otherBookmark"></param>.
        /// 
        /// If .Text strings are identical (minus any square brackets), combines the square
        /// brackets from <param name="otherBookmark"></param> to <param name="currentBookmark"></param>,
        /// then deletes <param name="currentBookmark"></param>.</remarks>
        /// <param name="bookmarksToDelete">The list of bookmarks to delete.</param>
        /// <param name="currentBookmark"></param>
        /// <param name="otherBookmark"></param>
        private void CombineBookmarks(Bookmark currentBookmark, Bookmark otherBookmark)
        {
            // Create strings without square brackets for comparison purposes.
            string current = Regex.Replace(currentBookmark.Text, @"[\[\]]", String.Empty);
            string other = Regex.Replace(otherBookmark.Text, @"[\[\]]", String.Empty);

            // If current text is longer, remove other.
            if (current.Length > other.Length)
            {
                otherBookmark.Name = "Remove";
                return;
            }

            // If current text is shorter, remove current.
            if (current.Length < other.Length)
            {
                otherBookmark.Name = "Remove";
                return;
            }

            // Text lengths are identical. Combine square brackets.
            // this [is a si]mple string with [square] brackets
            // this is a si[mple string] with [square] brackets

            // If otherBookmark.Text has no opening square bracket, there is nothing to add.
            if (otherBookmark.Text.IndexOf('[') == -1)
            {
                otherBookmark.Name = "Remove";
                return;
            }

            int currentBacketCount = 0;
            int otherBracketCount = 0;

            // Loop through the strings looking for square brackets.
            for (int i = 0; 
                i < currentBookmark.Text.Length - currentBacketCount &&
                i < currentBookmark.Text.Length - otherBracketCount;
                ++i)
            {
                // If a square bracket is found in current, adjust for it.
                if (currentBookmark.Text[i + currentBacketCount] == '[' ||
                    currentBookmark.Text[i + currentBacketCount] == ']')
                {
                    ++currentBacketCount;

                    // If an identical opening square bracket is found in other, skip over it.
                    if (otherBookmark.Text[i + otherBracketCount] == '[' ||
                        otherBookmark.Text[i + otherBracketCount] == ']')
                    {
                        ++otherBracketCount;
                    }
                }

                // If a square bracket is found in other, add it to current.
                else if (otherBookmark.Text[i + otherBracketCount] == '[' ||
                    otherBookmark.Text[i + otherBracketCount] == ']')
                {
                    String s1 = currentBookmark.Text.Substring(0, i + currentBacketCount);
                    String s2 = otherBookmark.Text[i + otherBracketCount].ToString();
                    String s3 = currentBookmark.Text.Substring(i + currentBacketCount);

                    currentBookmark.Text = currentBookmark.Text.Substring(0, i + currentBacketCount) +
                        otherBookmark.Text[i + otherBracketCount] +
                        currentBookmark.Text.Substring(i + currentBacketCount);
                }
            }

            otherBookmark.Name = "Remove";
            return;
        }

        private void AssembleRSVideoBookmarks()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates that the output file is valid, and does not reference the same file as the source video file.
        /// </summary>
        /// <returns></returns>
        private bool ValidateOutputVideoFile()
        {
            String outputPath = null;
            if (textBoxOutputFile.Text == String.Empty)
            {
                labelOutputVideoFileError.Text = "You must specify an output video file.";
                labelOutputVideoFileError.Visible = true;
                return false;
            }
            else if (ProjectSettings.SourceVideoFile.Trim().ToLower() == textBoxOutputFile.Text.Trim().ToLower())
            {
                labelOutputVideoFileError.Text = "The Output video file must not be the same as the Source video file.";
                labelOutputVideoFileError.Visible = true;
                return false;
            }
            else
            {
                try
                {
                    outputPath = Path.GetFullPath(textBoxOutputFile.Text);
                }
                catch (ArgumentNullException)
                {
                    labelOutputVideoFileError.Text = "The path is not valid.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }
                catch (NotSupportedException)
                {
                    labelOutputVideoFileError.Text = "The path is not valid.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }
                catch (PathTooLongException)
                {
                    labelOutputVideoFileError.Text = "The path is not valid.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }
                catch (SecurityException)
                {
                    labelOutputVideoFileError.Text = "You do not have permissions to access this file.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }
                catch (ArgumentException)
                {
                    labelOutputVideoFileError.Text = "The path is not valid.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }

                if (labelOutputVideoFileError.Visible == false &&
                    outputPath == Path.GetFullPath(ProjectSettings.SourceVideoFile))
                {
                    labelOutputVideoFileError.Text = "The Output video file must not be the same as the Source video file.";
                    labelOutputVideoFileError.Visible = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates the video offset.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ValidateVideoOffset()
        {
            // Get the video offset.
            if (textBoxVideoOffset.Text == String.Empty)
            {
                VideoOffset = 0.0d;
            }
            else
            {
                if (Double.TryParse(textBoxVideoOffset.Text, out double offset))
                {
                    VideoOffset = (double)offset;
                }
                else
                {
                    labelVideoOffsetError.Visible = true;
                    return false;
                }
            }

            return true;
        }


        // The .FmBok/.bok file format:
        // 
        // The file starts with a text description. This ends with two NUL bytes (0x0000).
        //     In my experiments, this always starts at 0x00BB, but why take the chance?
        //
        // Note that the text above does not count for 32 bit alignment. Alignment starts
        // from the Count of bookmarks.
        // 
        // The offset from the byte past the double NUL bytes:
        // +0000: Int32     Count of bookmarks.
        // +0004: Int32     Int32 that is the same as the length of bookmark name.
        // +0008: Int32     Actual length of bookmark name.
        // +000C: String    Bookmark name, followed by two NUL bytes.
        //  +0-3: Bytes     Enough NUL bytes to align the next data to a 32 bit boundary.
        // 
        // Offset from 32 bit bounary:
        // +0000: Int32     Unknown Int32.
        // +0004: Int32     Length of bookmark text.
        // +0008: String    Bookmark text, followed by two NUL bytes.
        //  +0-3: Bytes     Enough NUL bytes to align the next data to a 32 bit boundary.
        //
        // Offset from 32 bit boundary:
        // +0000: Int32     Unknown Int32.
        // +0004: Int32     Unknown Int32.
        // +0008: Int32     Unknown Int32.
        // +000C: Int32     Sample start.
        // +0010: Int32     Sample end.
        // +0014: Int32     Unknown Int32.
        //
        // New bookmark begins here:
        // +0018: Int32     Int32 that is the same as the length of next bookmark name.
        // +001C: Int32     Actual length of next bookmark name.
        //
        // Sometimes the bookmark text ends with the following values which 
        // throws off my algorithm by one Int32. I add a check for that specific value.
        byte[] notQuiteThere1 = { 00, 00, 00, 00, 0xFF, 00, 0x80, 00 };
        byte[] notQuiteThere2 = { 00, 00, 00, 00, 0xFF, 00,   00, 00 };

        Byte[] bookmarkFileBytes;

        /// <summary>
        /// Reads the contents of the .FmBok or .bok file into the BokBookmark list.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ReadBokBookmarkFile()
        {
            // Read the bookmark data.
            bookmarkFileBytes = File.ReadAllBytes(ProgramSettings.BookmarkFile);

            return true;
        }

        /// <summary>
        /// Extracts the bookmark data from the .FmBok or .bok file associated with the Reverse Speech Pro audio file.
        /// </summary>
        private Boolean ExtractBokBookmarkData()
        {
            int i;
            int dataStart = -1;

            // Find the count of bookmarks in the file.
            // Start by finding the first two NUL bytes.
            for (i = 0; i < bookmarkFileBytes.Length - 1; ++i)
            {
                if (bookmarkFileBytes[i] == 0x00 && bookmarkFileBytes[i + 1] == 0x00)
                {
                    // Mark the starting point for the data.
                    dataStart = i + 2;
                    break;
                }
            }

            // This seems to be what it's supposed to be.
            dataStart = 187;

            // Get the number of bookmarks in this file.
            // Byte[] bytes = new byte[] { bookmarkFileBytes[dataStart], bookmarkFileBytes[dataStart + 1], bookmarkFileBytes[dataStart + 2], bookmarkFileBytes[dataStart + 3] };
            Int32 countOfBookmarks = BitConverter.ToInt32(bookmarkFileBytes, dataStart);

            // Clear out the list of .FmBok/.bok bookmarks.
            BokBookmarks = new List<Bookmark>();

            // Load all the bookmarks.
            int baseAddress = dataStart + 4;
            int x = 0;
            i = 0;
            while (i < countOfBookmarks)
            {
                Bookmark newBookmark = new Bookmark();

                // Skip over the first Int32.
                x += 4;

                // Get bookmark name string length.
                Int32 len = BitConverter.ToInt32(bookmarkFileBytes, x + baseAddress);
                x += 4;

                // Check if the len is out of bounds.
                if (len + x + baseAddress >= bookmarkFileBytes.Length)
                {
                    ShowBokError(newBookmark, BokBookmarks, "Out of bounds on bookmark name string length.");
                    return false;
                }

                // Get bookmark name string.
                String name = Encoding.UTF8.GetString(bookmarkFileBytes, x + baseAddress, len);

                // Correct for a zero length bookmark name.
                if (len == 0) { name = "(blank)"; }

                // Sanitize the bookmark name since it will be used as a filename.
                newBookmark.Name = Regex.Replace(name, @"[/:*\?<>\|*$=]", "");  // Remove characters not allowed in filenames.

                // A zero length bookmark name is not allowed.
                if (len > 0)
                {
                    // Strings in the .bok file end with a double NUL byte.
                    x += len + 2;
                }

                // Advance to the next 32 bit boundary.
                while ((double)x / 4d != x / 4)
                {
                    ++x;
                }

                // Skip the Unknown Int32.
                x += 4;

                // Get bookmark text string length.
                len = BitConverter.ToInt32(bookmarkFileBytes, x + baseAddress);
                x += 4;

                // Check if the len is out of bounds.
                if (len + x + baseAddress >= bookmarkFileBytes.Length)
                {
                    ShowBokError(newBookmark, BokBookmarks, "Out of bounds on bookmark text string length.");
                    return false;
                }

                // Get bookmark text string.
                String text = Encoding.UTF8.GetString(bookmarkFileBytes, x + baseAddress, len);

                // If this is an opening or closing card, store it.
                switch (name.Trim().ToLower())
                {
                    case "openingcard":
                    case "opening card":
                        BokOpeningCard = text;
                        break;

                    case "closingcard":
                    case "closing card":
                        BokClosingCard = text;
                        break;
                }

                // Separate transcript text from explanatory text. Explanatory text is separated from transcript text by a blank line.
                Match match = Regex.Match(text, @"\r\n\s*\r\n");
                if (match.Success)
                {
                    // The blank line was found. Extract both strings.
                    int index = match.Index;
                    // newBookmark.Text = WordWrap(text.Substring(0, index)).Trim();
                    // newBookmark.Explanation = WordWrap(text.Substring(index).Trim());
                    newBookmark.Text = text.Substring(0, index).Trim();
                    newBookmark.Explanation = text.Substring(index).Trim();
                }
                else
                {
                    // There was no explanatory text.
                    // newBookmark.Text = WordWrap(text).Trim();
                    newBookmark.Text = text.Trim();
                    newBookmark.Explanation = String.Empty;
                }

                // Skip this for a zero-length string.
                if (len > 0)
                {
                    // Strings in the .bok file end with a double NUL byte.
                    x += len + 2;
                }

                // Advance to the next 32 bit boundary.
                while ((double)x / 4d != x / 4)
                {
                    ++x;
                }

                // At times, x will not be four bytes too short. This corrects that problem.
                bool notQuiteThereFound1 = true;
                int i1 = x + baseAddress;
                int i2 = 0;
                for (; i2 < notQuiteThere1.Length; ++i1, ++i2)
                {
                    if (bookmarkFileBytes[i1] != notQuiteThere1[i2])
                    {
                        notQuiteThereFound1 = false;
                        break;
                    }
                }
                bool notQuiteThereFound2 = true;
                i1 = x + baseAddress;
                i2 = 0;
                for (; i2 < notQuiteThere2.Length; ++i1, ++i2)
                {
                    if (bookmarkFileBytes[i1] != notQuiteThere2[i2])
                    {
                        notQuiteThereFound2 = false;
                        break;
                    }
                }

                if (notQuiteThereFound1 == true || notQuiteThereFound2 == true)
                    x += 4;

                // Skip three Int32 values.
                x += 12;

                // Get bookmark sample start.
                newBookmark.SampleStart = BitConverter.ToInt32(bookmarkFileBytes, x + baseAddress);
                
                // To avoid another error, add four if SampleStart is zero.
                if (newBookmark.SampleStart == 0)
                {
                    x += 4;
                    newBookmark.SampleStart = BitConverter.ToInt32(bookmarkFileBytes, x + baseAddress);
                }

                // Check if the SampleStart is zero.
                if (newBookmark.SampleStart == 0)
                {
                    ShowBokError(newBookmark, BokBookmarks, "SampleStart is zero.");
                    return false;
                }

                x += 4;

                // Get bookmark sample end.
                newBookmark.SampleEnd = BitConverter.ToInt32(bookmarkFileBytes, x + baseAddress);

                // If SampleEnd is less than SampleStart, then we have another problem.
                if (newBookmark.SampleEnd <= newBookmark.SampleStart)
                {
                    ShowBokError(newBookmark, BokBookmarks, "Sample end is less than SampleStart.");
                    return false;
                }

                x += 4;

                // Skip trailing Int32 value.
                x += 4;

                // Increment counter.
                ++i;

                // Add the bookmark to the list.
                BokBookmarks.Add(newBookmark);
            }

            // Sort the bookmarks.
            BokBookmarks = BokBookmarks.OrderBy(o => o.SampleStart).ToList();

            return true;
        }


        /// <summary>
        /// Displays a fatal Bok parsing error with text that may help the user get past the error.
        /// </summary>
        /// <param name="newBookmark">The bookmark currently being processed.</param>
        /// <param name="bokBookmarks">The list of bookmarks that have been processed.</param>
        /// <param name="ErrorMessage">The error message.</param>
        private void ShowBokError(Bookmark newBookmark, List<Bookmark> bokBookmarks, String ErrorMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ErrorMessage + "\r\n\r\n");

            foreach (Bookmark b in bokBookmarks)
            {
                sb.Append(String.Format("Name: \"{0}\"\r\nText: \"{1}\"\r\nExplanation: \"{2}\"\r\nSampleStart: {3}\r\nSampleEnd: {4}\r\n\r\n",
                    b.Name, b.Text, b.Explanation, b.SampleStart, b.SampleEnd));
            }

            sb.Append("newBookmark\r\n");
            sb.Append(String.Format("Name: \"{0}\"\r\nText: \"{1}\"\r\nExplanation: \"{2}\"\r\nSampleStart: {3}\r\nSampleEnd: {4}\r\n",
                    newBookmark.Name, newBookmark.Text, newBookmark.Explanation, newBookmark.SampleStart, newBookmark.SampleEnd));

            String s = sb.ToString();

            Rsp2Video.ParseError ParseError = new Rsp2Video.ParseError(s);
            ParseError.ShowDialog();
        }



        /// <summary>
        /// Returns the path to the most recently written .FmBok or .bok file.
        /// 
        /// If neither file can be found, an error message will be written to labelSoundFileError.Text and the method will return null.
        /// </summary>
        /// <param name="wavFilePath"></param>
        /// <returns>The path to the most recently written .FmBok or .bok file, or null if neither file was found.</returns>
        //private string GetBookmarkFilePath(string wavFilePath)
        //{
        //    String bmarkFilePath;

        //    // Search for the fileinfo for the .FmBok/.bok files.
        //    String fmBokFilePath = Path.ChangeExtension(wavFilePath, ".FmBok");
        //    FileInfo fmBokFileInfo = new FileInfo(fmBokFilePath);
        //    String bokFilePath = Path.ChangeExtension(wavFilePath, ".bok");
        //    FileInfo bokFileInfo = new FileInfo(bokFilePath);

        //    // If neither exist, it's an error.
        //    if (fmBokFileInfo.Exists == false && bokFileInfo.Exists == false)
        //    {
        //        labelSoundFileError.Text = "The bookmark (.FmBok/.bok) file for this sound file was not found.";
        //        labelSoundFileError.Visible = true;
        //        return null;
        //    }

        //    // If both exist, use the newest one.
        //    if (fmBokFileInfo.Exists == true && bokFileInfo.Exists == true)
        //    {
        //        bmarkFilePath = (fmBokFileInfo.LastWriteTime > bokFileInfo.LastWriteTime) ? fmBokFilePath : bokFilePath;
        //    }
        //    else
        //    {
        //        // Otherwise, use the one that exists.
        //        bmarkFilePath = (fmBokFileInfo.Exists) ? fmBokFilePath : bokFilePath;
        //    }

        //    return bmarkFilePath;
        //}

        /// <summary>
        /// Assembles bookmark lists (Forward and Reverse, orphan reversals, and quick check).
        /// </summary>
        private void AssembleBokBookmarks()
        {
            SeparateBookmarks();

            ConnectFnRBookmarks();

            AssembleOrphanReverseBookmarks();

            AssembleQuickCheckBookmarks();
        }

        /// <summary>
        /// Separates forward and reverse bookmarks from the bookmark collection.
        /// </summary>
        private void SeparateBookmarks()
        {
            // Clear the bookmark lists.
            BokForwardBookmarks = new List<Bookmark>();
            BokReverseBookmarks = new List<Bookmark>();

            // Go through forward bookmarks to find overlapping reverse bookmarks.
            foreach (Bookmark bookmark in BokBookmarks)
            {
                switch (bookmark.Name[0])
                {
                    case 'F':
                    case 'f':
                        BokForwardBookmarks.Add(bookmark);
                        break;

                    case 'R':
                    case 'r':
                        BokReverseBookmarks.Add(bookmark);
                        break;
                }
            }
        }

        /// <summary>
        /// Connects reverse bookmarks to forward bookmarks in BokBookmarks.
        /// </summary>
        private void ConnectFnRBookmarks()
        {
            // Go through forward bookmarks to find overlapping reverse bookmarks.
            foreach (Bookmark forwardBookmark in BokForwardBookmarks)
            {
                // Go through bookmarks to find all reverse bookmarks that overlap the forward bookmark.
                foreach (Bookmark reverseBookmark in BokReverseBookmarks)
                {
                    // RRRRRRRRRRRRRRRRRRRRRRR
                    //   FFFFFFFFFFFFFFFFFFF
                    // Is the forward fully contained in the reverse?
                    if (forwardBookmark.SampleStart >= reverseBookmark.SampleStart &&
                        forwardBookmark.SampleEnd <= reverseBookmark.SampleEnd)
                    {
                        // Connect them.
                        forwardBookmark.ReferencedBookmarks.Add(reverseBookmark);
                        reverseBookmark.ReferencedBookmarks.Add(forwardBookmark);

                        // Add them if they don't already exist.
                        //if (FindBookmarkByName(BokForwardBookmarks, forwardBookmark.Name) == null)
                        //{
                        //    BokForwardBookmarks.Add(forwardBookmark);
                        //}
                        //if (FindBookmarkByName(BokReverseBookmarks, reverseBookmark.Name) == null)
                        //{
                        //    BokReverseBookmarks.Add(reverseBookmark);
                        //}

                        // Sort the list of reverse bookmarks in this forward bookmark.
                        forwardBookmark.ReferencedBookmarks = forwardBookmark.ReferencedBookmarks.OrderBy(o => o.SampleStart).ToList();
                    }

                    //   RRRRRRRRRRRRRRRRRRRRRRR
                    // FFFFFFFFFFFFFFFFFFFFFFF
                    // Is the start of the reverse inside the forward?
                    else if (reverseBookmark.SampleStart >= forwardBookmark.SampleStart &&
                        reverseBookmark.SampleStart <= forwardBookmark.SampleEnd)
                    {
                        // Is the overlap is greater than MinBookmarkOverlap?
                        if (forwardBookmark.SampleEnd - reverseBookmark.SampleStart >=                       
                            (reverseBookmark.SampleEnd - reverseBookmark.SampleStart) * MinBookmarkOverlap)
                        {
                            // Connect them.
                            forwardBookmark.ReferencedBookmarks.Add(reverseBookmark);
                            reverseBookmark.ReferencedBookmarks.Add(forwardBookmark);

                            //// Add them if they don't already exist.
                            //if (FindBookmarkByName(BokForwardBookmarks, forwardBookmark.Name) == null)
                            //{
                            //    BokForwardBookmarks.Add(forwardBookmark);
                            //}
                            //if (FindBookmarkByName(BokReverseBookmarks, reverseBookmark.Name) == null)
                            //{
                            //    BokReverseBookmarks.Add(reverseBookmark);
                            //}

                            // Sort the list of reverse bookmarks in this forward bookmark.
                            forwardBookmark.ReferencedBookmarks = forwardBookmark.ReferencedBookmarks.OrderBy(o => o.SampleStart).ToList();
                        }
                    }

                    // RRRRRRRRRRRRRRRRRRRRRRR
                    //   FFFFFFFFFFFFFFFFFFFFFFF
                    // Is the end of the reverse inside the forward?
                    else if (reverseBookmark.SampleEnd >= forwardBookmark.SampleStart &&
                        reverseBookmark.SampleEnd <= forwardBookmark.SampleEnd)
                    {
                        if (reverseBookmark.SampleEnd - forwardBookmark.SampleStart >=
                            (reverseBookmark.SampleEnd - reverseBookmark.SampleStart) * MinBookmarkOverlap)
                        {
                            // Connect them.
                            forwardBookmark.ReferencedBookmarks.Add(reverseBookmark);
                            reverseBookmark.ReferencedBookmarks.Add(forwardBookmark);

                            //// Add them if they don't already exist.
                            //if (FindBookmarkByName(BokForwardBookmarks, forwardBookmark.Name) == null)
                            //{
                            //    BokForwardBookmarks.Add(forwardBookmark);
                            //}
                            //if (FindBookmarkByName(BokReverseBookmarks, reverseBookmark.Name) == null)
                            //{
                            //    BokReverseBookmarks.Add(reverseBookmark);
                            //}

                            // Sort the list of reverse bookmarks in this forward bookmark.
                            forwardBookmark.ReferencedBookmarks = forwardBookmark.ReferencedBookmarks.OrderBy(o => o.SampleStart).ToList();
                        }
                    }
                }
            }

            // Sort the forward bookmarks.
            BokForwardBookmarks = BokForwardBookmarks.OrderBy(o => o.SampleStart).ToList();
        }

        /// <summary>
        /// Adds reverse bookmarks to a list when those reverse bookmarks are not connected to a forward bookmark
        /// </summary>
        private void AssembleOrphanReverseBookmarks()
        {
            // Clear the list.
            BokOrphanReverseBookmarks = new List<Bookmark>();

            // Go through forward bookmarks to find reverse bookmarks with no references to forward bookmarks.
            foreach (Bookmark reverseBookmark in BokBookmarks)
            {
                if ((reverseBookmark.Name[0] == 'R' || reverseBookmark.Name[0] == 'r') &&
                    reverseBookmark.ReferencedBookmarks.Count == 0)
                {
                    BokOrphanReverseBookmarks.Add(reverseBookmark);
                }
            }

            // Sort the bookmarks.
            BokOrphanReverseBookmarks = BokOrphanReverseBookmarks.OrderBy(o => o.SampleStart).ToList();
        }

        /// <summary>
        /// Adds quick check bookmarks to a list.
        /// </summary>
        private void AssembleQuickCheckBookmarks()
        {
            // Clear the list.
            BokQuickCheckBookmarks = new List<Bookmark>();

            // Go through forward bookmarks to find reverse bookmarks with no references to forward bookmarks.
            foreach (Bookmark quickCheckBookmark in BokBookmarks)
            {
                if (quickCheckBookmark.Name[0] == 'Q' || quickCheckBookmark.Name[0] == 'q')
                {
                    BokQuickCheckBookmarks.Add(quickCheckBookmark);
                }
            }

            // Sort the bookmarks.
            BokQuickCheckBookmarks = BokQuickCheckBookmarks.OrderBy(o => o.SampleStart).ToList();
        }

        /// <summary>
        /// Parses the video file to extract the necessary data.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ValidateAndParseVideo()
        {
            // Get the XML data from ffprobe.
            String data = RunFfprobeXml(ProjectSettings.SourceVideoFile);
            if (String.IsNullOrEmpty(data))
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }

            // Get the XML data into an XML document.
            XmlDocument xmlVideoDocument = new XmlDocument();
            xmlVideoDocument.LoadXml(data);
            XmlElement root = xmlVideoDocument.DocumentElement;

            // Get the first video stream and the first audio stream.
            XmlNode videoStream = root.SelectSingleNode("/ffprobe/streams/stream[@codec_type='video']");
            XmlNode audioStream = root.SelectSingleNode("/ffprobe/streams/stream[@codec_type='audio']");
            XmlNode format = root.SelectSingleNode("/ffprobe/format");

            if (audioStream == null)
            {
                labelMainFileError.Text = "The video file does not contain an audio stream.";
                labelMainFileError.Visible = true;
                return false;
            }

            // Get the frame rate.
            if (String.IsNullOrEmpty(videoStream.Attributes["r_frame_rate"].InnerText))
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }
            else
            {
                // Extract the numerator and denominator from the frame rate string.
                String[]s = videoStream.Attributes["r_frame_rate"].InnerText.Split('/');
                if (s.Length != 2)
                {
                    labelMainFileError.Text = "There was an error reading the video file.";
                    labelMainFileError.Visible = true;
                    return false;
                }
                
                Double.TryParse(s[0], out Double numerator);
                Double.TryParse(s[1], out Double denominator);

                // Set the frame rate.
                FramesPerSecond = numerator / denominator;
            }

            // Get the video resolution.
            if (Int32.TryParse(videoStream.Attributes["width"].InnerText, out HorizontalResolution) == false ||
                Int32.TryParse(videoStream.Attributes["height"].InnerText, out VerticalResolution) == false)
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }

            // Get the SAR and DAR ratios.
            if (videoStream.Attributes["sample_aspect_ratio"] == null)
            {
                SampleAspectRatio = "1/1";
            }
            else
            {
                SampleAspectRatio = videoStream.Attributes["sample_aspect_ratio"].InnerText.Replace(':', '/');
            }

            if (videoStream.Attributes["display_aspect_ratio"] == null)
            {
                DisplayAspectRatio = String.Format("{0}/{1}", HorizontalResolution, VerticalResolution);
            }
            else
            {
                DisplayAspectRatio = videoStream.Attributes["display_aspect_ratio"].InnerText.Replace(':', '/');
            }

            // Get the video duration.
            if (Double.TryParse(format.Attributes["duration"].InnerText, out SourceVideoDuration) == false)
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }

            // Get the audio sample rate.
            if (Int32.TryParse(audioStream.Attributes["sample_rate"].InnerText, out SampleRate) == false)
            {
                labelMainFileError.Text = "There was an error reading the video file.";
                labelMainFileError.Visible = true;
                return false;
            }

            // Set the text height to be equal to 20 lines on the screen.

            // Prepare the font information
            LeftMarginSpaces = (int)Math.Round((decimal)HorizontalResolution / (decimal)VerticalResolution * 8.0m);
            FontName = "Calibri";
            FontHeight = (int)VerticalResolution / LinesOnScreen;
            TextPadSize = FontHeight / 6;
            FontForward = new Font(FontName, FontHeight, FontStyle.Regular, GraphicsUnit.Pixel);
            FontReverse = new Font(FontName, FontHeight, FontStyle.Italic | FontStyle.Bold, GraphicsUnit.Pixel);
            FontForwardUnderline = new Font(FontName, FontHeight, FontStyle.Underline, GraphicsUnit.Pixel);

            // If the output video file is empty, set it.
            if (String.IsNullOrEmpty(textBoxOutputFile.Text) == true)
            {
                // Set the output video file.
                if (radioButtonSeparateVideos.Checked)
                {
                    textBoxOutputFile.Text = Path.GetFileNameWithoutExtension(ProjectSettings.SourceVideoFile) + "-";
                }
                else
                {
                    textBoxOutputFile.Text = "Reverse Speech of " + Path.GetFileName(ProjectSettings.SourceVideoFile);
                }
            }

            return true;
        }

        /// <summary>
        /// Extracts the ffprobe.exe data for the given file as an XML string.
        /// </summary>
        /// <param name="filename">The name of the file to examine.</param>
        /// <returns>An XML string of data about the media file.</returns>
        String RunFfprobeXml(String filename)
        {
            // Create the Process to call the external program.
            Process process = new Process();

            // Create the arguments string.
            String arguments = String.Format("-v error -print_format xml -show_format -show_streams \"{0}\"",
                filename);

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmprobeApp,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Log the ffprobe command line options.
            File.AppendAllText(LogFile, String.Format("\r\n\r\n***Command line: \"{0}\" {1}\r\n\r\n",
                process.StartInfo.FileName, process.StartInfo.Arguments));

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            String FfprobeOutput = process.StandardOutput.ReadToEnd();

            // Log the ffprobe output.
            File.AppendAllText(LogFile, FfprobeOutput);

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            // Return success or failure.
            if (!(ExitCode == 0))
            {
                return null;
            }

            return FfprobeOutput;
        }

        //private bool ValidateVideo()
        //{
        //    // Validate ouptut video file.
        //    if (textBoxSourceVideoFile.Text == String.Empty)
        //    {
        //        labelSourceVideoFileError.Text = "You must specify a source video file.";
        //        labelSourceVideoFileError.Visible = true;
        //        return false;
        //    }

        //    if (File.Exists(textBoxSourceVideoFile.Text) == false)
        //    {
        //        labelSourceVideoFileError.Text = "The file was not found.";
        //        labelSourceVideoFileError.Visible = true;
        //        return false;
        //    }

        //    return true;
        //}

        /// <summary>
        /// Parses the Reverse Speech Pro transcript file to retrieve the text
        /// and location of each bookmark.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        //private bool ValidateAndParseTranscript()
        //{
        //    // An empty transcript file means the .FmBok/.bok file will be used.
        //    if (textBoxTranscriptFile.Text == String.Empty)
        //    {
        //        return true;
        //    }

        //    if (File.Exists(textBoxTranscriptFile.Text) == false)
        //    {
        //        labelTranscriptFileError.Text = "The file was not found.";
        //        labelTranscriptFileError.Visible = true;
        //        return false;
        //    }

        //    // Read the transcript file. Make sure there is one blank line at the start of the file.
        //    String text = "\r\n" + File.ReadAllText(textBoxTranscriptFile.Text);

        //    // Remove RTF encoding, if it's there.
        //    Transcript = Regex.Replace(text, @"\{\*?\\[^{}]+}|[{}]|\\\n?[A-Za-z]+\n?(?:-?\d+)?[ ]?", "");

        //    // Extract the details from the transcript file.
        //    ExtractTranscriptBookmarkData();

        //    return true;
        //}

        //private bool ValidateTranscript()
        //{
        //    // Validate Transcript file.
        //    if (textBoxTranscriptFile.Text == String.Empty)
        //    {
        //        return true;
        //    }

        //    if (File.Exists(textBoxTranscriptFile.Text) == false)
        //    {
        //        labelSourceVideoFileError.Text = "The file was not found.";
        //        labelSourceVideoFileError.Visible = true;
        //        return false;
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Parses the transcript file (.FmBok/.bok) for bookmark data.
        ///// </summary>
        //private bool ExtractTranscriptBookmarkData()
        //{
        //    // Parse through the upper section of the transcript file.
        //    String textArea = Transcript.Substring(0, Transcript.IndexOf("\r\nDo not edit anything below here. "));
        //    if (textArea == null) { return false; }

        //    // Extract the opening and closing cards.
        //    Match cardMatch = Regex.Match(textArea, @"(?i)(openingcard|opening card): ([\s\S]*?.+)\r\n(closing|f = )");
        //    if (cardMatch.Success)
        //    {
        //        TxtOpeningCard = cardMatch.Groups[2].Value;
        //    }

        //    cardMatch = Regex.Match(textArea, @"(?i)(closingcard|closing card): ([\s\S]*?.+)\r\n(opening|f = )");
        //    if (cardMatch.Success)
        //    {
        //        TxtClosingCard = cardMatch.Groups[2].Value;
        //    }

        //    // Extract the bookmark pairs.
        //    MatchCollection matches = Regex.Matches(textArea, @"([Ff]\d+): ([\s\S]*?.+)\r\n([Rr]\d+): ([\s\S]*?.+)\r\nF = ");
        //    if (matches.Count == 0) { return false; }

        //    // Clear out the .txt/.rtf bookmark lists.
        //    TxtForwardBookmarks = new List<Bookmark>();
        //    TxtReverseBookmarks = new List<Bookmark>();

        //    // Process the bookmark pairs.
        //    foreach (Match m in matches)
        //    {
        //        if (!m.Success) { continue; }

        //        // Get the bookmark names and texts.
        //        String forwardName = Regex.Replace(m.Groups[1].Value, @"[/:*\?<>\|*$=]", "");  // Remove characters not allowed in filenames.
        //        String forwardText = m.Groups[2].Value;
        //        String reverseName = Regex.Replace(m.Groups[3].Value, @"[/:*\?<>\|*$=]", "");  // Remove characters not allowed in filenames.
        //        String reverseText = m.Groups[4].Value;

        //        // If either of these bookmarks do not exist in the .FmBok/.bok file, don't use them.
        //        if (FindBookmarkByName(BokBookmarks, forwardName) == null || FindBookmarkByName(BokBookmarks, reverseName) == null )
        //        {
        //            continue;
        //        }

        //        // Find the bookmarks if they exist.
        //        Bookmark forwardBookmark = FindBookmarkByName(TxtForwardBookmarks, forwardName);
        //        Bookmark reverseBookmark = FindBookmarkByName(TxtReverseBookmarks, reverseName);

        //        // If the forward bookmark does not exist, create it.
        //        if (forwardBookmark == null)
        //        {
        //            forwardBookmark = new Bookmark();
        //            forwardBookmark.Name = forwardName;

        //            // Separate transcript text from explanatory text. Explanatory text is separated from transcript text by a blank line.
        //            Match match = Regex.Match(forwardText, @"\r\n\s*\r\n");
        //            if (match.Success)
        //            {
        //                // The blank line was found. Extract both strings.
        //                int index = match.Index;
        //                // forwardBookmark.Text = WordWrap(forwardText.Substring(0, index)).Trim();
        //                // forwardBookmark.Explanation = WordWrap(forwardText.Substring(index).Trim());
        //                forwardBookmark.Text = forwardText.Substring(0, index).Trim();
        //                forwardBookmark.Explanation = forwardText.Substring(index).Trim();
        //            }
        //            else
        //            {
        //                // There was no explanatory text.
        //                // forwardBookmark.Text = WordWrap(forwardText).Trim();
        //                forwardBookmark.Text = forwardText.Trim();
        //                forwardBookmark.Explanation = String.Empty;
        //            }

        //            // Get SampleStart and SampleEnd from the .FmBok/.bok file.
        //            Bookmark bookmark = FindBookmarkByName(BokBookmarks, forwardName);
        //            if (bookmark != null)
        //            {
        //                forwardBookmark.SampleStart = bookmark.SampleStart;
        //                forwardBookmark.SampleEnd = bookmark.SampleEnd;

        //                // Add this bookmark to the list.
        //                TxtForwardBookmarks.Add(forwardBookmark);
        //            }
        //        }

        //        // If the reverse bookmark does not exist, create it.
        //        if (reverseBookmark == null)
        //        {
        //            reverseBookmark = new Bookmark();
        //            reverseBookmark.Name = reverseName;

        //            // Separate transcript text from explanatory text. Explanatory text is separated from transcript text by a blank line.
        //            Match match = Regex.Match(reverseText, @"\r\n\s*\r\n");
        //            if (match.Success)
        //            {
        //                // The blank line was found. Extract both strings.
        //                int index = match.Index;
        //                // reverseBookmark.Text = WordWrap(reverseText.Substring(0, index)).Trim();
        //                // reverseBookmark.Explanation = WordWrap(reverseText.Substring(index).Trim());
        //                reverseBookmark.Text = reverseText.Substring(0, index).Trim();
        //                reverseBookmark.Explanation = reverseText.Substring(index).Trim();
        //            }
        //            else
        //            {
        //                // There was no explanatory text.
        //                // reverseBookmark.Text = WordWrap(reverseText).Trim();
        //                reverseBookmark.Text = reverseText.Trim();
        //                reverseBookmark.Explanation = String.Empty;
        //            }

        //            // Get SampleStart and SampleEnd from the .FmBok/.bok file.
        //            Bookmark bookmark = FindBookmarkByName(BokBookmarks, reverseName);
        //            if (bookmark != null)
        //            {
        //                reverseBookmark.SampleStart = bookmark.SampleStart;
        //                reverseBookmark.SampleEnd = bookmark.SampleEnd;

        //                // Add this bookmark to the list.
        //                TxtReverseBookmarks.Add(reverseBookmark);
        //            }
        //        }

        //        // Connect the bookmarks.
        //        forwardBookmark.ReferencedBookmarks.Add(reverseBookmark);
        //        reverseBookmark.ReferencedBookmarks.Add(forwardBookmark);
        //    }

        //    return true;
        //}

        /// <summary>
        /// Searches the specified list of bookmarks for a bookmark with the name specified by bookmarkName.
        /// </summary>
        /// <param name="Bookmarks">The list of bookmarks to search.</param>
        /// <param name="bookmarkName">The bookmark name to search for.</param>
        /// <returns>The first bookmark with the specified name if found; otherwise null.</returns>
        private Bookmark FindBookmarkByName(List<Bookmark> Bookmarks, string bookmarkName)
        {
            foreach (Bookmark bookmark in Bookmarks)
            {
                if (bookmark.Name == bookmarkName) { return bookmark; }
            }

            return null;
        }
    }
}