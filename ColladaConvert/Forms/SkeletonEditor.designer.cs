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
			this.RadioBox = new System.Windows.Forms.RadioButton();
			this.RadioSphere = new System.Windows.Forms.RadioButton();
			this.RadioCapsule = new System.Windows.Forms.RadioButton();
			this.AdjustBoneBound = new System.Windows.Forms.Button();
			this.DrawBounds = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// SkeletonTree
			// 
			this.SkeletonTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SkeletonTree.Location = new System.Drawing.Point(14, 14);
			this.SkeletonTree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SkeletonTree.Name = "SkeletonTree";
			this.SkeletonTree.Size = new System.Drawing.Size(303, 360);
			this.SkeletonTree.TabIndex = 0;
			this.SkeletonTree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyUp);
			// 
			// SelectUnUsedBones
			// 
			this.SelectUnUsedBones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SelectUnUsedBones.Location = new System.Drawing.Point(13, 380);
			this.SelectUnUsedBones.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SelectUnUsedBones.Name = "SelectUnUsedBones";
			this.SelectUnUsedBones.Size = new System.Drawing.Size(125, 29);
			this.SelectUnUsedBones.TabIndex = 1;
			this.SelectUnUsedBones.Text = "Mark Unused Bones";
			this.SelectUnUsedBones.UseVisualStyleBackColor = true;
			this.SelectUnUsedBones.Click += new System.EventHandler(this.OnSelectUnUsedBones);
			// 
			// RadioBox
			// 
			this.RadioBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RadioBox.AutoSize = true;
			this.RadioBox.Checked = true;
			this.RadioBox.Location = new System.Drawing.Point(251, 380);
			this.RadioBox.Name = "RadioBox";
			this.RadioBox.Size = new System.Drawing.Size(45, 19);
			this.RadioBox.TabIndex = 2;
			this.RadioBox.TabStop = true;
			this.RadioBox.Text = "Box";
			this.RadioBox.UseVisualStyleBackColor = true;
			this.RadioBox.CheckedChanged += new System.EventHandler(this.BoundShapeChanged);
			// 
			// RadioSphere
			// 
			this.RadioSphere.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RadioSphere.AutoSize = true;
			this.RadioSphere.Location = new System.Drawing.Point(251, 405);
			this.RadioSphere.Name = "RadioSphere";
			this.RadioSphere.Size = new System.Drawing.Size(61, 19);
			this.RadioSphere.TabIndex = 3;
			this.RadioSphere.Text = "Sphere";
			this.RadioSphere.UseVisualStyleBackColor = true;
			this.RadioSphere.CheckedChanged += new System.EventHandler(this.BoundShapeChanged);
			// 
			// RadioCapsule
			// 
			this.RadioCapsule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RadioCapsule.AutoSize = true;
			this.RadioCapsule.Location = new System.Drawing.Point(251, 430);
			this.RadioCapsule.Name = "RadioCapsule";
			this.RadioCapsule.Size = new System.Drawing.Size(67, 19);
			this.RadioCapsule.TabIndex = 4;
			this.RadioCapsule.Text = "Capsule";
			this.RadioCapsule.UseVisualStyleBackColor = true;
			this.RadioCapsule.CheckedChanged += new System.EventHandler(this.BoundShapeChanged);
			// 
			// AdjustBoneBound
			// 
			this.AdjustBoneBound.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AdjustBoneBound.Location = new System.Drawing.Point(170, 380);
			this.AdjustBoneBound.Name = "AdjustBoneBound";
			this.AdjustBoneBound.Size = new System.Drawing.Size(75, 63);
			this.AdjustBoneBound.TabIndex = 5;
			this.AdjustBoneBound.Text = "Adjust Bone Bound";
			this.AdjustBoneBound.UseVisualStyleBackColor = true;
			this.AdjustBoneBound.Click += new System.EventHandler(this.OnAdjustBone);
			// 
			// DrawBounds
			// 
			this.DrawBounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DrawBounds.AutoSize = true;
			this.DrawBounds.Location = new System.Drawing.Point(14, 415);
			this.DrawBounds.Name = "DrawBounds";
			this.DrawBounds.Size = new System.Drawing.Size(96, 19);
			this.DrawBounds.TabIndex = 6;
			this.DrawBounds.Text = "Draw Bounds";
			this.DrawBounds.UseVisualStyleBackColor = true;
			// 
			// SkeletonEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(331, 455);
			this.ControlBox = false;
			this.Controls.Add(this.DrawBounds);
			this.Controls.Add(this.AdjustBoneBound);
			this.Controls.Add(this.RadioCapsule);
			this.Controls.Add(this.RadioSphere);
			this.Controls.Add(this.RadioBox);
			this.Controls.Add(this.SkeletonTree);
			this.Controls.Add(this.SelectUnUsedBones);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "SkeletonEditor";
			this.Text = "SkeletonEditor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TreeView SkeletonTree;
		private System.Windows.Forms.Button SelectUnUsedBones;
		private RadioButton RadioBox;
		private RadioButton RadioSphere;
		private RadioButton RadioCapsule;
		private Button AdjustBoneBound;
		private CheckBox DrawBounds;
	}
}