﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using SharedForms;
using InputLib;
using MaterialLib;
using MeshLib;
using UtilityLib;
using Vortice.Direct3D11;
using Vortice.Mathematics;

//renderform and renderloop
using SharpDX.Windows;

using MatLib	=MaterialLib.MaterialLib;
using Color		=Vortice.Mathematics.Color;
using ColladaConvert.Forms;

namespace ColladaConvert;

//hold all the winforms stuff and events and such
internal class FormStuff
{
	AnimForm		mAF;
	StripElements	mSE;
	SkeletonEditor	mSKE;
	MaterialForm	mMF;
	CelTweakForm	mCT;
	Output			mOut;
	SeamEditor		mSME;
	CollisionForm	mCF;

	MatLib			mMatLib;
	AnimLib			mAnimLib;
	CommonPrims		mCPrims;
	BoneBoundEdit	mBBE;

	ID3D11Device	mGD;

	float		mScaleFactor;
	bool		mbRoughAdjust;
	Vector4		mLightArrowColour;
	Matrix4x4	mLightArrowMat;

	const int	RoughIndex	=6969;


	internal FormStuff(ID3D11Device gd, StuffKeeper sk)
	{
		mMatLib		=new MatLib(sk);
		mAnimLib	=new AnimLib();
		mCPrims		=new CommonPrims(gd, sk);
		mGD			=gd;

		//cel shading eventually
//		matLib.InitCelShading(1);
//		matLib.GenerateCelTexturePreset(gd.GD,
//			gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
//		matLib.SetCelTexture(0);

		mScaleFactor	=1f;	//start at meters?

		mAF		=new AnimForm(gd, mMatLib, mAnimLib, sk);
		mSE		=new StripElements();
		mSKE	=new SkeletonEditor();
		mMF		=new MaterialForm(mMatLib, sk);
		mCT		=new CelTweakForm(gd, mMatLib);
		mOut	=new Output();
		mSME	=new SeamEditor();
		mCF		=new CollisionForm(gd, sk, mOut);

		mBBE	=new BoneBoundEdit(mCPrims, mSKE, mAF);

		BindPositions();
		BindEvents();

		mAF.Visible		=true;
		mMF.Visible		=true;
		mSKE.Visible	=true;
		mCT.Visible		=true;
		mOut.Visible	=true;
		mCF.Visible		=true;

		mLightArrowColour	=Misc.SystemColorToV4Color(System.Drawing.Color.Gold);

		mOut.Print("Starting with meter scale factor.\n");
	}


