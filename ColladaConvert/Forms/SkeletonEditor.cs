using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace ColladaConvert
{
	public partial class SkeletonEditor : Form
	{
		Skeleton	mSkeleton;

		internal event EventHandler	eSelectUnUsedBones;
		internal event EventHandler	eBonesChanged;
		internal event EventHandler	ePrint;
		internal event EventHandler	eAdjustBone;
		internal event EventHandler	eChangeBoundShape;


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


		internal Skeleton GetSkeleton()
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

			//re-enable adjustish controls
			AdjustBoneBound.Enabled	=true;
			RadioBox.Enabled		=true;
			RadioSphere.Enabled		=true;
			RadioCapsule.Enabled	=true;
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


		void DeleteBone(object sender, EventArgs e)
		{
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
			TreeNode	toAdj	=SkeletonTree.SelectedNode;
			if(toAdj == null)
			{
				return;
			}

			string	bone;
			string	msg;

			//disable tree till adjusting done
			SkeletonTree.Enabled	=false;

			//disable shape and adjust button too
			AdjustBoneBound.Enabled	=false;
			RadioBox.Enabled		=false;
			RadioSphere.Enabled		=false;
			RadioCapsule.Enabled	=false;

			bone	=toAdj.Name;
			msg		="Adjusting bound of " + toAdj.Name
					+ ".  Use R / Shift-R to adjust radius, T / Shift-T to adjust "
					+ "length along the bone axis,\n"
					+ "M to mirror to opposite side (if possible), X when finished.\n";

			Misc.SafeInvoke(ePrint, msg);
			Misc.SafeInvoke(eAdjustBone, bone);
		}


		void BoundShapeChanged(object sender, EventArgs e)
		{
			string	shape	="";

			if(RadioBox.Checked)
			{
				shape	=RadioBox.Text;
			}
			else if(RadioSphere.Checked)
			{
				shape	=RadioSphere.Text;
			}
			else if(RadioCapsule.Checked)
			{
				shape	=RadioCapsule.Text;
			}

			Misc.SafeInvoke(eChangeBoundShape, shape);
		}
	}
}
