namespace ParticleEdit
{
	partial class ParticleForm
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
			this.MaxParticles = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.Cell = new System.Windows.Forms.CheckBox();
			this.Active = new System.Windows.Forms.CheckBox();
			this.label15 = new System.Windows.Forms.Label();
			this.StartAlpha = new System.Windows.Forms.NumericUpDown();
			this.LifeTimeMin = new System.Windows.Forms.NumericUpDown();
			this.label13 = new System.Windows.Forms.Label();
			this.LifeTimeMax = new System.Windows.Forms.NumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.StartSize = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.EmitPerMS = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.VelocityMax = new System.Windows.Forms.NumericUpDown();
			this.SizeVelocityMin = new System.Windows.Forms.NumericUpDown();
			this.SizeVelocityMax = new System.Windows.Forms.NumericUpDown();
			this.SpinVelocityMin = new System.Windows.Forms.NumericUpDown();
			this.SpinVelocityMax = new System.Windows.Forms.NumericUpDown();
			this.AlphaVelocityMin = new System.Windows.Forms.NumericUpDown();
			this.AlphaVelocityMax = new System.Windows.Forms.NumericUpDown();
			this.CreateEmitter = new System.Windows.Forms.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.VelocityMin = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label19 = new System.Windows.Forms.Label();
			this.GravityStrength = new System.Windows.Forms.NumericUpDown();
			this.label17 = new System.Windows.Forms.Label();
			this.GravityPitch = new System.Windows.Forms.NumericUpDown();
			this.label16 = new System.Windows.Forms.Label();
			this.GravityYaw = new System.Windows.Forms.NumericUpDown();
			this.ColorGroup = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.ShapeSize = new System.Windows.Forms.NumericUpDown();
			this.Shape = new System.Windows.Forms.ComboBox();
			this.ColorPanel = new System.Windows.Forms.Panel();
			this.button1 = new System.Windows.Forms.Button();
			this.EmitterListView = new System.Windows.Forms.ListView();
			((System.ComponentModel.ISupportInitialize)(this.MaxParticles)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartAlpha)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LifeTimeMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LifeTimeMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StartSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EmitPerMS)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VelocityMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeVelocityMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeVelocityMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinVelocityMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinVelocityMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AlphaVelocityMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AlphaVelocityMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VelocityMin)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GravityStrength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GravityPitch)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GravityYaw)).BeginInit();
			this.ColorGroup.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShapeSize)).BeginInit();
			this.SuspendLayout();
			// 
			// MaxParticles
			// 
			this.MaxParticles.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.MaxParticles.Location = new System.Drawing.Point(6, 19);
			this.MaxParticles.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.MaxParticles.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.MaxParticles.Name = "MaxParticles";
			this.MaxParticles.Size = new System.Drawing.Size(58, 20);
			this.MaxParticles.TabIndex = 0;
			this.MaxParticles.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.MaxParticles.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(70, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(70, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Max Particles";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.Cell);
			this.groupBox1.Controls.Add(this.Active);
			this.groupBox1.Controls.Add(this.label15);
			this.groupBox1.Controls.Add(this.StartAlpha);
			this.groupBox1.Controls.Add(this.LifeTimeMin);
			this.groupBox1.Controls.Add(this.label13);
			this.groupBox1.Controls.Add(this.LifeTimeMax);
			this.groupBox1.Controls.Add(this.label14);
			this.groupBox1.Controls.Add(this.StartSize);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.EmitPerMS);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.MaxParticles);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(161, 204);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Emitter Details";
			// 
			// Cell
			// 
			this.Cell.AutoSize = true;
			this.Cell.Enabled = false;
			this.Cell.Location = new System.Drawing.Point(68, 175);
			this.Cell.Name = "Cell";
			this.Cell.Size = new System.Drawing.Size(43, 17);
			this.Cell.TabIndex = 39;
			this.Cell.Text = "Cell";
			this.Cell.UseVisualStyleBackColor = true;
			this.Cell.CheckedChanged += new System.EventHandler(this.OnCellChanged);
			// 
			// Active
			// 
			this.Active.AutoSize = true;
			this.Active.Checked = true;
			this.Active.CheckState = System.Windows.Forms.CheckState.Checked;
			this.Active.Location = new System.Drawing.Point(6, 175);
			this.Active.Name = "Active";
			this.Active.Size = new System.Drawing.Size(56, 17);
			this.Active.TabIndex = 38;
			this.Active.Text = "Active";
			this.Active.UseVisualStyleBackColor = true;
			this.Active.CheckedChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(70, 73);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(59, 13);
			this.label15.TabIndex = 37;
			this.label15.Text = "Start Alpha";
			// 
			// StartAlpha
			// 
			this.StartAlpha.DecimalPlaces = 2;
			this.StartAlpha.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.StartAlpha.Location = new System.Drawing.Point(6, 71);
			this.StartAlpha.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.StartAlpha.Name = "StartAlpha";
			this.StartAlpha.Size = new System.Drawing.Size(58, 20);
			this.StartAlpha.TabIndex = 36;
			this.StartAlpha.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.StartAlpha.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// LifeTimeMin
			// 
			this.LifeTimeMin.DecimalPlaces = 1;
			this.LifeTimeMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.LifeTimeMin.Location = new System.Drawing.Point(6, 123);
			this.LifeTimeMin.Name = "LifeTimeMin";
			this.LifeTimeMin.Size = new System.Drawing.Size(58, 20);
			this.LifeTimeMin.TabIndex = 22;
			this.LifeTimeMin.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.LifeTimeMin.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(70, 125);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(63, 13);
			this.label13.TabIndex = 23;
			this.label13.Text = "Lifetime Min";
			// 
			// LifeTimeMax
			// 
			this.LifeTimeMax.DecimalPlaces = 1;
			this.LifeTimeMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.LifeTimeMax.Location = new System.Drawing.Point(6, 149);
			this.LifeTimeMax.Name = "LifeTimeMax";
			this.LifeTimeMax.Size = new System.Drawing.Size(58, 20);
			this.LifeTimeMax.TabIndex = 20;
			this.LifeTimeMax.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.LifeTimeMax.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(70, 151);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(66, 13);
			this.label14.TabIndex = 21;
			this.label14.Text = "Lifetime Max";
			// 
			// StartSize
			// 
			this.StartSize.DecimalPlaces = 1;
			this.StartSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.StartSize.Location = new System.Drawing.Point(6, 45);
			this.StartSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.StartSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.StartSize.Name = "StartSize";
			this.StartSize.Size = new System.Drawing.Size(58, 20);
			this.StartSize.TabIndex = 8;
			this.StartSize.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.StartSize.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(70, 47);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(52, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Start Size";
			// 
			// EmitPerMS
			// 
			this.EmitPerMS.DecimalPlaces = 3;
			this.EmitPerMS.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.EmitPerMS.Location = new System.Drawing.Point(6, 97);
			this.EmitPerMS.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.EmitPerMS.Name = "EmitPerMS";
			this.EmitPerMS.Size = new System.Drawing.Size(58, 20);
			this.EmitPerMS.TabIndex = 4;
			this.EmitPerMS.Value = new decimal(new int[] {
            4,
            0,
            0,
            131072});
			this.EmitPerMS.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(70, 99);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Emit Per MS";
			// 
			// VelocityMax
			// 
			this.VelocityMax.DecimalPlaces = 2;
			this.VelocityMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.VelocityMax.Location = new System.Drawing.Point(6, 97);
			this.VelocityMax.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.VelocityMax.Name = "VelocityMax";
			this.VelocityMax.Size = new System.Drawing.Size(58, 20);
			this.VelocityMax.TabIndex = 35;
			this.VelocityMax.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.VelocityMax.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// SizeVelocityMin
			// 
			this.SizeVelocityMin.DecimalPlaces = 2;
			this.SizeVelocityMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.SizeVelocityMin.Location = new System.Drawing.Point(6, 123);
			this.SizeVelocityMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.SizeVelocityMin.Name = "SizeVelocityMin";
			this.SizeVelocityMin.Size = new System.Drawing.Size(58, 20);
			this.SizeVelocityMin.TabIndex = 34;
			this.SizeVelocityMin.Value = new decimal(new int[] {
            5,
            0,
            0,
            -2147352576});
			this.SizeVelocityMin.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// SizeVelocityMax
			// 
			this.SizeVelocityMax.DecimalPlaces = 2;
			this.SizeVelocityMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.SizeVelocityMax.Location = new System.Drawing.Point(6, 149);
			this.SizeVelocityMax.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.SizeVelocityMax.Name = "SizeVelocityMax";
			this.SizeVelocityMax.Size = new System.Drawing.Size(58, 20);
			this.SizeVelocityMax.TabIndex = 33;
			this.SizeVelocityMax.Value = new decimal(new int[] {
            5,
            0,
            0,
            131072});
			this.SizeVelocityMax.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// SpinVelocityMin
			// 
			this.SpinVelocityMin.DecimalPlaces = 2;
			this.SpinVelocityMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.SpinVelocityMin.Location = new System.Drawing.Point(6, 175);
			this.SpinVelocityMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.SpinVelocityMin.Name = "SpinVelocityMin";
			this.SpinVelocityMin.Size = new System.Drawing.Size(58, 20);
			this.SpinVelocityMin.TabIndex = 32;
			this.SpinVelocityMin.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// SpinVelocityMax
			// 
			this.SpinVelocityMax.DecimalPlaces = 2;
			this.SpinVelocityMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.SpinVelocityMax.Location = new System.Drawing.Point(6, 201);
			this.SpinVelocityMax.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.SpinVelocityMax.Name = "SpinVelocityMax";
			this.SpinVelocityMax.Size = new System.Drawing.Size(58, 20);
			this.SpinVelocityMax.TabIndex = 31;
			this.SpinVelocityMax.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// AlphaVelocityMin
			// 
			this.AlphaVelocityMin.DecimalPlaces = 2;
			this.AlphaVelocityMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.AlphaVelocityMin.Location = new System.Drawing.Point(6, 19);
			this.AlphaVelocityMin.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.AlphaVelocityMin.Name = "AlphaVelocityMin";
			this.AlphaVelocityMin.Size = new System.Drawing.Size(58, 20);
			this.AlphaVelocityMin.TabIndex = 30;
			this.AlphaVelocityMin.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// AlphaVelocityMax
			// 
			this.AlphaVelocityMax.DecimalPlaces = 2;
			this.AlphaVelocityMax.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.AlphaVelocityMax.Location = new System.Drawing.Point(6, 45);
			this.AlphaVelocityMax.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.AlphaVelocityMax.Name = "AlphaVelocityMax";
			this.AlphaVelocityMax.Size = new System.Drawing.Size(58, 20);
			this.AlphaVelocityMax.TabIndex = 29;
			this.AlphaVelocityMax.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// CreateEmitter
			// 
			this.CreateEmitter.Location = new System.Drawing.Point(12, 222);
			this.CreateEmitter.Name = "CreateEmitter";
			this.CreateEmitter.Size = new System.Drawing.Size(75, 23);
			this.CreateEmitter.TabIndex = 28;
			this.CreateEmitter.Text = "Create";
			this.CreateEmitter.UseVisualStyleBackColor = true;
			this.CreateEmitter.Click += new System.EventHandler(this.OnCreate);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(70, 21);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(94, 13);
			this.label11.TabIndex = 27;
			this.label11.Text = "Alpha Velocity Min";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(70, 47);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(97, 13);
			this.label12.TabIndex = 25;
			this.label12.Text = "Alpha Velocity Max";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(70, 125);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(87, 13);
			this.label6.TabIndex = 19;
			this.label6.Text = "Size Velocity Min";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(70, 151);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(90, 13);
			this.label7.TabIndex = 17;
			this.label7.Text = "Size Velocity Max";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(70, 177);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(88, 13);
			this.label8.TabIndex = 15;
			this.label8.Text = "Spin Velocity Min";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(70, 203);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(91, 13);
			this.label9.TabIndex = 13;
			this.label9.Text = "Spin Velocity Max";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(70, 99);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(67, 13);
			this.label10.TabIndex = 11;
			this.label10.Text = "Velocity Max";
			// 
			// VelocityMin
			// 
			this.VelocityMin.DecimalPlaces = 2;
			this.VelocityMin.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
			this.VelocityMin.Location = new System.Drawing.Point(6, 71);
			this.VelocityMin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.VelocityMin.Name = "VelocityMin";
			this.VelocityMin.Size = new System.Drawing.Size(58, 20);
			this.VelocityMin.TabIndex = 2;
			this.VelocityMin.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.VelocityMin.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(70, 73);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Velocity Min";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.AlphaVelocityMin);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.VelocityMax);
			this.groupBox2.Controls.Add(this.VelocityMin);
			this.groupBox2.Controls.Add(this.SizeVelocityMin);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.SizeVelocityMax);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.SpinVelocityMin);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.SpinVelocityMax);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.AlphaVelocityMax);
			this.groupBox2.Controls.Add(this.label12);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Location = new System.Drawing.Point(179, 12);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(190, 233);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Velocities";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label19);
			this.groupBox3.Controls.Add(this.GravityStrength);
			this.groupBox3.Controls.Add(this.label17);
			this.groupBox3.Controls.Add(this.GravityPitch);
			this.groupBox3.Controls.Add(this.label16);
			this.groupBox3.Controls.Add(this.GravityYaw);
			this.groupBox3.Location = new System.Drawing.Point(375, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(156, 103);
			this.groupBox3.TabIndex = 29;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Forces";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(70, 73);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(83, 13);
			this.label19.TabIndex = 39;
			this.label19.Text = "Gravity Strength";
			// 
			// GravityStrength
			// 
			this.GravityStrength.DecimalPlaces = 3;
			this.GravityStrength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.GravityStrength.Location = new System.Drawing.Point(6, 71);
			this.GravityStrength.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.GravityStrength.Name = "GravityStrength";
			this.GravityStrength.Size = new System.Drawing.Size(58, 20);
			this.GravityStrength.TabIndex = 38;
			this.GravityStrength.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(70, 47);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(67, 13);
			this.label17.TabIndex = 34;
			this.label17.Text = "Gravity Pitch";
			// 
			// GravityPitch
			// 
			this.GravityPitch.Location = new System.Drawing.Point(6, 45);
			this.GravityPitch.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
			this.GravityPitch.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.GravityPitch.Name = "GravityPitch";
			this.GravityPitch.Size = new System.Drawing.Size(58, 20);
			this.GravityPitch.TabIndex = 35;
			this.GravityPitch.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(70, 21);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(64, 13);
			this.label16.TabIndex = 32;
			this.label16.Text = "Gravity Yaw";
			// 
			// GravityYaw
			// 
			this.GravityYaw.Location = new System.Drawing.Point(6, 19);
			this.GravityYaw.Maximum = new decimal(new int[] {
            359,
            0,
            0,
            0});
			this.GravityYaw.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            -2147483648});
			this.GravityYaw.Name = "GravityYaw";
			this.GravityYaw.Size = new System.Drawing.Size(58, 20);
			this.GravityYaw.TabIndex = 33;
			this.GravityYaw.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// ColorGroup
			// 
			this.ColorGroup.Controls.Add(this.label4);
			this.ColorGroup.Controls.Add(this.ShapeSize);
			this.ColorGroup.Controls.Add(this.Shape);
			this.ColorGroup.Controls.Add(this.ColorPanel);
			this.ColorGroup.Controls.Add(this.button1);
			this.ColorGroup.Location = new System.Drawing.Point(375, 148);
			this.ColorGroup.Name = "ColorGroup";
			this.ColorGroup.Size = new System.Drawing.Size(156, 97);
			this.ColorGroup.TabIndex = 30;
			this.ColorGroup.TabStop = false;
			this.ColorGroup.Text = "Color and Shape";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(71, 49);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(27, 13);
			this.label4.TabIndex = 41;
			this.label4.Text = "Size";
			// 
			// ShapeSize
			// 
			this.ShapeSize.DecimalPlaces = 1;
			this.ShapeSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.ShapeSize.Location = new System.Drawing.Point(6, 47);
			this.ShapeSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.ShapeSize.Name = "ShapeSize";
			this.ShapeSize.Size = new System.Drawing.Size(58, 20);
			this.ShapeSize.TabIndex = 32;
			this.ShapeSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.ShapeSize.ValueChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// Shape
			// 
			this.Shape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.Shape.FormattingEnabled = true;
			this.Shape.Items.AddRange(new object[] {
            "Point",
            "Sphere",
            "Box",
            "Line",
            "Plane"});
			this.Shape.Location = new System.Drawing.Point(6, 71);
			this.Shape.Name = "Shape";
			this.Shape.Size = new System.Drawing.Size(121, 21);
			this.Shape.TabIndex = 40;
			this.Shape.SelectedIndexChanged += new System.EventHandler(this.OnValueChanged);
			// 
			// ColorPanel
			// 
			this.ColorPanel.Location = new System.Drawing.Point(88, 13);
			this.ColorPanel.Name = "ColorPanel";
			this.ColorPanel.Size = new System.Drawing.Size(62, 29);
			this.ColorPanel.TabIndex = 1;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(6, 19);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Change";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnChangeColor);
			// 
			// EmitterListView
			// 
			this.EmitterListView.HideSelection = false;
			this.EmitterListView.Location = new System.Drawing.Point(12, 251);
			this.EmitterListView.MultiSelect = false;
			this.EmitterListView.Name = "EmitterListView";
			this.EmitterListView.Size = new System.Drawing.Size(519, 130);
			this.EmitterListView.TabIndex = 31;
			this.EmitterListView.UseCompatibleStateImageBehavior = false;
			this.EmitterListView.View = System.Windows.Forms.View.List;
			this.EmitterListView.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
			this.EmitterListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
			// 
			// ParticleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(543, 393);
			this.ControlBox = false;
			this.Controls.Add(this.EmitterListView);
			this.Controls.Add(this.ColorGroup);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.CreateEmitter);
			this.Name = "ParticleForm";
			this.Text = "Particle Form";
			((System.ComponentModel.ISupportInitialize)(this.MaxParticles)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartAlpha)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LifeTimeMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LifeTimeMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StartSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EmitPerMS)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VelocityMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeVelocityMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SizeVelocityMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinVelocityMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SpinVelocityMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlphaVelocityMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AlphaVelocityMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VelocityMin)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GravityStrength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GravityPitch)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GravityYaw)).EndInit();
			this.ColorGroup.ResumeLayout(false);
			this.ColorGroup.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShapeSize)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NumericUpDown MaxParticles;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button CreateEmitter;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.NumericUpDown LifeTimeMin;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown LifeTimeMax;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown StartSize;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown EmitPerMS;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown VelocityMin;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.NumericUpDown StartAlpha;
		private System.Windows.Forms.NumericUpDown VelocityMax;
		private System.Windows.Forms.NumericUpDown SizeVelocityMin;
		private System.Windows.Forms.NumericUpDown SizeVelocityMax;
		private System.Windows.Forms.NumericUpDown SpinVelocityMin;
		private System.Windows.Forms.NumericUpDown SpinVelocityMax;
		private System.Windows.Forms.NumericUpDown AlphaVelocityMin;
		private System.Windows.Forms.NumericUpDown AlphaVelocityMax;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.NumericUpDown GravityPitch;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.NumericUpDown GravityYaw;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.NumericUpDown GravityStrength;
		private System.Windows.Forms.GroupBox ColorGroup;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Panel ColorPanel;
		private System.Windows.Forms.ListView EmitterListView;
		private System.Windows.Forms.CheckBox Active;
		private System.Windows.Forms.CheckBox Cell;
		private System.Windows.Forms.ComboBox Shape;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown ShapeSize;
	}
}