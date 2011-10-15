namespace VisServer
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
			this.VisGroup = new System.Windows.Forms.GroupBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.NumRetries = new System.Windows.Forms.NumericUpDown();
			this.VisGranularity = new System.Windows.Forms.NumericUpDown();
			this.DistributeVis = new System.Windows.Forms.CheckBox();
			this.SortPortals = new System.Windows.Forms.CheckBox();
			this.FullVis = new System.Windows.Forms.CheckBox();
			this.button1 = new System.Windows.Forms.Button();
			this.VisGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumRetries)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VisGranularity)).BeginInit();
			this.SuspendLayout();
			// 
			// LoadGBSP
			// 
			this.LoadGBSP.Location = new System.Drawing.Point(192, 12);
			this.LoadGBSP.Name = "LoadGBSP";
			this.LoadGBSP.Size = new System.Drawing.Size(75, 23);
			this.LoadGBSP.TabIndex = 0;
			this.LoadGBSP.Text = "Vis";
			this.LoadGBSP.UseVisualStyleBackColor = true;
			this.LoadGBSP.Click += new System.EventHandler(this.OnLoadGBSP);
			// 
			// ConsoleOut
			// 
			this.ConsoleOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleOut.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConsoleOut.Location = new System.Drawing.Point(12, 134);
			this.ConsoleOut.Multiline = true;
			this.ConsoleOut.Name = "ConsoleOut";
			this.ConsoleOut.ReadOnly = true;
			this.ConsoleOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConsoleOut.Size = new System.Drawing.Size(482, 264);
			this.ConsoleOut.TabIndex = 17;
			// 
			// Progress1
			// 
			this.Progress1.Location = new System.Drawing.Point(192, 109);
			this.Progress1.Name = "Progress1";
			this.Progress1.Size = new System.Drawing.Size(302, 19);
			this.Progress1.TabIndex = 24;
			// 
			// VisGroup
			// 
			this.VisGroup.Controls.Add(this.label12);
			this.VisGroup.Controls.Add(this.label11);
			this.VisGroup.Controls.Add(this.NumRetries);
			this.VisGroup.Controls.Add(this.VisGranularity);
			this.VisGroup.Controls.Add(this.DistributeVis);
			this.VisGroup.Controls.Add(this.SortPortals);
			this.VisGroup.Controls.Add(this.FullVis);
			this.VisGroup.Location = new System.Drawing.Point(12, 12);
			this.VisGroup.Name = "VisGroup";
			this.VisGroup.Size = new System.Drawing.Size(174, 116);
			this.VisGroup.TabIndex = 29;
			this.VisGroup.TabStop = false;
			this.VisGroup.Text = "Vis Settings";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(71, 47);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(40, 13);
			this.label12.TabIndex = 34;
			this.label12.Text = "Retries";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(71, 21);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(78, 13);
			this.label11.TabIndex = 33;
			this.label11.Text = "Dist Granularity";
			// 
			// NumRetries
			// 
			this.NumRetries.Location = new System.Drawing.Point(6, 45);
			this.NumRetries.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.NumRetries.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NumRetries.Name = "NumRetries";
			this.NumRetries.Size = new System.Drawing.Size(59, 20);
			this.NumRetries.TabIndex = 32;
			this.NumRetries.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			// 
			// VisGranularity
			// 
			this.VisGranularity.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.VisGranularity.Location = new System.Drawing.Point(6, 19);
			this.VisGranularity.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.VisGranularity.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.VisGranularity.Name = "VisGranularity";
			this.VisGranularity.Size = new System.Drawing.Size(59, 20);
			this.VisGranularity.TabIndex = 31;
			this.VisGranularity.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
			// 
			// DistributeVis
			// 
			this.DistributeVis.AutoSize = true;
			this.DistributeVis.Checked = true;
			this.DistributeVis.CheckState = System.Windows.Forms.CheckState.Checked;
			this.DistributeVis.Enabled = false;
			this.DistributeVis.Location = new System.Drawing.Point(6, 71);
			this.DistributeVis.Name = "DistributeVis";
			this.DistributeVis.Size = new System.Drawing.Size(70, 17);
			this.DistributeVis.TabIndex = 3;
			this.DistributeVis.Text = "Distribute";
			this.DistributeVis.UseVisualStyleBackColor = true;
			// 
			// SortPortals
			// 
			this.SortPortals.AutoSize = true;
			this.SortPortals.Checked = true;
			this.SortPortals.CheckState = System.Windows.Forms.CheckState.Checked;
			this.SortPortals.Location = new System.Drawing.Point(88, 93);
			this.SortPortals.Name = "SortPortals";
			this.SortPortals.Size = new System.Drawing.Size(80, 17);
			this.SortPortals.TabIndex = 2;
			this.SortPortals.Text = "Sort Portals";
			this.SortPortals.UseVisualStyleBackColor = true;
			// 
			// FullVis
			// 
			this.FullVis.AutoSize = true;
			this.FullVis.Checked = true;
			this.FullVis.CheckState = System.Windows.Forms.CheckState.Checked;
			this.FullVis.Enabled = false;
			this.FullVis.Location = new System.Drawing.Point(6, 93);
			this.FullVis.Name = "FullVis";
			this.FullVis.Size = new System.Drawing.Size(59, 17);
			this.FullVis.TabIndex = 0;
			this.FullVis.Text = "Full Vis";
			this.FullVis.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(192, 41);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(103, 23);
			this.button1.TabIndex = 30;
			this.button1.Text = "Query VisFarm";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnQueryVisFarm);
			// 
			// VisServer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(506, 410);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.VisGroup);
			this.Controls.Add(this.Progress1);
			this.Controls.Add(this.ConsoleOut);
			this.Controls.Add(this.LoadGBSP);
			this.Name = "VisServer";
			this.Text = "Vis Server";
			this.VisGroup.ResumeLayout(false);
			this.VisGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumRetries)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VisGranularity)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button LoadGBSP;
		private System.Windows.Forms.TextBox ConsoleOut;
		private System.Windows.Forms.ProgressBar Progress1;
		private System.Windows.Forms.GroupBox VisGroup;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown NumRetries;
		private System.Windows.Forms.NumericUpDown VisGranularity;
		private System.Windows.Forms.CheckBox DistributeVis;
		private System.Windows.Forms.CheckBox SortPortals;
		private System.Windows.Forms.CheckBox FullVis;
		private System.Windows.Forms.Button button1;
	}
}

