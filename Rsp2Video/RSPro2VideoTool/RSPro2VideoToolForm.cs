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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RSPro2VideoTool
{
    public partial class RSPro2VideoToolForm : Form
    {
        public Object LogFileLock = new Object();
        public String LogFile;

        public int SampleRate;                                                 // The sample rate of the sound file.
        public double FramesPerSecond;                                         // The frames per second of the source and output video.
        public int HorizontalResolution;                                       // The horizontal resolution of the video.
        public int VerticalResolution;                                         // The vertical resolution of the video.
        public double SourceVideoDuration;                                     // The duration of the source video in seconds.
        public String FfprobeRawXmlData;                                       // The raw XML data from ffprobe.
        public String AudioDescription = String.Empty;                         // The ffprobe description of the audio.
        public String SourceVideoFile;
        public String FfmpegApp = String.Empty;
        public String FfmprobeApp = String.Empty;
        public String OutputVideoFilename = String.Empty;

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
        }

        /// <summary>
        /// Finds the supporting applications (ffmpeg.exe, ffprobe.exe).
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool FindSupportingApps()
        {
            // Get the Program Files directories.
            String ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            String ProgramFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // First, look in "C:\Program Files\ffmpeg\bin".

            Boolean ffmpegExists = false;
            Boolean ffprobeExists = false;

            FfmpegApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffmpeg.exe");
            ffmpegExists = File.Exists(FfmpegApp);

            FfmprobeApp = Path.Combine(ProgramFiles, "ffmpeg\\bin", "ffprobe.exe");
            ffprobeExists = File.Exists(FfmprobeApp);

            if (ffmpegExists == true && ffprobeExists == true)
            {
                return true;
            }

            // Next, look in "C:\Program Files\kdenlive\bin".

            ffmpegExists = false;
            ffprobeExists = false;

            FfmpegApp = Path.Combine(ProgramFiles, "kdenlive\\bin", "ffmpeg.exe");
            ffmpegExists = File.Exists(FfmpegApp);

            FfmprobeApp = Path.Combine(ProgramFiles, "kdenlive\\bin", "ffprobe.exe");
            ffprobeExists = File.Exists(FfmprobeApp);

            if (ffmpegExists == true && ffprobeExists == true)
            {
                return true;
            }

            // ffmpeg needs to be installed.
            MessageBox.Show("Unable to find ffmpeg. ffmpeg must be installed for RSPro2Video to work.\r\n\r\n"
                + "ffmpeg comes with the Kdenlive video editor. You can download and install the\r\n"
                + "Kdenlive for Windows Installable package from this webpage: https://kdenlive.org/en/download/\r\n\r\n"
                + "A standalone version of ffmpeg can be found here: https://www.gyan.dev/ffmpeg/builds",
                "Required application not installed");

            return false;
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
            toolTips.SetToolTip(this.buttonExtractMp3Audio, "Extracts an .mp3 file from the video.");
            toolTips.SetToolTip(this.buttonExtractWavAudio, "Extracts an .wav file from the video.");
            toolTips.SetToolTip(this.buttonReencodeVideo, "Re-encodes the video and audio into a form acceptable to RS Video.");
            toolTips.SetToolTip(this.buttonSyncVideo, "Synchronizes the video with the audio by moving the video\r\nforward or backward by the number of frames you specify.");
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
                        SourceVideoFile = file;

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

        private void buttonExtractWavAudio_Click(object sender, EventArgs e)
        {
            SaveAudio(AudioOutputType.WAV);
        }

        private void buttonExtractMp3Audio_Click(object sender, EventArgs e)
        {
            SaveAudio(AudioOutputType.MP3);
        }

        private void buttonReencodeVideo_Click(object sender, EventArgs e)
        {
            ReEncodeVideo();
        }

        private void buttonSyncVideo_Click(object sender, EventArgs e)
        {
            SyncVideo();
        }

        /// <summary>
        /// Processes the newly chosen video file. Validates the file, getting FfprobeRawXmlData from the ffprobe query, displays the FfprobeRawXmlData, 
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
                buttonExtractMp3Audio.Enabled = false;
                buttonExtractWavAudio.Enabled = false;
                buttonSyncVideo.Enabled = false;
                buttonReencodeVideo.Enabled = false;
                return;
            }

            // Display the video and audio descriptions.
            labelVideoDescription.Text = $"Video: {HorizontalResolution}x{VerticalResolution} {FramesPerSecond}fps.";
            labelAudioDescription.Text = AudioDescription;
            labelAudioDescription.Enabled = true;

            // Enable the buttons.
            buttonExtractMp3Audio.Enabled = true;
            buttonExtractWavAudio.Enabled = true;
            buttonSyncVideo.Enabled = true;
            buttonReencodeVideo.Enabled = true;
        }

        /// <summary>
        /// Parses the video file to determine the frames per second and resolution.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool ValidateAndParseVideo()
        {
            if (ValidateVideo() == false) { return false; }

            // Get the XML FfprobeRawXmlData from ffprobe.
            String XmlFfprobeRawData = RunFfprobeXml(textBoxSourceVideoFile.Text);
            if (String.IsNullOrEmpty(XmlFfprobeRawData))
            {
                labelStatus.Text = "There was an error reading the video file.";
                labelStatus.Visible = true;
                return false;
            }

            // Get the XML FfprobeRawXmlData into an XML document.
            XmlDocument xmlVideoDocument = new XmlDocument();
            xmlVideoDocument.LoadXml(XmlFfprobeRawData);
            XmlElement root = xmlVideoDocument.DocumentElement;

            // Get the first video stream and the first audio stream.
            XmlNode videoStream = root.SelectSingleNode("/ffprobe/streams/stream[@codec_type='video']");
            XmlNode audioStream = root.SelectSingleNode("/ffprobe/streams/stream[@codec_type='audio']");
            XmlNode format = root.SelectSingleNode("/ffprobe/format");

            // Give an error message if this file isn't suitable.
            if (videoStream == null)
            {
                MessageBox.Show("This file does not contain an video stream.", "Error");
                return false;
            }

            if (audioStream == null)
            {
                MessageBox.Show("This file does not contain a audio stream.", "Error");
                return false;
            }

            if (format == null)
            {
                MessageBox.Show("Unable to determine the format of this file.", "Error");
                return false;
            }

            // Get the text output of ffprobe for the same file.
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

            // Get the frame rate.
            if (String.IsNullOrEmpty(videoStream.Attributes["r_frame_rate"].InnerText))
            {
                labelStatus.Text = "There was an error reading the video file.";
                labelStatus.Visible = true;
                return false;
            }
            else
            {
                // Extract the numerator and denominator from the frame rate string.
                String[] s = videoStream.Attributes["r_frame_rate"].InnerText.Split('/');
                if (s.Length != 2)
                {
                    labelStatus.Text = "There was an error reading the video file.";
                    labelStatus.Visible = true;
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
                labelStatus.Text = "There was an error reading the video file.";
                labelStatus.Visible = true;
                return false;
            }

            // Get the video duration.
            if (Double.TryParse(format.Attributes["duration"].InnerText, out SourceVideoDuration) == false)
            {
                labelStatus.Text = "There was an error reading the video file.";
                labelStatus.Visible = true;
                return false;
            }

            // Get the audio sample rate.
            if (Int32.TryParse(audioStream.Attributes["sample_rate"].InnerText, out SampleRate) == false)
            {
                labelStatus.Text = "There was an error reading the video file.";
                labelStatus.Visible = true;
                return false;
            }
            
            // Parse for the audio description.
            Match match = Regex.Match(FfprobeOutput, @" Audio: ([\s\S]*?.+)\r\n");
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

        private async void SaveAudio(AudioOutputType audioOutputType)
        {
            // Let the user select the output outputFilename.
            String outputFilename = SaveAudioFileDialog(audioOutputType);
            if (outputFilename == null) { return; }

            SetLogFileLocation(outputFilename);

            // Update the user that their file is being saved.
            labelStatus.Text = "Working ...";
            panel1.Enabled = false;
            Application.DoEvents();

            // Write the audio file.
            bool result = false;

            // Show the progress bar.
            progressBarMain.Visible = true;

            // Change the mouse pointer to an hourglass.
            Application.UseWaitCursor = true;

            // Start a stopwatch.
            Stopwatch sw = Stopwatch.StartNew();

            // Run the re-encode process asynchronously.
            result = await Task.Run(() => SaveAudioFile(outputFilename));

            // Stop the stopwatch.
            sw.Stop();

            // Restore the mouse pointer to the normal arrow.
            Application.UseWaitCursor = false;

            // Hide the progress bar.
            progressBarMain.Visible = false;

            if (result == false)
            {
                labelStatus.Text = "An error occurred saving the audio file.";
                return;
            }

            // Update the status.
            labelStatus.Text = "Time to create audio file: " + FormatTimeSpan(sw.Elapsed.TotalSeconds);
            panel1.Enabled = true;

            // Delete the log file.
            if (checkBoxDeleteLogfile.Checked)
            {
                DeleteLogFile();
            }

            return;
        }

        private String SaveAudioFileDialog(AudioOutputType audioOutputType)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Select initial search directory
                String initialDirectory = Path.GetDirectoryName(textBoxSourceVideoFile.Text);

                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.AddExtension = true;
                saveFileDialog.ValidateNames = true;

                switch (audioOutputType)
                {
                    case AudioOutputType.WAV:
                        saveFileDialog.Title = "Save .wav audio file";
                        saveFileDialog.Filter = "Wave files (*.wav)|*.wav|All files (*.*)|*.*";
                        saveFileDialog.DefaultExt = "wav";
                        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + ".wav";
                        break;

                    case AudioOutputType.MP3:
                        saveFileDialog.Title = "Save .mp3 audio file";
                        saveFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
                        saveFileDialog.DefaultExt = "mp3";
                        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + ".mp3";
                        break;

                    default:
                        return null;
                }

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
                Arguments = $"-y -hide_banner -i \"{textBoxSourceVideoFile.Text}\" \"{audioOutputFile}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            WriteLog(MethodBase.GetCurrentMethod().Name, $"\"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n");

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
                WriteLog(MethodBase.GetCurrentMethod().Name, $"Error:\r\n{FfmpegOutput} \r\n\r\n");
                return false;
            }

            WriteLog(MethodBase.GetCurrentMethod().Name, $"{FfmpegOutput} \r\n\r\n");

            return true;
        }

        private async void ReEncodeVideo()
        {
            // Let the user select the output outputFilename.
            String outputFilename = SaveVideoFileDialog(VideoOutputType.Encode);
            if (outputFilename == null) { return; }

            SetLogFileLocation(outputFilename);
            
            // Update the user that their file is being saved.
            labelStatus.Text = "Working ...";
            panel1.Enabled = false;
            Application.DoEvents();

            // Write the video file.
            bool result = false;

            // Show the progress bar.
            progressBarMain.Visible = true;

            // Change the mouse pointer to an hourglass.
            Application.UseWaitCursor = true;

            // Start a stopwatch.
            Stopwatch sw = Stopwatch.StartNew();

            // Run the re-encode process asynchronously.
            result = await Task.Run(() => ReEncodeVideoFile(textBoxSourceVideoFile.Text, outputFilename));

            // Stop the stopwatch.
            sw.Stop();

            // Restore the mouse pointer to the normal arrow.
            Application.UseWaitCursor = false;

            // Hide the progress bar.
            progressBarMain.Visible = false;

            if (result == false)
            {
                panel1.Enabled = true;
                labelStatus.Text = "An error occurred saving the video file.";
                return;
            }

            // Update the status.
            panel1.Enabled = true;
            labelStatus.Text = labelStatus.Text = "Time to create video file: " + FormatTimeSpan(sw.Elapsed.TotalSeconds);

            // Delete the log file.
            if (checkBoxDeleteLogfile.Checked)
            {
                DeleteLogFile();
            }

            return;
        }

        public String SaveVideoFileDialog(VideoOutputType videoOutputType)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Select initial search directory
                String initialDirectory = Path.GetDirectoryName(textBoxSourceVideoFile.Text);

                switch (videoOutputType)
                {
                    case VideoOutputType.Encode:
                        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + " (Re-encoded).mp4";
                        break;
                    
                    case VideoOutputType.Sync:
                        saveFileDialog.FileName = Path.GetFileNameWithoutExtension(textBoxSourceVideoFile.Text) + " (synced).mp4";
                        break;

                    default:
                        return null;
                }

                saveFileDialog.Title = "Save video file";
                saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.Filter = "Video files (*.mp4, *.webm, *.avi, *.mov, *.mkv, *.mpg, *.mpeg, *.wmv)|*.mp4;*.webm;*.avi;*.mov;*.mkv;*.mpg;*.mpeg;*.wmv|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.ValidateNames = true;
                saveFileDialog.OverwritePrompt = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file.
                    return saveFileDialog.FileName;
                }
            }

            return null;
        }

        private bool ReEncodeVideoFile(String sourceVideoFile, String videoOutputFile)
        {
            // Set the sample rate for the output audio. If the rate is anything other than 44100, set it to 48000.
            int audioRate = (SampleRate == 44100) ? 44100 : 48000;

            Process process = new Process();

            String arguments = $"-y -hide_banner -i \"{sourceVideoFile}\" "
                + $"-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 23 -c:a aac -ar {audioRate} -q:a 1 -movflags +faststart "
                + $"\"{videoOutputFile}\"";

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

            WriteLog(MethodBase.GetCurrentMethod().Name, $"\"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n");

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
                WriteLog(MethodBase.GetCurrentMethod().Name, $"Error:\r\n{FfmpegOutput} \r\n\r\n");
                return false;
            }

            WriteLog(MethodBase.GetCurrentMethod().Name, $"{FfmpegOutput} \r\n\r\n");
            return true;
        }

        private void SyncVideo()
        {
            DialogResult dialogResult = new DialogResult();

            // Prepare to show the VideoSyncForm.
            using (var videoSyncForm = new VideoSyncForm(this))
            {
                // Show the video sync form in a non-blocking way.
                dialogResult = videoSyncForm.ShowDialog();
            }

            panel1.Enabled = true;

            // Update the status.
            switch(dialogResult)
            {
                case DialogResult.OK:
                    // The dialog updated the status text. No need to update it here.
                    break;

                case DialogResult.Abort:
                    labelStatus.Text = "An error occurred saving the video file.";
                    break;

                default:
                    labelStatus.Text = string.Empty;
                    break;
            }

            return;
        }

        private bool SyncVideoFile(string sourceVideoFile, string outputFilename, double videoOffset)
        {
            Process process = new Process();

            String arguments = $"-y -hide_banner -i \"{sourceVideoFile}\" "
                + $"-itsoffset {(double)videoOffset / FramesPerSecond} -i \"{sourceVideoFile}\" -map 1:v -map 0:a -c copy "
                + $"\"{outputFilename}\"";

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

            WriteLog(MethodBase.GetCurrentMethod().Name, $"\"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n");

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
                WriteLog(MethodBase.GetCurrentMethod().Name, $"Error:\r\n{FfmpegOutput} \r\n\r\n");
                return false;
            }

            WriteLog(MethodBase.GetCurrentMethod().Name, $"{FfmpegOutput} \r\n\r\n");
            return true;
        }

        /// <summary>
        /// Extracts the ffprobe.exe FfprobeRawXmlData for the given file as an XML string.
        /// </summary>
        /// <param name="filename">The name of the file to examine.</param>
        /// <returns>An XML string of FfprobeRawXmlData about the media file.</returns>
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

            // Start ffmpeg to extract the frames.
            process.Start();

            // Read the output of ffmpeg.
            FfprobeRawXmlData = process.StandardOutput.ReadToEnd();

            // Log the ffprobe command line options and output.
            // WriteLog(MethodBase.GetCurrentMethod().Name, $"\r\nComment: XML output from ffprobe for the source video.\r\nCommand line: \"{process.StartInfo.FileName}\" {process.StartInfo.Arguments}\r\n\r\n{FfprobeOutput}\r\n");

            // Wait here for the process to exit.
            process.WaitForExit();
            int ExitCode = process.ExitCode;
            process.Close();

            // Return success or failure.
            if (!(ExitCode == 0))
            {
                return null;
            }

            return FfprobeRawXmlData;
        }
        
        /// <summary>
        /// Creates a log file using the source video outputFilename + ".log"
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        public bool SetLogFileLocation(string sourceFile)
        {
            // Add ".log" to the end of the full path and outputFilename of the source video file, just like Kdenlive.
            LogFile = Path.GetFullPath(sourceFile) + ".log";

            // Delete the log file.
            DeleteLogFile();

            // Write the initial log entry.
            String LogEntry = $"\r\n***Log start time: {DateTime.Now}\r\nFilename: {sourceFile}\r\n";
            WriteLog(MethodBase.GetCurrentMethod().Name, LogEntry);

            // Write ffprobe XML file for this media file.
            WriteLog(MethodBase.GetCurrentMethod().Name,
                $"XML output from ffprobe for the source file: {sourceFile}\r\n{FfprobeRawXmlData}\r\n");

            return true;
        }

        public void WriteLog(String CreatorMethod, String LogEntry)
        {
            String entry = $"{CreatorMethod}: {LogEntry}\r\n";

            lock (LogFileLock)
            {
                File.AppendAllText(LogFile, entry);
            }
        }

        public void DeleteLogFile()
        {
            // Delete the log file.
            try
            {
                File.Delete(LogFile);
            }
            catch { }
        }

        public String FormatTimeSpan(double duration)
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
    }

    public static class VideoOffsetDialog
    {
        public static double ShowDialog()
        {
            Form prompt = new Form();
            prompt.Width = 202;
            prompt.Height = 125;
            prompt.Text = "Enter the video offset";
            Label textLabel = new Label() { Left = 12, Top = 18, Width = 108, Text = "Video offset in frames" };
            NumericUpDown inputBox = new NumericUpDown() { Left = 126, Top = 16, Width = 44, Value = 2 };
            Button confirmation = new Button() { Text = "OK", Left = 90, Top = 45, Width = 80 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.ShowDialog();
            return (double)inputBox.Value;
        }
    }

    public enum AudioOutputType { None, WAV, MP3 };
    public enum VideoOutputType { None, Sync, Encode };
    public enum TransitionType { None, XFade, HoldLastFrame }

}
