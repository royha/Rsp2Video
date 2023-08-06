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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSPro2VideoTool
{
    public partial class RSPro2VideoToolForm : Form
    {
        float FramesPerSecond;                                          // The frames per second of the source and output video.
        int HorizontalResolution;                                       // The horizontal resolution of the video.
        int VerticalResolution;                                         // The vertical resolution of the video.
        String AudioDescription = String.Empty;                         // The ffprobe description of the audio.
        String ToolAnimationFile = Path.Combine(Application.StartupPath, "toolanimation.gif");
        // String ToolAnimationFile = @"D:\Pictures\Animated Gifs\chicken wire.gif";

        String SoxApp = String.Empty;
        String FfmpegApp = String.Empty;
        String FfmprobeApp = String.Empty;
        String QmeltApp = String.Empty;

        public RSPro2VideoToolForm()
        {
            InitializeComponent();
            SetTooltips();
        }

        private void RSPro2VideoToolForm_Load(object sender, EventArgs e)
        {
            if (FindSupportingApps() == false)
            {
                Application.Exit();
            }

            // If the animation file can't be found, disable it.
            if (File.Exists(ToolAnimationFile) == false)
            {
                ToolAnimationFile = null;
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

            FfmpegApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffmpeg.exe");
            ffmpegExists = File.Exists(FfmpegApp);

            FfmprobeApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffprobe.exe");
            ffprobeExists = File.Exists(FfmprobeApp);


            if (ffmpegExists == false || ffprobeExists == false)
            {
                // ShotCut needs to be installed.
                MessageBox.Show("Unable to find ShotCut. ShotCut must be installed for RSPro2VideoTool to work.\r\n\r\n" +
                    "ShotCut can be downloaded at http://shotcut.org", "Required application not installed");
                return false;
            }
            
            return true;
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
            toolTips.SetToolTip(this.panel1, "Drag and drop a video file here.");
            toolTips.SetToolTip(this.groupBox1, "Drag and drop a video file here.");
            toolTips.SetToolTip(this.buttonExtractAudio, "Extracts a .wav file from the video.");
            toolTips.SetToolTip(this.buttonSaveVideo240p, "Saves a copy of the video, resized to 240 pixels in height.");
            toolTips.SetToolTip(this.buttonSaveVideo360p, "Saves a copy of the video, resized to 360 pixels in height.");
            toolTips.SetToolTip(this.buttonSaveVideo480p, "Saves a copy of the video, resized to 480 pixels in height.");
            toolTips.SetToolTip(this.buttonSaveVideo720p, "Saves a copy of the video, resized to 720 pixels in height.");
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
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
                        e.Effect = DragDropEffects.Copy;
                        break;

                    default:
                        break;
                }
            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }

            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (String file in files)
            {
                String ext = Path.GetExtension(file).ToLower();

                switch (ext)
                {
                    // Source video file.
                    case ".mp4":
                    case ".webm":
                    case ".avi":
                    case ".mov":
                    case ".mkv":
                    case ".mpg":
                    case ".mpeg":
                    case ".wmv":
                        textBoxSourceVideoFile.Text = file;

                        // Process the selected file.
                        FileChosen();

                        break;

                    default:
                        break;
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Select initial search directory
                String initialDirectory = String.Empty;

                openFileDialog.Title = "Source video file";
                openFileDialog.InitialDirectory = initialDirectory;
                openFileDialog.Filter = "Video files (*.mp4, *.webm, *.avi, *.mov, *.mkv, *.mpg, *.mpeg, *.wmv)|*.mp4;*.webm;*.avi;*.mov;*.mkv;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file.
                    textBoxSourceVideoFile.Text = openFileDialog.FileName;

                    // Process the selected file.
                    FileChosen();
                }
            }
        }


        private void buttonExtractAudio_Click(object sender, EventArgs e)
        {
            SaveAudio();
        }

        private void buttonSaveVideo240p_Click(object sender, EventArgs e)
        {
            SaveVideo(240);
        }

        private void buttonSaveVideo360p_Click(object sender, EventArgs e)
        {
            SaveVideo(360);
        }

        private void buttonSaveVideo480p_Click(object sender, EventArgs e)
        {
            SaveVideo(480);
        }

        private void buttonSaveVideo720p_Click(object sender, EventArgs e)
        {
            SaveVideo(720);
        }

        /// <summary>
        /// Processes the newly chosen video file. Validates the file, getting data from the ffprobe query, displays the data, 
        /// and enables buttons to save lower resolution versions of the source video file.
        /// </summary>
        private void FileChosen()
        {
            if (ValidateAndParseVideo() == false)
            {
                // If the video did not parse, reset the form.
                textBoxSourceVideoFile.Text = String.Empty;
                labelVideoDescription.Text = "To begin, drag and drop a video file onto this application.";
                labelAudioDescription.Enabled = false;
                buttonExtractAudio.Enabled = false;
                buttonSaveVideo240p.Enabled = false;
                buttonSaveVideo360p.Enabled = false;
                buttonSaveVideo480p.Enabled = false;
                buttonSaveVideo720p.Enabled = false;
                return;
            }

            // Display the video and audio descriptions.
            labelVideoDescription.Text = String.Format("Video: {0}x{1} {2}fps.", HorizontalResolution, VerticalResolution, FramesPerSecond);
            labelAudioDescription.Text = AudioDescription;
            labelAudioDescription.Enabled = true;

            // Enable the buttons.
            buttonExtractAudio.Enabled = true;
            if (VerticalResolution > 240) { buttonSaveVideo240p.Enabled = true; } else { buttonSaveVideo240p.Enabled = false; }
            if (VerticalResolution > 360) { buttonSaveVideo360p.Enabled = true; } else { buttonSaveVideo360p.Enabled = false; }
            if (VerticalResolution > 480) { buttonSaveVideo480p.Enabled = true; } else { buttonSaveVideo480p.Enabled = false; }
            if (VerticalResolution > 720) { buttonSaveVideo720p.Enabled = true; } else { buttonSaveVideo720p.Enabled = false; }
        }

        /// <summary>
        /// Parses the video file to determine the frames per second and resolution.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ValidateAndParseVideo()
        {
            if (ValidateVideo() == false) { return false; }

            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmprobeApp,
                Arguments = "\"" + textBoxSourceVideoFile.Text + "\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Start ffmpeg to get the video file information.
            process.Start();

            // Read the output of ffmpeg.
            String FfprobeOutput = process.StandardError.ReadToEnd();

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            if (!(ExitCode == 0))
            {
                MessageBox.Show("There was an error reading " + textBoxSourceVideoFile.Text + ":" + Environment.NewLine + Environment.NewLine + "Error message: " + FfprobeOutput,
                    "Error reading Source video file");
                return false;
            }

            // Parse for the frame rate using regular expressions.
            Match match = Regex.Match(FfprobeOutput, @"(\d+|\d+\.\d+) fps,");
            if (!match.Success)
            {
                labelStatus.Text = "There was an error reading this file.";
                return false;
            }

            if (float.TryParse(match.Groups[1].Value, out float rate))
            {
                FramesPerSecond = rate;
            }
            else
            {
                labelStatus.Text = "There was an error reading this file.";
                return false;
            }

            // Parse for the resolution using regular expressions.
            match = Regex.Match(FfprobeOutput, @" (\d\d\d+)x(\d\d+)[, ]");
            if (!match.Success)
            {
                labelStatus.Text = "There was an error reading this file.";
                return false;
            }

            if (Int32.TryParse(match.Groups[1].Value, out int horizontal))
            {
                HorizontalResolution = horizontal;
            }
            else
            {
                labelStatus.Text = "There was an error reading this file.";
                return false;
            }

            if (Int32.TryParse(match.Groups[2].Value, out int vertical))
            {
                VerticalResolution = vertical;
            }
            else
            {
                labelStatus.Text = "There was an error reading this file.";
                return false;
            }

            // Parse for the audio description.
            match = Regex.Match(FfprobeOutput, @" Audio: ([\s\S]*?.+)\r\n");
            if (!match.Success)
            {
                labelStatus.Text = "There was an error reading this file.";
                labelAudioDescription.Enabled = false;
                return false;
            }

            // Show the audio description.
            AudioDescription = "Audio: " + match.Groups[1].Value;

            return true;
        }

        private bool ValidateVideo()
        {
            // Validate ouptut video file.
            if (textBoxSourceVideoFile.Text == String.Empty)
            {
                labelStatus.Text = "You must specify a source video file.";
                return false;
            }

            if (File.Exists(textBoxSourceVideoFile.Text) == false)
            {
                labelStatus.Text = "The file was not found.";
                return false;
            }

            return true;
        }

        private bool SaveAudio()
        {
            // Let the user select the output filename.
            String filename = SaveAudioFileDialog();
            if (filename == null) { return false; }

            // Update the user that their file is being saved.
            labelStatus.Text = "Working ...";
            panel1.Enabled = false;
            Application.DoEvents();

            // Write the audio file.
            if (SaveAudioFile(filename) == false)
            {
                panel1.Enabled = true;
                labelStatus.Text = "An error occurred saving the audio file.";
                return false;
            }

            // Update the status.
            labelStatus.Text = "The audio file was saved.";
            panel1.Enabled = true;

            return true;
        }

        private String SaveAudioFileDialog()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Select initial search directory
                String initialDirectory = Path.GetDirectoryName(textBoxSourceVideoFile.Text);

                saveFileDialog.Title = "Save audio file";
                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.Filter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "wav";
                saveFileDialog.ValidateNames = true;
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + ".wav";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file.
                    return saveFileDialog.FileName;
                }
            }

            return null;
        }

        private bool SaveAudioFile(String audioOutputFile)
        {
            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-i \"{0}\" \"{1}\"",
                    textBoxSourceVideoFile.Text, audioOutputFile),
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

        private async void SaveVideo(int verticalResolution)
        {
            // Let the user select the output filename.
            String filename = SaveVideoFileDialog(verticalResolution);
            if (filename == null) { return; }

            // Update the user that their file is being saved.
            labelStatus.Text = "Working ...";
            panel1.Enabled = false;
            Application.DoEvents();

            // Start the animation.
            panel1.Visible = false;
            panel2.Visible = true;
            if (ToolAnimationFile != null)
            {
                pictureBoxToolAnimation.Image = Image.FromFile(ToolAnimationFile);

                // If the image is larger than the picture box.
                if (pictureBoxToolAnimation.Image.Width > pictureBoxToolAnimation.Width ||
                    pictureBoxToolAnimation.Image.Height > pictureBoxToolAnimation.Height)
                {
                    pictureBoxToolAnimation.SizeMode = PictureBoxSizeMode.Zoom;
                }
                else
                {
                    pictureBoxToolAnimation.SizeMode = PictureBoxSizeMode.CenterImage;
                }
            }

            // Write the video file.
            bool result = await Task.Run(() => SaveVideoFile(textBoxSourceVideoFile.Text, filename, verticalResolution));

            // Stop the animation.
            panel1.Visible = true;
            panel2.Visible = false;
            pictureBoxToolAnimation.Image = null;

            if (result == false)
            {
                panel1.Enabled = true;
                labelStatus.Text = "An error occurred saving the video file.";
                return;
            }

            // Update the status.
            labelStatus.Text = "The video file was saved.";
            panel1.Enabled = true;

            return;
        }

        private String SaveVideoFileDialog(int verticalResolution)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Select initial search directory
                String initialDirectory = Path.GetDirectoryName(textBoxSourceVideoFile.Text);

                saveFileDialog.Title = "Save video file";
                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.Filter = "Video files (*.mp4, *.webm, *.avi, *.mov, *.mkv, *.mpg, *.mpeg, *.wmv)|*.mp4;*.webm;*.avi;*.mov;*.mkv;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.ValidateNames = true;
                saveFileDialog.OverwritePrompt = false;
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + " " + verticalResolution.ToString() + "p" + Path.GetExtension(textBoxSourceVideoFile.Text);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file.
                    return saveFileDialog.FileName;
                }
            }

            return null;
        }

        private bool SaveVideoFile(String sourceVideoFile, String videoOutputFile, int verticalResolution)
        {
            Process process = new Process();

            String arguments = String.Format("-y -i \"{0}\" -vf scale=-2:{1} \"{2}\"",
                    sourceVideoFile, verticalResolution, videoOutputFile);

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

            return true;
        }
    }
}
