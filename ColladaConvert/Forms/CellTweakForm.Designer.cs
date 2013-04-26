namespace ColladaConvert
{
	partial class CellTweakForm
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
			this.CellTweakGrid = new System.Windows.Forms.DataGridView();
			this.ApplyShading = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.CellTweakGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// CellTweakGrid
			// 
			this.CellTweakGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CellTweakGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.CellTweakGrid.Location = new System.Drawing.Point(12, 12);
			this.CellTweakGrid.Name = "CellTweakGrid";
			this.CellTweakGrid.Size = new System.Drawing.Size(260, 208);
			this.CellTweakGrid.TabIndex = 0;
			// 
			// ApplyShading
			// 
			this.ApplyShading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ApplyShading.Location = new System.Drawing.Point(13, 226);
			this.ApplyShading.Name = "ApplyShading";
			this.ApplyShading.Size = new System.Drawing.Size(75, 23);
			this.ApplyShading.TabIndex = 1;
			this.ApplyShading.Text = "Apply";
			this.ApplyShading.UseVisualStyleBackColor = true;
			this.ApplyShading.Click += new System.EventHandler(this.OnApplyShading);
			// 
			// CellTweakForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.ControlBox = false;
			this.Controls.Add(this.ApplyShading);
			this.Controls.Add(this.CellTweakGrid);
			this.Name = "CellTweakForm";
			this.Text = "CellTweakForm";
			((System.ComponentModel.ISupportInitialize)(this.CellTweakGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView CellTweakGrid;
		private System.Windows.Forms.Button ApplyShading;
	}
}