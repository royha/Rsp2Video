namespace RSPro2VideoTool
{
    partial class VideoSyncForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoSyncForm));
            this.labelVideoOffset = new System.Windows.Forms.Label();
            this.numericOffset = new System.Windows.Forms.NumericUpDown();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.labelDuration = new System.Windows.Forms.Label();
            this.labelVideoName = new System.Windows.Forms.Label();
            this.labelDurationValue = new System.Windows.Forms.Label();
            this.buttonMakeTestRun = new System.Windows.Forms.Button();
            this.buttonMakeFinalSyncedVideo = new System.Windows.Forms.Button();
            this.labelStartTimeValue = new System.Windows.Forms.Label();
            this.labelVideoNameValue = new System.Windows.Forms.Label();
            this.groupBoxTestRunVideo = new System.Windows.Forms.GroupBox();
            this.trackBarVideoDuration = new System.Windows.Forms.TrackBar();
            this.trackBarVideoStartTime = new System.Windows.Forms.TrackBar();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelStep1a = new System.Windows.Forms.Label();
            this.labelStep1b = new System.Windows.Forms.Label();
            this.labelStep2a = new System.Windows.Forms.Label();
            this.labelStep2b = new System.Windows.Forms.Label();
            this.labelStep3 = new System.Windows.Forms.Label();
            this.labelStep1c = new System.Windows.Forms.Label();
            this.groupBoxVideoSync = new System.Windows.Forms.GroupBox();
            this.labelVideoSyncStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericOffset)).BeginInit();
            this.groupBoxTestRunVideo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoDuration)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoStartTime)).BeginInit();
            this.groupBoxVideoSync.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelVideoOffset
            // 
            this.labelVideoOffset.AutoSize = true;
            this.labelVideoOffset.Location = new System.Drawing.Point(17, 111);
            this.labelVideoOffset.Name = "labelVideoOffset";
            this.labelVideoOffset.Size = new System.Drawing.Size(111, 13);
            this.labelVideoOffset.TabIndex = 0;
            this.labelVideoOffset.Text = "Video offset in frames:";
            // 
            // numericOffset
            // 
            this.numericOffset.DecimalPlaces = 2;
            this.numericOffset.Location = new System.Drawing.Point(134, 109);
            this.numericOffset.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
            this.numericOffset.Name = "numericOffset";
            this.numericOffset.Size = new System.Drawing.Size(52, 20);
            this.numericOffset.TabIndex = 1;
            this.numericOffset.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(6, 25);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(54, 13);
            this.labelStartTime.TabIndex = 4;
            this.labelStartTime.Text = "Start time:";
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(6, 89);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(50, 13);
            this.labelDuration.TabIndex = 5;
            this.labelDuration.Text = "Duration:";
            // 
            // labelVideoName
            // 
            this.labelVideoName.AutoSize = true;
            this.labelVideoName.Location = new System.Drawing.Point(17, 76);
            this.labelVideoName.Name = "labelVideoName";
            this.labelVideoName.Size = new System.Drawing.Size(68, 13);
            this.labelVideoName.TabIndex = 6;
            this.labelVideoName.Text = "Video Name:";
            // 
            // labelDurationValue
            // 
            this.labelDurationValue.AutoSize = true;
            this.labelDurationValue.Location = new System.Drawing.Point(62, 89);
            this.labelDurationValue.Name = "labelDurationValue";
            this.labelDurationValue.Size = new System.Drawing.Size(96, 13);
            this.labelDurationValue.TabIndex = 7;
            this.labelDurationValue.Text = "labelDurationValue";
            // 
            // buttonMakeTestRun
            // 
            this.buttonMakeTestRun.Location = new System.Drawing.Point(6, 156);
            this.buttonMakeTestRun.Name = "buttonMakeTestRun";
            this.buttonMakeTestRun.Size = new System.Drawing.Size(152, 23);
            this.buttonMakeTestRun.TabIndex = 12;
            this.buttonMakeTestRun.Text = "Make and play test run video";
            this.buttonMakeTestRun.UseVisualStyleBackColor = true;
            this.buttonMakeTestRun.Click += new System.EventHandler(this.buttonMakeTestRun_Click);
            // 
            // buttonMakeFinalSyncedVideo
            // 
            this.buttonMakeFinalSyncedVideo.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonMakeFinalSyncedVideo.Location = new System.Drawing.Point(17, 405);
            this.buttonMakeFinalSyncedVideo.Name = "buttonMakeFinalSyncedVideo";
            this.buttonMakeFinalSyncedVideo.Size = new System.Drawing.Size(130, 23);
            this.buttonMakeFinalSyncedVideo.TabIndex = 14;
            this.buttonMakeFinalSyncedVideo.Text = "Make final synced video";
            this.buttonMakeFinalSyncedVideo.UseVisualStyleBackColor = true;
            this.buttonMakeFinalSyncedVideo.Click += new System.EventHandler(this.buttonMakeFinalVideo_Click);
            // 
            // labelStartTimeValue
            // 
            this.labelStartTimeValue.AutoSize = true;
            this.labelStartTimeValue.Location = new System.Drawing.Point(66, 25);
            this.labelStartTimeValue.Name = "labelStartTimeValue";
            this.labelStartTimeValue.Size = new System.Drawing.Size(101, 13);
            this.labelStartTimeValue.TabIndex = 15;
            this.labelStartTimeValue.Text = "labelStartTimeValue";
            // 
            // labelVideoNameValue
            // 
            this.labelVideoNameValue.AutoSize = true;
            this.labelVideoNameValue.Location = new System.Drawing.Point(91, 76);
            this.labelVideoNameValue.Name = "labelVideoNameValue";
            this.labelVideoNameValue.Size = new System.Drawing.Size(111, 13);
            this.labelVideoNameValue.TabIndex = 16;
            this.labelVideoNameValue.Text = "labelVideoNameValue";
            // 
            // groupBoxTestRunVideo
            // 
            this.groupBoxTestRunVideo.Controls.Add(this.trackBarVideoDuration);
            this.groupBoxTestRunVideo.Controls.Add(this.trackBarVideoStartTime);
            this.groupBoxTestRunVideo.Controls.Add(this.labelStartTime);
            this.groupBoxTestRunVideo.Controls.Add(this.labelDuration);
            this.groupBoxTestRunVideo.Controls.Add(this.buttonMakeTestRun);
            this.groupBoxTestRunVideo.Controls.Add(this.labelStartTimeValue);
            this.groupBoxTestRunVideo.Controls.Add(this.labelDurationValue);
            this.groupBoxTestRunVideo.Location = new System.Drawing.Point(17, 181);
            this.groupBoxTestRunVideo.Name = "groupBoxTestRunVideo";
            this.groupBoxTestRunVideo.Size = new System.Drawing.Size(391, 185);
            this.groupBoxTestRunVideo.TabIndex = 17;
            this.groupBoxTestRunVideo.TabStop = false;
            this.groupBoxTestRunVideo.Text = "Test run video settings";
            // 
            // trackBarVideoDuration
            // 
            this.trackBarVideoDuration.LargeChange = 1;
            this.trackBarVideoDuration.Location = new System.Drawing.Point(9, 105);
            this.trackBarVideoDuration.Maximum = 9;
            this.trackBarVideoDuration.Minimum = 1;
            this.trackBarVideoDuration.Name = "trackBarVideoDuration";
            this.trackBarVideoDuration.Size = new System.Drawing.Size(376, 45);
            this.trackBarVideoDuration.TabIndex = 15;
            this.trackBarVideoDuration.Value = 1;
            this.trackBarVideoDuration.ValueChanged += new System.EventHandler(this.trackBarVideoDuration_ValueChanged);
            // 
            // trackBarVideoStartTime
            // 
            this.trackBarVideoStartTime.Location = new System.Drawing.Point(9, 41);
            this.trackBarVideoStartTime.Name = "trackBarVideoStartTime";
            this.trackBarVideoStartTime.Size = new System.Drawing.Size(376, 45);
            this.trackBarVideoStartTime.TabIndex = 14;
            this.trackBarVideoStartTime.ValueChanged += new System.EventHandler(this.trackBarVideoStartTime_ValueChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(153, 405);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 18;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelStep1a
            // 
            this.labelStep1a.AutoSize = true;
            this.labelStep1a.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep1a.Location = new System.Drawing.Point(17, 25);
            this.labelStep1a.Name = "labelStep1a";
            this.labelStep1a.Size = new System.Drawing.Size(391, 13);
            this.labelStep1a.TabIndex = 19;
            this.labelStep1a.Text = "1. To synchronize the video with the audio, set the number of video";
            // 
            // labelStep1b
            // 
            this.labelStep1b.AutoSize = true;
            this.labelStep1b.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep1b.Location = new System.Drawing.Point(18, 38);
            this.labelStep1b.Name = "labelStep1b";
            this.labelStep1b.Size = new System.Drawing.Size(372, 13);
            this.labelStep1b.TabIndex = 20;
            this.labelStep1b.Text = "    frames to move the video forward or backward, using positive";
            // 
            // labelStep2a
            // 
            this.labelStep2a.AutoSize = true;
            this.labelStep2a.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep2a.Location = new System.Drawing.Point(17, 143);
            this.labelStep2a.Name = "labelStep2a";
            this.labelStep2a.Size = new System.Drawing.Size(388, 13);
            this.labelStep2a.TabIndex = 21;
            this.labelStep2a.Text = "2. Optionally create a short test video to verify the audio and video";
            // 
            // labelStep2b
            // 
            this.labelStep2b.AutoSize = true;
            this.labelStep2b.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep2b.Location = new System.Drawing.Point(17, 156);
            this.labelStep2b.Name = "labelStep2b";
            this.labelStep2b.Size = new System.Drawing.Size(258, 13);
            this.labelStep2b.TabIndex = 22;
            this.labelStep2b.Text = "    sync when played forward and backward.";
            // 
            // labelStep3
            // 
            this.labelStep3.AutoSize = true;
            this.labelStep3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep3.Location = new System.Drawing.Point(17, 378);
            this.labelStep3.Name = "labelStep3";
            this.labelStep3.Size = new System.Drawing.Size(191, 13);
            this.labelStep3.TabIndex = 23;
            this.labelStep3.Text = "3. Create the fully synced video.";
            // 
            // labelStep1c
            // 
            this.labelStep1c.AutoSize = true;
            this.labelStep1c.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep1c.Location = new System.Drawing.Point(17, 51);
            this.labelStep1c.Name = "labelStep1c";
            this.labelStep1c.Size = new System.Drawing.Size(312, 13);
            this.labelStep1c.TabIndex = 24;
            this.labelStep1c.Text = "    values or negative values respectively (eg 2, -1.5).";
            // 
            // groupBoxVideoSync
            // 
            this.groupBoxVideoSync.Controls.Add(this.labelStep1a);
            this.groupBoxVideoSync.Controls.Add(this.labelStep1c);
            this.groupBoxVideoSync.Controls.Add(this.labelVideoName);
            this.groupBoxVideoSync.Controls.Add(this.labelStep3);
            this.groupBoxVideoSync.Controls.Add(this.buttonMakeFinalSyncedVideo);
            this.groupBoxVideoSync.Controls.Add(this.labelStep2b);
            this.groupBoxVideoSync.Controls.Add(this.labelVideoNameValue);
            this.groupBoxVideoSync.Controls.Add(this.labelStep2a);
            this.groupBoxVideoSync.Controls.Add(this.groupBoxTestRunVideo);
            this.groupBoxVideoSync.Controls.Add(this.labelStep1b);
            this.groupBoxVideoSync.Controls.Add(this.numericOffset);
            this.groupBoxVideoSync.Controls.Add(this.labelVideoOffset);
            this.groupBoxVideoSync.Controls.Add(this.buttonCancel);
            this.groupBoxVideoSync.Location = new System.Drawing.Point(12, 12);
            this.groupBoxVideoSync.Name = "groupBoxVideoSync";
            this.groupBoxVideoSync.Size = new System.Drawing.Size(434, 441);
            this.groupBoxVideoSync.TabIndex = 25;
            this.groupBoxVideoSync.TabStop = false;
            // 
            // labelVideoSyncStatus
            // 
            this.labelVideoSyncStatus.AutoSize = true;
            this.labelVideoSyncStatus.Location = new System.Drawing.Point(13, 469);
            this.labelVideoSyncStatus.Name = "labelVideoSyncStatus";
            this.labelVideoSyncStatus.Size = new System.Drawing.Size(0, 13);
            this.labelVideoSyncStatus.TabIndex = 26;
            // 
            // VideoSyncForm
            // 
            this.AcceptButton = this.buttonMakeTestRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 501);
            this.Controls.Add(this.labelVideoSyncStatus);
            this.Controls.Add(this.groupBoxVideoSync);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VideoSyncForm";
            this.Text = "VideoSyncForm";
            ((System.ComponentModel.ISupportInitialize)(this.numericOffset)).EndInit();
            this.groupBoxTestRunVideo.ResumeLayout(false);
            this.groupBoxTestRunVideo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoDuration)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarVideoStartTime)).EndInit();
            this.groupBoxVideoSync.ResumeLayout(false);
            this.groupBoxVideoSync.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelVideoOffset;
        private System.Windows.Forms.NumericUpDown numericOffset;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelVideoName;
        private System.Windows.Forms.Label labelDurationValue;
        private System.Windows.Forms.Button buttonMakeTestRun;
        private System.Windows.Forms.Button buttonMakeFinalSyncedVideo;
        private System.Windows.Forms.Label labelStartTimeValue;
        private System.Windows.Forms.Label labelVideoNameValue;
        private System.Windows.Forms.GroupBox groupBoxTestRunVideo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelStep1a;
        private System.Windows.Forms.Label labelStep1b;
        private System.Windows.Forms.Label labelStep2a;
        private System.Windows.Forms.Label labelStep2b;
        private System.Windows.Forms.Label labelStep3;
        private System.Windows.Forms.Label labelStep1c;
        private System.Windows.Forms.TrackBar trackBarVideoStartTime;
        private System.Windows.Forms.TrackBar trackBarVideoDuration;
        private System.Windows.Forms.GroupBox groupBoxVideoSync;
        private System.Windows.Forms.Label labelVideoSyncStatus;
    }
}