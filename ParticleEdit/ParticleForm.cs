using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using SharpDX;
using UtilityLib;

using Color		=System.Drawing.Color;
using MatLib	=MaterialLib.MaterialLib;


namespace ParticleEdit
{
	internal partial class ParticleForm : Form
	{
		Color		mCurrentColor	=Color.White;
		ColorDialog	mColorPicker	=new ColorDialog();
		bool		mbUpdating;

		List<string>	mParticleTextures	=new List<string>();

		MatLib	mMats;

		internal event EventHandler	eCreate;
		internal event EventHandler	eItemNuked;
		internal event EventHandler	eValueChanged;
		internal event EventHandler	eSelectionChanged;
		internal event EventHandler	eCopyEmitterToClipBoard;
		internal event EventHandler	ePasteEmitterFromClipBoard;
		internal event EventHandler	eTextureChanged;


		internal ParticleForm(MatLib mats) : base()
		{
			InitializeComponent();

			mMats	=mats;

			StartColor.BackColor	=mCurrentColor;

			Array	shapeVals	=Enum.GetValues(typeof(ParticleLib.Emitter.Shapes));

			Shape.Items.Clear();

			foreach(object val in shapeVals)
			{
				Shape.Items.Add(val);
			}
			Shape.SelectedIndex	=0;

			mParticleTextures	=mats.GetParticleTextures();
			if(mParticleTextures.Count <= 0)
			{
				return;
			}

			foreach(string tex in mParticleTextures)
			{
				//strip particles off the front
				string	justTex	=tex.Substring(10);

				TextureBox.Items.Add(justTex);
			}
			TextureBox.SelectedIndex	=0;
		}

		public string EmTexture
		{
			get { return mParticleTextures[TextureBox.SelectedIndex]; }
		}

		public ParticleLib.Emitter.Shapes EmShape
		{
			get { return (ParticleLib.Emitter.Shapes)Shape.SelectedItem; }
			set
			{
				if(!Shape.Items.Contains(value))
				{
					return;
				}
				Action<ComboBox>	upVal	=box => box.SelectedIndex =box.Items.IndexOf(value);
				SharedForms.FormExtensions.Invoke(Shape, upVal);
			}
		}

		public float EmShapeSize
		{
			get { return (float)ShapeSize.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)value;
				SharedForms.FormExtensions.Invoke(ShapeSize, upVal);
			}
		}

		public int MaxParts
		{
			get { return (int)MaxParticles.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = value;
				SharedForms.FormExtensions.Invoke(MaxParticles, upVal);
			}
		}

