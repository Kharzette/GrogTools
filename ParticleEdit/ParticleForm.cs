using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UtilityLib;


namespace ParticleEdit
{
	public partial class ParticleForm : Form
	{
		public event EventHandler	eCreate;


		public int MaxParts
		{
			get { return (int)MaxParticles.Value; }
		}

		public float StartingSize
		{
			get { return (float)StartSize.Value; }
		}

		public int PartDuration
		{
			get { return (int)Duration.Value; }
		}

		public float EmitMS
		{
			get { return (float)EmitPerMS.Value; }
		}

		public int SpinMin
		{
			get { return (int)(SpinVelocityMin.Value * 1000); }
		}

		public int SpinMax
		{
			get { return (int)(SpinVelocityMax.Value * 1000); }
		}

		public int VelMin
		{
			get { return (int)(VelocityMin.Value * 1000); }
		}

		public int VelMax
		{
			get { return (int)(VelocityMax.Value * 1000); }
		}

		public int SizeMin
		{
			get { return (int)(SizeVelocityMin.Value * 1000); }
		}

		public int SizeMax
		{
			get { return (int)(SizeVelocityMax.Value * 1000); }
		}

		public int AlphaMin
		{
			get { return (int)(AlphaVelocityMin.Value * 1000); }
		}

		public int AlphaMax
		{
			get { return (int)(AlphaVelocityMax.Value * 1000); }
		}

		public int LifeMin
		{
			get { return (int)(LifeTimeMin.Value * 1000); }
		}

		public int LifeMax
		{
			get { return (int)(LifeTimeMax.Value * 1000); }
		}


		public ParticleForm()
		{
			InitializeComponent();
		}


		void OnCreate(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eCreate, null);
		}
	}
}
