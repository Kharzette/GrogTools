using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UtilityLib;
using ParticleLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MatLib	=MaterialLib.MaterialLib;


namespace ParticleEdit
{
	internal class ParticleEditor
	{
		GraphicsDevice	mGD;
		ParticleForm	mPF;
		ParticleBoss	mPB;
		MatLib			mMats;

		int	mCurSelection;


		internal ParticleEditor(GraphicsDevice gd, ParticleForm pf, MatLib mats)
		{
			mGD		=gd;
			mPF		=pf;
			mMats	=mats;
			mPB		=new ParticleBoss(gd.GD, mats);

			pf.eCreate						+=OnCreate;
			pf.eItemNuked					+=OnEmitterNuked;
			pf.eValueChanged				+=OnValueChanged;
			pf.eSelectionChanged			+=OnEmitterSelChanged;
			pf.eCopyEmitterToClipBoard		+=OnCopyEmitterToClipBoard;
			pf.ePasteEmitterFromClipBoard	+=OnPasteEmitterFromClipBoard;
			pf.eTextureChanged				+=OnTextureChanged;
		}


		void OnCreate(object sender, EventArgs ea)
		{
			float	yaw		=mPF.GravYaw;
			float	pitch	=mPF.GravPitch;
			float	str		=mPF.GravStrength;

			Mathery.WrapAngleDegrees(ref yaw);
			Mathery.WrapAngleDegrees(ref pitch);

			yaw		=MathUtil.DegreesToRadians(yaw);
			pitch	=MathUtil.DegreesToRadians(pitch);

			Vector4	colorMin	=new Vector4(mPF.ColorVelocityMin.X,
				mPF.ColorVelocityMin.Y, mPF.ColorVelocityMin.Z, mPF.AlphaMin);

			Vector4	colorMax	=new Vector4(mPF.ColorVelocityMax.X,
				mPF.ColorVelocityMax.Y, mPF.ColorVelocityMax.Z, mPF.AlphaMax);

			mPB.CreateEmitter(mPF.EmTexture, mPF.EMStartColor,
				mPF.EmShape, mPF.EmShapeSize,
				mPF.MaxParts, Vector3.Zero,
				mPF.GravYaw, mPF.GravPitch, mPF.GravStrength,
				mPF.StartingSize, mPF.EmitMS,
				mPF.SpinMin, mPF.SpinMax, mPF.VelMin, mPF.VelMax,
				mPF.SizeMin, mPF.SizeMax,
				colorMin, colorMax,
				mPF.LifeMin, mPF.LifeMax);

			UpdateListView();
		}


		void OnEmitterNuked(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}
			mPB.NukeEmitter(index.Value);

			UpdateListView();
		}


		void OnEmitterSelChanged(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}

			mCurSelection	=index.Value;

			UpdateControls(index.Value);
		}


		void OnTextureChanged(object sender, EventArgs ea)
		{
			string	tex	=sender as string;
			if(tex == null)
			{
				return;
			}

			if(mPB != null && mCurSelection >= 0)
			{
				mPB.SetTextureByIndex(mCurSelection, tex);
			}
		}


		void OnValueChanged(object sender, EventArgs ea)
		{
			if(mCurSelection < 0)
			{
				return;
			}

			ParticleLib.Emitter	em	=mPB.GetEmitterByIndex(mCurSelection);
			if(em == null)
			{
				return;
			}

			mPF.UpdateEmitter(em);
		}


		void OnCopyEmitterToClipBoard(object sender, EventArgs ea)
		{
			Nullable<int>	index	=sender as Nullable<int>;
			if(index == null)
			{
				return;
			}

			string	ent	=mPB.GetEmitterEntityString(index.Value);
			if(ent != null && ent != "")
			{
				System.Windows.Forms.Clipboard.SetText(ent);
			}
		}


		void OnPasteEmitterFromClipBoard(object sender, EventArgs ea)
		{
			string	ent	=System.Windows.Forms.Clipboard.GetText();

			mPB.CreateEmitterFromQuArK(ent);

			UpdateListView();
		}


		void UpdateListView()
		{
			int	count	=mPB.GetEmitterCount();
			if(count <= 0)
			{
				return;
			}

			List<string>	emitters	=new List<string>();
			List<int>		indexes		=new List<int>();

			int	j=0;
			for(int i=0;j < count;i++)
			{
				ParticleLib.Emitter	em	=mPB.GetEmitterByIndex(i);

				if(em == null)
				{
					continue;
				}

				emitters.Add("Emitter" + string.Format("{0:000}", i));
				indexes.Add(i);
				j++;
			}

			mPF.UpdateListView(emitters, indexes);
		}


		void UpdateControls(int index)
		{
			if(index < 0)
			{
				return;
			}

			mPF.UpdateControls(mPB.GetEmitterByIndex(index),
				mPB.GetTextureByIndex(index));
		}


		internal void Update(float msDelta)
		{
			mPB.Update(mGD.DC, msDelta);
		}


		internal void Draw()
		{
			mPB.Draw(mGD.DC, mGD.GCam.View, mGD.GCam.Projection);
		}


		internal void DrawDMN()
		{
			mPB.DrawDMN(mGD.DC, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);
		}
	}
}
