using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using System.Numerics;
using MaterialLib;
using SharedForms;
using UtilityLib;
using InputLib;
using MeshLib;


namespace ColladaConvert.Forms;

public partial class CollisionForm : Form
{
	CollisionTree	mColTree;
	CommonPrims		mCPrims;
	Output			mOut;
	StaticMesh		?mMesh;

	bool			mbEditMode;
	CollisionNode	mEditNode;
	float			mScaleFactor;

	//values in meters
	const float	RadiusIncrement			=0.004f;
	const float	LengthIncrement			=0.004f;
	const float	MoveAmount				=0.01f;


	public CollisionForm(ID3D11Device gd, StuffKeeper sk, Output op)
	{
		InitializeComponent();

		mCPrims		=new CommonPrims(gd, sk);
		mColTree	=new CollisionTree();
		mOut		=op;

		this.Enabled		=false;
		ShapeGroup.Enabled	=false;
		EditNode.Enabled	=false;
	}


	internal void UpdateKeys(List<Input.InputAction> acts)
	{
		if(!mbEditMode || mEditNode == null)
		{
			return;
		}

		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(Program.MyActions.BoneLengthUp))
			{
				mEditNode.AdjustLength(LengthIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusUp))
			{
				mEditNode.AdjustRadius(RadiusIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneDepthUp))
			{
				mEditNode.AdjustDepth(RadiusIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneLengthDown))
			{
				mEditNode.AdjustLength(-LengthIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneRadiusDown))
			{
				mEditNode.AdjustRadius(-RadiusIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneDepthDown))
			{
				mEditNode.AdjustDepth(-RadiusIncrement * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveX))
			{
				mEditNode.Move(Vector3.UnitX * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveY))
			{
				mEditNode.Move(Vector3.UnitY * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveZ))
			{
				mEditNode.Move(Vector3.UnitZ * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveNX))
			{
				mEditNode.Move(-Vector3.UnitX * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveNY))
			{
				mEditNode.Move(-Vector3.UnitY * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.ColMoveNZ))
			{
				mEditNode.Move(-Vector3.UnitZ * MoveAmount * mScaleFactor);
				AddShapeToCPrims(mEditNode);	//update shape
			}
			else if(act.mAction.Equals(Program.MyActions.BoneDone))
			{
				mbEditMode			=false;
				mEditNode			=null;
				ShapeGroup.Enabled	=true;
				EditNode.Enabled	=true;
				AddChild.Enabled	=true;
			}
		}
	}


	internal void RenderUpdate(GameCamera gcam, Vector3 lightDir, float updateTime)
	{
		mCPrims.Update(gcam, lightDir);
	}


	void	RenderNode(CollisionNode n)
	{
		if(n == null)
		{
			return;		
		}

		Vector4	drawColor	=Vector4.One * 0.5f;

		if(mbEditMode && mEditNode == n)
		{
			drawColor.X	=1f;
		}

		int	hash	=n.GetHashCode();
		int	choice	=n.GetShape();

		if(choice == CollisionNode.Box)
		{
			mCPrims.DrawBox(hash, mMesh.GetTransform(), drawColor);
		}
		else if(choice == CollisionNode.Sphere)
		{
			mCPrims.DrawSphere(hash, mMesh.GetTransform(), drawColor);
		}
		else if(choice == CollisionNode.Capsule)
		{
			mCPrims.DrawCapsule(hash, mMesh.GetTransform(), drawColor);
		}
	}

	void	RenderRecursive(TreeNode tn)
	{
		foreach(TreeNode kid in tn.Nodes)
		{
			RenderRecursive(kid);
		}

		CollisionNode	n	=tn.Tag as CollisionNode;
		if(n == null)
		{
			return;
		}

		if(DrawAll.Checked || tn == CollisionTreeView.SelectedNode)
		{
			RenderNode(n);
		}
	}

	internal void Render()
	{
		if(mMesh == null)
		{
			return;
		}

		if(CollisionTreeView.Nodes.Count == 0)
		{
			return;
		}

		CollisionNode	root	=mColTree.GetRoot();
		if(root == null)
		{
			return;
		}

		RenderRecursive(CollisionTreeView.Nodes[0]);
	}


	public void SetMesh(object ?m, float scaleFactor)
	{
		mMesh	=m as StaticMesh;

		if(mMesh == null)
		{
			this.Enabled	=false;
			mScaleFactor	=1f;
		}
		else
		{
			this.Enabled	=true;
			mScaleFactor	=scaleFactor;
		}
	}


	internal void FreeAll()
	{
		mCPrims.FreeAll();
	}


	void OnAddChild(object sender, EventArgs e)
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


	void AddShapeToCPrims(CollisionNode n)
	{
		if(n == null)
		{
			return;
		}

		int	hash	=n.GetHashCode();
		int	choice	=n.GetShape();

		if(choice == CollisionNode.Box)
		{
			mCPrims.AddBox(hash, n.mBox.Value);
		}
		else if(choice == CollisionNode.Sphere)
		{
			mCPrims.AddSphere(hash, n.mSphere.Value);
		}
		else if(choice == CollisionNode.Capsule)
		{
			mCPrims.AddCapsule(hash, n.mCapsule.Value);
		}
	}


	void ReBuildTree()
	{
		CollisionTreeView.Nodes.Clear();

		CollisionNode	root	=mColTree.GetRoot();
		if(root == null)
		{
			return;
		}

		AddShapeToCPrims(root);

		TreeNode	tn	=new TreeNode("root");

		tn.Tag	=root;

		CollisionTreeView.Nodes.Add(tn);

		BuildRecursive(tn);

		CollisionTreeView.ExpandAll();

		ShapeGroup.Enabled	=false;
		EditNode.Enabled	=false;
		AddChild.Enabled	=false;
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

			AddShapeToCPrims(kid);

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
			AddChild.Enabled	=false;
		}
		else
		{
			ShapeGroup.Enabled	=true;
			EditNode.Enabled	=true;
			AddChild.Enabled	=true;

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
		AddShapeToCPrims(cn);
	}


	void OnTreeKeyUp(object sender, KeyEventArgs e)
	{
		if(!CollisionTreeView.Focused)
		{
			return;
		}

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

		if(e.KeyCode == Keys.Delete)
		{
			TreeNode	parent	=sel.Parent;
			if(parent == null)
			{
				//deleting root?
				CollisionTreeView.Nodes.Clear();
				mColTree.NukeAll();
				return;
			}

			CollisionNode	parentCN	=parent.Tag as CollisionNode;
			if(parentCN == null)
			{
				//shouldn't happen part 3
				return;
			}		

			parentCN.NukeKid(cn);
			parent.Nodes.Remove(sel);
			e.Handled	=true;
		}
		else if(e.KeyCode == Keys.F4)
		{
			CollisionTreeView.ExpandAll();
		}
		//renaming is a pain
//		else if(e.KeyCode == Keys.F2)
//		{
//			sel.Text	="blort";
//		}
	}

	void OnEditNode(object sender, EventArgs e)
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

		mbEditMode			=true;
		mEditNode			=cn;
		ShapeGroup.Enabled	=false;
		EditNode.Enabled	=false;
		AddChild.Enabled	=false;

		string	msg	="Adjusting bound...\n";
		if(cn.GetShape() == CollisionNode.Box)
		{
			msg	+="Use R / Shift-R to adjust width, Y / Shift-Y to adjust depth,\n"
				+ "T / Shift-T to adjust length along the bone axis,\n"
				+ "Arrows to move XY, Shift Arrow to move Z, and X when finished.\n";
		}
		else
		{
			msg	+="Use R / Shift-R to adjust radius, X when finished.\n";
		}

		mOut.Print(msg);
	}
}