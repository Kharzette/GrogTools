namespace ColladaConvert
{
	partial class StripElements
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
			this.VertElements = new System.Windows.Forms.ListView();
			this.MeshName = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// VertElements
			// 
			this.VertElements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.VertElements.Location = new System.Drawing.Point(12, 38);
			this.VertElements.Name = "VertElements";
			this.VertElements.Size = new System.Drawing.Size(260, 211);
			this.VertElements.TabIndex = 0;
			this.VertElements.UseCompatibleStateImageBehavior = false;
			this.VertElements.View = System.Windows.Forms.View.List;
			this.VertElements.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnVertElementsKeyUp);
			// 
			// MeshName
			// 
			this.MeshName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MeshName.Location = new System.Drawing.Point(12, 12);
			this.MeshName.Name = "MeshName";
			this.MeshName.ReadOnly = true;
			this.MeshName.Size = new System.Drawing.Size(260, 20);
			this.MeshName.TabIndex = 1;
			// 
			// StripElements
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.ControlBox = false;
			this.Controls.Add(this.MeshName);
			this.Controls.Add(this.VertElements);
			this.Name = "StripElements";
			this.Text = "Strip Vertex Elements";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView VertElements;
		private System.Windows.Forms.TextBox MeshName;
	}
}