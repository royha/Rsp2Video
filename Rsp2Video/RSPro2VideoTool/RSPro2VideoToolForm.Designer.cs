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
            this.buttonExtractAudio = new System.Windows.Forms.Button();
            this.buttonSaveVideo240p = new System.Windows.Forms.Button();
            this.buttonSaveVideo360p = new System.Windows.Forms.Button();
            this.buttonSaveVideo480p = new System.Windows.Forms.Button();
            this.buttonSaveVideo720p = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxSourceVideoFile = new System.Windows.Forms.TextBox();
            this.labelVideo = new System.Windows.Forms.Label();
            this.labelVideoDescription = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
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
            // buttonExtractAudio
            // 
            this.buttonExtractAudio.Enabled = false;
            this.buttonExtractAudio.Location = new System.Drawing.Point(15, 29);
            this.buttonExtractAudio.Name = "buttonExtractAudio";
            this.buttonExtractAudio.Size = new System.Drawing.Size(380, 32);
            this.buttonExtractAudio.TabIndex = 0;
            this.buttonExtractAudio.Text = "Extract sound file from video";
            this.buttonExtractAudio.UseVisualStyleBackColor = true;
            this.buttonExtractAudio.Click += new System.EventHandler(this.buttonExtractAudio_Click);
            // 
            // buttonSaveVideo240p
            // 
            this.buttonSaveVideo240p.Enabled = false;
            this.buttonSaveVideo240p.Location = new System.Drawing.Point(15, 67);
            this.buttonSaveVideo240p.Name = "buttonSaveVideo240p";
            this.buttonSaveVideo240p.Size = new System.Drawing.Size(380, 32);
            this.buttonSaveVideo240p.TabIndex = 1;
            this.buttonSaveVideo240p.Text = "Save video as 240p";
            this.buttonSaveVideo240p.UseVisualStyleBackColor = true;
            this.buttonSaveVideo240p.Click += new System.EventHandler(this.buttonSaveVideo240p_Click);
            // 
            // buttonSaveVideo360p
            // 
            this.buttonSaveVideo360p.Enabled = false;
            this.buttonSaveVideo360p.Location = new System.Drawing.Point(15, 105);
            this.buttonSaveVideo360p.Name = "buttonSaveVideo360p";
            this.buttonSaveVideo360p.Size = new System.Drawing.Size(380, 32);
            this.buttonSaveVideo360p.TabIndex = 2;
            this.buttonSaveVideo360p.Text = "Save video as 360p";
            this.buttonSaveVideo360p.UseVisualStyleBackColor = true;
            this.buttonSaveVideo360p.Click += new System.EventHandler(this.buttonSaveVideo360p_Click);
            // 
            // buttonSaveVideo480p
            // 
            this.buttonSaveVideo480p.Enabled = false;
            this.buttonSaveVideo480p.Location = new System.Drawing.Point(15, 143);
            this.buttonSaveVideo480p.Name = "buttonSaveVideo480p";
            this.buttonSaveVideo480p.Size = new System.Drawing.Size(380, 32);
            this.buttonSaveVideo480p.TabIndex = 3;
            this.buttonSaveVideo480p.Text = "Save video as 480p";
            this.buttonSaveVideo480p.UseVisualStyleBackColor = true;
            this.buttonSaveVideo480p.Click += new System.EventHandler(this.buttonSaveVideo480p_Click);
            // 
            // buttonSaveVideo720p
            // 
            this.buttonSaveVideo720p.Enabled = false;
            this.buttonSaveVideo720p.Location = new System.Drawing.Point(15, 181);
            this.buttonSaveVideo720p.Name = "buttonSaveVideo720p";
            this.buttonSaveVideo720p.Size = new System.Drawing.Size(380, 32);
            this.buttonSaveVideo720p.TabIndex = 4;
            this.buttonSaveVideo720p.Text = "Save video as 720p";
            this.buttonSaveVideo720p.UseVisualStyleBackColor = true;
            this.buttonSaveVideo720p.Click += new System.EventHandler(this.buttonSaveVideo720p_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(366, 60);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(61, 23);
            this.buttonBrowse.TabIndex = 5;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxSourceVideoFile
            // 
            this.textBoxSourceVideoFile.Location = new System.Drawing.Point(57, 62);
            this.textBoxSourceVideoFile.Name = "textBoxSourceVideoFile";
            this.textBoxSourceVideoFile.Size = new System.Drawing.Size(303, 20);
            this.textBoxSourceVideoFile.TabIndex = 6;
            // 
            // labelVideo
            // 
            this.labelVideo.AutoSize = true;
            this.labelVideo.Location = new System.Drawing.Point(14, 65);
            this.labelVideo.Name = "labelVideo";
            this.labelVideo.Size = new System.Drawing.Size(34, 13);
            this.labelVideo.TabIndex = 7;
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
            this.groupBox1.Controls.Add(this.buttonExtractAudio);
            this.groupBox1.Controls.Add(this.buttonSaveVideo240p);
            this.groupBox1.Controls.Add(this.buttonSaveVideo360p);
            this.groupBox1.Controls.Add(this.buttonSaveVideo480p);
            this.groupBox1.Controls.Add(this.buttonSaveVideo720p);
            this.groupBox1.Location = new System.Drawing.Point(17, 146);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 235);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
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
            this.Name = "RSPro2VideoToolForm";
            this.Text = "Reverse Speech to Video Tool";
            this.Load += new System.EventHandler(this.RSPro2VideoToolForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxToolAnimation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonExtractAudio;
        private System.Windows.Forms.Button buttonSaveVideo240p;
        private System.Windows.Forms.Button buttonSaveVideo360p;
        private System.Windows.Forms.Button buttonSaveVideo480p;
        private System.Windows.Forms.Button buttonSaveVideo720p;
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
    }
}

