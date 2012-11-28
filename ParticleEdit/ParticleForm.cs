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

		public float StartingAlpha
		{
			get { return (float)StartAlpha.Value; }
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
			get { return (float)SpinVelocityMin.Value / 1000f; }
		}

		public float SpinMax
		{
			get { return (float)SpinVelocityMax.Value / 1000f; }
		}

		public float VelMin
		{
			get { return (float)VelocityMin.Value / 1000f; }
		}

		public float VelMax
		{
			get { return (float)VelocityMax.Value / 1000f; }
		}

		public float SizeMin
		{
			get { return (float)SizeVelocityMin.Value / 1000f; }
		}

		public float SizeMax
		{
			get { return (float)SizeVelocityMax.Value / 1000f; }
		}

		public float AlphaMin
		{
			get { return (float)AlphaVelocityMin.Value / 1000f; }
		}

		public float AlphaMax
		{
			get { return (float)AlphaVelocityMax.Value / 1000f; }
		}

		public int LifeMin
		{
			get { return (int)(LifeTimeMin.Value * 1000); }
		}

		public int LifeMax
		{
			get { return (int)(LifeTimeMax.Value * 1000); }
		}

		public float GravYaw
		{
			get { return (float)GravityYaw.Value; }
		}

		public float GravPitch
		{
			get { return (float)GravityPitch.Value; }
		}

		public float GravRoll
		{
			get { return (float)GravityRoll.Value; }
		}

		public float GravStrength
		{
			get { return (float)GravityStrength.Value; }
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
