using RSPro2Video.Properties;
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
            // CreateReversalAudioFiles();

            // Create all of the reversal video files.
            CreateAllReverseVideoTasks();
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
                    if (CreateReverseWav(reverseBookmark, 1, ProjectSettings.ReversalRate1) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 2, ProjectSettings.ReversalRate2) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 3, ProjectSettings.ReversalRate3) == false) { break; }
                    if (CreateReverseWav(reverseBookmark, 4, ProjectSettings.ReversalRate4) == false) { break; }
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
            double startSeconds = (double)reverseBookmark.SampleStart / (double)SampleRate;
            double lengthSeconds = ((double)reverseBookmark.SampleEnd / (double)SampleRate) - (double)startSeconds;

            // If speed and tone are the same, only one step is needed.
            if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
            {
                // Create a .wav file at the requested speed (ie., 70%) by using SoX "speed" feature which changes both length and tone.
                if (CallFfmpegSpeed(reverseBookmark.Name, reversalNumber, startSeconds, lengthSeconds, reversalRate.ReversalSpeed) == false) { return false; }
            }
            else
            // A two step process is needed.
            {
                // Change speed to achieve the desired tone (ReversalTone), then stretch to desired length.
                if (CallFfmpegStretch(reverseBookmark.Name, reversalNumber, startSeconds, lengthSeconds, reversalRate.ReversalSpeed, reversalRate.ReversalTone) == false) { return false; }
            }

            return true;
        }

        private bool CallFfmpegSpeed(string reversalName, int reversalNumber, double startSeconds, double lengthSeconds, int reversalSpeed)
        {
            Process process = new Process();

            String audioFilename = String.Format("{0}.{1}.{2}.wav", reversalName, reversalNumber, reversalSpeed);

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = string.Format("-y -hide_banner -ss {0:0.######} -t {1:0.######} -i \"{2}\" -vn -filter:a \"areverse, asetrate={3}*{4:0.######}, aresample={3}\" \"{5}\"",
                    startSeconds, lengthSeconds, WorkingInputVideoFile, SampleRate, reversalSpeed / (double)100.0, audioFilename),
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Log the ffmpeg command line options.
            File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

            // Run ffmpeg to create the .wav file.
            process.Start();

            // Read the error output of SoX, if there is any.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Log the ffmpeg output.
            File.AppendAllText(LogFile, FfmpegOutput);
            
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

        private bool CallFfmpegStretch(string reversalName, int reversalNumber, double startSeconds, double lengthSeconds, int reversalSpeed, int reversalTone)
        {
            Process process = new Process();

            String audioFilename = String.Format("{0}.{1}.{2}-{3}.wav", reversalName, reversalNumber, reversalSpeed, reversalTone);

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = string.Format("-y -hide_banner -ss {0:0.######} -t {1:0.######} -i \"{2}\" -vn -filter:a \"areverse, rubberband=pitch={3:0.######}, rubberband=tempo={4:0.######}, rubberband=pitchq=quality\" \"{5}\"",
                    startSeconds, lengthSeconds, WorkingInputVideoFile, reversalTone / (double)100.0, reversalSpeed / (double)100.0, audioFilename),
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Maximized
            };

            // Log the ffmpeg command line options.
            File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

            // Run ffmpeg to create the .wav file.
            process.Start();

            // Read the error output of SoX, if there is any.
            String FfmpegOutput = process.StandardError.ReadToEnd();

            // Log the ffmpeg output.
            File.AppendAllText(LogFile, FfmpegOutput);
            
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

        /// <summary>
        /// Creates the reversal .wav files.
        /// </summary>
        private void CreateAllReverseVideoTasks()
        {
            // Create the various reversals for each bookmarked reversal.
            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            {
                foreach (Bookmark reverseBookmark in forwardBookmark.ReferencedBookmarks)
                {
                    // Check to see if this bookmark is not selected for output.
                    if (reverseBookmark.Selected == false) { continue; }

                    // Update the user.
                    Progress.Report(String.Format("Working: Creating video for {0}: {1}", reverseBookmark.Name, reverseBookmark.Text));

                    // Create a video file for each selected reversal rate.
                    if (CreateReverseVideoTasks(reverseBookmark) == false) { break; }
                }
            }
        }

        /// <summary>
        /// Creates a reversal video file file based on the specified reversal rate.
        /// </summary>
        /// <param name="reversal"></param>
        /// <param name="reversalRate"></param>
        /// <returns></returns>
        private bool CreateReverseVideoTasks(Bookmark reversal)
        {
            // Create a task for each reversal rate.
            if (CreateReverseVideoTask(reversal, 1, ProjectSettings.ReversalRate1) == false) { return false; }
            if (CreateReverseVideoTask(reversal, 2, ProjectSettings.ReversalRate2) == false) { return false; }
            if (CreateReverseVideoTask(reversal, 3, ProjectSettings.ReversalRate3) == false) { return false; }
            if (CreateReverseVideoTask(reversal, 4, ProjectSettings.ReversalRate4) == false) { return false; }

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
        private bool ExtractReverseVideoFrames(string name, double startSeconds, double lengthSeconds)
        {
            Process process = new Process();

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -hide_banner -ss {0:0.######} -i \"{1}\" -pix_fmt rgb48 -an -q:v 1 -t {2:0.######} \"{3}\\{4}.%05d.png\"",
                    startSeconds, RelativePathToWorkingInputVideoFile, lengthSeconds, FRAMES_DIR, name),
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

            if (!(ExitCode == 0))
            {
                return false;
            }

            return true;
        }

        private String[] ReorderFrames(string videoFilename)
        {
            // Change into the _tmp directory.
            Directory.SetCurrentDirectory(Path.Combine(WorkingDirectory, videoFilename));

            // Get a list of all the .png files (with path) from this clip.
            String[] frames = Directory.GetFiles(".", "f*.png");

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

            return newFrames;
        }

        //private bool AssembleReverseVideoFrames(Bookmark reversal, int reversalNumber, ReversalRate reversalRate, int frameCount)
        //{
        //    // If this rate isn't selected, return.
        //    if (reversalRate.UseThisRate == false) { return true; }

        //    Process process = new Process();

        //    // Generate the filename without the extension, since this will be used to access the .wav and .mkv files.
        //    String videoFilename;
        //    if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
        //    {
        //        videoFilename = String.Format("{0}.{1}.{2}", reversal.Name, reversalNumber, reversalRate.ReversalSpeed);
        //    }
        //    else
        //    {
        //        videoFilename = String.Format("{0}.{1}.{2}-{3}", reversal.Name, reversalNumber, reversalRate.ReversalSpeed, reversalRate.ReversalTone);
        //    }

        //    // Calculate the frames per second for this reversal.
        //    double reversalFps = FramesPerSecond * ((double)reversalRate.ReversalSpeed / (double)100);

        //    // Create the ffmpeg argument string.
        //    String arguments = String.Format("-y -hide_banner -framerate {0:0.######} -i \"{1}\\r{2}.%05d.png\" -i \"{3}{4}\" {6} -filter:v \"fps=fps={7:0.######}:eof_action=pass\" \"{3}{5}\"",
        //            FramesPerSecond * ((double)reversalRate.ReversalSpeed / 100d),
        //            FRAMES_DIR,
        //            reversal.Name,
        //            videoFilename,
        //            OutputAudioInterimExtension,
        //            OutputVideoInterimExtension,
        //            OutputImageSequenceSettings,
        //            FramesPerSecond);

        //    // Configure the process using the StartInfo properties.
        //    process.StartInfo = new ProcessStartInfo
        //    {
        //        FileName = FfmpegApp,
        //        Arguments = arguments,
        //        UseShellExecute = false,
        //        RedirectStandardError = true,
        //        CreateNoWindow = true,
        //        WindowStyle = ProcessWindowStyle.Maximized
        //    };

        //    // Log the ffmpeg command line options.
        //    File.AppendAllText(LogFile, "\r\n\r\n***Command line: " + process.StartInfo.Arguments + "\r\n\r\n");

        //    // Start ffmpeg to extract the frames.
        //    process.Start();

        //    // Read the output of ffmpeg.
        //    String FfmpegOutput = process.StandardError.ReadToEnd();

        //    // Log the ffmpeg output.
        //    File.AppendAllText(LogFile, FfmpegOutput);

        //    // Wait here for the process to exit.
        //    process.WaitForExit();
        //    int ExitCode = process.ExitCode;
        //    process.Close();

        //    if (!(ExitCode == 0))
        //    {
        //        return false;
        //    }

        //    // Calculate the length, in seconds, of this clip.
        //    double EstimatedDuration = Math.Ceiling(frameCount / reversalFps);

        //    // Add the file to the clips lists.
        //    AddToClips(videoFilename, EstimatedDuration, AddToVideoOutputs: false);

        //    return true;
        //}

        private bool CreateReverseVideoTask(Bookmark reversal, int reversalNumber, ReversalRate reversalRate)
        {
            // If this rate isn't selected, return.
            if (reversalRate.UseThisRate == false) { return true; }

            double reversalSpeed = Math.Round((double)reversalRate.ReversalSpeed / 100.0d, 15);
            double reversalTone = Math.Round((double)reversalRate.ReversalTone / 100.0d, 15);

            // Calculate start, end, and length.
            double sampleBasedStartSeconds = (double)reversal.SampleStart / (double)SampleRate;
            double sampleBasedEndSeconds = (double)reversal.SampleEnd / (double)SampleRate;
            double sampleBasedDuration = Math.Round(sampleBasedEndSeconds - sampleBasedStartSeconds, 15);

            double originalFrameBasedStartSeconds = Math.Round(Math.Floor(sampleBasedStartSeconds * FramesPerSecond) / FramesPerSecond, 15);
            double originalFrameBasedEndSeconds = Math.Round(Math.Ceiling(sampleBasedEndSeconds * FramesPerSecond) / FramesPerSecond, 15);
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
                * FramesPerSecond) / FramesPerSecond, 15);
            double calculatedFrameBasedStartSeconds = 
                (originalFrameBasedEndSeconds - 
                    Math.Ceiling(
                        Math.Round(originalFrameBasedDuration / reversalSpeed, 15)      // Original duration, stretched.
                    * FramesPerSecond)                                                  // Frame count, celininged.
                / FramesPerSecond                                                       // Converted back to a duration.
                );                                                                      // Converted back to a specific time.
            double calculatedFrameBasedDuration = Math.Round(Math.Ceiling(Math.Round((calculatedFrameBasedEndSeconds - originalFrameBasedStartSeconds)
                / reversalSpeed * FramesPerSecond, 11)) / FramesPerSecond, 15);

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


            // The list of FFMpeg commands to add to the task.
            List<String> ffmpegCommandList = new List<String>();
            List<String> videoFilenames = new List<String>();

            // Create the filter_complex filtergraphs.
            String videoFilename;
            String audioFiltergraph;
            String interpolationFiltergraph = String.Empty;
            String reverseVideoFiltergraph = "[OverlayedV]reverse[v]";
            // String reverseAudioFiltergraph = "[SlowAudio]areverse[a]";
            Boolean minterpolationActive = false;


            //
            // Trying to get this working.
            //

            // These are not being used:
            videoFilename = $"{reversal.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}.Text";

            audioFiltergraph = $"[0:a]atrim=0:{silentAudioBeforeDuration:0.############},volume=volume=0,areverse[SilentAudioBefore];" +
                $"[0:a]atrim={silentAudioBeforeDuration:0.############}:duration={sampleBasedDuration:0.############},areverse[SelectedAudio];" +
                $"[0:a]atrim={silentAudioBeforeDuration + sampleBasedDuration:0.############}:" +
                $"duration={originalFrameBasedEndSeconds - sampleBasedEndSeconds:0.############}," +
                $"volume=volume=0,areverse[SilentAudioAfter];" +
                $"[SilentAudioBefore][SelectedAudio][SilentAudioAfter]concat=n=3:v=0:a=1[AllAudio];" +
                $"[AllAudio]asetpts=PTS-STARTPTS,rubberband=pitch={reversalTone:0.######}:tempo={reversalSpeed:0.######}:pitchq=quality[a]";

            interpolationFiltergraph = $"[0:v]trim=0:{originalFrameBasedDuration}," +
                $"setpts=PTS-STARTPTS," +
                $"setpts=PTS/{reversalSpeed:0.###}," +
                $"fps=fps={FramesPerSecond:0.############}:eof_action=pass[SlowForwardV];" +
                $"[SlowForwardV][1:v]overlay[OverlayedV]";
            
            String command = $"-y -hide_banner -ss {originalFrameBasedStartSeconds} " +
                $"-i \"{RelativePathToWorkingInputVideoFile}\" -i \"{reversal.Name}.Text.png\" -loop 1 " +
                $"-filter_complex \"{interpolationFiltergraph}; {reverseVideoFiltergraph}; {audioFiltergraph}\" " +
                $"-map [v] -map [a] -progress \"{videoFilename}.progress\" -threads 1 {OutputInterimSettings} " +
                $"\"{videoFilename}{OutputVideoInterimExtension}\"";

            //
            // Audio filtergraphs.
            //

            if (reversalRate.ReversalSpeed == reversalRate.ReversalTone)
            {
                // Normal audio slowdown.

                videoFilename = $"{reversal.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}.Text";

                audioFiltergraph = $"[0:a]atrim=0:{silentAudioBeforeDuration:0.############},volume=volume=0[SilentAudioBefore];" +
                    $"[0:a]atrim={silentAudioBeforeDuration:0.############}:duration={sampleBasedDuration:0.############}[SelectedAudio];" +
                    $"[0:a]atrim={silentAudioBeforeDuration + sampleBasedDuration:0.############}:" +
                    $"duration={originalFrameBasedEndSeconds - sampleBasedEndSeconds:0.############}," +
                    $"volume=volume=0,areverse[SilentAudioAfter];" +
                    $"[SilentAudioBefore][SelectedAudio][SilentAudioAfter]concat=n=3:v=0:a=1[AllAudio];" +
                    $"[AllAudio]asetpts=PTS-STARTPTS,asetrate={SampleRate}*{reversalSpeed:0.###},aresample={SampleRate}[SlowAudio];[SlowAudio]areverse[a]";
            }
            else
            {
                // Rubberband audio slowdown.

                videoFilename = $"{reversal.Name}.{reversalNumber}.{reversalRate.ReversalSpeed}-{reversalRate.ReversalTone}.Text";

                audioFiltergraph = $"[0:a]atrim=0:{silentAudioBeforeDuration:0.############},volume=volume=0,areverse[SilentAudioBefore];" +
                $"[0:a]atrim={silentAudioBeforeDuration:0.############}:duration={sampleBasedDuration:0.############},areverse[SelectedAudio];" +
                $"[0:a]atrim={silentAudioBeforeDuration + sampleBasedDuration:0.############}:" +
                $"duration={originalFrameBasedEndSeconds - sampleBasedEndSeconds:0.############}," +
                $"volume=volume=0,areverse[SilentAudioAfter];" +
                $"[SilentAudioBefore][SelectedAudio][SilentAudioAfter]concat=n=3:v=0:a=1[AllAudio];" +
                $"[AllAudio]asetpts=PTS-STARTPTS,rubberband=pitch={reversalTone:0.######}:tempo={reversalSpeed:0.######}:pitchq=quality[a]";
            }

            //
            // Video filtergraphs, without reverse.
            //
            if (ProjectSettings.MotionInterpolation == MotionInterpolation.None || reversalRate.ReversalSpeed == 100)
            {
                // No motion interpolation requested (MotionInterpolation.None) or required (reversalRate.ReversalSpeed == 100).

                interpolationFiltergraph = $"[0:v]trim=0:{originalFrameBasedDuration}," +
                    $"setpts=PTS-STARTPTS," +
                    $"setpts=PTS/{reversalSpeed:0.###}," +
                    $"fps=fps={FramesPerSecond:0.############}:eof_action=pass[SlowForwardV];" +
                    $"[SlowForwardV][1:v]overlay[OverlayedV]";
            }
            else
            {
                minterpolationActive = true;

                switch (ProjectSettings.MotionInterpolation)
                {
                    case MotionInterpolation.BlendFrames:
                        interpolationFiltergraph = $"[0:v]trim=0:{originalFrameBasedDuration}," +
                            $"setpts=PTS-STARTPTS,setpts=PTS/{reversalSpeed:0.###}," +
                            $"tblend=all_mode=average,fps=fps={FramesPerSecond:0.############}:eof_action=pass[SlowForwardV];" +
                            $"[SlowForwardV][1:v]overlay[OverlayedV]";
                        break;

                    case MotionInterpolation.MotionGood:
                        interpolationFiltergraph = $"[0:v]trim=0:{originalFrameBasedDuration}," +
                            $"setpts=PTS-STARTPTS," +
                            $"minterpolate=me=hexbs:fps={FramesPerSecond:0.############}/{reversalSpeed:0.###}," +
                            $"setpts=PTS/{reversalSpeed:0.###},fps=fps={FramesPerSecond:0.############}:eof_action=pass[SlowForwardV];" +
                            $"[SlowForwardV][1:v]overlay[OverlayedV]";
                        break;

                    case MotionInterpolation.MotionBest:
                        interpolationFiltergraph = $"[0:v]trim=0:{originalFrameBasedDuration}," +
                            $"setpts=PTS-STARTPTS," +
                            $"minterpolate=mi_mode=mci:mc_mode=aobmc:me_mode=bidir:vsbmc=1:fps={FramesPerSecond:0.############}/{reversalSpeed:0.###}," +
                            $"setpts=PTS/{reversalSpeed:0.###},fps=fps={FramesPerSecond:0.############}:eof_action=pass[SlowForwardV];" +
                            $"[SlowForwardV][1:v]overlay[OverlayedV]";
                        break;
                }
            }

            // The command to use when memory requirements allow the reverseFiltergraph method.
            // TODO: calculatedFrameBasedStartSeconds is way off and needs fixing.
            ffmpegCommandList.Add($"-y -hide_banner " +
                $"-ss {originalFrameBasedStartSeconds:0.############} -i \"{RelativePathToWorkingInputVideoFile}\" -i \"{reversal.Name}.Text.png\" -loop 1 " +
                $"-filter_complex \"{interpolationFiltergraph}; {reverseVideoFiltergraph}; {audioFiltergraph}\" " +
                $"-map [v] -map [a] -progress \"{videoFilename}.progress\" -threads {{0}} {OutputInterimSettings} " +
                $"\"{videoFilename}{OutputVideoInterimExtension}\"");
            
            videoFilenames.Add(videoFilename);

            // TODO: I am postponing the .png-based reversal video creation.

            // After this, extract the first and last frames of the output video, naming the first file
            // "{videoFilename}.First.png" and the last "{videoFilename}.Last.png".

            // String getFirst = $"-i \"{videoFilename}{OutputOptionsVideoInterimExtension}\" "
            //     + $"-pix_fmt rgb48 -an -filter:v \"select=eq(n\\,0)\" -f image2 -update 1 \"{videoFilename}.First.png\"";
            // int lastFrame = 43 - 1;
            // String getLast = $"-i \"{videoFilename}{OutputOptionsVideoInterimExtension}\" "
            //     + $"-pix_fmt rgb48 -an -filter:v \"select=eq(n\\,{lastFrame})\" -f image2 -update 1 \"{videoFilename}.Last.png\"";


            // If the memory requirements for this reversal video are too great to use reverseFiltergraph method, output a
            // series of .png files for the video.

            // Prior to this, create a directory named {videoFilename}

            ffmpegCommandList.Add($"-y -hide_banner " +
                $"-i \"{RelativePathToWorkingInputVideoFile}\" -i \"{reversal.Name}.Text.png\" -loop 1 -pix_fmt rgb48 -an " + 
                $"-filter_complex \"{interpolationFiltergraph}\" -map [OverlayedV] " +
                $"-progress \"{videoFilename}\\{videoFilename}.progress\" -threads {{0}} \"{videoFilename}\\f%05d.png\"");

            videoFilenames.Add(videoFilename);

            // After this, reorder the .png files in the directory named {videoFilename} by renaming them.

            ffmpegCommandList.Add($"-y -hide_banner " +
                $"-i \"{RelativePathToWorkingInputVideoFile}\" -i \"{videoFilename}\\rf%05d.png\" " +
                $"-filter_complex \"[1:v]null[v];{audioFiltergraph}\" -map [a] -map [v] " +
                $"-progress \"{videoFilename}.progress\" -threads {{0}} {OutputInterimSettings} \"{videoFilename}{OutputVideoInterimExtension}\"");

            videoFilenames.Add(videoFilename);

            // After this, move the first and last .png files from the directory named {videoFilename} to the current
            // directory, naming the first "{videoFilename}.First.png" and the last "{videoFilename}.Last.png".
            // Then delete the directory.

            // Add the video clip to the list of clips.
            Boolean createFFmpegTask = AddToClips(videoFilename, calculatedFrameBasedDuration / FramesPerSecond, AddToVideoOutputs: false);

            if (createFFmpegTask)
            {
                // Add the task to the list of tasks
                if (minterpolationActive)
                {
                    FFmpegTasks.Add(new FFmpegTask(FfmpegPhase.PhaseOne,
                        FfmpegTaskSortOrder.ReverseMinterpolateVideo,
                        calculatedFrameBasedDuration,
                        videoFilenames,
                        ffmpegCommandList));
                }
                else
                {
                    FFmpegTasks.Add(new FFmpegTask(FfmpegPhase.PhaseTwo,
                        FfmpegTaskSortOrder.ReverseVideo,
                        calculatedFrameBasedDuration,
                        videoFilenames,
                        ffmpegCommandList));
                }
            }

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
            double timeInSeconds = (double)sample / (double)SampleRate;
            double lengthInSeconds = (1d / FramesPerSecond) - (1d / 8192d);      // I subtract a small amount to make sure I get only one frame.

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -hide_banner -ss {0:0.######} -i \"{1}\" -pix_fmt rgb48 -an -q:v 1 -t {2:0.######} -f image2 \"{3}.{4}.png\"",
                    timeInSeconds, RelativePathToWorkingInputVideoFile, lengthInSeconds, bookmark.Name, suffix),
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
            double lengthInSeconds = (1d / FramesPerSecond) - (1d / 8192d);      // I subtract a small amount to make sure I get only one frame.

            // Configure the process using the StartInfo properties.
            process.StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegApp,
                Arguments = String.Format("-y -hide_banner -ss 0.0 -i \"{0}\" -pix_fmt rgb48 -an -q:v 1 -t {1:0.######} -f image2 \"OpeningFrame.png\"",
                    RelativePathToWorkingInputVideoFile, lengthInSeconds),
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

            if (!(ExitCode == 0))
            {
                return false;
            }

            return true;
        }

        private bool CreateClosingImage()
        {
            Process process = new Process();
            double lengthInSeconds = (1d / FramesPerSecond) - (1d / 8192d);      // I subtract a small amount to make sure I get only one frame.
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
                    Arguments = String.Format("-y -hide_banner -ss {0:0.######} -i \"{1}\" -pix_fmt rgb48 -an -q:v 1 -t {2:0.######} -f image2 \"ClosingFrame.png\"",
                        ClosingFrameTime, RelativePathToWorkingInputVideoFile, lengthInSeconds),
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
            CreateOpeningAndClosingCardImages();

            return true;
        }

        /// <summary>
        /// Creates .png files containing the overlay text.
        /// </summary>
        /// <returns>Returns true if successful; otherwise false.</returns>
        private bool CreateOverlayPngFiles()
        {
            String forwardOverlayText;
            String reverseOverlayText;

            foreach (Bookmark forwardBookmark in ForwardBookmarks)
            //Parallel.ForEach(ForwardBookmarks, forwardBookmark =>
            {
                // For Forward and Reverse, create a text overly from the forward bookmark text.
                if (ProjectSettings.BookmarkTypeFnR)
                {
                    // Write the text of the forward bookmark to a .png file.
                    if (ProjectSettings.VideoContents == VideoContents.SeparateVideos)
                    {
                        // Write the text of the forward bookmark for each specific reversal to a .png file.
                        for (int j = 0; j < forwardBookmark.ReferencedBookmarks.Count; ++j)
                        {
                            forwardOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                                forwardBookmark.Name + ": " + forwardBookmark.Text : forwardBookmark.Text;

                            forwardOverlayText = KeepSpecificBracketPair(j, forwardOverlayText);

                            // Write the text of the forward bookmark to a .png file.
                            CreateForwardTextOverlay(forwardBookmark.Name + "-" + j + ".Text.png", forwardOverlayText);
                        }
                    }
                    else
                    {
                        forwardOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                            forwardBookmark.Name + ": " + forwardBookmark.Text : forwardBookmark.Text;

                        // Write the text of the forward speech to a .png file.
                        CreateForwardTextOverlay(forwardBookmark.Name + ".Text.png", forwardOverlayText);
                    }
                }

                // if Orphaned Reversals or Quick Check, display the reverse bookmark name and text.
                if (ProjectSettings.BookmarkTypeQuickCheck || ProjectSettings.BookmarkTypeOrphanedReversals)
                {
                    // Get the first referenced bookmark.
                    Bookmark reverseBookmark = forwardBookmark.ReferencedBookmarks[0];

                    reverseOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                            reverseBookmark.Name + ": " + reverseBookmark.Text : reverseBookmark.Text;

                    // Save to the forward name ".Text.png", the name and text of the reverse bookmark.
                    CreateForwardTextOverlay(reverseBookmark.Name + ".Text.png", reverseOverlayText);
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
                    if (ProjectSettings.BookmarkTypeFnR)
                    {
                        // Write the text of the forward speech to a .png file.
                        if (ProjectSettings.VideoContents == VideoContents.SeparateVideos)
                        {
                            // Write the text of the forward speech for each specific reversal to a .png file.
                            forwardOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                                forwardBookmark.Name + ": " + forwardBookmark.Text : forwardBookmark.Text;
                            reverseOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                                reverseBookmark.Name + ": " + reverseBookmark.Text : reverseBookmark.Text;

                            forwardOverlayText = KeepSpecificBracketPair(i, forwardOverlayText);

                            // Write the text of the forward and reverse speech to a .png file.
                            CreateReverseTextOverlay(reverseBookmark.Name + "-" + i + ".Text.png", forwardOverlayText, reverseOverlayText, i);
                        }
                        else
                        {
                            forwardOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                                forwardBookmark.Name + ": " + forwardBookmark.Text : forwardBookmark.Text;
                            reverseOverlayText = ProjectSettings.IncludeBookmarkNameInTextOverlays ?
                                reverseBookmark.Name + ": " + reverseBookmark.Text : reverseBookmark.Text;

                            // Write the text of the forward and reverse speech to a .png file.
                            CreateReverseTextOverlay(reverseBookmark.Name + ".Text.png", forwardOverlayText, reverseOverlayText, i);
                        }
                    }

                    if (ProjectSettings.BookmarkTypeQuickCheck || ProjectSettings.BookmarkTypeOrphanedReversals)
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
            //});

            return true;
        }

        /// <summary>
        /// Removes all square bracket pairs except for the specified bracket pair.
        /// </summary>
        /// <param name="keep">The specified bracket pair to keep.</param>
        /// <param name="BookmarkText">The bookmark text which may contain square bracket pairs.</param>
        /// <returns>The bookmark string with only the specified bracket pair.</returns>
        private string KeepSpecificBracketPair(int keep, string BookmarkText)
        {
            int length = 0;
            String s;

            if (BookmarkText == null || BookmarkText == String.Empty)
            {
                return String.Empty;
            }

            List<BracketPair> bracketPairs = GetBracketLocations(BookmarkText);

            if (bracketPairs.Count == 0) 
            { 
                return BookmarkText; 
            }

            StringBuilder sb = new StringBuilder();

            // Add text before the opening bracket.
            if (bracketPairs[0].BracketOpen > 0)
            {
                length = bracketPairs[0].BracketOpen;
                s = BookmarkText.Substring(0, length);
                sb.Append(s);
            }

            // Walk through the bracket pairs.
            for (int i = 0; i < bracketPairs.Count; ++i)
            {
                length = bracketPairs[i].BracketClose - bracketPairs[i].BracketOpen;

                // Skip removal if this is the bracket pair to keep.
                if (i == keep)
                {
                    // Add the bracketed text with the brackets and the text to the next opening bracket or end of string.

                    // Is there another pair of brackets?
                    if (i + 1 < bracketPairs.Count)
                    {
                        length = bracketPairs[i + 1].BracketOpen - bracketPairs[i].BracketOpen;
                        s = BookmarkText.Substring(bracketPairs[i].BracketOpen, length);
                    }
                    else
                    {
                        // If not, take the rest of the BookmarkText.
                        s = BookmarkText.Substring(bracketPairs[i].BracketOpen);
                    }
                    sb.Append(s);

                    continue;
                }

                // Add text after the opening bracket to before the closing bracket.
                s = BookmarkText.Substring(bracketPairs[i].BracketOpen + 1, length - 1);
                sb.Append(s);

                // Add the text after the closing bracket up to the next opening bracket or end of string.

                // Is there another pair of brackets?
                if (i + 1 < bracketPairs.Count)
                {
                    // There is another bracket pair. Copy up to the opening bracket.
                    length = bracketPairs[i + 1].BracketOpen - 2 - bracketPairs[i].BracketClose + 1;
                    s = BookmarkText.Substring(bracketPairs[i].BracketClose + 1, length);
                    sb.Append(s);
                }
                else
                {
                    // Take the BookmarkText after the closing bracket.
                    // If this closing bracket is at the end of the string, do nothing.
                    if (bracketPairs[i].BracketClose + 1 < BookmarkText.Length)
                    {
                        s = BookmarkText.Substring(bracketPairs[i].BracketClose + 1);
                        sb.Append(s);
                    }
                }
            }

            // Return the new bookmark string.
            return sb.ToString();
        }

        /// <summary>
        /// Gets the locations
        /// </summary>
        /// <param name="bookmarkText"></param>
        /// <returns></returns>
        private List<BracketPair> GetBracketLocations(string bookmarkText)
        {
            List<BracketPair> bracketPairs = new List<BracketPair>();

            int i = 0, opening, closing;
            int index = 0;

            while ((index = bookmarkText.IndexOf('[', index)) >= 0)
            {
                opening = index;

                index = bookmarkText.IndexOf(']', index);
                if (index < 0) break;

                closing = index;

                bracketPairs.Add(new BracketPair(opening, closing));
                ++i;
            }

            return bracketPairs;
        }

        /// <summary>
        /// Creates the opening and closing cards.
        /// </summary>
        private bool CreateOpeningAndClosingCardImages()
        {
            // Get the opening and closing card text from the right source.
            //if (settings.BookmarkFileType == BookmarkFileType.bok || settings.BookmarkFileType == BookmarkFileType.FmBok)
            //{
            //    OpeningCard = BokOpeningCard;
            //    ClosingCard = BokClosingCard;
            //}
            //if (settings.SourceTranscriptFile)
            //{
            //    OpeningCard = TxtOpeningCard;
            //    ClosingCard = TxtClosingCard;
            //}

            OpeningCard = BokOpeningCard;
            ClosingCard = BokClosingCard;

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
            return WordWrap(sourceString, (int)(HorizontalResolution * 0.8d), FontForward);
        }

        /// <summary>
        /// Word wraps the string the pixelWidth.
        /// </summary>
        /// <param name="sourceString">tring to word wrap.</param>
        /// <param name="textOverlayFont">The font to use when measuring for word wrap.</param>
        /// <returns>The word wrapped string.</returns>
        String WordWrap(String sourceString, Font textOverlayFont)
        {
            return WordWrap(sourceString, (int)(HorizontalResolution * 0.8d), textOverlayFont);
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
            SizeF textForwardSize = g.MeasureString(wwForwardText, FontForward, (int)(overlayBmp.Width * 0.8d), formatForward);
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
            Rectangle rectBackground = new Rectangle(0, (int)(overlayBmp.Height * 0.9d - textHeight), overlayBmp.Width, textHeight + (int)((double)overlayBmp.Height / 80.0d));

            // Create a rectangle for the forward and reverse text.
            RectangleF rectForward = new RectangleF(0.0f, (float)overlayBmp.Height * 0.9f - textForwardSize.Height, (float)overlayBmp.Width + 32f, textForwardSize.Height);
            Rectangle rectReverse = new Rectangle(0, 0, 0, 0);
            if (String.IsNullOrWhiteSpace(reverseText) == false)
            {
                rectReverse = new Rectangle(0, (int)(overlayBmp.Height * 0.9d - textHeight), overlayBmp.Width + 32, textReverseSize.Height);
            }

            // Draw the text background (a transparent black rectangle).
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(ProjectSettings.TextBackgroundTransparency, 0, 0, 0)))
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
            Rectangle rect = new Rectangle((int)((double)cardBmp.Width * 0.1d), (int)((double)cardBmp.Height * 0.1d), (int)(cardBmp.Width * 0.8d), (int)(cardBmp.Height * 0.8d));

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