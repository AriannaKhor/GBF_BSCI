
namespace GreatechApp.Services.Utilities
{
    partial class CodeReaderDisplayControl
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
            this.picResultImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picResultImage)).BeginInit();
            this.SuspendLayout();
            // 
            // picResultImage
            // 
            this.picResultImage.Location = new System.Drawing.Point(12, 12);
            this.picResultImage.Name = "picResultImage";
            this.picResultImage.Size = new System.Drawing.Size(889, 796);
            this.picResultImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picResultImage.TabIndex = 2;
            this.picResultImage.TabStop = false;
            // 
            // CodeReaderDisplayControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 820);
            this.Controls.Add(this.picResultImage);
            this.Name = "CodeReaderDisplayControl";
            this.Text = "CodeReaderDisplayControl";
            ((System.ComponentModel.ISupportInitialize)(this.picResultImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picResultImage;
    }
}