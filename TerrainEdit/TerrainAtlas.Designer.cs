namespace TerrainEdit
{
	partial class TerrainAtlas
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
			this.AtlasPic01 = new System.Windows.Forms.PictureBox();
			this.AtlasGrid = new System.Windows.Forms.DataGridView();
			this.AtlasX = new System.Windows.Forms.NumericUpDown();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.AtlasY = new System.Windows.Forms.NumericUpDown();
			this.ReBuildAtlas = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.AtlasPic01)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AtlasGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AtlasX)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AtlasY)).BeginInit();
			this.SuspendLayout();
			// 
			// AtlasPic01
			// 
			this.AtlasPic01.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AtlasPic01.Location = new System.Drawing.Point(12, 12);
			this.AtlasPic01.Name = "AtlasPic01";
			this.AtlasPic01.Size = new System.Drawing.Size(455, 242);
			this.AtlasPic01.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.AtlasPic01.TabIndex = 0;
			this.AtlasPic01.TabStop = false;
			// 
			// AtlasGrid
			// 
			this.AtlasGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AtlasGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.AtlasGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.AtlasGrid.Location = new System.Drawing.Point(12, 256);
			this.AtlasGrid.Name = "AtlasGrid";
			this.AtlasGrid.Size = new System.Drawing.Size(551, 171);
			this.AtlasGrid.TabIndex = 1;
			// 
			// AtlasX
			// 
			this.AtlasX.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AtlasX.Location = new System.Drawing.Point(6, 19);
			this.AtlasX.Maximum = new decimal(new int[] {
            8192,
            0,
            0,
            0});
			this.AtlasX.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.AtlasX.Name = "AtlasX";
			this.AtlasX.Size = new System.Drawing.Size(76, 20);
			this.AtlasX.TabIndex = 2;
			this.AtlasX.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.AtlasY);
			this.groupBox1.Controls.Add(this.AtlasX);
			this.groupBox1.Location = new System.Drawing.Point(473, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(90, 78);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Atlas Size";
			// 
			// AtlasY
			// 
			this.AtlasY.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.AtlasY.Location = new System.Drawing.Point(6, 45);
			this.AtlasY.Maximum = new decimal(new int[] {
            8192,
            0,
            0,
            0});
			this.AtlasY.Minimum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.AtlasY.Name = "AtlasY";
			this.AtlasY.Size = new System.Drawing.Size(76, 20);
			this.AtlasY.TabIndex = 3;
			this.AtlasY.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
			// 
			// ReBuildAtlas
			// 
			this.ReBuildAtlas.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ReBuildAtlas.Location = new System.Drawing.Point(473, 96);
			this.ReBuildAtlas.Name = "ReBuildAtlas";
			this.ReBuildAtlas.Size = new System.Drawing.Size(90, 23);
			this.ReBuildAtlas.TabIndex = 4;
			this.ReBuildAtlas.Text = "Rebuild";
			this.ReBuildAtlas.UseVisualStyleBackColor = true;
			this.ReBuildAtlas.Click += new System.EventHandler(this.OnReBuildAtlas);
			// 
			// TerrainAtlas
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(575, 439);
			this.ControlBox = false;
			this.Controls.Add(this.ReBuildAtlas);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.AtlasGrid);
			this.Controls.Add(this.AtlasPic01);
			this.MinimumSize = new System.Drawing.Size(300, 360);
			this.Name = "TerrainAtlas";
			this.Text = "Terrain Texturing Stuff";
			((System.ComponentModel.ISupportInitialize)(this.AtlasPic01)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AtlasGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AtlasX)).EndInit();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AtlasY)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox AtlasPic01;
		private System.Windows.Forms.DataGridView AtlasGrid;
		private System.Windows.Forms.NumericUpDown AtlasX;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown AtlasY;
		private System.Windows.Forms.Button ReBuildAtlas;
	}
}