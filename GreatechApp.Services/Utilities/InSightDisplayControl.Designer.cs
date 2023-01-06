
namespace GreatechApp.Services.Utilities
{
    partial class InSightDisplayControl
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
            this.components = new System.ComponentModel.Container();
            this.grpImageZoom = new System.Windows.Forms.GroupBox();
            this.optFill = new System.Windows.Forms.RadioButton();
            this.optFit = new System.Windows.Forms.RadioButton();
            this.optNone = new System.Windows.Forms.RadioButton();
            this.cvsInSightDisplay1 = new Cognex.InSight.Controls.Display.CvsInSightDisplay();
            this.btnSave = new System.Windows.Forms.Button();
            this.SaveLiveVideo = new System.Windows.Forms.CheckBox();
            this.SaveLiveImage = new System.Windows.Forms.CheckBox();
            this.chkOnline = new System.Windows.Forms.CheckBox();
            this.chkImage = new System.Windows.Forms.CheckBox();
            this.chkGrid = new System.Windows.Forms.CheckBox();
            this.chkLive = new System.Windows.Forms.CheckBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.grpImageZoom.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpImageZoom
            // 
            this.grpImageZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpImageZoom.Controls.Add(this.optFill);
            this.grpImageZoom.Controls.Add(this.optFit);
            this.grpImageZoom.Controls.Add(this.optNone);
            this.grpImageZoom.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpImageZoom.Location = new System.Drawing.Point(608, 22);
            this.grpImageZoom.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpImageZoom.Name = "grpImageZoom";
            this.grpImageZoom.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.grpImageZoom.Size = new System.Drawing.Size(262, 83);
            this.grpImageZoom.TabIndex = 38;
            this.grpImageZoom.TabStop = false;
            this.grpImageZoom.Text = "Image Zoom";
            this.grpImageZoom.Visible = false;
            // 
            // optFill
            // 
            this.optFill.Location = new System.Drawing.Point(190, 37);
            this.optFill.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.optFill.Name = "optFill";
            this.optFill.Size = new System.Drawing.Size(60, 25);
            this.optFill.TabIndex = 2;
            this.optFill.Text = "Fill";
            // 
            // optFit
            // 
            this.optFit.Location = new System.Drawing.Point(105, 37);
            this.optFit.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.optFit.Name = "optFit";
            this.optFit.Size = new System.Drawing.Size(60, 25);
            this.optFit.TabIndex = 1;
            this.optFit.Text = "Fit";
            // 
            // optNone
            // 
            this.optNone.Location = new System.Drawing.Point(24, 37);
            this.optNone.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.optNone.Name = "optNone";
            this.optNone.Size = new System.Drawing.Size(72, 25);
            this.optNone.TabIndex = 0;
            this.optNone.Text = "None";
            // 
            // cvsInSightDisplay1
            // 
            this.cvsInSightDisplay1.DefaultTextScaleMode = Cognex.InSight.Controls.Display.CvsInSightDisplay.TextScaleModeType.Proportional;
            this.cvsInSightDisplay1.DialogIcon = null;
            this.cvsInSightDisplay1.Location = new System.Drawing.Point(82, 116);
            this.cvsInSightDisplay1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cvsInSightDisplay1.Name = "cvsInSightDisplay1";
            this.cvsInSightDisplay1.PreferredCropScaleMode = Cognex.InSight.Controls.Display.CvsInSightDisplayCropScaleMode.Default;
            this.cvsInSightDisplay1.Size = new System.Drawing.Size(760, 588);
            this.cvsInSightDisplay1.TabIndex = 37;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(712, 762);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(93, 37);
            this.btnSave.TabIndex = 45;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // SaveLiveVideo
            // 
            this.SaveLiveVideo.AutoSize = true;
            this.SaveLiveVideo.Location = new System.Drawing.Point(709, 712);
            this.SaveLiveVideo.Name = "SaveLiveVideo";
            this.SaveLiveVideo.Size = new System.Drawing.Size(116, 24);
            this.SaveLiveVideo.TabIndex = 44;
            this.SaveLiveVideo.Text = "Save Video";
            this.SaveLiveVideo.UseVisualStyleBackColor = true;
            // 
            // SaveLiveImage
            // 
            this.SaveLiveImage.AutoSize = true;
            this.SaveLiveImage.Location = new System.Drawing.Point(560, 713);
            this.SaveLiveImage.Name = "SaveLiveImage";
            this.SaveLiveImage.Size = new System.Drawing.Size(120, 24);
            this.SaveLiveImage.TabIndex = 43;
            this.SaveLiveImage.Text = "Save Image";
            this.SaveLiveImage.UseVisualStyleBackColor = true;
            // 
            // chkOnline
            // 
            this.chkOnline.Location = new System.Drawing.Point(373, 39);
            this.chkOnline.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkOnline.Name = "chkOnline";
            this.chkOnline.Size = new System.Drawing.Size(124, 25);
            this.chkOnline.TabIndex = 42;
            this.chkOnline.Text = "Online";
            // 
            // chkImage
            // 
            this.chkImage.Location = new System.Drawing.Point(196, 39);
            this.chkImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkImage.Name = "chkImage";
            this.chkImage.Size = new System.Drawing.Size(168, 25);
            this.chkImage.TabIndex = 41;
            this.chkImage.Text = "Show Image";
            // 
            // chkGrid
            // 
            this.chkGrid.Location = new System.Drawing.Point(42, 39);
            this.chkGrid.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkGrid.Name = "chkGrid";
            this.chkGrid.Size = new System.Drawing.Size(168, 25);
            this.chkGrid.TabIndex = 40;
            this.chkGrid.Text = "Show Grid";
            // 
            // chkLive
            // 
            this.chkLive.Location = new System.Drawing.Point(506, 39);
            this.chkLive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkLive.Name = "chkLive";
            this.chkLive.Size = new System.Drawing.Size(120, 25);
            this.chkLive.TabIndex = 39;
            this.chkLive.Text = "Live";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // InSightDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 820);
            this.Controls.Add(this.grpImageZoom);
            this.Controls.Add(this.cvsInSightDisplay1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.SaveLiveVideo);
            this.Controls.Add(this.SaveLiveImage);
            this.Controls.Add(this.chkOnline);
            this.Controls.Add(this.chkImage);
            this.Controls.Add(this.chkGrid);
            this.Controls.Add(this.chkLive);
            this.Name = "InSightDisplayControl";
            this.Text = "InSightDisplayControl";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InSightDisplayControl_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InSightDisplayControl_FormClosed);
            this.grpImageZoom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.GroupBox grpImageZoom;
        internal System.Windows.Forms.RadioButton optFill;
        internal System.Windows.Forms.RadioButton optFit;
        internal System.Windows.Forms.RadioButton optNone;
        private Cognex.InSight.Controls.Display.CvsInSightDisplay cvsInSightDisplay1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox SaveLiveVideo;
        private System.Windows.Forms.CheckBox SaveLiveImage;
        internal System.Windows.Forms.CheckBox chkOnline;
        internal System.Windows.Forms.CheckBox chkImage;
        internal System.Windows.Forms.CheckBox chkGrid;
        internal System.Windows.Forms.CheckBox chkLive;
        private System.Windows.Forms.Timer timer1;
    }
}