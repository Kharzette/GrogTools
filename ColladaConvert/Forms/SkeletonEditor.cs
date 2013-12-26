using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshLib;


namespace ColladaConvert
{
	public partial class SkeletonEditor : Form
	{
		Skeleton	mSkeleton;


		public SkeletonEditor()
		{
			InitializeComponent();
		}


		public void Initialize(Skeleton skel)
		{
			SkeletonTree.Nodes.Clear();

			mSkeleton	=skel;
			skel.IterateStructure(IterateStructure);

			SkeletonTree.ExpandAll();
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
	}
}
