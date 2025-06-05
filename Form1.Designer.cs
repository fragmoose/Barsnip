namespace Barsnip
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            snipReadCopy = new Button();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            statusStrip1 = new StatusStrip();
            verLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // snipReadCopy
            // 
            snipReadCopy.Location = new Point(114, 12);
            snipReadCopy.Name = "snipReadCopy";
            snipReadCopy.Size = new Size(133, 23);
            snipReadCopy.TabIndex = 1;
            snipReadCopy.Text = "Snip, Read and Copy";
            snipReadCopy.UseVisualStyleBackColor = true;
            snipReadCopy.Click += snipReadCopy_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(94, 86);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            label1.AutoSize = true;
            label1.Location = new Point(114, 38);
            label1.MinimumSize = new Size(130, 0);
            label1.Name = "label1";
            label1.Size = new Size(130, 15);
            label1.TabIndex = 4;
            label1.Text = "Awaiting snip...";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { verLabel });
            statusStrip1.Location = new Point(0, 128);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(255, 22);
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // verLabel
            // 
            verLabel.Name = "verLabel";
            verLabel.Size = new Size(61, 17);
            verLabel.Text = "Barsnip v. ";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(255, 150);
            Controls.Add(statusStrip1);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Controls.Add(snipReadCopy);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            Text = "Barsnip";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button snipReadCopy;
        private PictureBox pictureBox1;
        private Label label1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel verLabel;
    }
}
