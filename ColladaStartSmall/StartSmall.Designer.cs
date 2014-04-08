namespace ColladaStartSmall
{
	partial class StartSmall
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
			this.OpenStaticDAE = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// OpenStaticDAE
			// 
			this.OpenStaticDAE.Location = new System.Drawing.Point(84, 95);
			this.OpenStaticDAE.Name = "OpenStaticDAE";
			this.OpenStaticDAE.Size = new System.Drawing.Size(108, 23);
			this.OpenStaticDAE.TabIndex = 0;
			this.OpenStaticDAE.Text = "Open Static DAE";
			this.OpenStaticDAE.UseVisualStyleBackColor = true;
			this.OpenStaticDAE.Click += new System.EventHandler(this.OnOpenStaticDAE);
			// 
			// StartSmall
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.OpenStaticDAE);
			this.Name = "StartSmall";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button OpenStaticDAE;
	}
}

