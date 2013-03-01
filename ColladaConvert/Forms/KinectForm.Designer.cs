namespace ColladaConvert
{
	partial class KinectForm
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
			this.KinectBones = new System.Windows.Forms.DataGridView();
			this.CharBones = new System.Windows.Forms.DataGridView();
			this.AssignBone = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.LoadData = new System.Windows.Forms.Button();
			this.SaveData = new System.Windows.Forms.Button();
			this.ConvertToAnim = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.TotalTime = new System.Windows.Forms.TextBox();
			this.NumFrames = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SaveMap = new System.Windows.Forms.Button();
			this.LoadMap = new System.Windows.Forms.Button();
			this.TrimStart = new System.Windows.Forms.Button();
			this.TrimEnd = new System.Windows.Forms.Button();
			this.TrimAmount = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.KinectBones)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CharBones)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TrimAmount)).BeginInit();
			this.SuspendLayout();
			// 
			// KinectBones
			// 
			this.KinectBones.AllowUserToAddRows = false;
			this.KinectBones.AllowUserToDeleteRows = false;
			this.KinectBones.AllowUserToResizeRows = false;
			this.KinectBones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.KinectBones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.KinectBones.Location = new System.Drawing.Point(12, 12);
			this.KinectBones.MultiSelect = false;
			this.KinectBones.Name = "KinectBones";
			this.KinectBones.RowHeadersVisible = false;
			this.KinectBones.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.KinectBones.Size = new System.Drawing.Size(303, 242);
			this.KinectBones.TabIndex = 0;
			// 
			// CharBones
			// 
			this.CharBones.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CharBones.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.CharBones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.CharBones.Location = new System.Drawing.Point(321, 12);
			this.CharBones.Name = "CharBones";
			this.CharBones.ReadOnly = true;
			this.CharBones.RowHeadersVisible = false;
			this.CharBones.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.CharBones.Size = new System.Drawing.Size(174, 242);
			this.CharBones.TabIndex = 1;
			// 
			// AssignBone
			// 
			this.AssignBone.Location = new System.Drawing.Point(321, 260);
			this.AssignBone.Name = "AssignBone";
			this.AssignBone.Size = new System.Drawing.Size(125, 23);
			this.AssignBone.TabIndex = 2;
			this.AssignBone.Text = "<- Assign Char Bone";
			this.AssignBone.UseVisualStyleBackColor = true;
			this.AssignBone.Click += new System.EventHandler(this.OnAssignBone);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.TrimAmount);
			this.groupBox1.Controls.Add(this.TrimEnd);
			this.groupBox1.Controls.Add(this.TrimStart);
			this.groupBox1.Controls.Add(this.LoadData);
			this.groupBox1.Controls.Add(this.SaveData);
			this.groupBox1.Controls.Add(this.ConvertToAnim);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.TotalTime);
			this.groupBox1.Controls.Add(this.NumFrames);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 290);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(391, 138);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Captured Data";
			// 
			// LoadData
			// 
			this.LoadData.Location = new System.Drawing.Point(6, 48);
			this.LoadData.Name = "LoadData";
			this.LoadData.Size = new System.Drawing.Size(75, 23);
			this.LoadData.TabIndex = 7;
			this.LoadData.Text = "Load Data";
			this.LoadData.UseVisualStyleBackColor = true;
			this.LoadData.Click += new System.EventHandler(this.OnLoadData);
			// 
			// SaveData
			// 
			this.SaveData.Location = new System.Drawing.Point(6, 19);
			this.SaveData.Name = "SaveData";
			this.SaveData.Size = new System.Drawing.Size(75, 23);
			this.SaveData.TabIndex = 6;
			this.SaveData.Text = "Save Data";
			this.SaveData.UseVisualStyleBackColor = true;
			this.SaveData.Click += new System.EventHandler(this.OnSaveData);
			// 
			// ConvertToAnim
			// 
			this.ConvertToAnim.Location = new System.Drawing.Point(273, 48);
			this.ConvertToAnim.Name = "ConvertToAnim";
			this.ConvertToAnim.Size = new System.Drawing.Size(111, 23);
			this.ConvertToAnim.TabIndex = 5;
			this.ConvertToAnim.Text = "Convert to Anim";
			this.ConvertToAnim.UseVisualStyleBackColor = true;
			this.ConvertToAnim.Click += new System.EventHandler(this.OnConvertToAnim);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(273, 19);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(111, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "Toggle Recording";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnToggleRecording);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(101, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(57, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Total Time";
			// 
			// TotalTime
			// 
			this.TotalTime.Location = new System.Drawing.Point(164, 45);
			this.TotalTime.Name = "TotalTime";
			this.TotalTime.ReadOnly = true;
			this.TotalTime.Size = new System.Drawing.Size(84, 20);
			this.TotalTime.TabIndex = 2;
			// 
			// NumFrames
			// 
			this.NumFrames.Location = new System.Drawing.Point(164, 19);
			this.NumFrames.Name = "NumFrames";
			this.NumFrames.ReadOnly = true;
			this.NumFrames.Size = new System.Drawing.Size(84, 20);
			this.NumFrames.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(117, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Frames";
			// 
			// SaveMap
			// 
			this.SaveMap.Location = new System.Drawing.Point(13, 261);
			this.SaveMap.Name = "SaveMap";
			this.SaveMap.Size = new System.Drawing.Size(75, 23);
			this.SaveMap.TabIndex = 4;
			this.SaveMap.Text = "Save Map";
			this.SaveMap.UseVisualStyleBackColor = true;
			this.SaveMap.Click += new System.EventHandler(this.OnSaveMap);
			// 
			// LoadMap
			// 
			this.LoadMap.Location = new System.Drawing.Point(95, 261);
			this.LoadMap.Name = "LoadMap";
			this.LoadMap.Size = new System.Drawing.Size(75, 23);
			this.LoadMap.TabIndex = 5;
			this.LoadMap.Text = "Load Map";
			this.LoadMap.UseVisualStyleBackColor = true;
			this.LoadMap.Click += new System.EventHandler(this.OnLoadMap);
			// 
			// TrimStart
			// 
			this.TrimStart.Location = new System.Drawing.Point(7, 78);
			this.TrimStart.Name = "TrimStart";
			this.TrimStart.Size = new System.Drawing.Size(75, 23);
			this.TrimStart.TabIndex = 8;
			this.TrimStart.Text = "Trim Start";
			this.TrimStart.UseVisualStyleBackColor = true;
			this.TrimStart.Click += new System.EventHandler(this.OnTrimStart);
			// 
			// TrimEnd
			// 
			this.TrimEnd.Location = new System.Drawing.Point(7, 108);
			this.TrimEnd.Name = "TrimEnd";
			this.TrimEnd.Size = new System.Drawing.Size(75, 23);
			this.TrimEnd.TabIndex = 9;
			this.TrimEnd.Text = "Trim End";
			this.TrimEnd.UseVisualStyleBackColor = true;
			this.TrimEnd.Click += new System.EventHandler(this.OnTrimEnd);
			// 
			// TrimAmount
			// 
			this.TrimAmount.Location = new System.Drawing.Point(88, 94);
			this.TrimAmount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.TrimAmount.Name = "TrimAmount";
			this.TrimAmount.Size = new System.Drawing.Size(70, 20);
			this.TrimAmount.TabIndex = 10;
			// 
			// KinectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(507, 440);
			this.ControlBox = false;
			this.Controls.Add(this.LoadMap);
			this.Controls.Add(this.SaveMap);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.AssignBone);
			this.Controls.Add(this.CharBones);
			this.Controls.Add(this.KinectBones);
			this.Name = "KinectForm";
			this.Text = "Kinect";
			((System.ComponentModel.ISupportInitialize)(this.KinectBones)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CharBones)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TrimAmount)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView KinectBones;
		private System.Windows.Forms.DataGridView CharBones;
		private System.Windows.Forms.Button AssignBone;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox TotalTime;
		private System.Windows.Forms.TextBox NumFrames;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button SaveMap;
		private System.Windows.Forms.Button LoadMap;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button LoadData;
		private System.Windows.Forms.Button SaveData;
		private System.Windows.Forms.Button ConvertToAnim;
		private System.Windows.Forms.NumericUpDown TrimAmount;
		private System.Windows.Forms.Button TrimEnd;
		private System.Windows.Forms.Button TrimStart;
	}
}