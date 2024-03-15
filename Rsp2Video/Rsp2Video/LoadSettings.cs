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
        /// Loads the program and project settings objects.
        /// </summary>
        private void LoadSettings()
        {
            LoadProgramSettings();
        }

        /// <summary>
        /// Loads the program settings object from the program settings file.
        /// </summary>
        private void LoadProgramSettings()
        {
            ProgramSettings = DeSerializeObject<ProgramSettings>(ProgramSettingsFile);

            // If there was no program settings file, create a new program settings object.
            if (ProgramSettings == null)
            {
                ProgramSettings = new ProgramSettings();
            }

            // Set the main file.
            textBoxMainFile.Text = ProgramSettings.LastUsedFile;

            // Set the values to show the first panel.
            ShowPanel1();
        }

        /// <summary>
        /// Loads the project settings object from the project settings file.
        /// </summary>
        private void LoadProjectSettings(String settingsFile)
        {
            ProjectSettings = DeSerializeObject<ProjectSettings>(settingsFile);

            // If there was no settings file, create a new settings object.
            if (ProjectSettings == null)
            {
                ProjectSettings = new ProjectSettings();

                // Default settings.
                ProjectSettings.BookmarkTypeFnR = true;
                ProjectSettings.VideoContents = VideoContents.BookmarksOnly;
                ProjectSettings.VideoDelay = 0d;
                ProjectSettings.VideoQuality = VideoQuality.YouTube;
                ProjectSettings.ReversalRate1.UseThisRate = true;
                ProjectSettings.ReversalRate1.ReversalSpeed = 100;
                ProjectSettings.ReversalRate1.ReversalTone = 100;
                ProjectSettings.ReversalRate2.UseThisRate = true;
                ProjectSettings.ReversalRate2.ReversalSpeed = 85;
                ProjectSettings.ReversalRate2.ReversalTone = 85;
                ProjectSettings.ReversalRate3.UseThisRate = true;
                ProjectSettings.ReversalRate3.ReversalSpeed = 70;
                ProjectSettings.ReversalRate3.ReversalTone = 70;
                ProjectSettings.ReversalRate4.UseThisRate = false;
                ProjectSettings.ReversalRate4.ReversalSpeed = 50;
                ProjectSettings.ReversalRate4.ReversalTone = 60;
                ProjectSettings.TextForegroundColor = "#9f000000";
                ProjectSettings.TextBackgroundColor = "#ffffffff";
                ProjectSettings.TextBackgroundTransparency = 192;
                ProjectSettings.TextLinesOnScreen = 20;
                ProjectSettings.ReadingCharactersPerSecond = 19.0d;
                ProjectSettings.TransitionLengthCard = 1.0d;
                ProjectSettings.TransitionLengthMajor = 1.0d;
                ProjectSettings.TransitionLengthMinor = 0.5d;
                ProjectSettings.MotionInterpolation = MotionInterpolation.None;
                ProjectSettings.IncludeBackAndForth = true;
                ProjectSettings.ReplayForwardVideo = false;
                ProjectSettings.PlayForwardBookmarkCompletely = true;
                ProjectSettings.IncludeBookmarkNameInTextOverlays = false;
                ProjectSettings.TransitionType = TransitionType.XFade;
                ProjectSettings.XFadeTransitionType = "fade";
                ProjectSettings.IncludeOpeningCard = true;
                ProjectSettings.IncludeClosingCard = true;
                ProjectSettings.IncludeForwardExplanations = true;
                ProjectSettings.IncludeReverseExplanations = true;
                ProjectSettings.DeleteWorkingDirectoriesAtEnd = true;
            }

            // Set the UI elements to their corresponding values in the settings object.
            //textBoxSourceVideoFile.Text = settings.SourceVideoFile;
            //textBoxSoundFile.Text = settings.RspSoundFile;
            //textBoxTranscriptFile.Text = settings.RspTranscriptFile;
            //textBoxMainFile.Text = ProjectSettings.BookmarkFile;
            textBoxOutputFile.Text = ProjectSettings.OutputVideoFile;

            // If the stored video offset is zero, clear the text box.
            if (ProjectSettings.VideoDelay == 0)
            {
                textBoxVideoOffset.Text = String.Empty;
            }
            else
            {
                textBoxVideoOffset.Text = ProjectSettings.VideoDelay.ToString();
            }

            radioButtonBookmarkTypeFnR.Checked = ProjectSettings.BookmarkTypeFnR;
            radioButtonBookmarkTypeQuickCheck.Checked = ProjectSettings.BookmarkTypeQuickCheck;
            radioButtonBookmarkTypeOrphanedReversals.Checked = ProjectSettings.BookmarkTypeOrphanedReversals;
            // radioButtonSourceBookmarkFile.Checked = settings.SourceBookmarkFile;
            // radioButtonSourceTranscriptFile.Checked = ProjectSettings.SourceTranscriptFile;
            saveRadioButtonSourceBookmarkFile = radioButtonSourceBookmarkFile.Checked;
            saveRadioButtonSourceTranscriptFile = radioButtonSourceTranscriptFile.Checked;

            radioButtonContentsEntireVideo.Checked = ProjectSettings.VideoContents == VideoContents.FullVideo ? true : false;
            radioButtonContentsBookmarksOnly.Checked = ProjectSettings.VideoContents == VideoContents.BookmarksOnly ? true : false;
            radioButtonSeparateVideos.Checked = ProjectSettings.VideoContents == VideoContents.SeparateVideos ? true : false;
            saveRadioButtonContentsEntireVideo = radioButtonContentsEntireVideo.Checked;
            saveRadioButtonContentsBookmarksOnly = radioButtonContentsBookmarksOnly.Checked;
            saveRadioButtonSeparateVideos = radioButtonSeparateVideos.Checked;
            labelVideoQualityValue.Text = VideoQualityString[(int)ProjectSettings.VideoQuality];

            checkBoxReversal1.Checked = ProjectSettings.ReversalRate1.UseThisRate;
            textBoxSpeed1.Text = ProjectSettings.ReversalRate1.ReversalSpeed.ToString();
            textBoxTone1.Text = ProjectSettings.ReversalRate1.ReversalTone.ToString();
            checkBoxReversal2.Checked = ProjectSettings.ReversalRate2.UseThisRate;
            textBoxSpeed2.Text = ProjectSettings.ReversalRate2.ReversalSpeed.ToString();
            textBoxTone2.Text = ProjectSettings.ReversalRate2.ReversalTone.ToString();
            checkBoxReversal3.Checked = ProjectSettings.ReversalRate3.UseThisRate;
            textBoxSpeed3.Text = ProjectSettings.ReversalRate3.ReversalSpeed.ToString();
            textBoxTone3.Text = ProjectSettings.ReversalRate3.ReversalTone.ToString();
            checkBoxReversal4.Checked = ProjectSettings.ReversalRate4.UseThisRate;
            textBoxSpeed4.Text = ProjectSettings.ReversalRate4.ReversalSpeed.ToString();
            textBoxTone4.Text = ProjectSettings.ReversalRate4.ReversalTone.ToString();

            checkBoxIncludeBackAndForth.Checked = ProjectSettings.IncludeBackAndForth;
            checkBoxReplayForwardVideo.Checked = ProjectSettings.ReplayForwardVideo;
            saveCheckBoxIncludeBackAndForth = checkBoxIncludeBackAndForth.Checked;
            saveCheckBoxReplayForwardVideo = checkBoxReplayForwardVideo.Checked;

            trackBarVideoQuality.Value = (int)ProjectSettings.VideoQuality;
        }

        /// <summary>
        /// Saves the program and project settings.
        /// </summary>
        private void SaveSettings()
        {
            SaveProgramSettings();
            SaveProjectSettings();
        }

        /// <summary>
        /// Saves the program settings to the RSPro2Video.settings file.
        /// </summary>
        private void SaveProgramSettings()
        {
            String backupFile = ProgramSettingsFile + ".bak";

            // Delete the old backup file.
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }

            // Rename the existing settings file to the backup file.
            if (File.Exists(ProgramSettingsFile))
            {
                File.Move(ProgramSettingsFile, backupFile);
            }

            // Save the Settings file.
            SerializeObject<ProgramSettings>(ProgramSettings, ProgramSettingsFile);
        }

        /// <summary>
        /// Saves the project settings to the .RSPro2Video file.
        /// </summary>
        private void SaveProjectSettings()
        {
            // settings.SourceVideoFile = textBoxSourceVideoFile.Text;
            // settings.RspSoundFile = textBoxSoundFile.Text;
            // settings.RspTranscriptFile = textBoxTranscriptFile.Text;
            ProjectSettings.OutputVideoFile = textBoxOutputFile.Text;

            if (String.IsNullOrWhiteSpace(textBoxVideoOffset.Text))
            {
                ProjectSettings.VideoDelay = 0d;
            } else if (Double.TryParse(textBoxVideoOffset.Text, out Double offset))
            {
                ProjectSettings.VideoDelay = offset;
            }
            else
            {
                ProjectSettings.VideoDelay = 0d;
            }

            ProjectSettings.BookmarkTypeFnR = radioButtonBookmarkTypeFnR.Checked;
            ProjectSettings.BookmarkTypeQuickCheck = radioButtonBookmarkTypeQuickCheck.Checked;
            ProjectSettings.BookmarkTypeOrphanedReversals = radioButtonBookmarkTypeOrphanedReversals.Checked;
            // settings.SourceBookmarkFile = radioButtonSourceBookmarkFile.Checked;
            // ProjectSettings.SourceTranscriptFile = radioButtonSourceTranscriptFile.Checked;

            // Set the values in the settings object corresponding to the values in the UI controls.
            if (radioButtonContentsEntireVideo.Checked) { ProjectSettings.VideoContents = VideoContents.FullVideo; }
            else if (radioButtonContentsBookmarksOnly.Checked) { ProjectSettings.VideoContents = VideoContents.BookmarksOnly; }
            else if (radioButtonSeparateVideos.Checked) { ProjectSettings.VideoContents = VideoContents.SeparateVideos; }
            else { ProjectSettings.VideoContents = VideoContents.None; }

            // TODO: I need to validate the speed and tone since they are text boxes, but must store as int.

            ProjectSettings.ReversalRate1.UseThisRate = checkBoxReversal1.Checked;
            ProjectSettings.ReversalRate1.ReversalSpeed = Int32.Parse(textBoxSpeed1.Text);
            ProjectSettings.ReversalRate1.ReversalTone = Int32.Parse(textBoxTone1.Text);
            ProjectSettings.ReversalRate2.UseThisRate = checkBoxReversal2.Checked;
            ProjectSettings.ReversalRate2.ReversalSpeed = Int32.Parse(textBoxSpeed2.Text);
            ProjectSettings.ReversalRate2.ReversalTone = Int32.Parse(textBoxTone2.Text);
            ProjectSettings.ReversalRate3.UseThisRate = checkBoxReversal3.Checked;
            ProjectSettings.ReversalRate3.ReversalSpeed = Int32.Parse(textBoxSpeed3.Text);
            ProjectSettings.ReversalRate3.ReversalTone = Int32.Parse(textBoxTone3.Text);
            ProjectSettings.ReversalRate4.UseThisRate = checkBoxReversal4.Checked;
            ProjectSettings.ReversalRate4.ReversalSpeed = Int32.Parse(textBoxSpeed4.Text);
            ProjectSettings.ReversalRate4.ReversalTone = Int32.Parse(textBoxTone4.Text);

            ProjectSettings.IncludeBackAndForth = checkBoxIncludeBackAndForth.Checked;
            ProjectSettings.ReplayForwardVideo = checkBoxReplayForwardVideo.Checked;

            // Set the strings to reflect the selected video quality.
            OutputInterimSettings = OutputOptionsInterimSettings[(int)ProjectSettings.VideoQuality];
            OutputImageSequenceSettings = OutputOptionsImageSequenceSettings[(int)ProjectSettings.VideoQuality];
            OutputFinalSettings = OutputOptionsFinalSettings[(int)ProjectSettings.VideoQuality];
            OutputVideoInterimExtension = OutputOptionsVideoInterimExtension[(int)ProjectSettings.VideoQuality];
            OutputVideoFinalExtension = OutputOptionsVideoFinalExtension[(int)ProjectSettings.VideoQuality];
            OutputAudioInterimExtension = OutputOptionsAudioInterimExtension[(int)ProjectSettings.VideoQuality];

            String backupFile = ProjectSettings.ProjectFile + ".bak";

            // Delete the old backup file.
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }

            // Rename the existing settings file to the backup file.
            if (File.Exists(ProjectSettings.ProjectFile))
            {
                File.Move(ProjectSettings.ProjectFile, backupFile);
            }

            // Save the Settings file.
            SerializeObject<ProjectSettings>(ProjectSettings, ProjectSettings.ProjectFile);
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
