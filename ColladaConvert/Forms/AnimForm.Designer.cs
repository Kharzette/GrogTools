namespace ColladaConvert
{
	partial class AnimForm
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
			if(disposing && (components != null))
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
			this.LoadAnim = new System.Windows.Forms.Button();
			this.LoadModel = new System.Windows.Forms.Button();
			this.AnimGrid = new System.Windows.Forms.DataGridView();
			this.TimeScale = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.SaveAnimLib = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.SaveCharacter = new System.Windows.Forms.Button();
			this.LoadCharacter = new System.Windows.Forms.Button();
			this.ClearAll = new System.Windows.Forms.Button();
			this.Compress = new System.Windows.Forms.Button();
			this.MaxError = new System.Windows.Forms.NumericUpDown();
			this.LoadStaticModel = new System.Windows.Forms.Button();
			this.SaveStatic = new System.Windows.Forms.Button();
			this.LoadStatic = new System.Windows.Forms.Button();
			this.LoadMotionDat = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.MaxConvert = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.LoadBoneMap = new System.Windows.Forms.Button();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.BoundGroup = new System.Windows.Forms.GroupBox();
			this.ShowBox = new System.Windows.Forms.CheckBox();
			this.ShowSphere = new System.Windows.Forms.CheckBox();
			this.BoundMesh = new System.Windows.Forms.Button();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.Optimize = new System.Windows.Forms.Button();
			this.Shadowize = new System.Windows.Forms.Button();
			this.BakeVerts = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.AnimGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TimeScale)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxError)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.BoundGroup.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.SuspendLayout();
			// 
			// LoadAnim
			// 
			this.LoadAnim.Location = new System.Drawing.Point(6, 81);
			this.LoadAnim.Name = "LoadAnim";
			this.LoadAnim.Size = new System.Drawing.Size(103, 25);
			this.LoadAnim.TabIndex = 0;
			this.LoadAnim.Text = "Load Anim DAE";
			this.LoadAnim.UseVisualStyleBackColor = true;
			this.LoadAnim.Click += new System.EventHandler(this.OnLoadAnim);
			// 
			// LoadModel
			// 
			this.LoadModel.Location = new System.Drawing.Point(6, 19);
			this.LoadModel.Name = "LoadModel";
			this.LoadModel.Size = new System.Drawing.Size(128, 25);
			this.LoadModel.TabIndex = 1;
			this.LoadModel.Text = "Load DAE Char Model";
			this.LoadModel.UseVisualStyleBackColor = true;
			this.LoadModel.Click += new System.EventHandler(this.OnLoadModel);
			// 
			// AnimGrid
			// 
			this.AnimGrid.AllowUserToAddRows = false;
			this.AnimGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AnimGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.AnimGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.AnimGrid.Location = new System.Drawing.Point(11, 13);
			this.AnimGrid.MultiSelect = false;
			this.AnimGrid.Name = "AnimGrid";
			this.AnimGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.AnimGrid.Size = new System.Drawing.Size(507, 184);
			this.AnimGrid.TabIndex = 2;
			this.AnimGrid.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellValidated);
			this.AnimGrid.SelectionChanged += new System.EventHandler(this.AnimGrid_SelectionChanged);
			this.AnimGrid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.OnRowNuking);
			// 
			// TimeScale
			// 
			this.TimeScale.DecimalPlaces = 2;
			this.TimeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.TimeScale.Location = new System.Drawing.Point(6, 19);
			this.TimeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.TimeScale.Name = "TimeScale";
			this.TimeScale.Size = new System.Drawing.Size(51, 20);
			this.TimeScale.TabIndex = 3;
			this.TimeScale.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			this.TimeScale.ValueChanged += new System.EventHandler(this.TimeScale_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(63, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Time Scale";
			// 
			// SaveAnimLib
			// 
			this.SaveAnimLib.Location = new System.Drawing.Point(6, 50);
			this.SaveAnimLib.Name = "SaveAnimLib";
			this.SaveAnimLib.Size = new System.Drawing.Size(112, 25);
			this.SaveAnimLib.TabIndex = 5;
			this.SaveAnimLib.Text = "Save Anim Library";
			this.SaveAnimLib.UseVisualStyleBackColor = true;
			this.SaveAnimLib.Click += new System.EventHandler(this.OnSaveLibrary);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(6, 19);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(108, 25);
			this.button2.TabIndex = 6;
			this.button2.Text = "Load Anim Library";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.OnLoadLibrary);
			// 
			// SaveCharacter
			// 
			this.SaveCharacter.Location = new System.Drawing.Point(6, 81);
			this.SaveCharacter.Name = "SaveCharacter";
			this.SaveCharacter.Size = new System.Drawing.Size(94, 25);
			this.SaveCharacter.TabIndex = 7;
			this.SaveCharacter.Text = "Save Character";
			this.SaveCharacter.UseVisualStyleBackColor = true;
			this.SaveCharacter.Click += new System.EventHandler(this.OnSaveCharacter);
			// 
			// LoadCharacter
			// 
			this.LoadCharacter.Location = new System.Drawing.Point(6, 19);
			this.LoadCharacter.Name = "LoadCharacter";
			this.LoadCharacter.Size = new System.Drawing.Size(97, 25);
			this.LoadCharacter.TabIndex = 8;
			this.LoadCharacter.Text = "Load Character";
			this.LoadCharacter.UseVisualStyleBackColor = true;
			this.LoadCharacter.Click += new System.EventHandler(this.OnLoadCharacter);
			// 
			// ClearAll
			// 
			this.ClearAll.Location = new System.Drawing.Point(6, 81);
			this.ClearAll.Name = "ClearAll";
			this.ClearAll.Size = new System.Drawing.Size(70, 25);
			this.ClearAll.TabIndex = 9;
			this.ClearAll.Text = "Clear All";
			this.ClearAll.UseVisualStyleBackColor = true;
			this.ClearAll.Click += new System.EventHandler(this.OnClearAll);
			// 
			// Compress
			// 
			this.Compress.Location = new System.Drawing.Point(6, 45);
			this.Compress.Name = "Compress";
			this.Compress.Size = new System.Drawing.Size(69, 25);
			this.Compress.TabIndex = 10;
			this.Compress.Text = "Compress";
			this.Compress.UseVisualStyleBackColor = true;
			this.Compress.Click += new System.EventHandler(this.OnCompress);
			// 
			// MaxError
			// 
			this.MaxError.DecimalPlaces = 3;
			this.MaxError.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.MaxError.Location = new System.Drawing.Point(6, 19);
			this.MaxError.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.MaxError.Name = "MaxError";
			this.MaxError.Size = new System.Drawing.Size(51, 20);
			this.MaxError.TabIndex = 11;
			this.MaxError.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
			// 
			// LoadStaticModel
			// 
			this.LoadStaticModel.Location = new System.Drawing.Point(6, 50);
			this.LoadStaticModel.Name = "LoadStaticModel";
			this.LoadStaticModel.Size = new System.Drawing.Size(103, 25);
			this.LoadStaticModel.TabIndex = 12;
			this.LoadStaticModel.Text = "Load Static DAE";
			this.LoadStaticModel.UseVisualStyleBackColor = true;
			this.LoadStaticModel.Click += new System.EventHandler(this.OnLoadStaticModel);
			// 
			// SaveStatic
			// 
			this.SaveStatic.Location = new System.Drawing.Point(6, 112);
			this.SaveStatic.Name = "SaveStatic";
			this.SaveStatic.Size = new System.Drawing.Size(75, 25);
			this.SaveStatic.TabIndex = 13;
			this.SaveStatic.Text = "Save Static";
			this.SaveStatic.UseVisualStyleBackColor = true;
			this.SaveStatic.Click += new System.EventHandler(this.OnSaveStatic);
			// 
			// LoadStatic
			// 
			this.LoadStatic.Location = new System.Drawing.Point(6, 50);
			this.LoadStatic.Name = "LoadStatic";
			this.LoadStatic.Size = new System.Drawing.Size(75, 25);
			this.LoadStatic.TabIndex = 14;
			this.LoadStatic.Text = "Load Static";
			this.LoadStatic.UseVisualStyleBackColor = true;
			this.LoadStatic.Click += new System.EventHandler(this.OnLoadStatic);
			// 
			// LoadMotionDat
			// 
			this.LoadMotionDat.Location = new System.Drawing.Point(6, 50);
			this.LoadMotionDat.Name = "LoadMotionDat";
			this.LoadMotionDat.Size = new System.Drawing.Size(112, 25);
			this.LoadMotionDat.TabIndex = 15;
			this.LoadMotionDat.Text = "Load Kinect Motion";
			this.LoadMotionDat.UseVisualStyleBackColor = true;
			this.LoadMotionDat.Click += new System.EventHandler(this.OnLoadMotionDat);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.button2);
			this.groupBox1.Controls.Add(this.SaveAnimLib);
			this.groupBox1.Controls.Add(this.ClearAll);
			this.groupBox1.Location = new System.Drawing.Point(12, 204);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(129, 114);
			this.groupBox1.TabIndex = 16;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Library";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox2.Controls.Add(this.LoadCharacter);
			this.groupBox2.Controls.Add(this.LoadStatic);
			this.groupBox2.Controls.Add(this.SaveCharacter);
			this.groupBox2.Controls.Add(this.SaveStatic);
			this.groupBox2.Location = new System.Drawing.Point(147, 203);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(118, 144);
			this.groupBox2.TabIndex = 17;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Converted Meshes";
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox3.Controls.Add(this.BakeVerts);
			this.groupBox3.Controls.Add(this.MaxConvert);
			this.groupBox3.Controls.Add(this.LoadModel);
			this.groupBox3.Controls.Add(this.LoadStaticModel);
			this.groupBox3.Controls.Add(this.LoadAnim);
			this.groupBox3.Location = new System.Drawing.Point(271, 204);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(142, 157);
			this.groupBox3.TabIndex = 18;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Collada Files";
			// 
			// MaxConvert
			// 
			this.MaxConvert.AutoSize = true;
			this.MaxConvert.Location = new System.Drawing.Point(6, 112);
			this.MaxConvert.Name = "MaxConvert";
			this.MaxConvert.Size = new System.Drawing.Size(72, 17);
			this.MaxConvert.TabIndex = 13;
			this.MaxConvert.Text = "From Max";
			this.MaxConvert.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this.MaxError);
			this.groupBox4.Controls.Add(this.Compress);
			this.groupBox4.Location = new System.Drawing.Point(147, 353);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(99, 77);
			this.groupBox4.TabIndex = 19;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Compression";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(63, 21);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(29, 13);
			this.label2.TabIndex = 12;
			this.label2.Text = "Error";
			// 
			// groupBox5
			// 
			this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox5.Controls.Add(this.LoadBoneMap);
			this.groupBox5.Controls.Add(this.LoadMotionDat);
			this.groupBox5.Location = new System.Drawing.Point(12, 324);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(129, 82);
			this.groupBox5.TabIndex = 20;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Kinect Mocap";
			// 
			// LoadBoneMap
			// 
			this.LoadBoneMap.Location = new System.Drawing.Point(6, 19);
			this.LoadBoneMap.Name = "LoadBoneMap";
			this.LoadBoneMap.Size = new System.Drawing.Size(97, 25);
			this.LoadBoneMap.TabIndex = 16;
			this.LoadBoneMap.Text = "Load Bone Map";
			this.LoadBoneMap.UseVisualStyleBackColor = true;
			this.LoadBoneMap.Click += new System.EventHandler(this.OnLoadBoneMap);
			// 
			// groupBox6
			// 
			this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox6.Controls.Add(this.TimeScale);
			this.groupBox6.Controls.Add(this.label1);
			this.groupBox6.Location = new System.Drawing.Point(252, 367);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(133, 52);
			this.groupBox6.TabIndex = 21;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Playback";
			// 
			// BoundGroup
			// 
			this.BoundGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BoundGroup.Controls.Add(this.ShowBox);
			this.BoundGroup.Controls.Add(this.ShowSphere);
			this.BoundGroup.Controls.Add(this.BoundMesh);
			this.BoundGroup.Location = new System.Drawing.Point(419, 203);
			this.BoundGroup.Name = "BoundGroup";
			this.BoundGroup.Size = new System.Drawing.Size(98, 97);
			this.BoundGroup.TabIndex = 22;
			this.BoundGroup.TabStop = false;
			this.BoundGroup.Text = "Bounds";
			// 
			// ShowBox
			// 
			this.ShowBox.AutoSize = true;
			this.ShowBox.Location = new System.Drawing.Point(6, 73);
			this.ShowBox.Name = "ShowBox";
			this.ShowBox.Size = new System.Drawing.Size(74, 17);
			this.ShowBox.TabIndex = 2;
			this.ShowBox.Text = "Show Box";
			this.ShowBox.UseVisualStyleBackColor = true;
			this.ShowBox.CheckedChanged += new System.EventHandler(this.OnBoundShowBoxChanged);
			// 
			// ShowSphere
			// 
			this.ShowSphere.AutoSize = true;
			this.ShowSphere.Location = new System.Drawing.Point(6, 50);
			this.ShowSphere.Name = "ShowSphere";
			this.ShowSphere.Size = new System.Drawing.Size(90, 17);
			this.ShowSphere.TabIndex = 1;
			this.ShowSphere.Text = "Show Sphere";
			this.ShowSphere.UseVisualStyleBackColor = true;
			this.ShowSphere.CheckedChanged += new System.EventHandler(this.OnBoundShowSphereChanged);
			// 
			// BoundMesh
			// 
			this.BoundMesh.Location = new System.Drawing.Point(6, 19);
			this.BoundMesh.Name = "BoundMesh";
			this.BoundMesh.Size = new System.Drawing.Size(86, 25);
			this.BoundMesh.TabIndex = 0;
			this.BoundMesh.Text = "Calc Bound";
			this.BoundMesh.UseVisualStyleBackColor = true;
			this.BoundMesh.Click += new System.EventHandler(this.OnBoundMesh);
			// 
			// groupBox7
			// 
			this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox7.Controls.Add(this.Optimize);
			this.groupBox7.Controls.Add(this.Shadowize);
			this.groupBox7.Location = new System.Drawing.Point(419, 306);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(98, 81);
			this.groupBox7.TabIndex = 23;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "DirectX X Libs";
			// 
			// Optimize
			// 
			this.Optimize.Location = new System.Drawing.Point(6, 50);
			this.Optimize.Name = "Optimize";
			this.Optimize.Size = new System.Drawing.Size(86, 25);
			this.Optimize.TabIndex = 1;
			this.Optimize.Text = "Optimize";
			this.Optimize.UseVisualStyleBackColor = true;
			this.Optimize.Click += new System.EventHandler(this.OnOptimize);
			// 
			// Shadowize
			// 
			this.Shadowize.Location = new System.Drawing.Point(6, 19);
			this.Shadowize.Name = "Shadowize";
			this.Shadowize.Size = new System.Drawing.Size(86, 25);
			this.Shadowize.TabIndex = 0;
			this.Shadowize.Text = "Shadowize";
			this.Shadowize.UseVisualStyleBackColor = true;
			// 
			// BakeVerts
			// 
			this.BakeVerts.AutoSize = true;
			this.BakeVerts.Location = new System.Drawing.Point(6, 135);
			this.BakeVerts.Name = "BakeVerts";
			this.BakeVerts.Size = new System.Drawing.Size(106, 17);
			this.BakeVerts.TabIndex = 24;
			this.BakeVerts.Text = "Bake Transforms";
			this.BakeVerts.UseVisualStyleBackColor = true;
			// 
			// AnimForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(529, 442);
			this.ControlBox = false;
			this.Controls.Add(this.groupBox7);
			this.Controls.Add(this.BoundGroup);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.AnimGrid);
			this.MaximizeBox = false;
			this.Name = "AnimForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Animation";
			((System.ComponentModel.ISupportInitialize)(this.AnimGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TimeScale)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaxError)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.BoundGroup.ResumeLayout(false);
			this.BoundGroup.PerformLayout();
			this.groupBox7.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button LoadAnim;
		private System.Windows.Forms.Button LoadModel;
		private System.Windows.Forms.DataGridView AnimGrid;
		private System.Windows.Forms.NumericUpDown TimeScale;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button SaveAnimLib;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button SaveCharacter;
		private System.Windows.Forms.Button LoadCharacter;
		private System.Windows.Forms.Button ClearAll;
		private System.Windows.Forms.Button Compress;
		private System.Windows.Forms.NumericUpDown MaxError;
		private System.Windows.Forms.Button LoadStaticModel;
		private System.Windows.Forms.Button SaveStatic;
		private System.Windows.Forms.Button LoadStatic;
		private System.Windows.Forms.Button LoadMotionDat;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Button LoadBoneMap;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.GroupBox BoundGroup;
		private System.Windows.Forms.Button BoundMesh;
		private System.Windows.Forms.CheckBox ShowSphere;
		private System.Windows.Forms.CheckBox ShowBox;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.Button Optimize;
		private System.Windows.Forms.Button Shadowize;
		private System.Windows.Forms.CheckBox MaxConvert;
		private System.Windows.Forms.CheckBox BakeVerts;
	}
}