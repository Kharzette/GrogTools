using System;
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

	MatLib			mMatLib;
	AnimLib			mAnimLib;
	CommonPrims		mCPrims;
	BoneBoundEdit	mBBE;

	ID3D11Device	mGD;

	float	mScaleFactor;


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

		mBBE	=new BoneBoundEdit(mCPrims, mSKE);

		BindPositions();
		BindEvents();

		mAF.Visible		=true;
		mMF.Visible		=true;
		mSKE.Visible	=true;
		mCT.Visible		=true;
		mOut.Visible	=true;
	}


	internal void AdjustBone(List<Input.InputAction> acts)
	{
		mBBE.AdjustBone(acts);
	}


	internal float GetScaleFactor()
	{
		return	mScaleFactor;
	}


	internal void RenderUpdate(GameCamera gcam, Vector3 lightDir, float updateTime)
	{
		mMatLib.SetLightDirection(lightDir);

		mCPrims.Update(gcam, lightDir);

		mAF.RenderUpdate(updateTime);
	}


	internal void Render()
	{
		mAF.Render(mGD.ImmediateContext);

		if(mAF.GetDrawAxis())
		{
			mCPrims.DrawAxis();
		}

		if(mAF.GetDrawBound())
		{
//			if(mAF.GetBoundChoice() == BoundChoice.Sphere)
//			{
//				mCPrims.DrawSphere(Matrix4x4.Identity);
//			}
//			else
//			{
//				mCPrims.DrawBox(Matrix4x4.Identity);
//			}
		}

		mBBE.Render();
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

		StaticMesh	sm	=sender as StaticMesh;
		Character	chr	=sender as Character;

		BoundingBox		box;
		BoundingSphere	sph;

		if(sm != null)
		{
			sm.GenerateRoughBounds();
			sm.GetRoughBounds(out box, out sph);
		}
		else
		{
			chr.GenerateRoughBounds();
			chr.GetRoughBounds(out box, out sph);
		}

		mCPrims.AddBox(0, box);
		mCPrims.AddSphere(0, sph);
	}

	void OnAFMeshChanged(object ?sender, EventArgs ea)
	{
		mMF.SetMesh(sender);
		mBBE.MeshChanged(sender);
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

		StaticMesh	sm	=sender as StaticMesh;
		Character	chr	=sender as Character;

		if(sm != null)
		{
			sm.GenTangents(mGD, oea.mObj as List<int>, mMF.GetTexCoordSet());
		}
		else
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
	}


	void UnBindEvents()
	{
		//anim form events
		mAF.eBoundReCompute		-=OnAFBoundReCompute;
		mAF.eMeshChanged		-=OnAFMeshChanged;
		mAF.ePrint				-=OnAnyPrint;
		mAF.eSkeletonChanged	-=OnAFSkelChanged;
		mAF.eScaleFactorDecided	-=OnAFScaleFactorDecided;

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
	}
}
