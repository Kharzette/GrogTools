﻿using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using UtilityLib;


namespace ParticleEdit
{
	internal partial class ParticleForm : Form
	{
		Color		mCurrentColor	=Color.White;
		ColorDialog	mColorPicker	=new ColorDialog();
		bool		mbUpdating;

		internal event EventHandler	eCreate;
		internal event EventHandler	eItemNuked;
		internal event EventHandler	eCellChanged;
		internal event EventHandler	eValueChanged;
		internal event EventHandler	eSelectionChanged;
		internal event EventHandler	eCopyEmitterToClipBoard;


		internal ParticleForm() : base()
		{
			InitializeComponent();

			ColorPanel.BackColor	=mCurrentColor;

			Array	shapeVals	=Enum.GetValues(typeof(ParticleLib.Emitter.Shapes));

			Shape.Items.Clear();

			foreach(object val in shapeVals)
			{
				Shape.Items.Add(val);
			}

			Shape.SelectedIndex	=0;
		}

		public bool IsCell
		{
			get { return Cell.Checked; }
			set
			{
				Action<CheckBox>	upVal	=numer => numer.Checked = value;
				SharedForms.FormExtensions.Invoke(Cell, upVal);
			}
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

		public int GravYaw
		{
			get { return (int)GravityYaw.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = value;
				SharedForms.FormExtensions.Invoke(GravityYaw, upVal);
			}
		}

		public int GravPitch
		{
			get { return (int)GravityPitch.Value; }
			set
			{
				Action<NumericUpDown>	upVal	=numer => numer.Value = value;
				SharedForms.FormExtensions.Invoke(GravityPitch, upVal);
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

		public Microsoft.Xna.Framework.Vector4 PartColor
		{
			get { return Misc.ARGBToVector4(mCurrentColor.ToArgb()); }
			set
			{
				mCurrentColor	=Color.FromArgb(Misc.Vector4ToARGB(value));

				Action<Panel>	upVal	=pan => pan.BackColor = mCurrentColor;
				SharedForms.FormExtensions.Invoke(ColorPanel, upVal);
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
				ColorPanel.BackColor	=mCurrentColor;
			}

			Misc.SafeInvoke(eValueChanged, null);
		}


		internal void UpdateEmitter(ParticleLib.Emitter em)
		{
			em.mStartSize				=StartingSize;
			em.mStartAlpha				=StartingAlpha;
			em.mEmitMS					=EmitMS;
			em.mRotationalVelocityMin	=SpinMin;
			em.mRotationalVelocityMax	=SpinMax;
			em.mVelocityMin				=VelMin;
			em.mVelocityMax				=VelMax;
			em.mSizeVelocityMin			=SizeMin;
			em.mSizeVelocityMax			=SizeMax;
			em.mAlphaVelocityMin		=AlphaMin;
			em.mAlphaVelocityMax		=AlphaMax;
			em.mLifeMin					=LifeMin;
			em.mLifeMax					=LifeMax;
			em.mGravityYaw				=GravYaw;
			em.mGravityPitch			=GravPitch;
			em.mGravityStrength			=GravStrength;
			em.mbOn						=Active.Checked;
			em.mShape					=EmShape;
			em.mShapeSize				=EmShapeSize;

			em.UpdateGravity();
		}


		internal void UpdateControls(ParticleLib.Emitter em,
			Microsoft.Xna.Framework.Vector4 color, bool bCell)
		{
			if(mbUpdating)
			{
				return;
			}

			mbUpdating	=true;

			StartingSize	=em.mStartSize;
			StartingAlpha	=em.mStartAlpha;
			EmitMS			=em.mEmitMS;
			SpinMin			=em.mRotationalVelocityMin;
			SpinMax			=em.mRotationalVelocityMax;
			VelMin			=em.mVelocityMin;
			VelMax			=em.mVelocityMax;
			SizeMin			=em.mSizeVelocityMin;
			SizeMax			=em.mSizeVelocityMax;
			AlphaMin		=em.mAlphaVelocityMin;
			AlphaMax		=em.mAlphaVelocityMax;
			LifeMin			=em.mLifeMin;
			LifeMax			=em.mLifeMax;
			GravYaw			=em.mGravityYaw;
			GravPitch		=em.mGravityPitch;
			GravStrength	=em.mGravityStrength;
			PartColor		=color;
			Active.Checked	=em.mbOn;
			EmShape			=em.mShape;
			EmShapeSize		=em.mShapeSize;
			IsCell			=bCell;

			mbUpdating	=false;
		}


		internal void UpdateListView(List<string> list)
		{
			EmitterListView.Clear();

			foreach(string s in list)
			{
				ListViewItem	lvi	=new ListViewItem();

				lvi.Text	=s;

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
				Misc.SafeInvoke(eSelectionChanged, new Nullable<int>(EmitterListView.SelectedItems[0].Index));
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
			if(e.KeyCode == Keys.Delete)
			{
				if(EmitterListView.SelectedItems.Count == 1)
				{
					ListViewItem	itm	=EmitterListView.SelectedItems[0];

					int	index	=itm.Index;

					//blast from listview
					EmitterListView.Items.Remove(itm);

					//nuke from system
					Misc.SafeInvoke(eItemNuked, new Nullable<int>(index));
				}
			}
			else if(e.KeyCode == Keys.C && e.Control)
			{
				if(EmitterListView.SelectedItems.Count == 1)
				{
					ListViewItem	itm	=EmitterListView.SelectedItems[0];

					int	index	=itm.Index;

					Misc.SafeInvoke(eCopyEmitterToClipBoard, new Nullable<int>(index));
				}
			}
		}


		void OnCellChanged(object sender, EventArgs e)
		{
			if(mbUpdating)
			{
				return;
			}

			Misc.SafeInvoke(eCellChanged, new Nullable<bool>(Cell.Checked));
		}
	}
}
