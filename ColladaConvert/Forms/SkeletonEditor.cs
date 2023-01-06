using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace ColladaConvert;

public class BoundChoiceEventArgs : EventArgs
{
	public int	mChoice;
}

public partial class SkeletonEditor : Form
{
	Skeleton	?mSkeleton;
	bool		mbSelectionChanging;

	internal event EventHandler	?eSelectUnUsedBones;
	internal event EventHandler	?eBonesChanged;
	internal event EventHandler	?ePrint;
	internal event EventHandler	?eAdjustBone;
	internal event EventHandler	?eChangeBoundShape;

	internal event EventHandler<BoundChoiceEventArgs>	?eRequestShape;		//kind of backwards


	public SkeletonEditor()
	{
		InitializeComponent();
	}


	public void Initialize(Skeleton ?skel)
	{
		if(skel == null)
		{
			return;
		}

		SkeletonTree.Nodes.Clear();

		mSkeleton	=skel;
		skel.IterateStructure(IterateStructure);

		SkeletonTree.ExpandAll();
	}


	internal Skeleton? GetSkeleton()
	{
		return	mSkeleton;
	}


	internal bool GetDrawBounds()
	{
		return	DrawBounds.Checked;
	}


	internal void DoneBoneAdjust()
	{
		//re-enable tree
		SkeletonTree.Enabled	=true;

		//re-enable adjust button
		AdjustBoneBound.Enabled	=true;

		//disable radios
		RadioBox.Enabled		=false;
		RadioSphere.Enabled		=false;
		RadioCapsule.Enabled	=false;
	}


	void IterateStructure(string boneName, string parent)
	{
		Debug.WriteLine(boneName + ", " + parent);

		TreeNode	tn	=new TreeNode();

		tn.Text	=boneName;
		tn.Name	=boneName;

		if(parent != null)
		{
			TreeNode	[]found		=SkeletonTree.Nodes.Find(parent, true);

			Debug.Assert(found.Length == 1);

			found[0].Nodes.Add(tn);
		}
		else
		{
			SkeletonTree.Nodes.Add(tn);
		}
	}


	void DeleteBone(object ?sender, EventArgs ?e)
	{
		if(mSkeleton == null)
		{
			return;
		}

		TreeNode	toNuke	=SkeletonTree.SelectedNode;

		mSkeleton.NukeBone(toNuke.Name);

		//remove from tree
		toNuke.Remove();

		//Need to make characters remake the bone array
		Misc.SafeInvoke(eBonesChanged, null);
	}


	void OnSelectUnUsedBones(object sender, EventArgs ea)
	{
		List<string>	boneNames	=new List<string>();

		Misc.SafeInvoke(eSelectUnUsedBones, boneNames);

		foreach(TreeNode n in SkeletonTree.Nodes)
		{
			SelectThese(n, boneNames);
		}
	}


	void SelectThese(TreeNode n, List<string> names)
	{
		if(!names.Contains(n.Name))
		{
			n.BackColor	=System.Drawing.Color.Red;
		}

		foreach(TreeNode kid in n.Nodes)
		{
			SelectThese(kid, names);
		}
	}


	void OnTreeKeyUp(object sender, KeyEventArgs e)
	{
		if(!SkeletonTree.Focused)
		{
			return;
		}

		if(e.KeyCode == Keys.Delete)
		{
			DeleteBone(null, null);
			e.Handled	=true;
		}
		else if(e.KeyCode == Keys.F4)
		{
			SkeletonTree.ExpandAll();
		}
//			else if(e.KeyCode == Keys.F2)
//			{
//				OnRenameEntity(null, null);
//			}
	}


	void OnAdjustBone(object sender, EventArgs e)
	{
		if(mSkeleton == null)
		{
			return;
		}

		TreeNode	toAdj	=SkeletonTree.SelectedNode;
		if(toAdj == null)
		{
			return;
		}

		string	bone;
		string	msg;

		//disable tree till adjusting done
		SkeletonTree.Enabled	=false;

		//disable adjust button too
		AdjustBoneBound.Enabled	=false;

		//enable shape changes
		RadioBox.Enabled		=true;
		RadioSphere.Enabled		=true;
		RadioCapsule.Enabled	=true;

		int	idx	=mSkeleton.GetBoneIndex(toAdj.Name);

		//eventargs will return choice
		BoundChoiceEventArgs	bcea	=new BoundChoiceEventArgs();

		bcea.mChoice	=Skin.Invalid;

		Misc.SafeInvoke(eRequestShape, idx, bcea);

		bone	=toAdj.Name;
		msg		="Adjusting bound of " + toAdj.Name +".\n";

		if(bcea.mChoice == Skin.Box)
		{
			msg	+="Use R / Shift-R to adjust width, Y / Shift-Y to adjust depth,\n"
				+ "T / Shift-T to adjust length along the bone axis,\n"
				+ "M to mirror to opposite side (if possible), X when finished.\n";
		}
		else if(bcea.mChoice == Skin.Sphere)
		{
			msg	+="Use R / Shift-R to adjust radius, C to snap to joint pos,\n"
				+ "T / Shift-T to move along the bone axis,\n"
				+ "M to mirror to opposite side (if possible), X when finished.\n";
		}
		else if(bcea.mChoice == Skin.Capsule)
		{
			msg	+="Use R / Shift-R to adjust radius, T / Shift-T to adjust "
				+ "length along the bone axis,\n"
				+ "M to mirror to opposite side (if possible), X when finished.\n";
		}


		Misc.SafeInvoke(ePrint, msg);
		Misc.SafeInvoke(eAdjustBone, bone);
	}


	void BoundShapeChanged(object sender, EventArgs e)
	{
		if(mbSelectionChanging)
		{
			return;	//only react to user changes
		}

		int	choice	=Skin.Invalid;

		if(RadioBox.Checked)
		{
			choice	=Skin.Box;
		}
		else if(RadioSphere.Checked)
		{
			choice	=Skin.Sphere;
		}
		else if(RadioCapsule.Checked)
		{
			choice	=Skin.Capsule;
		}

		Misc.SafeInvoke(eChangeBoundShape, choice);
	}

	void OnTreeAfterSelect(object sender, TreeViewEventArgs e)
	{
		if(mSkeleton == null)
		{
			return;
		}
		
		TreeNode	toAdj	=SkeletonTree.SelectedNode;
		if(toAdj == null)
		{
			AdjustBoneBound.Enabled	=false;
			return;
		}

		int	idx	=mSkeleton.GetBoneIndex(toAdj.Name);
		if(idx == -1)
		{
			AdjustBoneBound.Enabled	=false;
			return;
		}

		//this lets the radio event know
		//that this method is active
		//and to ignore changes
		mbSelectionChanging	=true;

		//valid node selected?  Enable controls
		AdjustBoneBound.Enabled	=true;


		//eventargs will return choice
		BoundChoiceEventArgs	bcea	=new BoundChoiceEventArgs();

		bcea.mChoice	=Skin.Invalid;

		Misc.SafeInvoke(eRequestShape, idx, bcea);

		//set radio buttons to current shape
		if(bcea.mChoice == Skin.Box)
		{
			RadioBox.Checked		=true;
		}
		else if(bcea.mChoice == Skin.Sphere)
		{
			RadioSphere.Checked		=true;
		}
		else if(bcea.mChoice == Skin.Capsule)
		{
			RadioCapsule.Checked	=true;
		}
		else
		{
			//bone with no index or something
			AdjustBoneBound.Enabled	=false;
		}

		mbSelectionChanging	=false;
	}
}
