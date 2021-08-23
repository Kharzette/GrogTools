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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SaveAnimLib = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ReCollada = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.LoadCharacter = new System.Windows.Forms.Button();
            this.SaveCharacter = new System.Windows.Forms.Button();
            this.SaveStatic = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.CheckSkeleton = new System.Windows.Forms.CheckBox();
            this.LoadModel = new System.Windows.Forms.Button();
            this.LoadStaticModel = new System.Windows.Forms.Button();
            this.LoadAnim = new System.Windows.Forms.Button();
            this.DrawAxis = new System.Windows.Forms.CheckBox();
            this.BoundGroup = new System.Windows.Forms.GroupBox();
            this.ShowBox = new System.Windows.Forms.CheckBox();
            this.ShowSphere = new System.Windows.Forms.CheckBox();
            this.BoundMesh = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.AnimTimeScale = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.AnimList = new System.Windows.Forms.ListView();
            this.GroupUnits = new System.Windows.Forms.GroupBox();
            this.UnitsCentimeters = new System.Windows.Forms.RadioButton();
            this.UnitsMeters = new System.Windows.Forms.RadioButton();
            this.UnitsValve = new System.Windows.Forms.RadioButton();
            this.UnitsQuake = new System.Windows.Forms.RadioButton();
            this.UnitsGrog = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.BoundGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AnimTimeScale)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.GroupUnits.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.SaveAnimLib);
            this.groupBox1.Location = new System.Drawing.Point(445, 298);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(118, 97);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Anim Library";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(7, 22);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 29);
            this.button2.TabIndex = 6;
            this.button2.Text = "Load AnimLib";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnLoadAnimLib);
            // 
            // SaveAnimLib
            // 
            this.SaveAnimLib.Location = new System.Drawing.Point(7, 58);
            this.SaveAnimLib.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveAnimLib.Name = "SaveAnimLib";
            this.SaveAnimLib.Size = new System.Drawing.Size(104, 29);
            this.SaveAnimLib.TabIndex = 5;
            this.SaveAnimLib.Text = "Save AnimLib";
            this.SaveAnimLib.UseVisualStyleBackColor = true;
            this.SaveAnimLib.Click += new System.EventHandler(this.OnSaveAnimLib);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.ReCollada);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.LoadCharacter);
            this.groupBox2.Controls.Add(this.SaveCharacter);
            this.groupBox2.Controls.Add(this.SaveStatic);
            this.groupBox2.Location = new System.Drawing.Point(187, 238);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(128, 204);
            this.groupBox2.TabIndex = 18;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Converted Meshes";
            // 
            // ReCollada
            // 
            this.ReCollada.Location = new System.Drawing.Point(8, 166);
            this.ReCollada.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ReCollada.Name = "ReCollada";
            this.ReCollada.Size = new System.Drawing.Size(112, 29);
            this.ReCollada.TabIndex = 15;
            this.ReCollada.Text = "ReCollada";
            this.ReCollada.UseVisualStyleBackColor = true;
            this.ReCollada.Click += new System.EventHandler(this.OnReCollada);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 58);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 29);
            this.button1.TabIndex = 14;
            this.button1.Text = "Load Static";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnLoadStatic);
            // 
            // LoadCharacter
            // 
            this.LoadCharacter.Location = new System.Drawing.Point(7, 22);
            this.LoadCharacter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadCharacter.Name = "LoadCharacter";
            this.LoadCharacter.Size = new System.Drawing.Size(113, 29);
            this.LoadCharacter.TabIndex = 8;
            this.LoadCharacter.Text = "Load Character";
            this.LoadCharacter.UseVisualStyleBackColor = true;
            this.LoadCharacter.Click += new System.EventHandler(this.OnLoadCharacter);
            // 
            // SaveCharacter
            // 
            this.SaveCharacter.Location = new System.Drawing.Point(7, 93);
            this.SaveCharacter.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveCharacter.Name = "SaveCharacter";
            this.SaveCharacter.Size = new System.Drawing.Size(113, 29);
            this.SaveCharacter.TabIndex = 7;
            this.SaveCharacter.Text = "Save Character";
            this.SaveCharacter.UseVisualStyleBackColor = true;
            this.SaveCharacter.Click += new System.EventHandler(this.OnSaveCharacter);
            // 
            // SaveStatic
            // 
            this.SaveStatic.Location = new System.Drawing.Point(7, 129);
            this.SaveStatic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.SaveStatic.Name = "SaveStatic";
            this.SaveStatic.Size = new System.Drawing.Size(113, 29);
            this.SaveStatic.TabIndex = 13;
            this.SaveStatic.Text = "Save Static";
            this.SaveStatic.UseVisualStyleBackColor = true;
            this.SaveStatic.Click += new System.EventHandler(this.OnSaveStatic);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.CheckSkeleton);
            this.groupBox3.Controls.Add(this.LoadModel);
            this.groupBox3.Controls.Add(this.LoadStaticModel);
            this.groupBox3.Controls.Add(this.LoadAnim);
            this.groupBox3.Location = new System.Drawing.Point(13, 238);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(166, 158);
            this.groupBox3.TabIndex = 19;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Collada Files";
            // 
            // CheckSkeleton
            // 
            this.CheckSkeleton.AutoSize = true;
            this.CheckSkeleton.Location = new System.Drawing.Point(7, 129);
            this.CheckSkeleton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CheckSkeleton.Name = "CheckSkeleton";
            this.CheckSkeleton.Size = new System.Drawing.Size(107, 19);
            this.CheckSkeleton.TabIndex = 24;
            this.CheckSkeleton.Text = "Check Skeleton";
            this.CheckSkeleton.UseVisualStyleBackColor = true;
            // 
            // LoadModel
            // 
            this.LoadModel.Location = new System.Drawing.Point(7, 22);
            this.LoadModel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadModel.Name = "LoadModel";
            this.LoadModel.Size = new System.Drawing.Size(152, 29);
            this.LoadModel.TabIndex = 1;
            this.LoadModel.Text = "Load DAE Char Parts";
            this.LoadModel.UseVisualStyleBackColor = true;
            this.LoadModel.Click += new System.EventHandler(this.OnLoadCharacterDAE);
            // 
            // LoadStaticModel
            // 
            this.LoadStaticModel.Location = new System.Drawing.Point(7, 58);
            this.LoadStaticModel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadStaticModel.Name = "LoadStaticModel";
            this.LoadStaticModel.Size = new System.Drawing.Size(152, 29);
            this.LoadStaticModel.TabIndex = 12;
            this.LoadStaticModel.Text = "Load Static DAE";
            this.LoadStaticModel.UseVisualStyleBackColor = true;
            this.LoadStaticModel.Click += new System.EventHandler(this.OnOpenStaticDAE);
            // 
            // LoadAnim
            // 
            this.LoadAnim.Location = new System.Drawing.Point(7, 93);
            this.LoadAnim.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadAnim.Name = "LoadAnim";
            this.LoadAnim.Size = new System.Drawing.Size(152, 29);
            this.LoadAnim.TabIndex = 0;
            this.LoadAnim.Text = "Load Anim DAE";
            this.LoadAnim.UseVisualStyleBackColor = true;
            this.LoadAnim.Click += new System.EventHandler(this.OnLoadAnimDAE);
            // 
            // DrawAxis
            // 
            this.DrawAxis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DrawAxis.AutoSize = true;
            this.DrawAxis.Checked = true;
            this.DrawAxis.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DrawAxis.Location = new System.Drawing.Point(323, 416);
            this.DrawAxis.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DrawAxis.Name = "DrawAxis";
            this.DrawAxis.Size = new System.Drawing.Size(77, 19);
            this.DrawAxis.TabIndex = 25;
            this.DrawAxis.Text = "Draw Axis";
            this.DrawAxis.UseVisualStyleBackColor = true;
            // 
            // BoundGroup
            // 
            this.BoundGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BoundGroup.Controls.Add(this.ShowBox);
            this.BoundGroup.Controls.Add(this.ShowSphere);
            this.BoundGroup.Controls.Add(this.BoundMesh);
            this.BoundGroup.Location = new System.Drawing.Point(323, 298);
            this.BoundGroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BoundGroup.Name = "BoundGroup";
            this.BoundGroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BoundGroup.Size = new System.Drawing.Size(114, 112);
            this.BoundGroup.TabIndex = 23;
            this.BoundGroup.TabStop = false;
            this.BoundGroup.Text = "Bounds";
            // 
            // ShowBox
            // 
            this.ShowBox.AutoSize = true;
            this.ShowBox.Location = new System.Drawing.Point(7, 84);
            this.ShowBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowBox.Name = "ShowBox";
            this.ShowBox.Size = new System.Drawing.Size(77, 19);
            this.ShowBox.TabIndex = 2;
            this.ShowBox.Text = "Show Box";
            this.ShowBox.UseVisualStyleBackColor = true;
            this.ShowBox.CheckedChanged += new System.EventHandler(this.OnShowBoxChanged);
            // 
            // ShowSphere
            // 
            this.ShowSphere.AutoSize = true;
            this.ShowSphere.Location = new System.Drawing.Point(7, 58);
            this.ShowSphere.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ShowSphere.Name = "ShowSphere";
            this.ShowSphere.Size = new System.Drawing.Size(94, 19);
            this.ShowSphere.TabIndex = 1;
            this.ShowSphere.Text = "Show Sphere";
            this.ShowSphere.UseVisualStyleBackColor = true;
            this.ShowSphere.CheckedChanged += new System.EventHandler(this.OnShowSphereChanged);
            // 
            // BoundMesh
            // 
            this.BoundMesh.Location = new System.Drawing.Point(7, 22);
            this.BoundMesh.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BoundMesh.Name = "BoundMesh";
            this.BoundMesh.Size = new System.Drawing.Size(100, 29);
            this.BoundMesh.TabIndex = 0;
            this.BoundMesh.Text = "Calc Bound";
            this.BoundMesh.UseVisualStyleBackColor = true;
            this.BoundMesh.Click += new System.EventHandler(this.OnCalcBounds);
            // 
            // PauseButton
            // 
            this.PauseButton.Location = new System.Drawing.Point(150, 22);
            this.PauseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(64, 25);
            this.PauseButton.TabIndex = 5;
            this.PauseButton.Text = "Pause";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.OnPauseAnim);
            // 
            // AnimTimeScale
            // 
            this.AnimTimeScale.DecimalPlaces = 2;
            this.AnimTimeScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.AnimTimeScale.Location = new System.Drawing.Point(7, 22);
            this.AnimTimeScale.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AnimTimeScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.AnimTimeScale.Name = "AnimTimeScale";
            this.AnimTimeScale.Size = new System.Drawing.Size(59, 23);
            this.AnimTimeScale.TabIndex = 3;
            this.AnimTimeScale.Value = new decimal(new int[] {
            10,
            0,
            0,
            65536});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Time Scale";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.PauseButton);
            this.groupBox4.Controls.Add(this.AnimTimeScale);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Location = new System.Drawing.Point(323, 238);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(222, 54);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Playback";
            // 
            // AnimList
            // 
            this.AnimList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AnimList.GridLines = true;
            this.AnimList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.AnimList.HideSelection = false;
            this.AnimList.LabelEdit = true;
            this.AnimList.Location = new System.Drawing.Point(14, 14);
            this.AnimList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.AnimList.Name = "AnimList";
            this.AnimList.Size = new System.Drawing.Size(546, 218);
            this.AnimList.TabIndex = 24;
            this.AnimList.UseCompatibleStateImageBehavior = false;
            this.AnimList.View = System.Windows.Forms.View.Details;
            this.AnimList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.OnAnimRename);
            this.AnimList.SelectedIndexChanged += new System.EventHandler(this.OnAnimListSelectionChanged);
            this.AnimList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnAnimListKeyUp);
            // 
            // GroupUnits
            // 
            this.GroupUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GroupUnits.Controls.Add(this.UnitsCentimeters);
            this.GroupUnits.Controls.Add(this.UnitsMeters);
            this.GroupUnits.Controls.Add(this.UnitsValve);
            this.GroupUnits.Controls.Add(this.UnitsQuake);
            this.GroupUnits.Controls.Add(this.UnitsGrog);
            this.GroupUnits.Location = new System.Drawing.Point(12, 402);
            this.GroupUnits.Name = "GroupUnits";
            this.GroupUnits.Size = new System.Drawing.Size(168, 102);
            this.GroupUnits.TabIndex = 26;
            this.GroupUnits.TabStop = false;
            this.GroupUnits.Text = "Units";
            // 
            // UnitsCentimeters
            // 
            this.UnitsCentimeters.AutoSize = true;
            this.UnitsCentimeters.Location = new System.Drawing.Point(71, 47);
            this.UnitsCentimeters.Name = "UnitsCentimeters";
            this.UnitsCentimeters.Size = new System.Drawing.Size(89, 19);
            this.UnitsCentimeters.TabIndex = 4;
            this.UnitsCentimeters.Text = "Centimeters";
            this.UnitsCentimeters.UseVisualStyleBackColor = true;
            // 
            // UnitsMeters
            // 
            this.UnitsMeters.AutoSize = true;
            this.UnitsMeters.Location = new System.Drawing.Point(71, 22);
            this.UnitsMeters.Name = "UnitsMeters";
            this.UnitsMeters.Size = new System.Drawing.Size(61, 19);
            this.UnitsMeters.TabIndex = 3;
            this.UnitsMeters.Text = "Meters";
            this.UnitsMeters.UseVisualStyleBackColor = true;
            // 
            // UnitsValve
            // 
            this.UnitsValve.AutoSize = true;
            this.UnitsValve.Location = new System.Drawing.Point(6, 72);
            this.UnitsValve.Name = "UnitsValve";
            this.UnitsValve.Size = new System.Drawing.Size(53, 19);
            this.UnitsValve.TabIndex = 2;
            this.UnitsValve.Text = "Valve";
            this.UnitsValve.UseVisualStyleBackColor = true;
            // 
            // UnitsQuake
            // 
            this.UnitsQuake.AutoSize = true;
            this.UnitsQuake.Location = new System.Drawing.Point(6, 47);
            this.UnitsQuake.Name = "UnitsQuake";
            this.UnitsQuake.Size = new System.Drawing.Size(59, 19);
            this.UnitsQuake.TabIndex = 1;
            this.UnitsQuake.Text = "Quake";
            this.UnitsQuake.UseVisualStyleBackColor = true;
            // 
            // UnitsGrog
            // 
            this.UnitsGrog.AutoSize = true;
            this.UnitsGrog.Checked = true;
            this.UnitsGrog.Location = new System.Drawing.Point(6, 22);
            this.UnitsGrog.Name = "UnitsGrog";
            this.UnitsGrog.Size = new System.Drawing.Size(51, 19);
            this.UnitsGrog.TabIndex = 0;
            this.UnitsGrog.TabStop = true;
            this.UnitsGrog.Text = "Grog";
            this.UnitsGrog.UseVisualStyleBackColor = true;
            // 
            // AnimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 516);
            this.ControlBox = false;
            this.Controls.Add(this.GroupUnits);
            this.Controls.Add(this.DrawAxis);
            this.Controls.Add(this.AnimList);
            this.Controls.Add(this.BoundGroup);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnimForm";
            this.ShowInTaskbar = false;
            this.Text = "Animation Stuff";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.BoundGroup.ResumeLayout(false);
            this.BoundGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AnimTimeScale)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.GroupUnits.ResumeLayout(false);
            this.GroupUnits.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button SaveAnimLib;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button LoadCharacter;
		private System.Windows.Forms.Button SaveCharacter;
		private System.Windows.Forms.Button SaveStatic;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox CheckSkeleton;
		private System.Windows.Forms.Button LoadModel;
		private System.Windows.Forms.Button LoadStaticModel;
		private System.Windows.Forms.Button LoadAnim;
		private System.Windows.Forms.GroupBox BoundGroup;
		private System.Windows.Forms.CheckBox ShowBox;
		private System.Windows.Forms.CheckBox ShowSphere;
		private System.Windows.Forms.Button BoundMesh;
		private System.Windows.Forms.Button PauseButton;
		private System.Windows.Forms.NumericUpDown AnimTimeScale;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox DrawAxis;
		private System.Windows.Forms.ListView AnimList;
		private System.Windows.Forms.Button ReCollada;
        private System.Windows.Forms.GroupBox GroupUnits;
        private System.Windows.Forms.RadioButton UnitsCentimeters;
        private System.Windows.Forms.RadioButton UnitsMeters;
        private System.Windows.Forms.RadioButton UnitsValve;
        private System.Windows.Forms.RadioButton UnitsQuake;
        private System.Windows.Forms.RadioButton UnitsGrog;
    }
}

