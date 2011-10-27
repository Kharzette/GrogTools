﻿namespace VisServer
{
	partial class VisServer
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
			this.LoadGBSP = new System.Windows.Forms.Button();
			this.ConsoleOut = new System.Windows.Forms.TextBox();
			this.Progress1 = new System.Windows.Forms.ProgressBar();
			this.QueryVisFarm = new System.Windows.Forms.Button();
			this.ReloadBuildFarm = new System.Windows.Forms.Button();
			this.Stop = new System.Windows.Forms.Button();
			this.StatusBottom = new System.Windows.Forms.StatusStrip();
			this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.ResumeVis = new System.Windows.Forms.Button();
			this.StatusBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// LoadGBSP
			// 
			this.LoadGBSP.Location = new System.Drawing.Point(12, 12);
			this.LoadGBSP.Name = "LoadGBSP";
			this.LoadGBSP.Size = new System.Drawing.Size(90, 23);
			this.LoadGBSP.TabIndex = 0;
			this.LoadGBSP.Text = "Vis GBSP File";
			this.LoadGBSP.UseVisualStyleBackColor = true;
			this.LoadGBSP.Click += new System.EventHandler(this.OnLoadGBSP);
			// 
			// ConsoleOut
			// 
			this.ConsoleOut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleOut.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConsoleOut.Location = new System.Drawing.Point(12, 66);
			this.ConsoleOut.Multiline = true;
			this.ConsoleOut.Name = "ConsoleOut";
			this.ConsoleOut.ReadOnly = true;
			this.ConsoleOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConsoleOut.Size = new System.Drawing.Size(697, 223);
			this.ConsoleOut.TabIndex = 17;
			// 
			// Progress1
			// 
			this.Progress1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Progress1.Location = new System.Drawing.Point(12, 41);
			this.Progress1.Name = "Progress1";
			this.Progress1.Size = new System.Drawing.Size(697, 19);
			this.Progress1.TabIndex = 24;
			// 
			// QueryVisFarm
			// 
			this.QueryVisFarm.Location = new System.Drawing.Point(195, 12);
			this.QueryVisFarm.Name = "QueryVisFarm";
			this.QueryVisFarm.Size = new System.Drawing.Size(95, 23);
			this.QueryVisFarm.TabIndex = 30;
			this.QueryVisFarm.Text = "Query VisFarm";
			this.QueryVisFarm.UseVisualStyleBackColor = true;
			this.QueryVisFarm.Click += new System.EventHandler(this.OnQueryVisFarm);
			// 
			// ReloadBuildFarm
			// 
			this.ReloadBuildFarm.Location = new System.Drawing.Point(296, 12);
			this.ReloadBuildFarm.Name = "ReloadBuildFarm";
			this.ReloadBuildFarm.Size = new System.Drawing.Size(87, 23);
			this.ReloadBuildFarm.TabIndex = 31;
			this.ReloadBuildFarm.Text = "ReLoad Farm";
			this.ReloadBuildFarm.UseVisualStyleBackColor = true;
			this.ReloadBuildFarm.Click += new System.EventHandler(this.OnReLoadBuildFarm);
			// 
			// Stop
			// 
			this.Stop.Location = new System.Drawing.Point(389, 12);
			this.Stop.Name = "Stop";
			this.Stop.Size = new System.Drawing.Size(44, 23);
			this.Stop.TabIndex = 32;
			this.Stop.Text = "Stop";
			this.Stop.UseVisualStyleBackColor = true;
			// 
			// StatusBottom
			// 
			this.StatusBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
			this.StatusBottom.Location = new System.Drawing.Point(0, 292);
			this.StatusBottom.Name = "StatusBottom";
			this.StatusBottom.Size = new System.Drawing.Size(721, 22);
			this.StatusBottom.TabIndex = 33;
			this.StatusBottom.Text = "statusStrip1";
			// 
			// StatusLabel
			// 
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = new System.Drawing.Size(26, 17);
			this.StatusLabel.Text = "Idle";
			// 
			// ResumeVis
			// 
			this.ResumeVis.Location = new System.Drawing.Point(108, 12);
			this.ResumeVis.Name = "ResumeVis";
			this.ResumeVis.Size = new System.Drawing.Size(81, 23);
			this.ResumeVis.TabIndex = 34;
			this.ResumeVis.Text = "Resume Vis";
			this.ResumeVis.UseVisualStyleBackColor = true;
			// 
			// VisServer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(721, 314);
			this.Controls.Add(this.ResumeVis);
			this.Controls.Add(this.StatusBottom);
			this.Controls.Add(this.Stop);
			this.Controls.Add(this.ReloadBuildFarm);
			this.Controls.Add(this.QueryVisFarm);
			this.Controls.Add(this.Progress1);
			this.Controls.Add(this.ConsoleOut);
			this.Controls.Add(this.LoadGBSP);
			this.Name = "VisServer";
			this.Text = "Vis Server";
			this.StatusBottom.ResumeLayout(false);
			this.StatusBottom.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button LoadGBSP;
		private System.Windows.Forms.TextBox ConsoleOut;
		private System.Windows.Forms.ProgressBar Progress1;
		private System.Windows.Forms.Button QueryVisFarm;
		private System.Windows.Forms.Button ReloadBuildFarm;
		private System.Windows.Forms.Button Stop;
		private System.Windows.Forms.StatusStrip StatusBottom;
		private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
		private System.Windows.Forms.Button ResumeVis;
	}
}

