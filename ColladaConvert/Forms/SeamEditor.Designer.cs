namespace ColladaConvert
{
	partial class SeamEditor
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
			this.WeightFirst = new System.Windows.Forms.Button();
			this.WeightSecond = new System.Windows.Forms.Button();
			this.WeightAverage = new System.Windows.Forms.Button();
			this.SeamList = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// WeightFirst
			// 
			this.WeightFirst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.WeightFirst.Location = new System.Drawing.Point(12, 226);
			this.WeightFirst.Name = "WeightFirst";
			this.WeightFirst.Size = new System.Drawing.Size(75, 23);
			this.WeightFirst.TabIndex = 0;
			this.WeightFirst.Text = "Use First";
			this.WeightFirst.UseVisualStyleBackColor = true;
			this.WeightFirst.Click += new System.EventHandler(this.OnUseFirst);
			// 
			// WeightSecond
			// 
			this.WeightSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.WeightSecond.Location = new System.Drawing.Point(93, 226);
			this.WeightSecond.Name = "WeightSecond";
			this.WeightSecond.Size = new System.Drawing.Size(75, 23);
			this.WeightSecond.TabIndex = 1;
			this.WeightSecond.Text = "Use Second";
			this.WeightSecond.UseVisualStyleBackColor = true;
			this.WeightSecond.Click += new System.EventHandler(this.OnUseSecond);
			// 
			// WeightAverage
			// 
			this.WeightAverage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.WeightAverage.Location = new System.Drawing.Point(174, 226);
			this.WeightAverage.Name = "WeightAverage";
			this.WeightAverage.Size = new System.Drawing.Size(75, 23);
			this.WeightAverage.TabIndex = 2;
			this.WeightAverage.Text = "Average";
			this.WeightAverage.UseVisualStyleBackColor = true;
			this.WeightAverage.Click += new System.EventHandler(this.OnAverage);
			// 
			// SeamList
			// 
			this.SeamList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SeamList.FullRowSelect = true;
			this.SeamList.GridLines = true;
			this.SeamList.Location = new System.Drawing.Point(12, 12);
			this.SeamList.Name = "SeamList";
			this.SeamList.Size = new System.Drawing.Size(260, 208);
			this.SeamList.TabIndex = 3;
			this.SeamList.UseCompatibleStateImageBehavior = false;
			this.SeamList.View = System.Windows.Forms.View.Details;
			// 
			// SeamEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.SeamList);
			this.Controls.Add(this.WeightAverage);
			this.Controls.Add(this.WeightSecond);
			this.Controls.Add(this.WeightFirst);
			this.Name = "SeamEditor";
			this.Text = "Seam Editor";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button WeightFirst;
		private System.Windows.Forms.Button WeightSecond;
		private System.Windows.Forms.Button WeightAverage;
		private System.Windows.Forms.ListView SeamList;
	}
}