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
            this.labelVideoLength = new System.Windows.Forms.Label();
            this.numericStartTimeMinutes = new System.Windows.Forms.NumericUpDown();
            this.labelSeconds = new System.Windows.Forms.Label();
            this.numericStartTimeSeconds = new System.Windows.Forms.NumericUpDown();
            this.numericDuration = new System.Windows.Forms.NumericUpDown();
            this.buttonMakeTestRun = new System.Windows.Forms.Button();
            this.buttonViewTestRunVideo = new System.Windows.Forms.Button();
            this.buttonMakeFinalSyncedVideo = new System.Windows.Forms.Button();
            this.labelVideoLengthValue = new System.Windows.Forms.Label();
            this.labelVideoNameValue = new System.Windows.Forms.Label();
            this.groupBoxTestRunVideo = new System.Windows.Forms.GroupBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelStep1a = new System.Windows.Forms.Label();
            this.labelStep1b = new System.Windows.Forms.Label();
            this.labelStep2a = new System.Windows.Forms.Label();
            this.labelStep2b = new System.Windows.Forms.Label();
            this.labelStep3 = new System.Windows.Forms.Label();
            this.labelStep1c = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStartTimeMinutes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStartTimeSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDuration)).BeginInit();
            this.groupBoxTestRunVideo.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelVideoOffset
            // 
            this.labelVideoOffset.AutoSize = true;
            this.labelVideoOffset.Location = new System.Drawing.Point(12, 104);
            this.labelVideoOffset.Name = "labelVideoOffset";
            this.labelVideoOffset.Size = new System.Drawing.Size(111, 13);
            this.labelVideoOffset.TabIndex = 0;
            this.labelVideoOffset.Text = "Video offset in frames:";
            // 
            // numericOffset
            // 
            this.numericOffset.DecimalPlaces = 2;
            this.numericOffset.Location = new System.Drawing.Point(129, 102);
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
            this.labelStartTime.Size = new System.Drawing.Size(100, 13);
            this.labelStartTime.TabIndex = 4;
            this.labelStartTime.Text = "Start time: Minutes::";
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(6, 51);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(98, 13);
            this.labelDuration.TabIndex = 5;
            this.labelDuration.Text = "Duration: Seconds:";
            // 
            // labelVideoName
            // 
            this.labelVideoName.AutoSize = true;
            this.labelVideoName.Location = new System.Drawing.Point(12, 69);
            this.labelVideoName.Name = "labelVideoName";
            this.labelVideoName.Size = new System.Drawing.Size(68, 13);
            this.labelVideoName.TabIndex = 6;
            this.labelVideoName.Text = "Video Name:";
            // 
            // labelVideoLength
            // 
            this.labelVideoLength.AutoSize = true;
            this.labelVideoLength.Location = new System.Drawing.Point(12, 82);
            this.labelVideoLength.Name = "labelVideoLength";
            this.labelVideoLength.Size = new System.Drawing.Size(73, 13);
            this.labelVideoLength.TabIndex = 7;
            this.labelVideoLength.Text = "Video Length:";
            // 
            // numericStartTimeMinutes
            // 
            this.numericStartTimeMinutes.Location = new System.Drawing.Point(112, 23);
            this.numericStartTimeMinutes.Name = "numericStartTimeMinutes";
            this.numericStartTimeMinutes.Size = new System.Drawing.Size(33, 20);
            this.numericStartTimeMinutes.TabIndex = 8;
            this.numericStartTimeMinutes.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelSeconds
            // 
            this.labelSeconds.AutoSize = true;
            this.labelSeconds.Location = new System.Drawing.Point(151, 25);
            this.labelSeconds.Name = "labelSeconds";
            this.labelSeconds.Size = new System.Drawing.Size(52, 13);
            this.labelSeconds.TabIndex = 9;
            this.labelSeconds.Text = "Seconds:";
            // 
            // numericStartTimeSeconds
            // 
            this.numericStartTimeSeconds.Location = new System.Drawing.Point(209, 23);
            this.numericStartTimeSeconds.Name = "numericStartTimeSeconds";
            this.numericStartTimeSeconds.Size = new System.Drawing.Size(33, 20);
            this.numericStartTimeSeconds.TabIndex = 10;
            // 
            // numericDuration
            // 
            this.numericDuration.Location = new System.Drawing.Point(112, 49);
            this.numericDuration.Name = "numericDuration";
            this.numericDuration.Size = new System.Drawing.Size(33, 20);
            this.numericDuration.TabIndex = 11;
            this.numericDuration.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // buttonMakeTestRun
            // 
            this.buttonMakeTestRun.Location = new System.Drawing.Point(9, 75);
            this.buttonMakeTestRun.Name = "buttonMakeTestRun";
            this.buttonMakeTestRun.Size = new System.Drawing.Size(109, 23);
            this.buttonMakeTestRun.TabIndex = 12;
            this.buttonMakeTestRun.Text = "Make test run video";
            this.buttonMakeTestRun.UseVisualStyleBackColor = true;
            this.buttonMakeTestRun.Click += new System.EventHandler(this.buttonMakeTestRun_Click);
            // 
            // buttonViewTestRunVideo
            // 
            this.buttonViewTestRunVideo.Location = new System.Drawing.Point(9, 104);
            this.buttonViewTestRunVideo.Name = "buttonViewTestRunVideo";
            this.buttonViewTestRunVideo.Size = new System.Drawing.Size(108, 23);
            this.buttonViewTestRunVideo.TabIndex = 13;
            this.buttonViewTestRunVideo.Text = "View test run video";
            this.buttonViewTestRunVideo.UseVisualStyleBackColor = true;
            // 
            // buttonMakeFinalSyncedVideo
            // 
            this.buttonMakeFinalSyncedVideo.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonMakeFinalSyncedVideo.Location = new System.Drawing.Point(12, 353);
            this.buttonMakeFinalSyncedVideo.Name = "buttonMakeFinalSyncedVideo";
            this.buttonMakeFinalSyncedVideo.Size = new System.Drawing.Size(130, 23);
            this.buttonMakeFinalSyncedVideo.TabIndex = 14;
            this.buttonMakeFinalSyncedVideo.Text = "Make final synced video";
            this.buttonMakeFinalSyncedVideo.UseVisualStyleBackColor = true;
            this.buttonMakeFinalSyncedVideo.Click += new System.EventHandler(this.buttonMakeFinalVideo_Click);
            // 
            // labelVideoLengthValue
            // 
            this.labelVideoLengthValue.AutoSize = true;
            this.labelVideoLengthValue.Location = new System.Drawing.Point(86, 82);
            this.labelVideoLengthValue.Name = "labelVideoLengthValue";
            this.labelVideoLengthValue.Size = new System.Drawing.Size(116, 13);
            this.labelVideoLengthValue.TabIndex = 15;
            this.labelVideoLengthValue.Text = "labelVideoLengthValue";
            // 
            // labelVideoNameValue
            // 
            this.labelVideoNameValue.AutoSize = true;
            this.labelVideoNameValue.Location = new System.Drawing.Point(86, 69);
            this.labelVideoNameValue.Name = "labelVideoNameValue";
            this.labelVideoNameValue.Size = new System.Drawing.Size(111, 13);
            this.labelVideoNameValue.TabIndex = 16;
            this.labelVideoNameValue.Text = "labelVideoNameValue";
            // 
            // groupBoxTestRunVideo
            // 
            this.groupBoxTestRunVideo.Controls.Add(this.labelStartTime);
            this.groupBoxTestRunVideo.Controls.Add(this.labelDuration);
            this.groupBoxTestRunVideo.Controls.Add(this.numericStartTimeMinutes);
            this.groupBoxTestRunVideo.Controls.Add(this.labelSeconds);
            this.groupBoxTestRunVideo.Controls.Add(this.numericStartTimeSeconds);
            this.groupBoxTestRunVideo.Controls.Add(this.numericDuration);
            this.groupBoxTestRunVideo.Controls.Add(this.buttonViewTestRunVideo);
            this.groupBoxTestRunVideo.Controls.Add(this.buttonMakeTestRun);
            this.groupBoxTestRunVideo.Location = new System.Drawing.Point(12, 174);
            this.groupBoxTestRunVideo.Name = "groupBoxTestRunVideo";
            this.groupBoxTestRunVideo.Size = new System.Drawing.Size(391, 136);
            this.groupBoxTestRunVideo.TabIndex = 17;
            this.groupBoxTestRunVideo.TabStop = false;
            this.groupBoxTestRunVideo.Text = "Test run video settings";
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(148, 353);
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
            this.labelStep1a.Location = new System.Drawing.Point(12, 18);
            this.labelStep1a.Name = "labelStep1a";
            this.labelStep1a.Size = new System.Drawing.Size(391, 13);
            this.labelStep1a.TabIndex = 19;
            this.labelStep1a.Text = "1. To synchronize the video with the audio, set the number of video";
            // 
            // labelStep1b
            // 
            this.labelStep1b.AutoSize = true;
            this.labelStep1b.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep1b.Location = new System.Drawing.Point(13, 31);
            this.labelStep1b.Name = "labelStep1b";
            this.labelStep1b.Size = new System.Drawing.Size(372, 13);
            this.labelStep1b.TabIndex = 20;
            this.labelStep1b.Text = "    frames to move the video forward or backward, using positive";
            // 
            // labelStep2a
            // 
            this.labelStep2a.AutoSize = true;
            this.labelStep2a.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep2a.Location = new System.Drawing.Point(12, 136);
            this.labelStep2a.Name = "labelStep2a";
            this.labelStep2a.Size = new System.Drawing.Size(388, 13);
            this.labelStep2a.TabIndex = 21;
            this.labelStep2a.Text = "2. Optionally create a short test video to verify the audio and video";
            // 
            // labelStep2b
            // 
            this.labelStep2b.AutoSize = true;
            this.labelStep2b.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep2b.Location = new System.Drawing.Point(12, 149);
            this.labelStep2b.Name = "labelStep2b";
            this.labelStep2b.Size = new System.Drawing.Size(258, 13);
            this.labelStep2b.TabIndex = 22;
            this.labelStep2b.Text = "    sync when played forward and backward.";
            // 
            // labelStep3
            // 
            this.labelStep3.AutoSize = true;
            this.labelStep3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep3.Location = new System.Drawing.Point(12, 326);
            this.labelStep3.Name = "labelStep3";
            this.labelStep3.Size = new System.Drawing.Size(191, 13);
            this.labelStep3.TabIndex = 23;
            this.labelStep3.Text = "3. Create the fully synced video.";
            // 
            // labelStep1c
            // 
            this.labelStep1c.AutoSize = true;
            this.labelStep1c.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStep1c.Location = new System.Drawing.Point(12, 44);
            this.labelStep1c.Name = "labelStep1c";
            this.labelStep1c.Size = new System.Drawing.Size(312, 13);
            this.labelStep1c.TabIndex = 24;
            this.labelStep1c.Text = "    values or negative values respectively (eg 2, -1.5).";
            // 
            // VideoSyncForm
            // 
            this.AcceptButton = this.buttonMakeTestRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 387);
            this.Controls.Add(this.labelStep1c);
            this.Controls.Add(this.labelStep3);
            this.Controls.Add(this.labelStep2b);
            this.Controls.Add(this.labelStep2a);
            this.Controls.Add(this.labelStep1b);
            this.Controls.Add(this.labelStep1a);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelVideoOffset);
            this.Controls.Add(this.numericOffset);
            this.Controls.Add(this.groupBoxTestRunVideo);
            this.Controls.Add(this.labelVideoNameValue);
            this.Controls.Add(this.labelVideoLengthValue);
            this.Controls.Add(this.buttonMakeFinalSyncedVideo);
            this.Controls.Add(this.labelVideoLength);
            this.Controls.Add(this.labelVideoName);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VideoSyncForm";
            this.Text = "VideoSyncForm";
            ((System.ComponentModel.ISupportInitialize)(this.numericOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStartTimeMinutes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStartTimeSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericDuration)).EndInit();
            this.groupBoxTestRunVideo.ResumeLayout(false);
            this.groupBoxTestRunVideo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelVideoOffset;
        private System.Windows.Forms.NumericUpDown numericOffset;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Label labelVideoName;
        private System.Windows.Forms.Label labelVideoLength;
        private System.Windows.Forms.NumericUpDown numericStartTimeMinutes;
        private System.Windows.Forms.Label labelSeconds;
        private System.Windows.Forms.NumericUpDown numericStartTimeSeconds;
        private System.Windows.Forms.NumericUpDown numericDuration;
        private System.Windows.Forms.Button buttonMakeTestRun;
        private System.Windows.Forms.Button buttonViewTestRunVideo;
        private System.Windows.Forms.Button buttonMakeFinalSyncedVideo;
        private System.Windows.Forms.Label labelVideoLengthValue;
        private System.Windows.Forms.Label labelVideoNameValue;
        private System.Windows.Forms.GroupBox groupBoxTestRunVideo;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelStep1a;
        private System.Windows.Forms.Label labelStep1b;
        private System.Windows.Forms.Label labelStep2a;
        private System.Windows.Forms.Label labelStep2b;
        private System.Windows.Forms.Label labelStep3;
        private System.Windows.Forms.Label labelStep1c;
    }
}