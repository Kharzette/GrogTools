using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshLib;
using InputLib;


namespace ColladaConvert;

internal class BoneBoundEdit
{
	CommonPrims	mPrims;
	bool		mbActive;		//edit mode?
	int			mBoneIndex;		//active bone
	string		mBoneName;		//active bone name


	internal BoneBoundEdit(CommonPrims cp, SkeletonEditor se)
	{
		mPrims	=cp;

		se.eAdjustBone			+=OnAdjustBone;
		se.eBonesChanged		+=OnBonesChanged;
		se.eChangeBoundShape	+=OnChangeBoundShape;
	}


	void OnAdjustBone(object sender, EventArgs ea)
	{
	}
	void OnBonesChanged(object sender, EventArgs ea)
	{
	}
	void OnChangeBoundShape(object sender, EventArgs ea)
	{
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
