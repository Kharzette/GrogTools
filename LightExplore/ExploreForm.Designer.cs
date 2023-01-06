namespace LightExplore
{
    partial class ExploreForm
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
            this.OpenGBSP = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FaceNumber = new System.Windows.Forms.TextBox();
            this.GBSPFileName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OpenGBSP
            // 
            this.OpenGBSP.Location = new System.Drawing.Point(12, 12);
            this.OpenGBSP.Name = "OpenGBSP";
            this.OpenGBSP.Size = new System.Drawing.Size(102, 23);
            this.OpenGBSP.TabIndex = 0;
            this.OpenGBSP.Text = "Load Lit GBSP";
            this.OpenGBSP.UseVisualStyleBackColor = true;
            this.OpenGBSP.Click += new System.EventHandler(this.OnOpenGBSP);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Current Face:";
            // 
            // FaceNumber
            // 
            this.FaceNumber.Location = new System.Drawing.Point(95, 46);
            this.FaceNumber.Name = "FaceNumber";
            this.FaceNumber.ReadOnly = true;
            this.FaceNumber.Size = new System.Drawing.Size(100, 23);
            this.FaceNumber.TabIndex = 2;
            // 
            // GBSPFileName
            // 
            this.GBSPFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GBSPFileName.Location = new System.Drawing.Point(120, 12);
            this.GBSPFileName.Name = "GBSPFileName";
            this.GBSPFileName.ReadOnly = true;
            this.GBSPFileName.Size = new System.Drawing.Size(225, 23);
            this.GBSPFileName.TabIndex = 3;
            // 
            // ExploreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 110);
            this.Controls.Add(this.GBSPFileName);
            this.Controls.Add(this.FaceNumber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OpenGBSP);
            this.Name = "ExploreForm";
            this.Text = "Light Explorer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenGBSP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FaceNumber;
        private System.Windows.Forms.TextBox GBSPFileName;
    }
}

