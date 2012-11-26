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

		public float SpinMin
		{
			get { return (float)SpinVelocityMin.Value; }
		}

		public float SpinMax
		{
			get { return (float)SpinVelocityMax.Value; }
		}

		public float VelMin
		{
			get { return (float)VelocityMin.Value; }
		}

		public float VelMax
		{
			get { return (float)VelocityMax.Value; }
		}

		public float SizeMin
		{
			get { return (float)SizeVelocityMin.Value; }
		}

		public float SizeMax
		{
			get { return (float)SizeVelocityMax.Value; }
		}

		public float AlphaMin
		{
			get { return (float)AlphaVelocityMin.Value; }
		}

		public float AlphaMax
		{
			get { return (float)AlphaVelocityMax.Value; }
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
