namespace ZoneBuild
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
			this.ConsoleOut = new System.Windows.Forms.TextBox();
			this.GenerateMaterials = new System.Windows.Forms.Button();
			this.GroupFileIO = new System.Windows.Forms.GroupBox();
			this.LoadDebug = new System.Windows.Forms.Button();
			this.SaveEmissives = new System.Windows.Forms.Button();
			this.VisGBSP = new System.Windows.Forms.Button();
			this.SaveZone = new System.Windows.Forms.Button();
			this.LoadGBSP = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.NumPortals = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.NumClusters = new System.Windows.Forms.TextBox();
			this.NumVerts = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.NumPlanes = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.AtlasSize = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			this.GroupFileIO.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AtlasSize)).BeginInit();
			this.SuspendLayout();
			// 
			// ConsoleOut
			// 
			this.ConsoleOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleOut.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConsoleOut.Location = new System.Drawing.Point(12, 160);
			this.ConsoleOut.Multiline = true;
			this.ConsoleOut.Name = "ConsoleOut";
			this.ConsoleOut.ReadOnly = true;
			this.ConsoleOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConsoleOut.Size = new System.Drawing.Size(452, 182);
			this.ConsoleOut.TabIndex = 17;
			// 
			// GenerateMaterials
			// 
			this.GenerateMaterials.Location = new System.Drawing.Point(6, 19);
			this.GenerateMaterials.Name = "GenerateMaterials";
			this.GenerateMaterials.Size = new System.Drawing.Size(75, 23);
			this.GenerateMaterials.TabIndex = 24;
			this.GenerateMaterials.Text = "Gen Mats";
			this.GenerateMaterials.UseVisualStyleBackColor = true;
			this.GenerateMaterials.Click += new System.EventHandler(this.OnGenerateMaterials);
			// 
			// GroupFileIO
			// 
			this.GroupFileIO.Controls.Add(this.LoadDebug);
			this.GroupFileIO.Controls.Add(this.SaveEmissives);
			this.GroupFileIO.Controls.Add(this.VisGBSP);
			this.GroupFileIO.Controls.Add(this.SaveZone);
			this.GroupFileIO.Controls.Add(this.GenerateMaterials);
			this.GroupFileIO.Controls.Add(this.LoadGBSP);
			this.GroupFileIO.Location = new System.Drawing.Point(12, 12);
			this.GroupFileIO.Name = "GroupFileIO";
			this.GroupFileIO.Size = new System.Drawing.Size(189, 109);
			this.GroupFileIO.TabIndex = 25;
			this.GroupFileIO.TabStop = false;
			this.GroupFileIO.Text = "File IO";
			// 
			// LoadDebug
			// 
			this.LoadDebug.Location = new System.Drawing.Point(87, 77);
			this.LoadDebug.Name = "LoadDebug";
			this.LoadDebug.Size = new System.Drawing.Size(75, 23);
			this.LoadDebug.TabIndex = 27;
			this.LoadDebug.Text = "Load Debug";
			this.LoadDebug.UseVisualStyleBackColor = true;
			this.LoadDebug.Click += new System.EventHandler(this.OnLoadPortals);
			// 
			// SaveEmissives
			// 
			this.SaveEmissives.Location = new System.Drawing.Point(87, 19);
			this.SaveEmissives.Name = "SaveEmissives";
			this.SaveEmissives.Size = new System.Drawing.Size(96, 23);
			this.SaveEmissives.TabIndex = 26;
			this.SaveEmissives.Text = "Save Emissives";
			this.SaveEmissives.UseVisualStyleBackColor = true;
			this.SaveEmissives.Click += new System.EventHandler(this.OnSaveEmissives);
			// 
			// VisGBSP
			// 
			this.VisGBSP.Location = new System.Drawing.Point(6, 77);
			this.VisGBSP.Name = "VisGBSP";
			this.VisGBSP.Size = new System.Drawing.Size(75, 23);
			this.VisGBSP.TabIndex = 25;
			this.VisGBSP.Text = "Material Vis";
			this.VisGBSP.UseVisualStyleBackColor = true;
			this.VisGBSP.Click += new System.EventHandler(this.OnMaterialVis);
			// 
			// SaveZone
			// 
			this.SaveZone.Enabled = false;
			this.SaveZone.Location = new System.Drawing.Point(87, 48);
			this.SaveZone.Name = "SaveZone";
			this.SaveZone.Size = new System.Drawing.Size(75, 23);
			this.SaveZone.TabIndex = 21;
			this.SaveZone.Text = "Save Zone";
			this.SaveZone.UseVisualStyleBackColor = true;
			this.SaveZone.Click += new System.EventHandler(this.OnSaveZone);
			// 
			// LoadGBSP
			// 
			this.LoadGBSP.Location = new System.Drawing.Point(6, 48);
			this.LoadGBSP.Name = "LoadGBSP";
			this.LoadGBSP.Size = new System.Drawing.Size(75, 23);
			this.LoadGBSP.TabIndex = 19;
			this.LoadGBSP.Text = "Zone GBSP";
			this.LoadGBSP.UseVisualStyleBackColor = true;
			this.LoadGBSP.Click += new System.EventHandler(this.OnZone);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 100);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 20;
			this.label1.Text = "Portals";
			// 
			// NumPortals
			// 
			this.NumPortals.Location = new System.Drawing.Point(51, 97);
			this.NumPortals.Name = "NumPortals";
			this.NumPortals.ReadOnly = true;
			this.NumPortals.Size = new System.Drawing.Size(76, 20);
			this.NumPortals.TabIndex = 19;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 13);
			this.label2.TabIndex = 18;
			this.label2.Text = "Clusters";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 13);
			this.label3.TabIndex = 17;
			this.label3.Text = "Verts";
			// 
			// NumClusters
			// 
			this.NumClusters.Location = new System.Drawing.Point(51, 71);
			this.NumClusters.Name = "NumClusters";
			this.NumClusters.ReadOnly = true;
			this.NumClusters.Size = new System.Drawing.Size(76, 20);
			this.NumClusters.TabIndex = 15;
			// 
			// NumVerts
			// 
			this.NumVerts.Location = new System.Drawing.Point(51, 45);
			this.NumVerts.Name = "NumVerts";
			this.NumVerts.ReadOnly = true;
			this.NumVerts.Size = new System.Drawing.Size(76, 20);
			this.NumVerts.TabIndex = 14;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 22);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(39, 13);
			this.label4.TabIndex = 12;
			this.label4.Text = "Planes";
			// 
			// NumPlanes
			// 
			this.NumPlanes.Location = new System.Drawing.Point(51, 19);
			this.NumPlanes.Name = "NumPlanes";
			this.NumPlanes.ReadOnly = true;
			this.NumPlanes.Size = new System.Drawing.Size(76, 20);
			this.NumPlanes.TabIndex = 13;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.NumPortals);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.NumClusters);
			this.groupBox1.Controls.Add(this.NumVerts);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.NumPlanes);
			this.groupBox1.Location = new System.Drawing.Point(207, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(134, 127);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Statistics";
			// 
			// AtlasSize
			// 
			this.AtlasSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AtlasSize.Location = new System.Drawing.Point(366, 28);
			this.AtlasSize.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.AtlasSize.Minimum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.AtlasSize.Name = "AtlasSize";
			this.AtlasSize.Size = new System.Drawing.Size(53, 20);
			this.AtlasSize.TabIndex = 39;
			this.AtlasSize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(347, 12);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(99, 13);
			this.label10.TabIndex = 40;
			this.label10.Text = "Lightmap Atlas Size";
			// 
			// ZoneForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(476, 354);
			this.ControlBox = false;
			this.Controls.Add(this.label10);
			this.Controls.Add(this.AtlasSize);
			this.Controls.Add(this.GroupFileIO);
			this.Controls.Add(this.ConsoleOut);
			this.Controls.Add(this.groupBox1);
			this.Name = "ZoneForm";
			this.Text = "ZoneForm";
			this.GroupFileIO.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AtlasSize)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ConsoleOut;
		private System.Windows.Forms.Button GenerateMaterials;
		private System.Windows.Forms.GroupBox GroupFileIO;
		private System.Windows.Forms.Button SaveZone;
		private System.Windows.Forms.Button LoadGBSP;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox NumPortals;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox NumClusters;
		private System.Windows.Forms.TextBox NumVerts;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox NumPlanes;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button VisGBSP;
		private System.Windows.Forms.NumericUpDown AtlasSize;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button SaveEmissives;
		private System.Windows.Forms.Button LoadDebug;
	}
}