		public float StartingSize
		{
			get { return (float)StartSize.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)value;
				SharedForms.FormExtensions.Invoke(StartSize, upVal);
			}
		}

		public float StartingAlpha
		{
			get { return (float)StartAlpha.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)value;
				SharedForms.FormExtensions.Invoke(StartAlpha, upVal);
			}
		}

		public float EmitMS
		{
			get { return (float)EmitPerMS.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)value;
				SharedForms.FormExtensions.Invoke(EmitPerMS, upVal);
			}
		}

		public Vector3 ColorVelocityMin
		{
			get
			{
 				return	new Vector3((float)RedVelocityMin.Value / 10000f,
					(float)GreenVelocityMin.Value / 10000f,
					(float)BlueVelocityMin.Value / 10000f);
			}
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value.X * 10000f);
				SharedForms.FormExtensions.Invoke(RedVelocityMin, upVal);
				upVal	=numer => numer.Value = (decimal)(value.Y * 10000f);
				SharedForms.FormExtensions.Invoke(GreenVelocityMin, upVal);
				upVal	=numer => numer.Value = (decimal)(value.Z * 10000f);
				SharedForms.FormExtensions.Invoke(BlueVelocityMin, upVal);
			}
		}

		public Vector3 ColorVelocityMax
		{
			get
			{
 				return	new Vector3((float)RedVelocityMax.Value / 10000f,
					(float)GreenVelocityMax.Value / 10000f,
					(float)BlueVelocityMax.Value / 10000f);
			}
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value.X * 10000f);
				SharedForms.FormExtensions.Invoke(RedVelocityMax, upVal);
				upVal	=numer => numer.Value = (decimal)(value.Y * 10000f);
				SharedForms.FormExtensions.Invoke(GreenVelocityMax, upVal);
				upVal	=numer => numer.Value = (decimal)(value.Z * 10000f);
				SharedForms.FormExtensions.Invoke(BlueVelocityMax, upVal);
			}
		}

		public float SpinMin
		{
			get { return (float)SpinVelocityMin.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(SpinVelocityMin, upVal);
			}
		}

		public float SpinMax
		{
			get { return (float)SpinVelocityMax.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(SpinVelocityMax, upVal);
			}
		}

		public float VelMin
		{
			get { return (float)VelocityMin.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(VelocityMin, upVal);
			}
		}

		public float VelMax
		{
			get { return (float)VelocityMax.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(VelocityMax, upVal);
			}
		}

		public float VelCap
		{
			get { return (float)VelocityCap.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value);
				SharedForms.FormExtensions.Invoke(VelocityCap, upVal);
			}
		}

		public float SizeMin
		{
			get { return (float)SizeVelocityMin.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(SizeVelocityMin, upVal);
			}
		}

		public float SizeMax
		{
			get { return (float)SizeVelocityMax.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(SizeVelocityMax, upVal);
			}
		}

		public float AlphaMin
		{
			get { return (float)AlphaVelocityMin.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(AlphaVelocityMin, upVal);
			}
		}

		public float AlphaMax
		{
			get { return (float)AlphaVelocityMax.Value / 1000f; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value * 1000f);
				SharedForms.FormExtensions.Invoke(AlphaVelocityMax, upVal);
			}
		}

		public int LifeMin
		{
			get { return (int)(LifeTimeMin.Value * 1000); }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value) / 1000;
				SharedForms.FormExtensions.Invoke(LifeTimeMin, upVal);
			}
		}

		public int LifeMax
		{
			get { return (int)(LifeTimeMax.Value * 1000); }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)(value) / 1000;
				SharedForms.FormExtensions.Invoke(LifeTimeMax, upVal);
			}
		}

		public Vector3 GravPos
		{
			get	{ return Misc.StringToVector3(GravityPosition.Text); }
			set
			{
				string	gravPos	=Misc.VectorToString(value, 2);
				Action<TextBox>	upVal	=tbox => tbox.Text = gravPos;
				SharedForms.FormExtensions.Invoke(GravityPosition, upVal);
			}
		}

		public float GravStrength
		{
			get { return (float)GravityStrength.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = (decimal)value;
				SharedForms.FormExtensions.Invoke(GravityStrength, upVal);
			}
		}

		public Vector4 EMStartColor
		{
			get { return Misc.ARGBToVector4(mCurrentColor.ToArgb()); }
			set
			{
				mCurrentColor	=Color.FromArgb(Misc.Vector4ToARGB(value));

				Action<Panel>	upVal	=pan => pan.BackColor = mCurrentColor;
				SharedForms.FormExtensions.Invoke(StartColor, upVal);
			}
		}


		void OnCreate(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eCreate, null);
		}


		void OnChangeColor(object sender, EventArgs e)
		{
			mColorPicker.Color	=mCurrentColor;

			DialogResult	dr	=mColorPicker.ShowDialog();
			if(dr == System.Windows.Forms.DialogResult.OK)
			{
				mCurrentColor			=mColorPicker.Color;
				StartColor.BackColor	=mCurrentColor;
			}

			Misc.SafeInvoke(eValueChanged, null);
		}


		internal void UpdateEmitter(ParticleLib.Emitter em)
		{
			em.mStartSize				=StartingSize;
			em.mStartColor				=EMStartColor;
			em.mStartColor.W			=StartingAlpha;
			em.mEmitMS					=EmitMS;
			em.mRotationalVelocityMin	=SpinMin;
			em.mRotationalVelocityMax	=SpinMax;
			em.mVelocityMin				=VelMin;
			em.mVelocityMax				=VelMax;
			em.mVelocityCap				=VelCap;
			em.mSizeVelocityMin			=SizeMin;
			em.mSizeVelocityMax			=SizeMax;
			em.mLifeMin					=LifeMin;
			em.mLifeMax					=LifeMax;
			em.mGravityLocation			=GravPos;
			em.mGravityStrength			=GravStrength;
			em.mbOn						=true;
			em.mShape					=EmShape;
			em.mShapeSize				=EmShapeSize;

			em.mColorVelocityMin		=new Vector4(
				ColorVelocityMin.X, ColorVelocityMin.Y, ColorVelocityMin.Z,
				AlphaMin);
			em.mColorVelocityMax		=new Vector4(
				ColorVelocityMax.X, ColorVelocityMax.Y, ColorVelocityMax.Z,
				AlphaMax);
		}


		internal void UpdateControls(ParticleLib.Emitter em, string tex)
		{
			if(mbUpdating)
			{
				return;
			}

			mbUpdating	=true;

			StartingSize		=em.mStartSize;
			StartingAlpha		=em.mStartColor.W;
			EmitMS				=em.mEmitMS;
			SpinMin				=em.mRotationalVelocityMin;
			SpinMax				=em.mRotationalVelocityMax;
			VelMin				=em.mVelocityMin;
			VelMax				=em.mVelocityMax;
			VelCap				=em.mVelocityCap;
			SizeMin				=em.mSizeVelocityMin;
			SizeMax				=em.mSizeVelocityMax;
			AlphaMin			=em.mColorVelocityMin.W;
			AlphaMax			=em.mColorVelocityMax.W;
			LifeMin				=em.mLifeMin;
			LifeMax				=em.mLifeMax;
			GravPos				=em.mGravityLocation;
			GravStrength		=em.mGravityStrength;
			EMStartColor		=em.mStartColor;
			EmShape				=em.mShape;
			EmShapeSize			=em.mShapeSize;

			ColorVelocityMin	=new Vector3(
				em.mColorVelocityMin.X, em.mColorVelocityMin.Y, em.mColorVelocityMin.Z);
			ColorVelocityMax	=new Vector3(
				em.mColorVelocityMax.X, em.mColorVelocityMax.Y, em.mColorVelocityMax.Z);

			foreach(string pt in mParticleTextures)
			{
				if(pt == tex)
				{
					TextureBox.SelectedIndex	=mParticleTextures.IndexOf(pt);
					break;
				}
			}

			mbUpdating	=false;
		}


		internal void UpdateListView(List<string> list, List<int> indexes)
		{
			Debug.Assert(list.Count == indexes.Count);

			EmitterListView.Clear();

			for(int i=0;i < list.Count;i++)
			{
				ListViewItem	lvi	=new ListViewItem();

				lvi.Text	=list[i];
				lvi.Tag		=indexes[i];

				EmitterListView.Items.Add(lvi);
			}
		}


		void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			Debug.Assert(EmitterListView.SelectedItems.Count < 2);

			if(EmitterListView.SelectedItems.Count <= 0)
			{
				MaxParticles.Enabled	=true;
				Misc.SafeInvoke(eSelectionChanged, new Nullable<int>(-1));
			}
			else
			{
				MaxParticles.Enabled	=false;
				Misc.SafeInvoke(eSelectionChanged,
					new Nullable<int>((int)EmitterListView.SelectedItems[0].Tag));
			}
		}


		void OnValueChanged(object sender, EventArgs e)
		{
			if(mbUpdating)
			{
				return;
			}

			Misc.SafeInvoke(eValueChanged, null);
		}


		void OnKeyUp(object sender, KeyEventArgs e)
		{
			e.Handled	=true;
			if(e.KeyCode == Keys.Delete)
			{
				if(EmitterListView.SelectedItems.Count == 1)
				{
					ListViewItem	itm	=EmitterListView.SelectedItems[0];

					int	index	=itm.Index;

					//blast from listview
					EmitterListView.Items.Remove(itm);

					//nuke from system
					Misc.SafeInvoke(eItemNuked, new Nullable<int>((int)itm.Tag));
				}
			}
			else if(e.KeyCode == Keys.C && e.Control)
			{
				if(EmitterListView.SelectedItems.Count == 1)
				{
					ListViewItem	itm	=EmitterListView.SelectedItems[0];

					Misc.SafeInvoke(eCopyEmitterToClipBoard, new Nullable<int>((int)itm.Tag));
				}
			}
			else if(e.KeyCode == Keys.V && e.Control)
			{
				Misc.SafeInvoke(ePasteEmitterFromClipBoard, null);
			}
			else
			{
				e.Handled	=false;
			}
		}


		void OnTextureChanged(object sender, EventArgs e)
		{			
			Misc.SafeInvoke(eTextureChanged, mParticleTextures[TextureBox.SelectedIndex]);
		}
	}
}
