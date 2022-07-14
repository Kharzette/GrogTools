using System.Numerics;
using System.Collections.Generic;
using Vortice.Mathematics;
using Vortice.Direct3D11;
using MeshLib;
using UtilityLib;
using MaterialLib;
using InputLib;


namespace ColladaConvert;

internal class BoneBoundEdit
{
	CommonPrims			mCPrims;
	Mesh.MeshAndArch	?mMAA;
	SkeletonEditor		mEditor;

	bool		mbActive;		//edit mode?
	int			mBoneIndex;		//active bone
	string		?mBoneName;		//active bone name


	internal BoneBoundEdit(CommonPrims cp, SkeletonEditor se)
	{
		mCPrims	=cp;
		mEditor	=se;

		se.eAdjustBone			+=OnAdjustBone;
		se.eBonesChanged		+=OnBonesChanged;
		se.eChangeBoundShape	+=OnChangeBoundShape;
	}


	internal void Render()
	{
		if(!mbActive)
		{
			return;
		}
		Skin		?sk		=mMAA?.mArch.GetSkin();
		Skeleton	skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
		{
			return;
		}

		Matrix4x4	mat	=sk.GetBoneByNameNoBind(mBoneName, skel);

		mCPrims.DrawCapsule(mBoneIndex, Matrix4x4.Transpose(mat));
	}


	void OnAdjustBone(object ?sender, EventArgs ea)
	{
		if(mbActive)
		{
			//toggle
			mbActive	=false;
			return;
		}

		Skeleton	?skel	=mEditor.GetSkeleton();
		if(skel == null)
		{
			return;
		}

		mBoneName	=sender as string;
		if(mBoneName == null)
		{
			mBoneIndex	=-1;
			mbActive	=false;
		}
		else
		{
			mBoneIndex	=skel.GetBoneIndex(mBoneName);
			mbActive	=true;
		}
	}

	void OnBonesChanged(object ?sender, EventArgs ea)
	{
	}
	void OnChangeBoundShape(object ?sender, EventArgs ea)
	{
	}


	//when a new one is loaded/created
	internal void MeshChanged(object ?mesh)
	{
		mMAA	=mesh as Mesh.MeshAndArch;

		CharacterArch	?ca	=mMAA?.mArch as CharacterArch;

		ca?.BuildDebugBoundDrawData(mCPrims);
	}

	
	internal void AdjustBone(List<Input.InputAction> acts)
	{
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(Program.MyActions.BoneLengthUp))
			{
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusUp))
			{
			}
			else if(act.mAction.Equals(Program.MyActions.BoneLengthDown))
			{
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusDown))
			{
			}
			else if(act.mAction.Equals(Program.MyActions.BoneDone))
			{
			}
		}
	}
}
