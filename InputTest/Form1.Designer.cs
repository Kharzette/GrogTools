namespace InputTest
{
	partial class Form1
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
			this.Update = new System.Windows.Forms.Button();
			this.InfoConsole = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// Update
			// 
			this.Update.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.Update.Location = new System.Drawing.Point(12, 226);
			this.Update.Name = "Update";
			this.Update.Size = new System.Drawing.Size(75, 23);
			this.Update.TabIndex = 0;
			this.Update.Text = "Update";
			this.Update.UseVisualStyleBackColor = true;
			this.Update.Click += new System.EventHandler(this.OnUpdate);
			// 
			// InfoConsole
			// 
			this.InfoConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InfoConsole.Location = new System.Drawing.Point(12, 12);
			this.InfoConsole.Multiline = true;
			this.InfoConsole.Name = "InfoConsole";
			this.InfoConsole.ReadOnly = true;
			this.InfoConsole.Size = new System.Drawing.Size(260, 208);
			this.InfoConsole.TabIndex = 1;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.InfoConsole);
			this.Controls.Add(this.Update);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button Update;
		private System.Windows.Forms.TextBox InfoConsole;
	}
}

