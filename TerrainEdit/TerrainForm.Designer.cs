namespace TerrainEdit
{
	partial class TerrainForm
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
			this.GridSize = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.ChunkSize = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.MedianHeight = new System.Windows.Forms.NumericUpDown();
			this.Variance = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.Evaporation = new System.Windows.Forms.NumericUpDown();
			this.Solubility = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.RainFall = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.ErosionIterations = new System.Windows.Forms.NumericUpDown();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label11 = new System.Windows.Forms.Label();
			this.PolySize = new System.Windows.Forms.NumericUpDown();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label10 = new System.Windows.Forms.Label();
			this.BorderSize = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.TileIterations = new System.Windows.Forms.NumericUpDown();
			this.Build = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.GridSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChunkSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MedianHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Variance)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Evaporation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Solubility)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RainFall)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErosionIterations)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PolySize)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BorderSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TileIterations)).BeginInit();
			this.SuspendLayout();
			// 
			// GridSize
			// 
			this.GridSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.GridSize.Location = new System.Drawing.Point(6, 19);
			this.GridSize.Maximum = new decimal(new int[] {
            16384,
            0,
            0,
            0});
			this.GridSize.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.GridSize.Name = "GridSize";
			this.GridSize.Size = new System.Drawing.Size(72, 20);
			this.GridSize.TabIndex = 0;
			this.GridSize.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(84, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Grid Size";
			// 
			// ChunkSize
			// 
			this.ChunkSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.ChunkSize.Location = new System.Drawing.Point(6, 45);
			this.ChunkSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.ChunkSize.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.ChunkSize.Name = "ChunkSize";
			this.ChunkSize.Size = new System.Drawing.Size(72, 20);
			this.ChunkSize.TabIndex = 2;
			this.ChunkSize.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(84, 47);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Chunk Size";
			// 
			// MedianHeight
			// 
			this.MedianHeight.Location = new System.Drawing.Point(6, 71);
			this.MedianHeight.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.MedianHeight.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            -2147483648});
			this.MedianHeight.Name = "MedianHeight";
			this.MedianHeight.Size = new System.Drawing.Size(72, 20);
			this.MedianHeight.TabIndex = 4;
			this.MedianHeight.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// Variance
			// 
			this.Variance.DecimalPlaces = 2;
			this.Variance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.Variance.Location = new System.Drawing.Point(6, 97);
			this.Variance.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.Variance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.Variance.Name = "Variance";
			this.Variance.Size = new System.Drawing.Size(72, 20);
			this.Variance.TabIndex = 5;
			this.Variance.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(84, 73);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Median Height";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(84, 99);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(49, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Variance";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.Evaporation);
			this.groupBox1.Controls.Add(this.Solubility);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.RainFall);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.ErosionIterations);
			this.groupBox1.Location = new System.Drawing.Point(182, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(154, 127);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Erosion";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(84, 99);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(64, 13);
			this.label8.TabIndex = 14;
			this.label8.Text = "Evaporation";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(84, 73);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(48, 13);
			this.label7.TabIndex = 13;
			this.label7.Text = "Solubility";
			// 
			// Evaporation
			// 
			this.Evaporation.DecimalPlaces = 2;
			this.Evaporation.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.Evaporation.Location = new System.Drawing.Point(6, 97);
			this.Evaporation.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.Evaporation.Name = "Evaporation";
			this.Evaporation.Size = new System.Drawing.Size(72, 20);
			this.Evaporation.TabIndex = 12;
			this.Evaporation.Value = new decimal(new int[] {
            15,
            0,
            0,
            131072});
			// 
			// Solubility
			// 
			this.Solubility.DecimalPlaces = 2;
			this.Solubility.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.Solubility.Location = new System.Drawing.Point(6, 71);
			this.Solubility.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.Solubility.Name = "Solubility";
			this.Solubility.Size = new System.Drawing.Size(72, 20);
			this.Solubility.TabIndex = 11;
			this.Solubility.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(84, 47);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(42, 13);
			this.label6.TabIndex = 10;
			this.label6.Text = "Rainfall";
			// 
			// RainFall
			// 
			this.RainFall.DecimalPlaces = 1;
			this.RainFall.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.RainFall.Location = new System.Drawing.Point(6, 45);
			this.RainFall.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.RainFall.Name = "RainFall";
			this.RainFall.Size = new System.Drawing.Size(72, 20);
			this.RainFall.TabIndex = 9;
			this.RainFall.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(84, 21);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(50, 13);
			this.label5.TabIndex = 8;
			this.label5.Text = "Iterations";
			// 
			// ErosionIterations
			// 
			this.ErosionIterations.Location = new System.Drawing.Point(6, 19);
			this.ErosionIterations.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.ErosionIterations.Name = "ErosionIterations";
			this.ErosionIterations.Size = new System.Drawing.Size(72, 20);
			this.ErosionIterations.TabIndex = 6;
			this.ErosionIterations.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.PolySize);
			this.groupBox2.Controls.Add(this.GridSize);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.ChunkSize);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.MedianHeight);
			this.groupBox2.Controls.Add(this.Variance);
			this.groupBox2.Location = new System.Drawing.Point(12, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(164, 153);
			this.groupBox2.TabIndex = 10;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Dimensions";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(84, 125);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(50, 13);
			this.label11.TabIndex = 9;
			this.label11.Text = "Poly Size";
			// 
			// PolySize
			// 
			this.PolySize.Location = new System.Drawing.Point(6, 123);
			this.PolySize.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
			this.PolySize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.PolySize.Name = "PolySize";
			this.PolySize.Size = new System.Drawing.Size(72, 20);
			this.PolySize.TabIndex = 8;
			this.PolySize.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.BorderSize);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.TileIterations);
			this.groupBox3.Location = new System.Drawing.Point(12, 171);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(164, 75);
			this.groupBox3.TabIndex = 11;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Tiling";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(84, 47);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(61, 13);
			this.label10.TabIndex = 16;
			this.label10.Text = "Border Size";
			// 
			// BorderSize
			// 
			this.BorderSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.BorderSize.Location = new System.Drawing.Point(6, 45);
			this.BorderSize.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			this.BorderSize.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.BorderSize.Name = "BorderSize";
			this.BorderSize.Size = new System.Drawing.Size(72, 20);
			this.BorderSize.TabIndex = 15;
			this.BorderSize.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(84, 21);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(50, 13);
			this.label9.TabIndex = 14;
			this.label9.Text = "Iterations";
			// 
			// TileIterations
			// 
			this.TileIterations.Location = new System.Drawing.Point(6, 19);
			this.TileIterations.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.TileIterations.Name = "TileIterations";
			this.TileIterations.Size = new System.Drawing.Size(72, 20);
			this.TileIterations.TabIndex = 13;
			this.TileIterations.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// Build
			// 
			this.Build.Location = new System.Drawing.Point(233, 171);
			this.Build.Name = "Build";
			this.Build.Size = new System.Drawing.Size(75, 23);
			this.Build.TabIndex = 12;
			this.Build.Text = "Build";
			this.Build.UseVisualStyleBackColor = true;
			this.Build.Click += new System.EventHandler(this.OnBuild);
			// 
			// TerrainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(347, 258);
			this.ControlBox = false;
			this.Controls.Add(this.Build);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "TerrainForm";
			this.Text = "Terrain Construction";
			((System.ComponentModel.ISupportInitialize)(this.GridSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChunkSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MedianHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Variance)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Evaporation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Solubility)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RainFall)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErosionIterations)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PolySize)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BorderSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TileIterations)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown GridSize;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown ChunkSize;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown MedianHeight;
		private System.Windows.Forms.NumericUpDown Variance;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown Evaporation;
		private System.Windows.Forms.NumericUpDown Solubility;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown RainFall;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown ErosionIterations;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown BorderSize;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown TileIterations;
		private System.Windows.Forms.Button Build;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown PolySize;
	}
}