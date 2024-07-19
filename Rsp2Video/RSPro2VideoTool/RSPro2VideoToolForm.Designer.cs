namespace RSPro2VideoTool
{
    partial class RSPro2VideoToolForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RSPro2VideoToolForm));
            this.buttonExtractMp3Audio = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxSourceVideoFile = new System.Windows.Forms.TextBox();
            this.labelVideo = new System.Windows.Forms.Label();
            this.labelVideoDescription = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxDeleteLogfile = new System.Windows.Forms.CheckBox();
            this.buttonReencodeVideo = new System.Windows.Forms.Button();
            this.buttonSyncVideo = new System.Windows.Forms.Button();
            this.buttonExtractWavAudio = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labelAudioDescription = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBoxToolAnimation = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToolAnimation)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonExtractMp3Audio
            // 
            this.buttonExtractMp3Audio.Enabled = false;
            this.buttonExtractMp3Audio.Location = new System.Drawing.Point(15, 67);
            this.buttonExtractMp3Audio.Name = "buttonExtractMp3Audio";
            this.buttonExtractMp3Audio.Size = new System.Drawing.Size(380, 32);
            this.buttonExtractMp3Audio.TabIndex = 1;
            this.buttonExtractMp3Audio.Text = "Extract .mp3 file from video";
            this.buttonExtractMp3Audio.UseVisualStyleBackColor = true;
            this.buttonExtractMp3Audio.Click += new System.EventHandler(this.buttonExtractMp3Audio_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(366, 60);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(61, 23);
            this.buttonBrowse.TabIndex = 11;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxSourceVideoFile
            // 
            this.textBoxSourceVideoFile.Location = new System.Drawing.Point(57, 62);
            this.textBoxSourceVideoFile.Name = "textBoxSourceVideoFile";
            this.textBoxSourceVideoFile.Size = new System.Drawing.Size(303, 20);
            this.textBoxSourceVideoFile.TabIndex = 10;
            // 
            // labelVideo
            // 
            this.labelVideo.AutoSize = true;
            this.labelVideo.Location = new System.Drawing.Point(14, 65);
            this.labelVideo.Name = "labelVideo";
            this.labelVideo.Size = new System.Drawing.Size(34, 13);
            this.labelVideo.TabIndex = 12;
            this.labelVideo.Text = "Video";
            // 
            // labelVideoDescription
            // 
            this.labelVideoDescription.AutoSize = true;
            this.labelVideoDescription.Location = new System.Drawing.Point(14, 97);
            this.labelVideoDescription.Name = "labelVideoDescription";
            this.labelVideoDescription.Size = new System.Drawing.Size(275, 13);
            this.labelVideoDescription.TabIndex = 9;
            this.labelVideoDescription.Text = "To begin, drag and drop a video file onto this application.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxDeleteLogfile);
            this.groupBox1.Controls.Add(this.buttonReencodeVideo);
            this.groupBox1.Controls.Add(this.buttonSyncVideo);
            this.groupBox1.Controls.Add(this.buttonExtractWavAudio);
            this.groupBox1.Controls.Add(this.buttonExtractMp3Audio);
            this.groupBox1.Location = new System.Drawing.Point(17, 146);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 235);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            // 
            // checkBoxDeleteLogfile
            // 
            this.checkBoxDeleteLogfile.AutoSize = true;
            this.checkBoxDeleteLogfile.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBoxDeleteLogfile.Checked = true;
            this.checkBoxDeleteLogfile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDeleteLogfile.Location = new System.Drawing.Point(207, 212);
            this.checkBoxDeleteLogfile.Name = "checkBoxDeleteLogfile";
            this.checkBoxDeleteLogfile.Size = new System.Drawing.Size(188, 17);
            this.checkBoxDeleteLogfile.TabIndex = 4;
            this.checkBoxDeleteLogfile.Text = "Delete log file after task completes";
            this.checkBoxDeleteLogfile.UseVisualStyleBackColor = true;
            // 
            // buttonReencodeVideo
            // 
            this.buttonReencodeVideo.Enabled = false;
            this.buttonReencodeVideo.Location = new System.Drawing.Point(15, 105);
            this.buttonReencodeVideo.Name = "buttonReencodeVideo";
            this.buttonReencodeVideo.Size = new System.Drawing.Size(380, 32);
            this.buttonReencodeVideo.TabIndex = 2;
            this.buttonReencodeVideo.Text = "Re-encode video for RS-Video";
            this.buttonReencodeVideo.UseVisualStyleBackColor = true;
            this.buttonReencodeVideo.Click += new System.EventHandler(this.buttonReencodeVideo_Click);
            // 
            // buttonSyncVideo
            // 
            this.buttonSyncVideo.Enabled = false;
            this.buttonSyncVideo.Location = new System.Drawing.Point(15, 143);
            this.buttonSyncVideo.Name = "buttonSyncVideo";
            this.buttonSyncVideo.Size = new System.Drawing.Size(380, 32);
            this.buttonSyncVideo.TabIndex = 3;
            this.buttonSyncVideo.Text = "Synchronize video with audio";
            this.buttonSyncVideo.UseVisualStyleBackColor = true;
            this.buttonSyncVideo.Click += new System.EventHandler(this.buttonSyncVideo_Click);
            // 
            // buttonExtractWavAudio
            // 
            this.buttonExtractWavAudio.Enabled = false;
            this.buttonExtractWavAudio.Location = new System.Drawing.Point(15, 29);
            this.buttonExtractWavAudio.Name = "buttonExtractWavAudio";
            this.buttonExtractWavAudio.Size = new System.Drawing.Size(380, 32);
            this.buttonExtractWavAudio.TabIndex = 0;
            this.buttonExtractWavAudio.Text = "Extract .wav file from video";
            this.buttonExtractWavAudio.UseVisualStyleBackColor = true;
            this.buttonExtractWavAudio.Click += new System.EventHandler(this.buttonExtractWavAudio_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(9, 452);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(413, 23);
            this.labelStatus.TabIndex = 11;
            this.labelStatus.Text = "Drag and drop a video file here.";
            // 
            // panel1
            // 
            this.panel1.AllowDrop = true;
            this.panel1.Controls.Add(this.labelAudioDescription);
            this.panel1.Controls.Add(this.labelVideoDescription);
            this.panel1.Controls.Add(this.labelVideo);
            this.panel1.Controls.Add(this.textBoxSourceVideoFile);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.buttonBrowse);
            this.panel1.Location = new System.Drawing.Point(0, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(440, 440);
            this.panel1.TabIndex = 12;
            this.panel1.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel1_DragDrop);
            this.panel1.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel1_DragEnter);
            // 
            // labelAudioDescription
            // 
            this.labelAudioDescription.AutoSize = true;
            this.labelAudioDescription.Enabled = false;
            this.labelAudioDescription.Location = new System.Drawing.Point(14, 119);
            this.labelAudioDescription.Name = "labelAudioDescription";
            this.labelAudioDescription.Size = new System.Drawing.Size(40, 13);
            this.labelAudioDescription.TabIndex = 12;
            this.labelAudioDescription.Text = "Audio: ";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pictureBoxToolAnimation);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(440, 440);
            this.panel2.TabIndex = 13;
            this.panel2.Visible = false;
            // 
            // pictureBoxToolAnimation
            // 
            this.pictureBoxToolAnimation.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxToolAnimation.Name = "pictureBoxToolAnimation";
            this.pictureBoxToolAnimation.Size = new System.Drawing.Size(440, 440);
            this.pictureBoxToolAnimation.TabIndex = 0;
            this.pictureBoxToolAnimation.TabStop = false;
            // 
            // RSPro2VideoToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 478);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.labelStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RSPro2VideoToolForm";
            this.Text = "Reverse Speech to Video Tool";
            this.Load += new System.EventHandler(this.RSPro2VideoToolForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToolAnimation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonExtractMp3Audio;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxSourceVideoFile;
        private System.Windows.Forms.Label labelVideo;
        private System.Windows.Forms.Label labelVideoDescription;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelAudioDescription;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBoxToolAnimation;
        private System.Windows.Forms.Button buttonExtractWavAudio;
        private System.Windows.Forms.Button buttonReencodeVideo;
        private System.Windows.Forms.Button buttonSyncVideo;
        private System.Windows.Forms.CheckBox checkBoxDeleteLogfile;
    }
}

