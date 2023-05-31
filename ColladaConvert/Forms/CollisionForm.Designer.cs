namespace ColladaConvert.Forms
{
	partial class CollisionForm
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
			this.CollisionTreeView = new System.Windows.Forms.TreeView();
			this.AddChild = new System.Windows.Forms.Button();
			this.RadioBox = new System.Windows.Forms.RadioButton();
			this.ShapeGroup = new System.Windows.Forms.GroupBox();
			this.RadioCapsule = new System.Windows.Forms.RadioButton();
			this.RadioSphere = new System.Windows.Forms.RadioButton();
			this.EditNode = new System.Windows.Forms.Button();
			this.DrawAll = new System.Windows.Forms.CheckBox();
			this.ShapeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// CollisionTreeView
			// 
			this.CollisionTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CollisionTreeView.HideSelection = false;
			this.CollisionTreeView.Location = new System.Drawing.Point(12, 12);
			this.CollisionTreeView.Name = "CollisionTreeView";
			this.CollisionTreeView.Size = new System.Drawing.Size(388, 321);
			this.CollisionTreeView.TabIndex = 0;
			this.CollisionTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnAfterSelect);
			this.CollisionTreeView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyUp);
			// 
			// AddChild
			// 
			this.AddChild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AddChild.Location = new System.Drawing.Point(104, 339);
			this.AddChild.Name = "AddChild";
			this.AddChild.Size = new System.Drawing.Size(75, 23);
			this.AddChild.TabIndex = 1;
			this.AddChild.Text = "Add Child";
			this.AddChild.UseVisualStyleBackColor = true;
			this.AddChild.Click += new System.EventHandler(this.OnAddChild);
			// 
			// RadioBox
			// 
			this.RadioBox.AutoSize = true;
			this.RadioBox.Location = new System.Drawing.Point(6, 22);
			this.RadioBox.Name = "RadioBox";
			this.RadioBox.Size = new System.Drawing.Size(45, 19);
			this.RadioBox.TabIndex = 2;
			this.RadioBox.TabStop = true;
			this.RadioBox.Text = "Box";
			this.RadioBox.UseVisualStyleBackColor = true;
			this.RadioBox.CheckedChanged += new System.EventHandler(this.OnNodeShapeChanged);
			// 
			// ShapeGroup
			// 
			this.ShapeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShapeGroup.Controls.Add(this.RadioCapsule);
			this.ShapeGroup.Controls.Add(this.RadioSphere);
			this.ShapeGroup.Controls.Add(this.RadioBox);
			this.ShapeGroup.Location = new System.Drawing.Point(12, 339);
			this.ShapeGroup.Name = "ShapeGroup";
			this.ShapeGroup.Size = new System.Drawing.Size(86, 99);
			this.ShapeGroup.TabIndex = 3;
			this.ShapeGroup.TabStop = false;
			this.ShapeGroup.Text = "Node Shape";
			// 
			// RadioCapsule
			// 
			this.RadioCapsule.AutoSize = true;
			this.RadioCapsule.Location = new System.Drawing.Point(6, 72);
			this.RadioCapsule.Name = "RadioCapsule";
			this.RadioCapsule.Size = new System.Drawing.Size(67, 19);
			this.RadioCapsule.TabIndex = 4;
			this.RadioCapsule.TabStop = true;
			this.RadioCapsule.Text = "Capsule";
			this.RadioCapsule.UseVisualStyleBackColor = true;
			this.RadioCapsule.CheckedChanged += new System.EventHandler(this.OnNodeShapeChanged);
			// 
			// RadioSphere
			// 
			this.RadioSphere.AutoSize = true;
			this.RadioSphere.Location = new System.Drawing.Point(6, 47);
			this.RadioSphere.Name = "RadioSphere";
			this.RadioSphere.Size = new System.Drawing.Size(61, 19);
			this.RadioSphere.TabIndex = 3;
			this.RadioSphere.TabStop = true;
			this.RadioSphere.Text = "Sphere";
			this.RadioSphere.UseVisualStyleBackColor = true;
			this.RadioSphere.CheckedChanged += new System.EventHandler(this.OnNodeShapeChanged);
			// 
			// EditNode
			// 
			this.EditNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditNode.Location = new System.Drawing.Point(104, 368);
			this.EditNode.Name = "EditNode";
			this.EditNode.Size = new System.Drawing.Size(75, 23);
			this.EditNode.TabIndex = 4;
			this.EditNode.Text = "Edit Node";
			this.EditNode.UseVisualStyleBackColor = true;
			// 
			// DrawAll
			// 
			this.DrawAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DrawAll.AutoSize = true;
			this.DrawAll.Location = new System.Drawing.Point(104, 397);
			this.DrawAll.Name = "DrawAll";
			this.DrawAll.Size = new System.Drawing.Size(70, 19);
			this.DrawAll.TabIndex = 5;
			this.DrawAll.Text = "Draw All";
			this.DrawAll.UseVisualStyleBackColor = true;
			// 
			// CollisionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(412, 450);
			this.Controls.Add(this.DrawAll);
			this.Controls.Add(this.EditNode);
			this.Controls.Add(this.ShapeGroup);
			this.Controls.Add(this.AddChild);
			this.Controls.Add(this.CollisionTreeView);
			this.Name = "CollisionForm";
			this.Text = "CollisionForm";
			this.ShapeGroup.ResumeLayout(false);
			this.ShapeGroup.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TreeView CollisionTreeView;
		private Button AddChild;
		private RadioButton RadioBox;
		private GroupBox ShapeGroup;
		private RadioButton RadioCapsule;
		private RadioButton RadioSphere;
		private Button EditNode;
		private CheckBox DrawAll;
	}
}