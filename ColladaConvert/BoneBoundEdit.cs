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
	Character			?mChr;
	SkeletonEditor		mEditor;
	AnimForm			mAForm;

	bool		mbActive;		//edit mode?
	bool		mbRoughMode;	//rough edit mode?
	int			mBoneIndex;		//active bone
	string		?mBoneName;		//active bone name

	//values in meters
	const float	RadiusIncrement			=0.03f;
	const float	LengthIncrement			=0.03f;
	const int	RoughIndex				=6969;


	internal BoneBoundEdit(CommonPrims cp, SkeletonEditor se, AnimForm af)
	{
		mCPrims	=cp;
		mEditor	=se;
		mAForm	=af;

		se.eAdjustBone			+=OnAdjustBone;
		se.eBonesChanged		+=OnBonesChanged;
		se.eChangeBoundShape	+=OnChangeBoundShape;
		se.eRequestShape		+=OnRequestShape;

		af.eBoundAdjust			+=OnRoughAdjust;
	}


	internal void Render()
	{
		if(mChr == null)
		{
			return;
		}

		Skin		?sk		=mChr.GetSkin();
		Skeleton	?skel	=mEditor.GetSkeleton();

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
					mCPrims.DrawBox(i, mat, Vector4.One * 0.5f);
				}
				if(choice == Skin.Sphere)
				{
					mCPrims.DrawSphere(i, mat, Vector4.One * 0.5f);
				}
				if(choice == Skin.Capsule)
				{
					mCPrims.DrawCapsule(i, mat, Vector4.One * 0.5f);
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
			mCPrims.DrawBox(mBoneIndex, actMat, selectedColor);
		}
		else if(selChoice == Skin.Sphere)
		{
			mCPrims.DrawSphere(mBoneIndex, actMat, selectedColor);
		}
		else if(selChoice == Skin.Capsule)
		{
			mCPrims.DrawCapsule(mBoneIndex, actMat, selectedColor);
		}
	}


	void OnRoughAdjust(object ?sender, EventArgs ea)
	{
		mbRoughMode	=!mbRoughMode;
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
		if(sender == null || !mbActive || mChr == null)
		{
			return;
		}

		Skin	?sk	=mChr.GetSkin();
		if(sk == null)
		{
			return;
		}

		sk.SetBoundChoice(mBoneIndex, (int)sender);

		mChr.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}

	void OnRequestShape(object ?sender, BoundChoiceEventArgs ea)
	{
		if(sender == null || mChr == null)
		{
			return;
		}

		int	idx	=(int)sender;

		Skin	?sk		=mChr.GetSkin();
		if(sk == null)
		{
			return;
		}

		ea.mChoice	=sk.GetBoundChoice(idx);
	}


	//when a new one is loaded/created
	internal void MeshChanged(object ?mesh)
	{
		mChr	=mesh as Character;

		if(mChr != null)
		{
			mChr.BuildDebugBoundDrawData(mCPrims);
		}
	}


	void Snap()
	{
		Skin	?sk	=mChr?.GetSkin();
		if(sk == null)
		{
			return;
		}

		sk.SnapBoneBoundToJoint(mBoneIndex);
	}


	void Mirror()
	{
		Skin			?sk		=mChr?.GetSkin();
		Skeleton		?skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
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

		mChr?.BuildDebugBoundDrawData(mirIdx, mCPrims);
	}


	void IncDecLength(float amount)
	{
		Skin			?sk		=mChr?.GetSkin();
		Skeleton		?skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
		{
			return;
		}

		sk?.AdjustBoneBoundLength(mBoneIndex, amount);

		mChr?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}


	void IncDecRadius(float amount)
	{
		Skin			?sk		=mChr?.GetSkin();
		Skeleton		?skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
		{
			return;
		}

		sk?.AdjustBoneBoundRadius(mBoneIndex, amount);

		mChr?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}


	void IncDecDepth(float amount)
	{
		Skin			?sk		=mChr?.GetSkin();
		Skeleton		?skel	=mEditor.GetSkeleton();

		if(sk == null || skel == null)
		{
			return;
		}

		sk?.AdjustBoneBoundDepth(mBoneIndex, amount);

		//this is probably overkill
		mChr?.BuildDebugBoundDrawData(mBoneIndex, mCPrims);
	}


	void IncDecRoughLength(float amount)
	{
		mChr?.AdjustBoundLength(amount, mAForm.GetBoundChoice());

		BoundingBox		?box	=null;
		BoundingSphere	?sph	=null;

		mChr?.GetRoughBounds(out box, out sph);

		if(box == null || sph == null)
		{
			return;
		}
		mCPrims.AddBox(RoughIndex, box.Value);
		mCPrims.AddSphere(RoughIndex, sph.Value);
	}


	void IncDecRoughRadius(float amount)
	{
		mChr?.AdjustBoundRadius(amount, mAForm.GetBoundChoice());

		BoundingBox		?box	=null;
		BoundingSphere	?sph	=null;

		mChr?.GetRoughBounds(out box, out sph);

		if(box == null || sph == null)
		{
			return;
		}
		mCPrims.AddBox(RoughIndex, box.Value);
		mCPrims.AddSphere(RoughIndex, sph.Value);
	}


	void IncDecRoughDepth(float amount)
	{
		mChr?.AdjustBoundDepth(amount, mAForm.GetBoundChoice());

		BoundingBox		?box	=null;
		BoundingSphere	?sph	=null;

		mChr?.GetRoughBounds(out box, out sph);

		if(box == null || sph == null)
		{
			return;
		}
		mCPrims.AddBox(RoughIndex, box.Value);
		mCPrims.AddSphere(RoughIndex, sph.Value);
	}


	internal void AdjustBone(List<Input.InputAction> acts)
	{
		if(!mbActive && !mbRoughMode)
		{
			return;
		}

		if(mbActive)
		{
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
				else if(act.mAction.Equals(Program.MyActions.BoneDepthUp))
				{
					IncDecDepth(RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneLengthDown))
				{
					IncDecLength(-LengthIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneRadiusDown))
				{
					IncDecRadius(-RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneDepthDown))
				{
					IncDecDepth(-RadiusIncrement);
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
				else if(act.mAction.Equals(Program.MyActions.BoneSphereSnap))
				{
					Snap();
				}
			}
		}
		else	//rough adjust
		{
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(Program.MyActions.BoneLengthUp))
				{
					IncDecRoughLength(LengthIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneRadiusUp))
				{
					IncDecRoughRadius(RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneDepthUp))
				{
					IncDecRoughDepth(RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneLengthDown))
				{
					IncDecRoughLength(-LengthIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneRadiusDown))
				{
					IncDecRoughRadius(-RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneDepthDown))
				{
					IncDecRoughDepth(-RadiusIncrement);
				}
				else if(act.mAction.Equals(Program.MyActions.BoneDone))
				{
					mbRoughMode	=false;
					mAForm.DoneBoneAdjust();
				}
			}
		}
	}
}