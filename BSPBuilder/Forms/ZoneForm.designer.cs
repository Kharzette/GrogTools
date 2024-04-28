namespace BSPBuilder
{
	partial class ZoneForm
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
			this.GroupFileIO = new System.Windows.Forms.GroupBox();
			this.LoadDebug = new System.Windows.Forms.Button();
			this.VisGBSP = new System.Windows.Forms.Button();
			this.SaveZone = new System.Windows.Forms.Button();
			this.LoadQBSP = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.AtlasSize = new System.Windows.Forms.NumericUpDown();
			this.SaveDebug = new System.Windows.Forms.CheckBox();
			this.mTips = new System.Windows.Forms.ToolTip(this.components);
			this.DumpTextures = new System.Windows.Forms.Button();
			this.GroupFileIO.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AtlasSize)).BeginInit();
			this.SuspendLayout();
			// 
			// GroupFileIO
			// 
			this.GroupFileIO.Controls.Add(this.LoadDebug);
			this.GroupFileIO.Controls.Add(this.VisGBSP);
			this.GroupFileIO.Controls.Add(this.SaveZone);
			this.GroupFileIO.Controls.Add(this.LoadQBSP);
			this.GroupFileIO.Location = new System.Drawing.Point(14, 14);
			this.GroupFileIO.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.GroupFileIO.Name = "GroupFileIO";
			this.GroupFileIO.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.GroupFileIO.Size = new System.Drawing.Size(220, 95);
			this.GroupFileIO.TabIndex = 26;
			this.GroupFileIO.TabStop = false;
			this.GroupFileIO.Text = "File IO";
			// 
			// LoadDebug
			// 
			this.LoadDebug.Location = new System.Drawing.Point(103, 56);
			this.LoadDebug.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.LoadDebug.Name = "LoadDebug";
			this.LoadDebug.Size = new System.Drawing.Size(88, 27);
			this.LoadDebug.TabIndex = 27;
			this.LoadDebug.Text = "Load Debug";
			this.mTips.SetToolTip(this.LoadDebug, "Load debug files for drawing portals or other misc data");
			this.LoadDebug.UseVisualStyleBackColor = true;
			this.LoadDebug.Click += new System.EventHandler(this.OnLoadDebug);
			// 
			// VisGBSP
			// 
			this.VisGBSP.Location = new System.Drawing.Point(8, 56);
			this.VisGBSP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.VisGBSP.Name = "VisGBSP";
			this.VisGBSP.Size = new System.Drawing.Size(88, 27);
			this.VisGBSP.TabIndex = 25;
			this.VisGBSP.Text = "Material Vis";
			this.mTips.SetToolTip(this.VisGBSP, "Compute which materials are visible from any given cluster");
			this.VisGBSP.UseVisualStyleBackColor = true;
			this.VisGBSP.Click += new System.EventHandler(this.OnMaterialVis);
			// 
			// SaveZone
			// 
			this.SaveZone.Enabled = false;
			this.SaveZone.Location = new System.Drawing.Point(103, 22);
			this.SaveZone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SaveZone.Name = "SaveZone";
			this.SaveZone.Size = new System.Drawing.Size(88, 27);
			this.SaveZone.TabIndex = 21;
			this.SaveZone.Text = "Save Zone";
			this.mTips.SetToolTip(this.SaveZone, "Save the zone, also creates a ZoneDraw file");
			this.SaveZone.UseVisualStyleBackColor = true;
			this.SaveZone.Click += new System.EventHandler(this.OnSaveZone);
			// 
			// LoadQBSP
			// 
			this.LoadQBSP.Location = new System.Drawing.Point(8, 22);
			this.LoadQBSP.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.LoadQBSP.Name = "LoadQBSP";
			this.LoadQBSP.Size = new System.Drawing.Size(88, 27);
			this.LoadQBSP.TabIndex = 19;
			this.LoadQBSP.Text = "Zone QBSP";
			this.mTips.SetToolTip(this.LoadQBSP, "Grind a qbsp file into Zone and ZoneDraw information");
			this.LoadQBSP.UseVisualStyleBackColor = true;
			this.LoadQBSP.Click += new System.EventHandler(this.OnZone);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(310, 16);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(110, 15);
			this.label10.TabIndex = 41;
			this.label10.Text = "Lightmap Atlas Size";
			// 
			// AtlasSize
			// 
			this.AtlasSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AtlasSize.Location = new System.Drawing.Point(241, 14);
			this.AtlasSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.AtlasSize.Maximum = new decimal(new int[] {
            8192,
            0,
            0,
            0});
			this.AtlasSize.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.AtlasSize.Name = "AtlasSize";
			this.AtlasSize.Size = new System.Drawing.Size(62, 23);
			this.AtlasSize.TabIndex = 42;
			this.mTips.SetToolTip(this.AtlasSize, "Size of the atlas that contains the lightmaps.  Smaller the better, but you may r" +
        "un out of space.");
			this.AtlasSize.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
			// 
			// SaveDebug
			// 
			this.SaveDebug.AutoSize = true;
			this.SaveDebug.Checked = true;
			this.SaveDebug.CheckState = System.Windows.Forms.CheckState.Checked;
			this.SaveDebug.Location = new System.Drawing.Point(241, 44);
			this.SaveDebug.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.SaveDebug.Name = "SaveDebug";
			this.SaveDebug.Size = new System.Drawing.Size(112, 19);
			this.SaveDebug.TabIndex = 43;
			this.SaveDebug.Text = "Save Debug Info";
			this.mTips.SetToolTip(this.SaveDebug, "Save the Zone with leaf face and other data a final game has no need of");
			this.SaveDebug.UseVisualStyleBackColor = true;
			// 
			// DumpTextures
			// 
			this.DumpTextures.Location = new System.Drawing.Point(241, 70);
			this.DumpTextures.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.DumpTextures.Name = "DumpTextures";
			this.DumpTextures.Size = new System.Drawing.Size(102, 39);
			this.DumpTextures.TabIndex = 44;
			this.DumpTextures.Text = "Dump Textures Used";
			this.DumpTextures.UseVisualStyleBackColor = true;
			this.DumpTextures.Click += new System.EventHandler(this.OnDumpTextures);
			// 
			// ZoneForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(454, 120);
			this.ControlBox = false;
			this.Controls.Add(this.DumpTextures);
			this.Controls.Add(this.SaveDebug);
			this.Controls.Add(this.AtlasSize);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.GroupFileIO);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "ZoneForm";
			this.Text = "ZoneForm";
			this.GroupFileIO.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AtlasSize)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox GroupFileIO;
		private System.Windows.Forms.Button LoadDebug;
		private System.Windows.Forms.Button VisGBSP;
		private System.Windows.Forms.Button SaveZone;
		private System.Windows.Forms.Button LoadQBSP;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown AtlasSize;
		private System.Windows.Forms.CheckBox SaveDebug;
		private System.Windows.Forms.ToolTip mTips;
		private System.Windows.Forms.Button DumpTextures;
	}
}