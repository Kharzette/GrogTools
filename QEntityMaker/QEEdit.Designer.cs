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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QEEdit));
			this.QuarkEntityFile = new System.Windows.Forms.TextBox();
			this.SetQuArKEntityFile = new System.Windows.Forms.Button();
			this.EntityTree = new System.Windows.Forms.TreeView();
			this.EntityFields = new System.Windows.Forms.DataGridView();
			this.Save = new System.Windows.Forms.Button();
			this.AddEntity = new System.Windows.Forms.Button();
			this.DeleteEntity = new System.Windows.Forms.Button();
			this.RenameEntity = new System.Windows.Forms.Button();
			this.mTips = new System.Windows.Forms.ToolTip(this.components);
			this.AutoAddOrigin = new System.Windows.Forms.CheckBox();
			this.AutoAddMeshName = new System.Windows.Forms.CheckBox();
			this.AutoAddDesc = new System.Windows.Forms.CheckBox();
			this.AddGroupBox = new System.Windows.Forms.GroupBox();
			this.RenameGroupBox = new System.Windows.Forms.GroupBox();
			this.RenameBox = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.EntityFields)).BeginInit();
			this.AddGroupBox.SuspendLayout();
			this.RenameGroupBox.SuspendLayout();
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
			this.mTips.SetToolTip(this.SetQuArKEntityFile, "Load the entities file");
			this.SetQuArKEntityFile.UseVisualStyleBackColor = true;
			this.SetQuArKEntityFile.Click += new System.EventHandler(this.OnBrowseForEntityFile);
			// 
			// EntityTree
			// 
			this.EntityTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EntityTree.HideSelection = false;
			this.EntityTree.Location = new System.Drawing.Point(12, 41);
			this.EntityTree.Name = "EntityTree";
			this.EntityTree.Size = new System.Drawing.Size(368, 255);
			this.EntityTree.TabIndex = 9;
			this.EntityTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnAfterSelect);
			this.EntityTree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyUp);
			// 
			// EntityFields
			// 
			this.EntityFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EntityFields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
			this.EntityFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.EntityFields.Location = new System.Drawing.Point(12, 302);
			this.EntityFields.Name = "EntityFields";
			this.EntityFields.Size = new System.Drawing.Size(574, 164);
			this.EntityFields.TabIndex = 10;
			// 
			// Save
			// 
			this.Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Save.Location = new System.Drawing.Point(511, 472);
			this.Save.Name = "Save";
			this.Save.Size = new System.Drawing.Size(75, 23);
			this.Save.TabIndex = 11;
			this.Save.Text = "Save";
			this.mTips.SetToolTip(this.Save, "Write the entity file to disk");
			this.Save.UseVisualStyleBackColor = true;
			this.Save.Click += new System.EventHandler(this.OnSave);
			// 
			// AddEntity
			// 
			this.AddEntity.Location = new System.Drawing.Point(119, 19);
			this.AddEntity.Name = "AddEntity";
			this.AddEntity.Size = new System.Drawing.Size(75, 23);
			this.AddEntity.TabIndex = 12;
			this.AddEntity.Text = "Add";
			this.mTips.SetToolTip(this.AddEntity, "Add a new entity to a category");
			this.AddEntity.UseVisualStyleBackColor = true;
			this.AddEntity.Click += new System.EventHandler(this.OnAddEntity);
			// 
			// DeleteEntity
			// 
			this.DeleteEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteEntity.Enabled = false;
			this.DeleteEntity.Location = new System.Drawing.Point(511, 137);
			this.DeleteEntity.Name = "DeleteEntity";
			this.DeleteEntity.Size = new System.Drawing.Size(75, 23);
			this.DeleteEntity.TabIndex = 13;
			this.DeleteEntity.Text = "Delete";
			this.mTips.SetToolTip(this.DeleteEntity, "(Del) Nuke the selected entity");
			this.DeleteEntity.UseVisualStyleBackColor = true;
			this.DeleteEntity.Click += new System.EventHandler(this.OnDeleteEntity);
			// 
			// RenameEntity
			// 
			this.RenameEntity.Location = new System.Drawing.Point(6, 24);
			this.RenameEntity.Name = "RenameEntity";
			this.RenameEntity.Size = new System.Drawing.Size(75, 23);
			this.RenameEntity.TabIndex = 14;
			this.RenameEntity.Text = "Rename";
			this.mTips.SetToolTip(this.RenameEntity, "(F2) Rename the selected entity");
			this.RenameEntity.UseVisualStyleBackColor = true;
			this.RenameEntity.Click += new System.EventHandler(this.OnRenameEntity);
			// 
			// AutoAddOrigin
			// 
			this.AutoAddOrigin.AutoSize = true;
			this.AutoAddOrigin.Checked = true;
			this.AutoAddOrigin.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AutoAddOrigin.Location = new System.Drawing.Point(6, 19);
			this.AutoAddOrigin.Name = "AutoAddOrigin";
			this.AutoAddOrigin.Size = new System.Drawing.Size(100, 17);
			this.AutoAddOrigin.TabIndex = 16;
			this.AutoAddOrigin.Text = "Auto Add Origin";
			this.mTips.SetToolTip(this.AutoAddOrigin, "Creates an origin key with a default value of 0 0 0");
			this.AutoAddOrigin.UseVisualStyleBackColor = true;
			// 
			// AutoAddMeshName
			// 
			this.AutoAddMeshName.AutoSize = true;
			this.AutoAddMeshName.Location = new System.Drawing.Point(6, 42);
			this.AutoAddMeshName.Name = "AutoAddMeshName";
			this.AutoAddMeshName.Size = new System.Drawing.Size(125, 17);
			this.AutoAddMeshName.TabIndex = 17;
			this.AutoAddMeshName.Text = "Auto Add Meshname";
			this.mTips.SetToolTip(this.AutoAddMeshName, "Adds a meshname key with a default value of default.static");
			this.AutoAddMeshName.UseVisualStyleBackColor = true;
			// 
			// AutoAddDesc
			// 
			this.AutoAddDesc.AutoSize = true;
			this.AutoAddDesc.Checked = true;
			this.AutoAddDesc.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AutoAddDesc.Location = new System.Drawing.Point(6, 66);
			this.AutoAddDesc.Name = "AutoAddDesc";
			this.AutoAddDesc.Size = new System.Drawing.Size(98, 17);
			this.AutoAddDesc.TabIndex = 18;
			this.AutoAddDesc.Text = "Auto Add Desc";
			this.mTips.SetToolTip(this.AutoAddDesc, "Adds a ;desc key with default value of Description");
			this.AutoAddDesc.UseVisualStyleBackColor = true;
			// 
			// AddGroupBox
			// 
			this.AddGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddGroupBox.Controls.Add(this.AutoAddOrigin);
			this.AddGroupBox.Controls.Add(this.AutoAddDesc);
			this.AddGroupBox.Controls.Add(this.AutoAddMeshName);
			this.AddGroupBox.Controls.Add(this.AddEntity);
			this.AddGroupBox.Enabled = false;
			this.AddGroupBox.Location = new System.Drawing.Point(386, 41);
			this.AddGroupBox.Name = "AddGroupBox";
			this.AddGroupBox.Size = new System.Drawing.Size(200, 90);
			this.AddGroupBox.TabIndex = 19;
			this.AddGroupBox.TabStop = false;
			this.AddGroupBox.Text = "Add Entity";
			this.mTips.SetToolTip(this.AddGroupBox, "Add a new entity with default values checked");
			// 
			// RenameGroupBox
			// 
			this.RenameGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RenameGroupBox.Controls.Add(this.RenameEntity);
			this.RenameGroupBox.Controls.Add(this.RenameBox);
			this.RenameGroupBox.Enabled = false;
			this.RenameGroupBox.Location = new System.Drawing.Point(386, 166);
			this.RenameGroupBox.Name = "RenameGroupBox";
			this.RenameGroupBox.Size = new System.Drawing.Size(200, 84);
			this.RenameGroupBox.TabIndex = 20;
			this.RenameGroupBox.TabStop = false;
			this.RenameGroupBox.Text = "RenameEntity";
			this.mTips.SetToolTip(this.RenameGroupBox, "Give the selected entity a new name");
			// 
			// RenameBox
			// 
			this.RenameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RenameBox.Enabled = false;
			this.RenameBox.Location = new System.Drawing.Point(6, 53);
			this.RenameBox.Name = "RenameBox";
			this.RenameBox.Size = new System.Drawing.Size(141, 20);
			this.RenameBox.TabIndex = 15;
			this.RenameBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnRenameBoxKey);
			// 
			// QEEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(598, 507);
			this.Controls.Add(this.RenameGroupBox);
			this.Controls.Add(this.AddGroupBox);
			this.Controls.Add(this.DeleteEntity);
			this.Controls.Add(this.Save);
			this.Controls.Add(this.EntityFields);
			this.Controls.Add(this.EntityTree);
			this.Controls.Add(this.QuarkEntityFile);
			this.Controls.Add(this.SetQuArKEntityFile);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "QEEdit";
			this.Text = "Entity Editor for QuArK";
			((System.ComponentModel.ISupportInitialize)(this.EntityFields)).EndInit();
			this.AddGroupBox.ResumeLayout(false);
			this.AddGroupBox.PerformLayout();
			this.RenameGroupBox.ResumeLayout(false);
			this.RenameGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox QuarkEntityFile;
		private System.Windows.Forms.Button SetQuArKEntityFile;
		private System.Windows.Forms.TreeView EntityTree;
		private System.Windows.Forms.DataGridView EntityFields;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button AddEntity;
		private System.Windows.Forms.Button DeleteEntity;
		private System.Windows.Forms.Button RenameEntity;
		private System.Windows.Forms.ToolTip mTips;
		private System.Windows.Forms.TextBox RenameBox;
		private System.Windows.Forms.CheckBox AutoAddOrigin;
		private System.Windows.Forms.CheckBox AutoAddMeshName;
		private System.Windows.Forms.CheckBox AutoAddDesc;
		private System.Windows.Forms.GroupBox AddGroupBox;
		private System.Windows.Forms.GroupBox RenameGroupBox;
	}
}

