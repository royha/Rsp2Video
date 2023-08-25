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
        public RSPro2VideoForm()
        {
            InitializeComponent();
            SetTooltips();
            LoadSettings();
        }

        /// <summary>
        /// Sets the tooltip text for all of the controls.
        /// </summary>
        private void SetTooltips()
        {
            // Create the ToolTip and associate with the Form container.
            ToolTip toolTips = new ToolTip();

            // I prefer the baloon tooltips.
            toolTips.IsBalloon = true;

            // Set up the ToolTip text for panel1 controls.
            //toolTips.SetToolTip(this.labelSourceVideoFile, "Enter the original video file here.");
            //toolTips.SetToolTip(this.textBoxSourceVideoFile, "Enter the original video file here.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Find the supporting applications (ffmpeg.exe, sox.exe, etc).
            if (FindSupportingApps() == false)
            {
                Application.Exit();
            }

            // If the animation file can't be found, disable it.
            if (File.Exists(AnimationFile) == false)
            {
                AnimationFile = null;
            }
        }

        /// <summary>
        /// Finds the supporting applications (ffmpeg.exe, ffprobe.exe, qmelt.exe, sox.exe).
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool FindSupportingApps()
        {
            // Get the Program Files directories.
            String ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            String ProgramFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            Boolean ffmpegExists = false;
            Boolean ffprobeExists = false;
            Boolean qmeltExists = false;
            Boolean soxExists = false;

            FfmpegApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffmpeg.exe");
            ffmpegExists = File.Exists(FfmpegApp);

            FfmprobeApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffprobe.exe");
            ffprobeExists = File.Exists(FfmprobeApp);

            QmeltApp = Path.Combine(ProgramFiles, "Shotcut", "qmelt.exe");
            qmeltExists = File.Exists(QmeltApp);

            String[] soxApps = Directory.GetDirectories(ProgramFilesX86, "sox*");
            soxApps = soxApps.OrderByDescending(c => c).ToArray();
            if (soxApps.Length > 0)
            {
                SoxApp = Path.Combine(ProgramFilesX86, soxApps[0], @"sox.exe");
                soxExists = File.Exists(SoxApp);
            }

            if (soxExists == false && (ffmpegExists == false || ffprobeExists == false || qmeltExists == false))
            {
                // Sox and ShotCut need to be installed.
                MessageBox.Show("Unable to find SoX or ShotCut. Both must be installed for RSPro2Video to work.\r\n\r\n" +
                    "ShotCut can be downloaded at http://shotcut.org\r\nSoX can be downloaded at http://sox.sourceforge.net", "Required applications not installed");
                return false;
            }
            else if (ffmpegExists == false || ffprobeExists == false || qmeltExists == false)
            {
                // ShotCut needs to be installed.
                MessageBox.Show("Unable to find ShotCut. ShotCut must be installed for RSPro2Video to work.\r\n\r\n" +
                    "ShotCut can be downloaded at http://shotcut.org", "Required application not installed");
                return false;
            }
            else if (soxExists == false)
            {
                // SoX needs to be installed.
                MessageBox.Show("Unable to find the Sound eXchange (SoX) program. SoX must be installed for RSPro2Video to work.\r\n\r\n" +
                    "SoX can be downloaded at http://sox.sourceforge.net", "Required application not installed");
                return false;
            }

            return true;
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            // Make the correct panel visible, and alter the Back and Next buttons for the specific panel.
            if (panel1.Visible) { return; }
            if (panel2.Visible) { ShowPanel1(); return; }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            // Exit the program.
            Application.Exit();
        }

        private async void buttonNext_ClickAsync(object sender, EventArgs e)
        {
            // Make the correct panel visible and set the UI to the current values.
            if (panel1.Visible)
            {
                // Validate the values for panel1.
                if (ValidatePanel1() == true)
                {
                    // Show panel2 only if panel1 settings are valid.
                    ShowPanel2();
                }
                return;
            }

            if (panel2.Visible)
            {
                if (ValidatePanel2() == false)
                {
                    return;
                }
            }

            // If this button was clicked on panel3 (when it is labeled "Create video"), begin creating the video.
            ShowPanel3();

            // Save the current settings. This also fills the settings object with the data from the Windows form.
            SaveSettings();

            // Allow the long running process to update the status label.
            Progress = new Progress<String>(s => labelStatus.Text = s);

            // Start the stopwatch
            Stopwatch timer = Stopwatch.StartNew();

            // Create the video.
            await Task.Factory.StartNew(() => CreateVideoAsync(Progress), TaskCreationOptions.LongRunning);

            // Stop the stopwatch and store the result.
            timer.Stop();
            timeToRun = timer.Elapsed;

            // Hide the status.
            labelStatus.Text = String.Empty;

            // Show the final panel.
            ShowPanel4();
        }

        /// <summary>
        /// Sets panel1 to Visible.
        /// </summary>
        private void ShowPanel1()
        {
            // Make panel1 the only visible panel.
            panel1.Visible = true;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = false;

            // Since this is the first panel, disable the Back button.
            buttonBack.Enabled = false;
        }

        /// <summary>
        /// Sets panel2 to Visible.
        /// </summary>
        private void ShowPanel2()
        {
            // Make panel2 the only visible panel.
            panel1.Visible = false;
            panel2.Visible = true;
            panel3.Visible = false;
            panel4.Visible = false;

            // Since we left the first panel, enable the Back button.
            buttonBack.Enabled = true;

            // Set the Next button.
            buttonNext.Text = "Next >";

            // If there is no transcript file, gray out the Bookmark source.
            //if (textBoxTranscriptFile.Text == null || textBoxTranscriptFile.Text == String.Empty)
            //{
            //    groupBoxBookmarkSource.Enabled = false;
            //    radioButtonSourceBookmarkFile.Checked = true;
            //}
            //else
            //{
            //    groupBoxBookmarkSource.Enabled = true;
            //}

            groupBoxBookmarkSource.Enabled = false;
            radioButtonSourceBookmarkFile.Checked = true;

            // Fill the TreeView.
            // Forward and backward selected?
            if (radioButtonBookmarkTypeFnR.Checked)
            {
                if (radioButtonSourceBookmarkFile.Checked)
                {
                    FillTreeView(BokForwardBookmarks);
                }

                if (radioButtonSourceTranscriptFile.Checked)
                {
                    FillTreeView(TxtForwardBookmarks);
                }
            }

            // Quick check selected?
            if (radioButtonBookmarkTypeQuickCheck.Checked)
            {
                groupBoxBookmarkSource.Enabled = false;
                radioButtonSourceBookmarkFile.Checked = true;
                FillTreeView(BokQuickCheckBookmarks);
            }

            // Orphan reversals selected?
            if (radioButtonBookmarkTypeOrphanedReversals.Checked)
            {
                groupBoxBookmarkSource.Enabled = false;
                radioButtonSourceBookmarkFile.Checked = true;
                FillTreeView(BokOrphanReverseBookmarks);
            }
        }

        private void ShowPanel3()
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = true;
            panel4.Visible = false;

            if (AnimationFile != null)
            {
                pictureBoxAnimation.Image = Image.FromFile(AnimationFile);
            }

            buttonExit.Text = "Exit";

            Application.DoEvents();

            buttonBack.Enabled = false;
            buttonNext.Enabled = false;
        }

        private void ShowPanel4()
        {
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            panel4.Visible = true;

            pictureBoxAnimation.Image = null;

            labelVideoRenderingTime.Text = "Video rendering time: " + timeToRun.ToString("hh':'mm':'ss'.'fff");

            buttonBack.Enabled = false;
            buttonNext.Enabled = false;
            buttonExit.Text = "Exit";

            if (settings.VideoContents == VideoContents.SeparateVideos)
            {
                buttonViewVideo.Enabled = false;
            }
        }

        /// <summary>
        /// Validates the data entered into panel1.
        /// </summary>
        /// <returns>Returns true if the data is valid. Returns false if the data is not valid and needs to be edited by the user.</returns>
        private bool ValidatePanel1()
        {
            if (ValidateAndParseFiles() == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the data entered into panel2.
        /// </summary>
        /// <returns>Returns true if the data is valid. Returns false if the data is not valid and needs to be edited by the user.</returns>
        private bool ValidatePanel2()
        {
            // Clear the list of bookmarks.
            ForwardBookmarks = new List<Bookmark>();

            // Search through the forward bookmarks.
            foreach (TreeNode forwardNode in treeView1.Nodes)
            {
                // Only selected bookmarks.
                if (forwardNode.Checked)
                {
                    // Get the bookmark from the TreeView.
                    Bookmark forwardBookmark = CloneBookmark((Bookmark)forwardNode.Tag);

                    // Only look for reverse bookmarks if Forward and reverse is checked.
                    if (radioButtonBookmarkTypeFnR.Checked)
                    {
                        // Add this bookmark to the list of forward bookmarks.
                        ForwardBookmarks.Add((Bookmark)forwardNode.Tag);

                        // Clear out the list of referenced bookmarks.
                        forwardBookmark.ReferencedBookmarks = new List<Bookmark>();

                        foreach (TreeNode reverseNode in forwardNode.Nodes)
                        {
                            if (reverseNode.Checked)
                            {
                                // No need to clone this bookmark as no changes are being made to it.
                                forwardBookmark.ReferencedBookmarks.Add((Bookmark)reverseNode.Tag);
                            }
                        }                    
                    }
                    else
                    {
                        // Orphaned reversals or Quick check was selected. 
                        // Clone the bookmark.
                        Bookmark reverseBookmark = CloneBookmark(forwardBookmark);

                        // Rename the forward bookmark. Many operations require forward bookmarks to have "F" as their first character.
                        forwardBookmark.Name = "F" + forwardBookmark.Name.Substring(1);

                        // Connect the forward bookmark to its reverse bookmark clone.
                        forwardBookmark.ReferencedBookmarks.Add(reverseBookmark);
                        
                        // Add this bookmark to the list of forward bookmarks.
                        ForwardBookmarks.Add(forwardBookmark);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Clones the specified bookmark.
        /// </summary>
        /// <param name="sourceBookmark">The bookmark to clone.</param>
        /// <returns>The cloned bookmark.</returns>
        private Bookmark CloneBookmark(Bookmark sourceBookmark)
        {
            Bookmark cloneBookmark = new Bookmark
            {
                Name = sourceBookmark.Name,
                Text = sourceBookmark.Text,
                Explanation = sourceBookmark.Explanation,
                SampleStart = sourceBookmark.SampleStart,
                SampleEnd = sourceBookmark.SampleEnd,
                ReferencedBookmarks = sourceBookmark.ReferencedBookmarks
            };

            return cloneBookmark;
        }

        private void FillTreeView(List<Bookmark> Bookmarks)
        {
            StringBuilder sb = new StringBuilder();

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            for (int i = 0; i < Bookmarks.Count; ++i)
            {
                Bookmark bookmark = Bookmarks[i];

                TreeNode treeNode = new TreeNode(bookmark.Name + ": " + bookmark.Text);
                treeNode.Tag = bookmark;
                treeNode.Checked = true;
                treeView1.Nodes.Add(treeNode);

                if (sb.Length > 0) { sb.Append("\r\n"); }
                sb.Append(bookmark.Name + ": " + bookmark.Text + "\r\n");
                if (String.IsNullOrWhiteSpace(bookmark.Explanation) == false)
                {
                    sb.Append("\r\n" + bookmark.Explanation + "\r\n");
                }

                for (int j = 0; j < bookmark.ReferencedBookmarks.Count; ++j)
                {
                    Bookmark referencedbookmark = bookmark.ReferencedBookmarks[j];

                    TreeNode childTreeNode = new TreeNode(referencedbookmark.Name + ": " + referencedbookmark.Text);
                    childTreeNode.Tag = referencedbookmark;
                    childTreeNode.Checked = true;
                    treeView1.Nodes[i].Nodes.Add(childTreeNode);

                    sb.Append(referencedbookmark.Name + ": " + referencedbookmark.Text + "\r\n");
                    if (String.IsNullOrWhiteSpace(referencedbookmark.Explanation) == false)
                    {
                        sb.Append("\r\n" + referencedbookmark.Explanation + "\r\n\r\n");
                    }
                }
            }

            treeView1.ExpandAll();
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0) { treeView1.SelectedNode = treeView1.Nodes[0]; }

            if (sb.Length > 0) { Clipboard.SetText(sb.ToString()); }
        }

        /// <summary>
        /// Creates the video file or video project file.
        /// </summary>
        private void CreateVideoAsync(IProgress<String> progress)
        {
            progress.Report("Working: Getting started.");

            // Create the working directories.
            CreateDirectories();

            // Copies the source video to the working directory.
            CopySourceVideoToWorkingDirectory();

            // Create text files from the bookmark data.
            CreateTextImageFiles();

            // Create the still images for the first and last frames of the forward video clips.
            CreateForwardStills();

            // Create the reversal clips.
            CreateReversalClips();

            // Remove the frames directory.
            RemoveFrames_DirDirectory();

            progress.Report("Working: Creating text overlays.");

            // Run MELT to output the video.
            SaveVideo();

            // Move the completed video to the destination directory.
            // MoveVideoToDestinationDirectory();

            // Remove the temp directory.
            
            RemoveTemp_DirDirectory();
        }

        /// <summary>
        /// Moves the completed video file to the output directory.
        /// </summary>
        private void MoveVideoToDestinationDirectory()
        {
            try
            {
                // Delete the destination file if it exists.
                File.Delete(settings.OutputVideoFile);

                // Move the file to the destination.
                File.Move(Path.Combine(Path.GetDirectoryName(WorkingInputVideoFile), Path.GetFileName(settings.OutputVideoFile)), 
                    settings.OutputVideoFile);
            }
            catch (IOException e)
            {
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Unable to finish saving video due to an IO Exception. " + 
                    "This frequently is caused by the video file being open in a video player. " + 
                    "If that is the case, please close the player and retry. " + 
                    "Retry?\r\n\r\nError message: " + e.Message, "Error", buttons);

                if (result == DialogResult.Yes)
                {
                    MoveVideoToDestinationDirectory();
                }
                else if (result == DialogResult.No)
                {
                    Application.Exit();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result = MessageBox.Show("Unauthorized access tring to finish saving video. Retry?\r\n\r\nError message: " + e.Message, "Error", buttons);

                if (result == DialogResult.Yes)
                {
                    MoveVideoToDestinationDirectory();
                }
                else if (result == DialogResult.No)
                {
                    Application.Exit();
                }
            }
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
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(settings.SourceVideoFile)), TEMP_DIR);

            // Create the working directory.
            try
            {
                // Try to create the directory.
                diTmpDirectory = Directory.CreateDirectory(WorkingDirectory);
            }
            catch { return false; }

            // Set the frames directory.
            FramesDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(settings.SourceVideoFile)), TEMP_DIR, FRAMES_DIR);

            // Create the frames directory.
            try
            {
                // Try to create the directory.
                fiTmpDirectory = Directory.CreateDirectory(FramesDirectory);
            }
            catch { return false; }

            // Set the current directory to the working directory.
            Directory.SetCurrentDirectory(WorkingDirectory);

            return true;
        }

        /// <summary>
        /// Copies the source video file to the working directory. 
        /// </summary>
        private void CopySourceVideoToWorkingDirectory()
        {
            // Set the name of the working input file. Typically, "v.mp4".
            WorkingInputVideoFile = Path.Combine(WorkingDirectory, "v" + Path.GetExtension(settings.SourceVideoFile));
            RelativePathToWorkingInputVideoFile = Path.GetFileName(WorkingInputVideoFile);

            // Copy the source video to the working directory.
            File.Copy(settings.SourceVideoFile, WorkingInputVideoFile, true);
        }

        /// <summary>
        /// Removes the TEMP_DIR directory and the log file.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RemoveTemp_DirDirectory()
        {
            Directory.SetCurrentDirectory(StoredCurrentDirectory);

            if (DeleteWorkingDirectories == true)
            {
                // Delete the working directory.
                try
                {
                    // Delete the directory and any files and directories in that directory.
                    diTmpDirectory.Delete(true);
                }
                catch { return false; }

                // Delete the log file.
                //try
                //{
                //    File.Delete(LogFile);
                //}
                //catch { return false; }
            }

            return true;
        }

        /// <summary>
        /// Removes the FRAMES_DIR directory.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool RemoveFrames_DirDirectory()
        {
            if (DeleteWorkingDirectories == true)
            {
                try
                {
                    // Delete the directory and any files and directories in that directory.
                    fiTmpDirectory.Delete(true);
                }
                catch { return false; }
            }

            return true;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fromPath"/> or <paramref name="toPath"/> is <c>null</c>.</exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        private static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        private void browseButtonBookmarkFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Select initial search directory
                String initialDirectory = String.Empty;
                if (textBoxBookmarkFile.Text != String.Empty)
                { 
                    initialDirectory = Path.GetDirectoryName(textBoxBookmarkFile.Text);
                }

                openFileDialog.Title = "Bookmark file";
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.Filter = "Bookmark files (*.RSVideo, *.FmBok, *.bok)|*.RSVideo;*.FmBok;*.bok|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file.
                    textBoxBookmarkFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseOutputVideoFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Select initial search directory
                String initialDirectory = String.Empty;
                if (buttonBrowseOutputVideoFile.Text != String.Empty) { initialDirectory = Path.GetDirectoryName(buttonBrowseOutputVideoFile.Text); }

                openFileDialog.Title = "Output video file";
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.Filter = "Video files (*.mp4, *.webm, *.avi, *.mov, *.mkv, *.mpg, *.mpeg, *.wmv)|*.mp4;*.webm;*.avi;*.mov;*.mkv;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    textBoxOutputFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            // Default is to show that we don't accept the drag-drop.
            e.Effect = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }

            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (String file in files)
            {
                String ext = Path.GetExtension(file).ToLower();

                switch (ext)
                {
                    case ".mp4":
                    case ".webm":
                    case ".avi":
                    case ".mov":
                    case ".mkv":
                    case ".mpg":
                    case ".mpeg":
                    case ".wmv":

                    case ".wav":
                    case ".mp3":

                    case ".rtf":
                    case ".txt":

                    case ".rsp2video":
                    case ".rsvideo":
                    case ".fmbok":
                    case ".bok":
                        e.Effect = DragDropEffects.Copy;
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Capture the drag and drop files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            // If there are no files, return.
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }

            //Boolean videoFileDropped = false;
            //Boolean soundFileDropped = false;
            //Boolean transcriptFileDropped = false;

            // Get the list of files.
            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Search through all files.
            foreach (String file in files)
            {
                // Case insensitive search.
                String ext = Path.GetExtension(file).ToLower();

                switch (ext)
                {
                    //// Source video file.
                    //case ".mp4":
                    //case ".webm":
                    //case ".avi":
                    //case ".mov":
                    //case ".mkv":
                    //case ".mpg":
                    //case ".mpeg":
                    //case ".wmv":
                    //    // If the extension is a video file, set the textbox to the filename.
                    //    // textBoxSourceVideoFile.Text = file;

                    //    // Set the flag that we received a video file.
                    //    //videoFileDropped = true;

                    //    // Create the output video file textbox.
                    //    if (radioButtonSeparateVideos.Checked)
                    //    {
                    //        textBoxOutputFile.Text = Path.GetFileNameWithoutExtension(file) + "-";
                    //    }
                    //    else
                    //    {
                    //        textBoxOutputFile.Text = "Reverse Speech of " + Path.GetFileName(file);
                    //    }
                    //    break;

                    //case ".wav":
                    //case ".mp3":
                    //    // If the extension is an audio file, set the textbox to the filename.
                    //    textBoxSoundFile.Text = file;

                    //    // Set the flag that we received an audio file.
                    //    //soundFileDropped = true;
                    //    break;

                    //case ".rtf":
                    //case ".txt":
                    //    // If the extension is a transcript file, set the textbox to the filename.
                    //    textBoxTranscriptFile.Text = file;

                    //    // Set the flag that we received a transcript file.
                    //    //transcriptFileDropped = true;
                    //    break;

                    case ".rsvideo":
                        // If the extension is an RSVideo bookmark file, set the textbox to the filename.
                        textBoxBookmarkFile.Text = file;

                        // Create the output video file textbox.
                        if (radioButtonSeparateVideos.Checked)
                        {
                            textBoxOutputFile.Text = Path.GetFileNameWithoutExtension(file) + "-";
                        }
                        else
                        {
                            // Remove the .RSVideo extension from the bookmark filename to obtain the video filename.
                            textBoxOutputFile.Text = "Reverse Speech of " + Path.GetFileNameWithoutExtension(file);
                        }
                        break;

                    case ".fmbok":
                    case ".bok":
                        // If the extension is an .FmBok or .bok bookmark file, set the textbox to the filename.
                        textBoxBookmarkFile.Text = file;

                        // Create the output video file textbox.
                        if (radioButtonSeparateVideos.Checked)
                        {
                            textBoxOutputFile.Text = Path.GetFileNameWithoutExtension(file) + "-";
                        }
                        else
                        {
                            // Create the output filename with the hardcoded extension.
                            textBoxOutputFile.Text = "Reverse Speech of " + Path.GetFileNameWithoutExtension(file) + ".mp4";
                        }
                        break;

                    case ".rsp2video":
                        // If the extension is a project file, find the bookmark file, then set the project filename.
                        // textBoxProjectFile.Text = file;
                        break;

                    default:
                        break;
                }
            }

            // If there was a video file and a sound file, but no transcript file, clear the transcript file textbox.
            //if (videoFileDropped == true && soundFileDropped == true && transcriptFileDropped == false)
            //{
            //    textBoxTranscriptFile.Text = String.Empty;
            //}
        }

        private void radioButtonFnR_CheckedChanged(object sender, EventArgs e)
        {
            // If orphaned reversals or quick check is checked (ie., forward and reverse is not checked).
            if (radioButtonBookmarkTypeQuickCheck.Checked || radioButtonBookmarkTypeOrphanedReversals.Checked)
            {
                // Gray out the bookmark source control.
                groupBoxBookmarkSource.Enabled = false;

                // Set the bookmark source to the bookmark file (.FmBok/.bok)
                radioButtonSourceTranscriptFile.Checked = false;
                radioButtonSourceBookmarkFile.Checked = true;
            }

            // Things to do when forward and reverse gets checked.
            if (radioButtonBookmarkTypeFnR.Checked)
            {
                // If we have a transcript file, enable the bookmark source group box.
                //if (textBoxTranscriptFile.Text != null && textBoxTranscriptFile.Text != String.Empty)
                //{
                //    groupBoxBookmarkSource.Enabled = true;
                //}

                // Fill the tree view with the bookmarks from the specified source.
                if (radioButtonSourceTranscriptFile.Checked)
                {
                    FillTreeView(TxtForwardBookmarks);
                }

                if (radioButtonSourceBookmarkFile.Checked)
                {
                    FillTreeView(BokForwardBookmarks);
                }
            }
        }

        private void checkBoxReversal1_CheckedChanged(object sender, EventArgs e)
        {
            textBoxSpeed1.Enabled = checkBoxReversal1.Checked;
            textBoxTone1.Enabled = checkBoxReversal1.Checked;
        }

        private void checkBoxReversal2_CheckedChanged(object sender, EventArgs e)
        {
            textBoxSpeed2.Enabled = checkBoxReversal2.Checked;
            textBoxTone2.Enabled = checkBoxReversal2.Checked;
        }

        private void checkBoxReversal3_CheckedChanged(object sender, EventArgs e)
        {
            textBoxSpeed3.Enabled = checkBoxReversal3.Checked;
            textBoxTone3.Enabled = checkBoxReversal3.Checked;
        }

        private void checkBoxReversal4_CheckedChanged(object sender, EventArgs e)
        {
            textBoxSpeed4.Enabled = checkBoxReversal4.Checked;
            textBoxTone4.Enabled = checkBoxReversal4.Checked;
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Nodes.Count > 0)
                {
                    /* Calls the CheckAllChildNodes method, passing in the current 
                    Checked value of the TreeNode whose checked state changed. */
                    this.CheckAllChildNodes(e.Node, e.Node.Checked);
                }
            }
        }

        // Updates all child tree nodes recursively.
        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                node.Checked = nodeChecked;
                if (node.Nodes.Count > 0)
                {
                    // If the current node has child nodes, call the CheckAllChildsNodes method recursively.
                    this.CheckAllChildNodes(node, nodeChecked);
                }
            }
        }

        private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            // The code only executes if the user caused the checked state to change.
            if (e.Action != TreeViewAction.Unknown)
            {
                if (e.Node.Parent != null && e.Node.Parent.GetType() == typeof(TreeNode) && e.Node.Parent.Checked == false)
                {
                    e.Cancel = true;
                }
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    node.Checked = true;
                    CheckAllChildNodes(node, true);
                }
            }
        }

        private void buttonDeselectAll_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Count > 0)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    node.Checked = false;
                    CheckAllChildNodes(node, false);
                }
            }
        }

        // Saves the state of the Video contents group box.
        bool saveRadioButtonContentsEntireVideo = false;
        bool saveRadioButtonContentsBookmarksOnly = false;
        bool saveRadioButtonSeparateVideos = false;
        bool saveCheckBoxIncludeBackAndForth = false;
        bool saveCheckBoxReplayForwardVideo = false;
        bool saveRadioButtonSourceBookmarkFile = false;
        bool saveRadioButtonSourceTranscriptFile = false;

        private void radioButtonQuickCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBookmarkTypeQuickCheck.Checked)
            {
                // Save the state of the group boxes.
                saveRadioButtonContentsEntireVideo = radioButtonContentsEntireVideo.Checked;
                saveRadioButtonContentsBookmarksOnly = radioButtonContentsBookmarksOnly.Checked;
                saveRadioButtonSeparateVideos = radioButtonSeparateVideos.Checked;
                saveCheckBoxIncludeBackAndForth = checkBoxIncludeBackAndForth.Checked;
                saveCheckBoxReplayForwardVideo = checkBoxReplayForwardVideo.Checked;
                saveRadioButtonSourceBookmarkFile = radioButtonSourceBookmarkFile.Checked;
                saveRadioButtonSourceTranscriptFile = radioButtonSourceTranscriptFile.Checked;


                // Set the Video contents group box for quick check.
                radioButtonContentsEntireVideo.Checked = false;
                radioButtonContentsBookmarksOnly.Checked = true;
                radioButtonSeparateVideos.Checked = false;
                checkBoxIncludeBackAndForth.Checked = false;
                checkBoxReplayForwardVideo.Checked = false;
                radioButtonSourceBookmarkFile.Checked = true;
                radioButtonSourceTranscriptFile.Checked = false;
                // groupBoxVideoContents.Enabled = false;
                // groupBoxAtEndOfReversals.Enabled = false;
                // groupBoxBookmarkSource.Enabled = false;

                // Fill the tree view with the list of quick check bookmarks.
                FillTreeView(BokQuickCheckBookmarks);
            }
            else
            {
                // Restore the state of the group boxes.
                radioButtonContentsEntireVideo.Checked = saveRadioButtonContentsEntireVideo;
                radioButtonContentsBookmarksOnly.Checked = saveRadioButtonContentsBookmarksOnly;
                radioButtonSeparateVideos.Checked = saveRadioButtonSeparateVideos;
                checkBoxIncludeBackAndForth.Checked = saveCheckBoxIncludeBackAndForth;
                checkBoxReplayForwardVideo.Checked = saveCheckBoxReplayForwardVideo;
                radioButtonSourceBookmarkFile.Checked = saveRadioButtonSourceBookmarkFile;
                radioButtonSourceTranscriptFile.Checked = saveRadioButtonSourceTranscriptFile;
                // groupBoxVideoContents.Enabled = true;
                // groupBoxAtEndOfReversals.Enabled = true;
                // groupBoxBookmarkSource.Enabled = true;

            }
        }

        private void radioButtonOrphanedReversals_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBookmarkTypeOrphanedReversals.Checked)
            {
                // Fill the tree view with the list of orphaned reverse bookmarks.
                FillTreeView(BokOrphanReverseBookmarks);

                // Orphaned reversals don't do back and forth or replay forward.
                // groupBoxAtEndOfReversals.Enabled = false;
            }
            else
            {
                // groupBoxAtEndOfReversals.Enabled = true;
            }
        }

        private void radioButtonBookmarkFile_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSourceBookmarkFile.Checked)
            {
                FillTreeView(BokForwardBookmarks);
            }
        }

        private void radioButtonTranscriptFile_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSourceTranscriptFile.Checked)
            {
                FillTreeView(TxtForwardBookmarks);
            }
        }

        private void buttonViewVideo_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Path.Combine(Path.GetDirectoryName(settings.SourceVideoFile), settings.OutputVideoFile));
            }
            catch { }
        }

        private void radioButtonSeparateVideos_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSeparateVideos.Checked == true)
            {
                labelOutputVideoFile.Text = "Output video file prefix";
                textBoxOutputFile.Text = Path.GetFileNameWithoutExtension(settings.SourceVideoFile) + " - ";
                // checkBoxReplayForwardVideo.Checked = false;
                // checkBoxReplayForwardVideo.Enabled = false;
                labelProgressing.Enabled = false;
            }
            else
            {
                labelOutputVideoFile.Text = "Output video filename";
                textBoxOutputFile.Text = "Reverse Speech of " + Path.GetFileName(settings.SourceVideoFile);
                // checkBoxReplayForwardVideo.Enabled = true;
                labelProgressing.Enabled = true;
            }
        }

        private void radioButtonContentsBookmarksOnly_CheckedChanged(object sender, EventArgs e)
        {
            //if (radioButtonContentsBookmarksOnly.Checked)
            //{
            //    checkBoxReplayForwardVideo.Checked = false;
            //    checkBoxReplayForwardVideo.Enabled = false;
            //}
            //else
            //{
            //    checkBoxReplayForwardVideo.Enabled = true;
            //}
        }

        /// <summary>
        /// Adjusts the video quality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarVideoQuality_Scroll(object sender, EventArgs e)
        {
            // Display the quality text.
            labelVideoQualityValue.Text = VideoQualityString[trackBarVideoQuality.Value];

            // Store the quality in the settings class/file.
            switch (trackBarVideoQuality.Value)
            {
                case 0:
                    settings.VideoQuality = VideoQuality.Fast;
                    break;

                case 1:
                    settings.VideoQuality = VideoQuality.Small;
                    break;

                case 2:
                    settings.VideoQuality = VideoQuality.High;
                    break;
            }

            // Change the extension of the output video file.
            if (radioButtonSeparateVideos.Checked == false)
            {
                string filenameWithExtension = Path.GetFileNameWithoutExtension(textBoxOutputFile.Text) + 
                    settings.OutputOptionsVideoFinalExtension[(int)settings.VideoQuality];

                textBoxOutputFile.Text = filenameWithExtension;
            }
        }
    }
}
