namespace ColladaStartSmall
{
	partial class MaterialForm
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
			this.MaterialList = new System.Windows.Forms.ListView();
			this.MaterialProperties = new ColladaStartSmall.TabbyPropertyGrid();
			this.SuspendLayout();
			// 
			// MaterialList
			// 
			this.MaterialList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MaterialList.FullRowSelect = true;
			this.MaterialList.GridLines = true;
			this.MaterialList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.MaterialList.HideSelection = false;
			this.MaterialList.LabelEdit = true;
			this.MaterialList.Location = new System.Drawing.Point(12, 264);
			this.MaterialList.MultiSelect = false;
			this.MaterialList.Name = "MaterialList";
			this.MaterialList.Size = new System.Drawing.Size(656, 168);
			this.MaterialList.TabIndex = 0;
			this.MaterialList.UseCompatibleStateImageBehavior = false;
			this.MaterialList.View = System.Windows.Forms.View.Details;
			this.MaterialList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnMaterialRename);
			this.MaterialList.SelectedIndexChanged += new System.EventHandler(this.OnMaterialSelectionChanged);
			this.MaterialList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMatListClick);
			// 
			// MaterialProperties
			// 
			this.MaterialProperties.Location = new System.Drawing.Point(12, 12);
			this.MaterialProperties.Name = "MaterialProperties";
			this.MaterialProperties.Size = new System.Drawing.Size(656, 246);
			this.MaterialProperties.TabIndex = 1;
			// 
			// MaterialForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(680, 444);
			this.Controls.Add(this.MaterialProperties);
			this.Controls.Add(this.MaterialList);
			this.Name = "MaterialForm";
			this.Text = "MaterialForm";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView MaterialList;
		private TabbyPropertyGrid MaterialProperties;
	}
}