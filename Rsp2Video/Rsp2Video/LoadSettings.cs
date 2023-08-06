using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace RSPro2Video
{
    public partial class RSPro2VideoForm : Form
    {
        /// <summary>
        /// Loads the settings object from the settings file.
        /// </summary>
        private void LoadSettings()
        {
            settings = DeSerializeObject<Settings>(SettingsFile);

            // If there was no settings file, create a new settings object.
            if (settings == null)
            {
                settings = new Settings();

                // Default settings.
                settings.BookmarkTypeFnR = true;
                settings.SourceBookmarkFile = true;
                settings.VideoContents = VideoContents.BookmarksOnly;
                settings.VideoQuality = VideoQuality.Small;
                settings.IncludeBackAndForth = true;
                settings.ReversalRate1.UseThisRate = true;
                settings.ReversalRate1.ReversalSpeed = 100;
                settings.ReversalRate1.ReversalTone = 100;
                settings.ReversalRate2.UseThisRate = true;
                settings.ReversalRate2.ReversalSpeed = 85;
                settings.ReversalRate2.ReversalTone = 85;
                settings.ReversalRate3.UseThisRate = true;
                settings.ReversalRate3.ReversalSpeed = 70;
                settings.ReversalRate3.ReversalTone = 70;
                settings.ReversalRate4.UseThisRate = true;
                settings.ReversalRate4.ReversalSpeed = 50;
                settings.ReversalRate4.ReversalTone = 62;
                settings.AudioDelay = 0;

                settings.OutputOptionsInterimSettings = new List<String>(new String[] {
                    "-qscale:v 2",
                    "-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 18 -c:a aac -b:a 384k -movflags +faststart -reset_timestamps 1",
                    // "-c:v h264_nvenc -preset p2 -profile:v high -b:v 5M -bufsize 5M -c:a aac -b:a 384k -movflags +faststart -reset_timestamps 1",
                    "-pix_fmt yuv420p -c:v libx264 -preset slow -profile:v high -bf 2 -g 30 -coder 1 -crf 16 -c:a aac -b:a 384k -movflags +faststart -reset_timestamps 1" });
                settings.OutputOptionsInterimSettingsQmelt = new List<String>(new String[] {
                    "qscale=2",
                    "vcodec=libx264 preset=ultrafast crf=18 acodec=libmp3lame",
                    "vcodec=libx264 preset=slow crf=16 acodec=libmp3lame" });
                settings.OutputOptionsImageSequenceSettings = new List<String>(new String[] {
                    "-qscale:v 2",
                    "-c:v libx264 -preset ultrafast -crf 18 -threads 0 -c:a aac -b:a 384k -movflags +faststart",
                    // "-c:v h264_nvenc -preset p2 -profile:v high -b:v 5M -bufsize 5M -c:a aac -b:a 384k -movflags +faststart",
                    "-c:v libx264 -preset slow -crf 16 -threads 0 -c:a aac -b:a 384k -movflags +faststart" });
                settings.OutputOptionsFinalSettings = new List<String>(new String[] {
                    "-qscale:v 2",
                    "-pix_fmt yuv420p -c:v libx264 -preset ultrafast -profile:v high -bf 2 -g 30 -coder 1 -crf 18 -c:a aac -b:a 384k -movflags +faststart",
                    "-pix_fmt yuv420p -c:v libx264 -preset slow -profile:v high -bf 2 -g 30 -coder 1 -crf 16 -c:a aac -b:a 384k -movflags +faststart" });
                settings.OutputOptionsFinalSettingsQmelt = new List<String>(new String[] {
                    "qscale=2",
                    "vcodec=libx264 preset=ultrafast crf=23 acodec=libmp3lame",
                    "vcodec=libx264 preset=slow crf=18 acodec=libmp3lame" });
                settings.OutputOptionsVideoInterimExtension = new List<String>(new String[] { ".ts", ".mp4", ".mp4" });
                settings.OutputOptionsVideoFinalExtension = new List<String>(new String[] { ".ts", ".mp4", ".mp4" });
                settings.OutputOptionsAudioInterimExtension = new List<String>(new String[] { ".wav", ".wav", ".wav" });
            }

            // Set the UI elements to their corresponding values in the settings object.
            textBoxSourceVideoFile.Text = settings.SourceVideoFile;
            textBoxSoundFile.Text = settings.RspSoundFile;
            textBoxTranscriptFile.Text = settings.RspTranscriptFile;
            textBoxOutputFile.Text = settings.OutputVideoFile;

            // If the stored video offset is zero, clear the text box.
            if (settings.AudioDelay == 0)
            {
                textBoxVideoOffset.Text = String.Empty;
            }
            else
            {
                textBoxVideoOffset.Text = settings.AudioDelay.ToString();
            }

            radioButtonBookmarkTypeFnR.Checked = settings.BookmarkTypeFnR;
            radioButtonBookmarkTypeQuickCheck.Checked = settings.BookmarkTypeQuickCheck;
            radioButtonBookmarkTypeOrphanedReversals.Checked = settings.BookmarkTypeOrphanedReversals;
            radioButtonSourceBookmarkFile.Checked = settings.SourceBookmarkFile;
            radioButtonSourceTranscriptFile.Checked = settings.SourceTranscriptFile;
            saveRadioButtonSourceBookmarkFile = radioButtonSourceBookmarkFile.Checked;
            saveRadioButtonSourceTranscriptFile = radioButtonSourceTranscriptFile.Checked;

            radioButtonContentsEntireVideo.Checked = settings.VideoContents == VideoContents.FullVideo ? true : false;
            radioButtonContentsBookmarksOnly.Checked = settings.VideoContents == VideoContents.BookmarksOnly ? true : false;
            radioButtonSeparateVideos.Checked = settings.VideoContents == VideoContents.SeparateVideos ? true : false;
            saveRadioButtonContentsEntireVideo = radioButtonContentsEntireVideo.Checked;
            saveRadioButtonContentsBookmarksOnly = radioButtonContentsBookmarksOnly.Checked;
            saveRadioButtonSeparateVideos = radioButtonSeparateVideos.Checked;
            labelVideoQualityValue.Text = VideoQualityString[(int)settings.VideoQuality];

            checkBoxReversal1.Checked = settings.ReversalRate1.UseThisRate;
            textBoxSpeed1.Text = settings.ReversalRate1.ReversalSpeed.ToString();
            textBoxTone1.Text = settings.ReversalRate1.ReversalTone.ToString();
            checkBoxReversal2.Checked = settings.ReversalRate2.UseThisRate;
            textBoxSpeed2.Text = settings.ReversalRate2.ReversalSpeed.ToString();
            textBoxTone2.Text = settings.ReversalRate2.ReversalTone.ToString();
            checkBoxReversal3.Checked = settings.ReversalRate3.UseThisRate;
            textBoxSpeed3.Text = settings.ReversalRate3.ReversalSpeed.ToString();
            textBoxTone3.Text = settings.ReversalRate3.ReversalTone.ToString();
            checkBoxReversal4.Checked = settings.ReversalRate4.UseThisRate;
            textBoxSpeed4.Text = settings.ReversalRate4.ReversalSpeed.ToString();
            textBoxTone4.Text = settings.ReversalRate4.ReversalTone.ToString();

            checkBoxIncludeBackAndForth.Checked = settings.IncludeBackAndForth;
            checkBoxReplayForwardVideo.Checked = settings.ReplayForwardVideo;
            saveCheckBoxIncludeBackAndForth = checkBoxIncludeBackAndForth.Checked;
            saveCheckBoxReplayForwardVideo = checkBoxReplayForwardVideo.Checked;

            trackBarVideoQuality.Value = (int)settings.VideoQuality;

            // Set the values to show the first panel.
            ShowPanel1();
        }

        /// <summary>
        /// Saves the settings from the UI to the settings object and to the settings file.
        /// </summary>
        private void SaveSettings()
        {
            settings.SourceVideoFile = textBoxSourceVideoFile.Text;
            settings.RspSoundFile = textBoxSoundFile.Text;
            settings.RspTranscriptFile = textBoxTranscriptFile.Text;
            settings.OutputVideoFile = textBoxOutputFile.Text;
            settings.AudioDelay = String.IsNullOrWhiteSpace(textBoxVideoOffset.Text) ? 0 : Int32.Parse(textBoxVideoOffset.Text);

            settings.BookmarkTypeFnR = radioButtonBookmarkTypeFnR.Checked;
            settings.BookmarkTypeQuickCheck = radioButtonBookmarkTypeQuickCheck.Checked;
            settings.BookmarkTypeOrphanedReversals = radioButtonBookmarkTypeOrphanedReversals.Checked;
            settings.SourceBookmarkFile = radioButtonSourceBookmarkFile.Checked;
            settings.SourceTranscriptFile = radioButtonSourceTranscriptFile.Checked;

            // Set the values in the settings object corresponding to the values in the UI controls.
            if (radioButtonContentsEntireVideo.Checked) { settings.VideoContents = VideoContents.FullVideo; }
            else if (radioButtonContentsBookmarksOnly.Checked) { settings.VideoContents = VideoContents.BookmarksOnly; }
            else if (radioButtonSeparateVideos.Checked) { settings.VideoContents = VideoContents.SeparateVideos; }
            else { settings.VideoContents = VideoContents.None; }

            // TODO: I need to validate the speed and tone since they are text boxes, but must store as int.

            settings.ReversalRate1.UseThisRate = checkBoxReversal1.Checked;
            settings.ReversalRate1.ReversalSpeed = Int32.Parse(textBoxSpeed1.Text);
            settings.ReversalRate1.ReversalTone = Int32.Parse(textBoxTone1.Text);
            settings.ReversalRate2.UseThisRate = checkBoxReversal2.Checked;
            settings.ReversalRate2.ReversalSpeed = Int32.Parse(textBoxSpeed2.Text);
            settings.ReversalRate2.ReversalTone = Int32.Parse(textBoxTone2.Text);
            settings.ReversalRate3.UseThisRate = checkBoxReversal3.Checked;
            settings.ReversalRate3.ReversalSpeed = Int32.Parse(textBoxSpeed3.Text);
            settings.ReversalRate3.ReversalTone = Int32.Parse(textBoxTone3.Text);
            settings.ReversalRate4.UseThisRate = checkBoxReversal4.Checked;
            settings.ReversalRate4.ReversalSpeed = Int32.Parse(textBoxSpeed4.Text);
            settings.ReversalRate4.ReversalTone = Int32.Parse(textBoxTone4.Text);

            settings.IncludeBackAndForth = checkBoxIncludeBackAndForth.Checked;
            settings.ReplayForwardVideo = checkBoxReplayForwardVideo.Checked;

            // Set the strings to reflect the selected video quality.
            OutputInterimSettings = settings.OutputOptionsInterimSettings[(int)settings.VideoQuality];
            OutputInterimSettingsQmelt = settings.OutputOptionsInterimSettingsQmelt[(int)settings.VideoQuality];
            OutputImageSequenceSettings = settings.OutputOptionsImageSequenceSettings[(int)settings.VideoQuality];
            OutputFinalSettings = settings.OutputOptionsFinalSettings[(int)settings.VideoQuality];
            OutputFinalSettingsQmelt = settings.OutputOptionsFinalSettingsQmelt[(int)settings.VideoQuality];
            OutputVideoInterimExtension = settings.OutputOptionsVideoInterimExtension[(int)settings.VideoQuality];
            OutputVideoFinalExtension = settings.OutputOptionsVideoFinalExtension[(int)settings.VideoQuality];
            OutputAudioInterimExtension = settings.OutputOptionsAudioInterimExtension[(int)settings.VideoQuality];

            SerializeObject<Settings>(settings, SettingsFile);
        }


        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T">The type of object to save to disk.</typeparam>
        /// <param name="serializableObject">The object to serialize and save to disk.</param>
        /// <param name="fileName">>The filename for the serialized object.</param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            catch
            {
                // Log exception here
            }
        }


        /// <summary>
        /// Deserializes an XML file into an object list
        /// </summary>
        /// <typeparam name="T">The type of object to read from disk.</typeparam>
        /// <param name="fileName">The filename to read from disk.</param>
        /// <returns>The deserialized object.</returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch
            {
                //Log exception here
            }

            return objectOut;
        }
    }
}
