namespace QEntityMaker
{
	partial class QEEdit
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
			this.QuarkEntityFile = new System.Windows.Forms.TextBox();
			this.SetQuArKEntityFile = new System.Windows.Forms.Button();
			this.EntityTree = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// QuarkEntityFile
			// 
			this.QuarkEntityFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QuarkEntityFile.Location = new System.Drawing.Point(159, 14);
			this.QuarkEntityFile.Name = "QuarkEntityFile";
			this.QuarkEntityFile.ReadOnly = true;
			this.QuarkEntityFile.Size = new System.Drawing.Size(427, 20);
			this.QuarkEntityFile.TabIndex = 8;
			// 
			// SetQuArKEntityFile
			// 
			this.SetQuArKEntityFile.Location = new System.Drawing.Point(12, 12);
			this.SetQuArKEntityFile.Name = "SetQuArKEntityFile";
			this.SetQuArKEntityFile.Size = new System.Drawing.Size(141, 23);
			this.SetQuArKEntityFile.TabIndex = 7;
			this.SetQuArKEntityFile.Text = "QuArk Addon Entity File";
			this.SetQuArKEntityFile.UseVisualStyleBackColor = true;
			this.SetQuArKEntityFile.Click += new System.EventHandler(this.OnBrowseForEntityFile);
			// 
			// EntityTree
			// 
			this.EntityTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EntityTree.Location = new System.Drawing.Point(12, 41);
			this.EntityTree.Name = "EntityTree";
			this.EntityTree.Size = new System.Drawing.Size(574, 235);
			this.EntityTree.TabIndex = 9;
			// 
			// QEEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(598, 400);
			this.Controls.Add(this.EntityTree);
			this.Controls.Add(this.QuarkEntityFile);
			this.Controls.Add(this.SetQuArKEntityFile);
			this.Name = "QEEdit";
			this.Text = "Entity Editor for QuArK";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox QuarkEntityFile;
		private System.Windows.Forms.Button SetQuArKEntityFile;
		private System.Windows.Forms.TreeView EntityTree;
	}
}

