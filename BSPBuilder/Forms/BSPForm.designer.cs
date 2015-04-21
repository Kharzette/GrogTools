namespace BSPBuilder
{
	partial class BSPForm
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
			this.OpenStatic = new System.Windows.Forms.Button();
			this.FullBuild = new System.Windows.Forms.Button();
			this.UpdateEntities = new System.Windows.Forms.Button();
			this.LightGBSP = new System.Windows.Forms.Button();
			this.BuildGBSP = new System.Windows.Forms.Button();
			this.OpenBrushFile = new System.Windows.Forms.Button();
			this.SaveGBSP = new System.Windows.Forms.Button();
			this.GroupBuildSettings = new System.Windows.Forms.GroupBox();
			this.label11 = new System.Windows.Forms.Label();
			this.MaxThreads = new System.Windows.Forms.NumericUpDown();
			this.FixTJunctions = new System.Windows.Forms.CheckBox();
			this.BuildAsBModel = new System.Windows.Forms.CheckBox();
			this.VerboseBSP = new System.Windows.Forms.CheckBox();
			this.LightSettingsGroupBox = new System.Windows.Forms.GroupBox();
			this.DynamicLights = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.NumSamples = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.LightGridSize = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.MaxIntensity = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.MinLightZ = new System.Windows.Forms.NumericUpDown();
			this.MinLightX = new System.Windows.Forms.NumericUpDown();
			this.MinLightY = new System.Windows.Forms.NumericUpDown();
			this.SeamCorrection = new System.Windows.Forms.CheckBox();
			this.mTips = new System.Windows.Forms.ToolTip(this.components);
			this.MapToStatic = new System.Windows.Forms.Button();
			this.GroupFileIO.SuspendLayout();
			this.GroupBuildSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxThreads)).BeginInit();
			this.LightSettingsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumSamples)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LightGridSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxIntensity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightZ)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightY)).BeginInit();
			this.SuspendLayout();
			// 
			// GroupFileIO
			// 
			this.GroupFileIO.Controls.Add(this.MapToStatic);
			this.GroupFileIO.Controls.Add(this.OpenStatic);
			this.GroupFileIO.Controls.Add(this.FullBuild);
			this.GroupFileIO.Controls.Add(this.UpdateEntities);
			this.GroupFileIO.Controls.Add(this.LightGBSP);
			this.GroupFileIO.Controls.Add(this.BuildGBSP);
			this.GroupFileIO.Controls.Add(this.OpenBrushFile);
			this.GroupFileIO.Controls.Add(this.SaveGBSP);
			this.GroupFileIO.Location = new System.Drawing.Point(12, 12);
			this.GroupFileIO.Name = "GroupFileIO";
			this.GroupFileIO.Size = new System.Drawing.Size(180, 166);
			this.GroupFileIO.TabIndex = 22;
			this.GroupFileIO.TabStop = false;
			this.GroupFileIO.Text = "File IO";
			// 
			// OpenStatic
			// 
			this.OpenStatic.Location = new System.Drawing.Point(6, 106);
			this.OpenStatic.Name = "OpenStatic";
			this.OpenStatic.Size = new System.Drawing.Size(87, 23);
			this.OpenStatic.TabIndex = 21;
			this.OpenStatic.Text = "Static to Map";
			this.OpenStatic.UseVisualStyleBackColor = true;
			this.OpenStatic.Click += new System.EventHandler(this.OnOpenStatic);
			// 
			// FullBuild
			// 
			this.FullBuild.Location = new System.Drawing.Point(99, 92);
			this.FullBuild.Name = "FullBuild";
			this.FullBuild.Size = new System.Drawing.Size(52, 38);
			this.FullBuild.TabIndex = 20;
			this.FullBuild.Text = "Full Build";
			this.mTips.SetToolTip(this.FullBuild, "Generate lightmaps for the level.  Needs vis data first.");
			this.FullBuild.UseVisualStyleBackColor = true;
			this.FullBuild.Click += new System.EventHandler(this.OnFullBuild);
			// 
			// UpdateEntities
			// 
			this.UpdateEntities.Location = new System.Drawing.Point(99, 19);
			this.UpdateEntities.Name = "UpdateEntities";
			this.UpdateEntities.Size = new System.Drawing.Size(75, 38);
			this.UpdateEntities.TabIndex = 19;
			this.UpdateEntities.Text = "Update Entities";
			this.mTips.SetToolTip(this.UpdateEntities, "Generate lightmaps for the level.  Needs vis data first.");
			this.UpdateEntities.UseVisualStyleBackColor = true;
			this.UpdateEntities.Click += new System.EventHandler(this.OnUpdateEntities);
			// 
			// LightGBSP
			// 
			this.LightGBSP.Location = new System.Drawing.Point(99, 63);
			this.LightGBSP.Name = "LightGBSP";
			this.LightGBSP.Size = new System.Drawing.Size(75, 23);
			this.LightGBSP.TabIndex = 18;
			this.LightGBSP.Text = "Light GBSP";
			this.mTips.SetToolTip(this.LightGBSP, "Generate lightmaps for the level.  Needs vis data first.");
			this.LightGBSP.UseVisualStyleBackColor = true;
			this.LightGBSP.Click += new System.EventHandler(this.OnLightGBSP);
			// 
			// BuildGBSP
			// 
			this.BuildGBSP.Enabled = false;
			this.BuildGBSP.Location = new System.Drawing.Point(6, 48);
			this.BuildGBSP.Name = "BuildGBSP";
			this.BuildGBSP.Size = new System.Drawing.Size(75, 23);
			this.BuildGBSP.TabIndex = 17;
			this.BuildGBSP.Text = "Build GBSP";
			this.mTips.SetToolTip(this.BuildGBSP, "Build loaded data into saveable bsp and portal data");
			this.BuildGBSP.UseVisualStyleBackColor = true;
			this.BuildGBSP.Click += new System.EventHandler(this.OnBuildGBSP);
			// 
			// OpenBrushFile
			// 
			this.OpenBrushFile.Location = new System.Drawing.Point(6, 19);
			this.OpenBrushFile.Name = "OpenBrushFile";
			this.OpenBrushFile.Size = new System.Drawing.Size(75, 23);
			this.OpenBrushFile.TabIndex = 0;
			this.OpenBrushFile.Text = "Open Map";
			this.mTips.SetToolTip(this.OpenBrushFile, "Load convex volumes from .map or .vmf source files");
			this.OpenBrushFile.UseVisualStyleBackColor = true;
			this.OpenBrushFile.Click += new System.EventHandler(this.OnOpenMap);
			// 
			// SaveGBSP
			// 
			this.SaveGBSP.Enabled = false;
			this.SaveGBSP.Location = new System.Drawing.Point(6, 77);
			this.SaveGBSP.Name = "SaveGBSP";
			this.SaveGBSP.Size = new System.Drawing.Size(75, 23);
			this.SaveGBSP.TabIndex = 15;
			this.SaveGBSP.Text = "Save GBSP";
			this.mTips.SetToolTip(this.SaveGBSP, "Save built data to a .gbsp file");
			this.SaveGBSP.UseVisualStyleBackColor = true;
			this.SaveGBSP.Click += new System.EventHandler(this.OnSaveGBSP);
			// 
			// GroupBuildSettings
			// 
			this.GroupBuildSettings.Controls.Add(this.label11);
			this.GroupBuildSettings.Controls.Add(this.MaxThreads);
			this.GroupBuildSettings.Controls.Add(this.FixTJunctions);
			this.GroupBuildSettings.Controls.Add(this.BuildAsBModel);
			this.GroupBuildSettings.Controls.Add(this.VerboseBSP);
			this.GroupBuildSettings.Location = new System.Drawing.Point(198, 12);
			this.GroupBuildSettings.Name = "GroupBuildSettings";
			this.GroupBuildSettings.Size = new System.Drawing.Size(137, 116);
			this.GroupBuildSettings.TabIndex = 31;
			this.GroupBuildSettings.TabStop = false;
			this.GroupBuildSettings.Text = "Build Settings";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(57, 90);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(69, 13);
			this.label11.TabIndex = 34;
			this.label11.Text = "Max Threads";
			// 
			// MaxThreads
			// 
			this.MaxThreads.Location = new System.Drawing.Point(6, 88);
			this.MaxThreads.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.MaxThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MaxThreads.Name = "MaxThreads";
			this.MaxThreads.Size = new System.Drawing.Size(45, 20);
			this.MaxThreads.TabIndex = 33;
			this.mTips.SetToolTip(this.MaxThreads, "Limit the active threads so builds don\'t cripple the machine");
			this.MaxThreads.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// FixTJunctions
			// 
			this.FixTJunctions.AutoSize = true;
			this.FixTJunctions.Checked = true;
			this.FixTJunctions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.FixTJunctions.Location = new System.Drawing.Point(6, 65);
			this.FixTJunctions.Name = "FixTJunctions";
			this.FixTJunctions.Size = new System.Drawing.Size(94, 17);
			this.FixTJunctions.TabIndex = 22;
			this.FixTJunctions.Text = "Fix TJunctions";
			this.mTips.SetToolTip(this.FixTJunctions, "Split faces with edges that form a T causing visible cracks in the world");
			this.FixTJunctions.UseVisualStyleBackColor = true;
			// 
			// BuildAsBModel
			// 
			this.BuildAsBModel.AutoSize = true;
			this.BuildAsBModel.Location = new System.Drawing.Point(6, 42);
			this.BuildAsBModel.Name = "BuildAsBModel";
			this.BuildAsBModel.Size = new System.Drawing.Size(102, 17);
			this.BuildAsBModel.TabIndex = 21;
			this.BuildAsBModel.Text = "Build as BModel";
			this.mTips.SetToolTip(this.BuildAsBModel, "Build the entire map as if it were a brush model");
			this.BuildAsBModel.UseVisualStyleBackColor = true;
			// 
			// VerboseBSP
			// 
			this.VerboseBSP.AutoSize = true;
			this.VerboseBSP.Location = new System.Drawing.Point(6, 19);
			this.VerboseBSP.Name = "VerboseBSP";
			this.VerboseBSP.Size = new System.Drawing.Size(65, 17);
			this.VerboseBSP.TabIndex = 20;
			this.VerboseBSP.Text = "Verbose";
			this.mTips.SetToolTip(this.VerboseBSP, "Turns on a lot of spam, which can slow down the build");
			this.VerboseBSP.UseVisualStyleBackColor = true;
			this.VerboseBSP.CheckedChanged += new System.EventHandler(this.OnVerbose);
			// 
			// LightSettingsGroupBox
			// 
			this.LightSettingsGroupBox.Controls.Add(this.DynamicLights);
			this.LightSettingsGroupBox.Controls.Add(this.label10);
			this.LightSettingsGroupBox.Controls.Add(this.NumSamples);
			this.LightSettingsGroupBox.Controls.Add(this.label9);
			this.LightSettingsGroupBox.Controls.Add(this.LightGridSize);
			this.LightSettingsGroupBox.Controls.Add(this.label8);
			this.LightSettingsGroupBox.Controls.Add(this.MaxIntensity);
			this.LightSettingsGroupBox.Controls.Add(this.label6);
			this.LightSettingsGroupBox.Controls.Add(this.label5);
			this.LightSettingsGroupBox.Controls.Add(this.label4);
			this.LightSettingsGroupBox.Controls.Add(this.MinLightZ);
			this.LightSettingsGroupBox.Controls.Add(this.MinLightX);
			this.LightSettingsGroupBox.Controls.Add(this.MinLightY);
			this.LightSettingsGroupBox.Controls.Add(this.SeamCorrection);
			this.LightSettingsGroupBox.Location = new System.Drawing.Point(12, 184);
			this.LightSettingsGroupBox.Name = "LightSettingsGroupBox";
			this.LightSettingsGroupBox.Size = new System.Drawing.Size(369, 100);
			this.LightSettingsGroupBox.TabIndex = 32;
			this.LightSettingsGroupBox.TabStop = false;
			this.LightSettingsGroupBox.Text = "Light Settings";
			// 
			// DynamicLights
			// 
			this.DynamicLights.AutoSize = true;
			this.DynamicLights.Checked = true;
			this.DynamicLights.CheckState = System.Windows.Forms.CheckState.Checked;
			this.DynamicLights.Location = new System.Drawing.Point(6, 43);
			this.DynamicLights.Name = "DynamicLights";
			this.DynamicLights.Size = new System.Drawing.Size(98, 17);
			this.DynamicLights.TabIndex = 43;
			this.DynamicLights.Text = "Dynamic Lights";
			this.DynamicLights.UseVisualStyleBackColor = true;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(168, 21);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(47, 13);
			this.label10.TabIndex = 39;
			this.label10.Text = "Samples";
			// 
			// NumSamples
			// 
			this.NumSamples.Location = new System.Drawing.Point(117, 19);
			this.NumSamples.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
			this.NumSamples.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NumSamples.Name = "NumSamples";
			this.NumSamples.Size = new System.Drawing.Size(45, 20);
			this.NumSamples.TabIndex = 38;
			this.mTips.SetToolTip(this.NumSamples, "Extra ray samples to soften the results");
			this.NumSamples.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(169, 73);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(68, 13);
			this.label9.TabIndex = 37;
			this.label9.Text = "Light Density";
			// 
			// LightGridSize
			// 
			this.LightGridSize.Location = new System.Drawing.Point(117, 71);
			this.LightGridSize.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.LightGridSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.LightGridSize.Name = "LightGridSize";
			this.LightGridSize.Size = new System.Drawing.Size(45, 20);
			this.LightGridSize.TabIndex = 36;
			this.mTips.SetToolTip(this.LightGridSize, "World units per light map texel");
			this.LightGridSize.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(168, 47);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(69, 13);
			this.label8.TabIndex = 35;
			this.label8.Text = "Max Intensity";
			// 
			// MaxIntensity
			// 
			this.MaxIntensity.Location = new System.Drawing.Point(117, 45);
			this.MaxIntensity.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.MaxIntensity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.MaxIntensity.Name = "MaxIntensity";
			this.MaxIntensity.Size = new System.Drawing.Size(45, 20);
			this.MaxIntensity.TabIndex = 34;
			this.mTips.SetToolTip(this.MaxIntensity, "Maximum light intensity");
			this.MaxIntensity.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(293, 73);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(60, 13);
			this.label6.TabIndex = 33;
			this.label6.Text = "Min Light B";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(293, 47);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(61, 13);
			this.label5.TabIndex = 32;
			this.label5.Text = "Min Light G";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(293, 21);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(61, 13);
			this.label4.TabIndex = 31;
			this.label4.Text = "Min Light R";
			// 
			// MinLightZ
			// 
			this.MinLightZ.Location = new System.Drawing.Point(242, 71);
			this.MinLightZ.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.MinLightZ.Name = "MinLightZ";
			this.MinLightZ.Size = new System.Drawing.Size(45, 20);
			this.MinLightZ.TabIndex = 29;
			this.mTips.SetToolTip(this.MinLightZ, "Level wide ambient blue");
			// 
			// MinLightX
			// 
			this.MinLightX.Location = new System.Drawing.Point(242, 19);
			this.MinLightX.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.MinLightX.Name = "MinLightX";
			this.MinLightX.Size = new System.Drawing.Size(45, 20);
			this.MinLightX.TabIndex = 30;
			this.mTips.SetToolTip(this.MinLightX, "Level wide ambient red");
			// 
			// MinLightY
			// 
			this.MinLightY.Location = new System.Drawing.Point(242, 45);
			this.MinLightY.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.MinLightY.Name = "MinLightY";
			this.MinLightY.Size = new System.Drawing.Size(45, 20);
			this.MinLightY.TabIndex = 28;
			this.mTips.SetToolTip(this.MinLightY, "Level wide ambient green");
			// 
			// SeamCorrection
			// 
			this.SeamCorrection.AutoSize = true;
			this.SeamCorrection.Checked = true;
			this.SeamCorrection.CheckState = System.Windows.Forms.CheckState.Checked;
			this.SeamCorrection.Location = new System.Drawing.Point(6, 20);
			this.SeamCorrection.Name = "SeamCorrection";
			this.SeamCorrection.Size = new System.Drawing.Size(104, 17);
			this.SeamCorrection.TabIndex = 1;
			this.SeamCorrection.Text = "Seam Correction";
			this.mTips.SetToolTip(this.SeamCorrection, "Shifts light points around a bit to help points just inside solid");
			this.SeamCorrection.UseVisualStyleBackColor = true;
			// 
			// MapToStatic
			// 
			this.MapToStatic.Location = new System.Drawing.Point(6, 135);
			this.MapToStatic.Name = "MapToStatic";
			this.MapToStatic.Size = new System.Drawing.Size(87, 23);
			this.MapToStatic.TabIndex = 22;
			this.MapToStatic.Text = "Map to Static";
			this.MapToStatic.UseVisualStyleBackColor = true;
			this.MapToStatic.Click += new System.EventHandler(this.OnMapToStatic);
			// 
			// BSPForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(393, 296);
			this.ControlBox = false;
			this.Controls.Add(this.LightSettingsGroupBox);
			this.Controls.Add(this.GroupBuildSettings);
			this.Controls.Add(this.GroupFileIO);
			this.Name = "BSPForm";
			this.Text = "BSPForm";
			this.GroupFileIO.ResumeLayout(false);
			this.GroupBuildSettings.ResumeLayout(false);
			this.GroupBuildSettings.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MaxThreads)).EndInit();
			this.LightSettingsGroupBox.ResumeLayout(false);
			this.LightSettingsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumSamples)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LightGridSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxIntensity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightZ)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MinLightY)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox GroupFileIO;
		private System.Windows.Forms.Button LightGBSP;
		private System.Windows.Forms.Button BuildGBSP;
		private System.Windows.Forms.Button OpenBrushFile;
		private System.Windows.Forms.Button SaveGBSP;
		private System.Windows.Forms.GroupBox GroupBuildSettings;
		private System.Windows.Forms.CheckBox FixTJunctions;
		private System.Windows.Forms.CheckBox BuildAsBModel;
		private System.Windows.Forms.CheckBox VerboseBSP;
		private System.Windows.Forms.GroupBox LightSettingsGroupBox;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown LightGridSize;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown MaxIntensity;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown MinLightZ;
		private System.Windows.Forms.NumericUpDown MinLightX;
		private System.Windows.Forms.NumericUpDown MinLightY;
		private System.Windows.Forms.CheckBox SeamCorrection;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown NumSamples;
		private System.Windows.Forms.NumericUpDown MaxThreads;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.ToolTip mTips;
		private System.Windows.Forms.Button FullBuild;
		private System.Windows.Forms.Button UpdateEntities;
		private System.Windows.Forms.CheckBox DynamicLights;
		private System.Windows.Forms.Button OpenStatic;
		private System.Windows.Forms.Button MapToStatic;
	}
}