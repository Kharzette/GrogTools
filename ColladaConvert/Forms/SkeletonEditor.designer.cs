namespace ColladaConvert
{
	partial class SkeletonEditor
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
			this.SkeletonTree = new System.Windows.Forms.TreeView();
			this.SelectUnUsedBones = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// SkeletonTree
			// 
			this.SkeletonTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SkeletonTree.Location = new System.Drawing.Point(12, 12);
			this.SkeletonTree.Name = "SkeletonTree";
			this.SkeletonTree.Size = new System.Drawing.Size(260, 210);
			this.SkeletonTree.TabIndex = 0;
			this.SkeletonTree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyUp);
			// 
			// SelectUnUsedBones
			// 
			this.SelectUnUsedBones.Location = new System.Drawing.Point(12, 225);
			this.SelectUnUsedBones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectUnUsedBones.Name = "SelectUnUsedBones";
			this.SelectUnUsedBones.Size = new System.Drawing.Size(140, 25);
			this.SelectUnUsedBones.TabIndex = 1;
			this.SelectUnUsedBones.Text = "Mark Unused Bones";
			this.SelectUnUsedBones.UseVisualStyleBackColor = true;
			this.SelectUnUsedBones.Click += new System.EventHandler(this.OnSelectUnUsedBones);
			// 
			// SkeletonEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.ControlBox = false;
			this.Controls.Add(this.SkeletonTree);
			this.Controls.Add(this.SelectUnUsedBones);
			this.Name = "SkeletonEditor";
			this.Text = "SkeletonEditor";
			this.ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.TreeView SkeletonTree;
		private System.Windows.Forms.Button SelectUnUsedBones;
	}
}