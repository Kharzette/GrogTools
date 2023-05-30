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
			TreeNode	sel	=CollisionTreeView.SelectedNode;
			if(sel == null)
			{
				ShapeGroup.Enabled	=false;
				EditNode.Enabled	=false;
			}
			else
			{
				ShapeGroup.Enabled	=true;
				EditNode.Enabled	=true;

				CollisionNode	cn	=sel.Tag as CollisionNode;
				if(cn == null)
				{
					RadioBox.Checked	=true;
					EditNode.Enabled	=false;
				}

				int	shape	=cn.GetShape();
				if(shape == CollisionNode.Box)
				{
					RadioBox.Checked	=true;
				}
				else if(shape == CollisionNode.Sphere)
				{
					RadioSphere.Checked	=true;
				}
				else if(shape == CollisionNode.Capsule)
				{
					RadioCapsule.Checked	=true;
				}
				else
				{
					ShapeGroup.Enabled		=false;
					RadioBox.Checked		=false;
					RadioSphere.Checked		=false;
					RadioCapsule.Checked	=false;
				}
			}
		}


		void OnNodeShapeChanged(object sender, EventArgs e)
		{
			TreeNode	sel	=CollisionTreeView.SelectedNode;
			if(sel == null)
			{
				return;		//shouldn't happen
			}

			CollisionNode	cn	=sel.Tag as CollisionNode;
			if(cn == null)
			{
				//also shouldn't happen
				return;
			}

			if(RadioBox.Checked)
			{
				cn.ChangeShape(CollisionNode.Box);
			}
			else if(RadioSphere.Checked)
			{
				cn.ChangeShape(CollisionNode.Sphere);
			}
			else if(RadioCapsule.Checked)
			{
				cn.ChangeShape(CollisionNode.Capsule);
			}
		}
	}
}