	internal void AdjustBone(List<Input.InputAction> acts)
	{
		mCF.UpdateKeys(acts);
		mBBE.AdjustBone(acts);

		if(!mbRoughAdjust)
		{
			return;
		}

		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(Program.MyActions.BoneDone))
			{
				mbRoughAdjust	=false;
			}
		}
	}


	internal float GetScaleFactor()
	{
		return	mScaleFactor;
	}


	internal void RenderUpdate(GameCamera gcam, Vector3 lightDir, float updateTime)
	{
		//get a good side perp vec
		Vector3	lightSide	=Vector3.Cross(Vector3.UnitY, lightDir);
		if(lightSide.Equals(Vector3.Zero))
		{
			lightSide	=Vector3.Cross(Vector3.UnitX, lightDir);
		}

		//good up vec
		Vector3	lightUp	=Vector3.Cross(lightSide, lightDir);

		Vector3	camPos	=Vector3.UnitY * mScaleFactor * 3f;

		//the arrow mesh is backwards?
		mLightArrowMat	=Matrix4x4.CreateWorld(camPos, -lightDir, -lightUp);

		mMatLib.SetLightDirection(lightDir);

		mCPrims.Update(gcam, lightDir);

		mAF.RenderUpdate(updateTime);
		mCF.RenderUpdate(gcam, lightDir, updateTime);
	}


	internal void Render()
	{
		mAF.Render(mGD.ImmediateContext);

		mCPrims.DrawLightArrow(mLightArrowMat, mLightArrowColour);

		if(mAF.GetDrawAxis())
		{
			mCPrims.DrawAxis();
		}

		if(mAF.GetDrawBound())
		{
			Vector4	selectedColor	=Vector4.One * 0.5f;
			selectedColor.X	=1f;

			if(mAF.GetBoundChoice() == false)
			{
				if(mbRoughAdjust)
				{
					mCPrims.DrawSphere(RoughIndex, Matrix4x4.Identity, selectedColor);
				}
				else
				{
					mCPrims.DrawSphere(RoughIndex, Matrix4x4.Identity, Vector4.One * 0.5f);
				}
			}
			else
			{
				if(mbRoughAdjust)
				{
					mCPrims.DrawBox(RoughIndex, Matrix4x4.Identity, selectedColor);
				}
				else
				{
					mCPrims.DrawBox(RoughIndex, Matrix4x4.Identity, Vector4.One * 0.5f);
				}
			}
		}

		mBBE.Render();
		mCF.Render();
	}


	internal void FreeAll()
	{
		mCPrims.FreeAll();
		mMatLib.FreeAll();

		UnBindEvents();
	}


	#region Anim Form Events
	void OnAFBoundReCompute(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}

		StaticMesh	?sm		=sender as StaticMesh;
		Character	?chr	=sender as Character;

		BoundingBox		?box;
		BoundingSphere	?sph;

		if(sm != null)
		{
			sm.GenerateRoughBounds();
			sm.GetRoughBounds(out box, out sph);
		}
		else if(chr != null)
		{
			chr.GenerateRoughBounds();
			chr.GetRoughBounds(out box, out sph);
		}
		else
		{
			return;
		}

		if(box == null || sph == null)
		{
			return;
		}

		mCPrims.AddBox(RoughIndex, box.Value);
		mCPrims.AddSphere(RoughIndex, sph.Value);
	}

	void OnAFMeshChanged(object ?sender, EventArgs ea)
	{
		mMF.SetMesh(sender);
		mBBE.MeshChanged(sender);
		mCF.SetMesh(sender, mScaleFactor);

		StaticMesh	?sm		=sender as StaticMesh;
		Character	?chr	=sender as Character;

		BoundingBox		?box;
		BoundingSphere	?sph;

		if(sm != null)
		{
			sm.GetRoughBounds(out box, out sph);
		}
		else if(chr != null)
		{
			chr.GetRoughBounds(out box, out sph);
		}
		else
		{
			return;
		}

		if(box == null || sph == null)
		{
			return;
		}

		mCPrims.AddBox(RoughIndex, box.Value);
		mCPrims.AddSphere(RoughIndex, sph.Value);
	}

	void OnAFSkelChanged(object ?sender, EventArgs ea)
	{
		Skeleton	?skel	=sender as Skeleton;
		mSKE.Initialize(skel);
	}

	void OnAFScaleFactorDecided(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}
		mScaleFactor	=(float)sender;
		mCPrims.SetAxisScale(mScaleFactor);
		mOut.Print("Using scale factor " + mScaleFactor + ".\n");
	}

	void OnAFBoundAdjust(object ?sender, EventArgs ea)
	{
		mbRoughAdjust	=true;
	}
	#endregion

	#region Strip Elements Events
	void OnSEDeleteElement(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}
		List<int>	elements	=(List<int>)sender;

		mAF.NukeVertexElement(mSE.GetIndexes(), elements);
		mSE.Populate(null, null);
		mSE.Visible	=false;
		mMF.RefreshMeshPartList();
	}

	void OnSEEscape(object ?sender, EventArgs ea)
	{
		mSE.Populate(null, null);
		mSE.Visible	=false;
	}
	#endregion

	#region Skeleton Editor Events
	void OnSKEAdjustBone(object ?sender, EventArgs ea)
	{
	}
	void OnSKEBonesChanged(object ?sender, EventArgs ea)
	{
	}
	void OnSKEChangeBoundShape(object ?sender, EventArgs ea)
	{
	}
	void OnSKESelectUnUsedBones(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}
		mAF.GetBoneNamesInUseByDraw((List<string>)sender);
	}
	#endregion

	#region Material Form Events
	void OnMFFoundSeams(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}

		if(mSME.IsDisposed)
		{
			mSME	=new SeamEditor();
		}

		mSME.Initialize(mGD);
		mSME.AddSeams((List<EditorMesh.WeightSeam>)sender);
		mSME.SizeColumns();
		mSME.Visible	=true;
	}

	void OnMFGenTangents(object ?sender, ObjEventArgs oea)
	{
		if(sender == null)
		{
			return;
		}

		StaticMesh	?sm		=sender as StaticMesh;
		Character	?chr	=sender as Character;

		if(sm != null)
		{
			sm.GenTangents(mGD, oea.mObj as List<int>, mMF.GetTexCoordSet());
		}
		else if(chr != null)
		{
			chr.GenTangents(mGD, oea.mObj as List<int>, mMF.GetTexCoordSet());
		}
	}

	void OnMFNukedMeshPart(object ?sender, EventArgs ea)
	{
		if(sender == null)
		{
			return;
		}
		mAF.NukeMeshPart((List<int>)sender);
	}

	void OnMFStripElements(object ?sender, ObjEventArgs oea)
	{
		if(mSE.Visible)
		{
			return;
		}
		mSE.Populate(sender, oea.mObj as List<int>);
	}
	#endregion

	void OnAnyPrint(object ?sender, EventArgs ea)
	{
		mOut.Print(sender as string);
	}


	void BindEvents()
	{
		//anim form events
		mAF.eBoundReCompute		+=OnAFBoundReCompute;
		mAF.eMeshChanged		+=OnAFMeshChanged;
		mAF.ePrint				+=OnAnyPrint;
		mAF.eSkeletonChanged	+=OnAFSkelChanged;
		mAF.eScaleFactorDecided	+=OnAFScaleFactorDecided;
		mAF.eBoundAdjust		+=OnAFBoundAdjust;

		//strip elements
		mSE.eDeleteElement	+=OnSEDeleteElement;
		mSE.eEscape			+=OnSEEscape;

		//skeleton editor
		mSKE.eAdjustBone		+=OnSKEAdjustBone;
		mSKE.eBonesChanged		+=OnSKEBonesChanged;
		mSKE.eChangeBoundShape	+=OnSKEChangeBoundShape;
		mSKE.ePrint				+=OnAnyPrint;
		mSKE.eSelectUnUsedBones	+=OnSKESelectUnUsedBones;

		//material form
		mMF.eFoundSeams		+=OnMFFoundSeams;
		mMF.eGenTangents	+=OnMFGenTangents;
		mMF.eNukedMeshPart	+=OnMFNukedMeshPart;
		mMF.eStripElements	+=OnMFStripElements;
		mMF.ePrint			+=OnAnyPrint;
	}


	void UnBindEvents()
	{
		//anim form events
		mAF.eBoundReCompute		-=OnAFBoundReCompute;
		mAF.eMeshChanged		-=OnAFMeshChanged;
		mAF.ePrint				-=OnAnyPrint;
		mAF.eSkeletonChanged	-=OnAFSkelChanged;
		mAF.eScaleFactorDecided	-=OnAFScaleFactorDecided;
		mAF.eBoundAdjust		-=OnAFBoundAdjust;

		//strip elements
		mSE.eDeleteElement	-=OnSEDeleteElement;
		mSE.eEscape			-=OnSEEscape;

		//skeleton editor
		mSKE.eAdjustBone		-=OnSKEAdjustBone;
		mSKE.eBonesChanged		-=OnSKEBonesChanged;
		mSKE.eChangeBoundShape	-=OnSKEChangeBoundShape;
		mSKE.ePrint				-=OnAnyPrint;
		mSKE.eSelectUnUsedBones	-=OnSKESelectUnUsedBones;

		//material form
		mMF.eFoundSeams		-=OnMFFoundSeams;
		mMF.eGenTangents	-=OnMFGenTangents;
		mMF.eNukedMeshPart	-=OnMFNukedMeshPart;
		mMF.eStripElements	-=OnMFStripElements;
		mMF.ePrint			-=OnAnyPrint;
	}


	void BindPositions()
	{
		//save positions
		mMF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "MaterialFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mMF.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "MaterialFormSize", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mAF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "AnimFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mSKE.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "SkeletonEditorFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mSKE.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "SkeletonEditorFormSize", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mCT.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "CelTweakFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mOut.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "OutputFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mOut.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "OutputFormSize", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mSME.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "SeamEditorFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mSME.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "SeamEditorFormSize", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mCF.DataBindings.Add(new System.Windows.Forms.Binding("Location",
			Properties.Settings.Default, "CollisionFormPos", true,
			DataSourceUpdateMode.OnPropertyChanged));

		mCF.DataBindings.Add(new System.Windows.Forms.Binding("Size",
			Properties.Settings.Default, "CollisionFormSize", true,
			DataSourceUpdateMode.OnPropertyChanged));
	}
}
