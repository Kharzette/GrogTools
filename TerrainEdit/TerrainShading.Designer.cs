namespace TerrainEdit
{
	partial class TerrainShading
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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.FogEnd = new System.Windows.Forms.NumericUpDown();
			this.FogStart = new System.Windows.Forms.NumericUpDown();
			this.FogEnabled = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.SkyColor1 = new System.Windows.Forms.Panel();
			this.SkyColor0 = new System.Windows.Forms.Panel();
			this.Apply = new System.Windows.Forms.Button();
			this.ChunkRange = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FogEnd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FogStart)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChunkRange)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.FogEnd);
			this.groupBox2.Controls.Add(this.FogStart);
			this.groupBox2.Controls.Add(this.FogEnabled);
			this.groupBox2.Location = new System.Drawing.Point(12, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(163, 101);
			this.groupBox2.TabIndex = 8;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Fog";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(77, 70);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "End Distance";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(77, 44);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Start Distance";
			// 
			// FogEnd
			// 
			this.FogEnd.Location = new System.Drawing.Point(6, 68);
			this.FogEnd.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.FogEnd.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.FogEnd.Name = "FogEnd";
			this.FogEnd.Size = new System.Drawing.Size(65, 20);
			this.FogEnd.TabIndex = 2;
			this.FogEnd.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
			// 
			// FogStart
			// 
			this.FogStart.Location = new System.Drawing.Point(6, 42);
			this.FogStart.Maximum = new decimal(new int[] {
            8192,
            0,
            0,
            0});
			this.FogStart.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.FogStart.Name = "FogStart";
			this.FogStart.Size = new System.Drawing.Size(65, 20);
			this.FogStart.TabIndex = 1;
			this.FogStart.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// FogEnabled
			// 
			this.FogEnabled.AutoSize = true;
			this.FogEnabled.Checked = true;
			this.FogEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.FogEnabled.Location = new System.Drawing.Point(6, 19);
			this.FogEnabled.Name = "FogEnabled";
			this.FogEnabled.Size = new System.Drawing.Size(65, 17);
			this.FogEnabled.TabIndex = 0;
			this.FogEnabled.Text = "Enabled";
			this.FogEnabled.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.SkyColor1);
			this.groupBox1.Controls.Add(this.SkyColor0);
			this.groupBox1.Location = new System.Drawing.Point(12, 119);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(94, 107);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Sky";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(44, 57);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(37, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Color1";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(44, 19);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(37, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Color0";
			// 
			// SkyColor1
			// 
			this.SkyColor1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SkyColor1.Location = new System.Drawing.Point(6, 57);
			this.SkyColor1.Name = "SkyColor1";
			this.SkyColor1.Size = new System.Drawing.Size(32, 32);
			this.SkyColor1.TabIndex = 5;
			this.SkyColor1.Click += new System.EventHandler(this.OnColor1Clicked);
			// 
			// SkyColor0
			// 
			this.SkyColor0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SkyColor0.Location = new System.Drawing.Point(6, 19);
			this.SkyColor0.Name = "SkyColor0";
			this.SkyColor0.Size = new System.Drawing.Size(32, 32);
			this.SkyColor0.TabIndex = 4;
			this.SkyColor0.Click += new System.EventHandler(this.OnColor0Clicked);
			// 
			// Apply
			// 
			this.Apply.Location = new System.Drawing.Point(112, 203);
			this.Apply.Name = "Apply";
			this.Apply.Size = new System.Drawing.Size(63, 23);
			this.Apply.TabIndex = 10;
			this.Apply.Text = "Apply";
			this.Apply.UseVisualStyleBackColor = true;
			this.Apply.Click += new System.EventHandler(this.OnApply);
			// 
			// ChunkRange
			// 
			this.ChunkRange.Location = new System.Drawing.Point(112, 135);
			this.ChunkRange.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.ChunkRange.Name = "ChunkRange";
			this.ChunkRange.Size = new System.Drawing.Size(50, 20);
			this.ChunkRange.TabIndex = 11;
			this.ChunkRange.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(112, 119);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(73, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Chunk Range";
			// 
			// TerrainShading
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(188, 234);
			this.ControlBox = false;
			this.Controls.Add(this.label6);
			this.Controls.Add(this.ChunkRange);
			this.Controls.Add(this.Apply);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox2);
			this.Name = "TerrainShading";
			this.Text = "Shading";
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FogEnd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FogStart)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChunkRange)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.NumericUpDown FogEnd;
		private System.Windows.Forms.NumericUpDown FogStart;
		private System.Windows.Forms.CheckBox FogEnabled;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel SkyColor1;
		private System.Windows.Forms.Panel SkyColor0;
		private System.Windows.Forms.Button Apply;
		private System.Windows.Forms.NumericUpDown ChunkRange;
		private System.Windows.Forms.Label label6;
	}
}