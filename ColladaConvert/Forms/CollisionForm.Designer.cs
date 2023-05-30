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
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.ShapeGroup = new System.Windows.Forms.GroupBox();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.EditNode = new System.Windows.Forms.Button();
			this.ShapeGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// CollisionTreeView
			// 
			this.CollisionTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CollisionTreeView.Location = new System.Drawing.Point(12, 12);
			this.CollisionTreeView.Name = "CollisionTreeView";
			this.CollisionTreeView.Size = new System.Drawing.Size(388, 321);
			this.CollisionTreeView.TabIndex = 0;
			this.CollisionTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnAfterSelect);
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
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Location = new System.Drawing.Point(6, 22);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(45, 19);
			this.radioButton1.TabIndex = 2;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Box";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// ShapeGroup
			// 
			this.ShapeGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShapeGroup.Controls.Add(this.radioButton3);
			this.ShapeGroup.Controls.Add(this.radioButton2);
			this.ShapeGroup.Controls.Add(this.radioButton1);
			this.ShapeGroup.Location = new System.Drawing.Point(12, 339);
			this.ShapeGroup.Name = "ShapeGroup";
			this.ShapeGroup.Size = new System.Drawing.Size(86, 99);
			this.ShapeGroup.TabIndex = 3;
			this.ShapeGroup.TabStop = false;
			this.ShapeGroup.Text = "Node Shape";
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Location = new System.Drawing.Point(6, 72);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(67, 19);
			this.radioButton3.TabIndex = 4;
			this.radioButton3.TabStop = true;
			this.radioButton3.Text = "Capsule";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Location = new System.Drawing.Point(6, 47);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(61, 19);
			this.radioButton2.TabIndex = 3;
			this.radioButton2.TabStop = true;
			this.radioButton2.Text = "Sphere";
			this.radioButton2.UseVisualStyleBackColor = true;
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
			// CollisionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(412, 450);
			this.Controls.Add(this.EditNode);
			this.Controls.Add(this.ShapeGroup);
			this.Controls.Add(this.AddChild);
			this.Controls.Add(this.CollisionTreeView);
			this.Name = "CollisionForm";
			this.Text = "CollisionForm";
			this.ShapeGroup.ResumeLayout(false);
			this.ShapeGroup.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private TreeView CollisionTreeView;
		private Button AddChild;
		private RadioButton radioButton1;
		private GroupBox ShapeGroup;
		private RadioButton radioButton3;
		private RadioButton radioButton2;
		private Button EditNode;
	}
}