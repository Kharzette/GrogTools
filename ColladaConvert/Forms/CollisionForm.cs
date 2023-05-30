using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vortice.Mathematics;
using System.Numerics;
using MeshLib;


namespace ColladaConvert.Forms
{
	public partial class CollisionForm : Form
	{
		CollisionTree	mColTree;


		public CollisionForm()
		{
			InitializeComponent();

			mColTree	=new CollisionTree();
		}


		private void OnAddChild(object sender, EventArgs e)
		{
			TreeNode	tn	=CollisionTreeView.SelectedNode;

			if(tn == null)
			{
				//create at root
				CollisionNode	root	=mColTree.GetRoot();
				if(root == null)
				{
					root	=new CollisionNode(new BoundingBox(-Vector3.One, Vector3.One));
					mColTree.SetRoot(root);
				}
			}
			else
			{
				CollisionNode	?cn	=tn.Tag as CollisionNode;
				if(cn == null)
				{
					return;
				}

				cn.CreateKid(new BoundingBox(-Vector3.One, Vector3.One));
			}

			ReBuildTree();
		}


		void ReBuildTree()
		{
			CollisionTreeView.Nodes.Clear();

			CollisionNode	root	=mColTree.GetRoot();

			TreeNode	tn	=new TreeNode("root");

			tn.Tag	=root;

			CollisionTreeView.Nodes.Add(tn);

			BuildRecursive(tn);

			CollisionTreeView.ExpandAll();
		}


		void BuildRecursive(TreeNode cur)
		{
			CollisionNode	?curNode	=cur.Tag as CollisionNode;
			if(curNode == null)
			{
				return;
			}

			foreach(CollisionNode kid in curNode.mKids)
			{
				TreeNode	tn	=new TreeNode("kid" + curNode.mKids.IndexOf(kid));

				tn.Tag	=kid;

				cur.Nodes.Add(tn);

				BuildRecursive(tn);
			}
		}


		void OnAfterSelect(object sender, TreeViewEventArgs e)
		{
		}
	}
}
