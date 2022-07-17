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

	//values in meters
	const float	RadiusIncrement	=0.05f;
	const float	LengthIncrement	=0.05f;


	internal BoneBoundEdit(CommonPrims cp, SkeletonEditor se)
	{
		mCPrims	=cp;
		mEditor	=se;

		se.eAdjustBone			+=OnAdjustBone;
		se.eBonesChanged		+=OnBonesChanged;
		se.eChangeBoundShape	+=OnChangeBoundShape;
		se.eRequestShape		+=OnRequestShape;
	}


	internal void Render()
	{
		Skin		?sk		=mMAA?.mArch.GetSkin();
		Skeleton	skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
		{
			return;
		}

		if(mEditor.GetDrawBounds())
		{
			for(int i=0;i < skel.GetNumIndexedBones();i++)
			{
				if(mbActive && i == mBoneIndex)
				{
					continue;
				}

				int			choice	=sk.GetBoundChoice(i);
				Matrix4x4	mat		=sk.GetBoneByIndexNoBind(i, skel);

				if(choice == Skin.Box)
				{
					mCPrims.DrawBox(i, Matrix4x4.Transpose(mat), Vector4.One * 0.5f);
				}
				if(choice == Skin.Sphere)
				{
					mCPrims.DrawSphere(i, Matrix4x4.Transpose(mat), Vector4.One * 0.5f);
				}
				if(choice == Skin.Capsule)
				{
					mCPrims.DrawCapsule(i, Matrix4x4.Transpose(mat), Vector4.One * 0.5f);
				}
			}
		}

		if(!mbActive)
		{
			return;
		}

		Matrix4x4	actMat	=sk.GetBoneByNameNoBind(mBoneName, skel);

		Vector4	selectedColor	=Vector4.One * 0.5f;
		selectedColor.X	=1f;

		int	selChoice	=sk.GetBoundChoice(mBoneIndex);

		if(selChoice == Skin.Box)
		{
			mCPrims.DrawBox(mBoneIndex, Matrix4x4.Transpose(actMat), selectedColor);
		}
		else if(selChoice == Skin.Sphere)
		{
			mCPrims.DrawSphere(mBoneIndex, Matrix4x4.Transpose(actMat), selectedColor);
		}
		else if(selChoice == Skin.Capsule)
		{
			mCPrims.DrawCapsule(mBoneIndex, Matrix4x4.Transpose(actMat), selectedColor);
		}
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
		if(sender == null || !mbActive)
		{
			return;
		}

		CharacterArch	?ca	=mMAA?.mArch as CharacterArch;
		Skin			?sk	=mMAA?.mArch.GetSkin();
		if(sk == null || ca == null)
		{
			return;
		}

		sk.SetBoundChoice(mBoneIndex, (int)sender);

		ca?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}

	void OnRequestShape(object ?sender, BoundChoiceEventArgs ea)
	{
		if(sender == null)
		{
			return;
		}

		int	idx	=(int)sender;

		Skin	?sk		=mMAA?.mArch.GetSkin();
		if(sk == null)
		{
			return;
		}

		ea.mChoice	=sk.GetBoundChoice(idx);
	}


	//when a new one is loaded/created
	internal void MeshChanged(object ?mesh)
	{
		mMAA	=mesh as Mesh.MeshAndArch;

		CharacterArch	?ca	=mMAA?.mArch as CharacterArch;

		ca?.BuildDebugBoundDrawData(mCPrims);
	}


	void Mirror()
	{
		Skin			?sk		=mMAA?.mArch.GetSkin();
		Skeleton		skel	=mEditor.GetSkeleton();
		CharacterArch	?ca		=mMAA?.mArch as CharacterArch;

		if(sk == null || skel == null || ca == null)
		{
			return;
		}

		string	mirror	=skel.GetBoneNameMirror(mBoneName);
		if(mirror == null)
		{
			return;
		}

		int	mirIdx	=skel.GetBoneIndex(mirror);

		sk.CopyBound(mBoneIndex, mirIdx);

		//this is probably overkill
		ca?.BuildDebugBoundDrawData(mirIdx, mCPrims);
	}


	void IncDecLength(float amount)
	{
		Skin			?sk		=mMAA?.mArch.GetSkin();
		Skeleton		skel	=mEditor.GetSkeleton();
		CharacterArch	?ca		=mMAA?.mArch as CharacterArch;

		if(sk == null || skel == null || ca == null)
		{
			return;
		}

		sk?.AdjustBoneBoundLength(mBoneIndex, amount);

		//this is probably overkill
		ca?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}


	void IncDecRadius(float amount)
	{
		Skin			?sk		=mMAA?.mArch.GetSkin();
		Skeleton		skel	=mEditor.GetSkeleton();
		CharacterArch	?ca		=mMAA?.mArch as CharacterArch;

		if(sk == null || skel == null || ca == null)
		{
			return;
		}

		sk?.AdjustBoneBoundRadius(mBoneIndex, amount);

		//this is probably overkill
		ca?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}


	internal void AdjustBone(List<Input.InputAction> acts)
	{
		if(!mbActive)
		{
			return;
		}

		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(Program.MyActions.BoneLengthUp))
			{
				IncDecLength(LengthIncrement);
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusUp))
			{
				IncDecRadius(RadiusIncrement);
			}
			else if(act.mAction.Equals(Program.MyActions.BoneLengthDown))
			{
				IncDecLength(-LengthIncrement);
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusDown))
			{
				IncDecRadius(-RadiusIncrement);
			}
			else if(act.mAction.Equals(Program.MyActions.BoneDone))
			{
				mbActive	=false;
				mEditor.DoneBoneAdjust();
			}
			else if(act.mAction.Equals(Program.MyActions.BoneMirror))
			{
				Mirror();
			}
		}
	}
}
