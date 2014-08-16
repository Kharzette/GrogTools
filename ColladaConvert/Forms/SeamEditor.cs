using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshLib;
using SharedForms;

using SharpDX;
using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;


namespace ColladaConvert
{
	public partial class SeamEditor : Form
	{
		Device		mDevice;		


		public SeamEditor()
		{
			InitializeComponent();

			SeamList.Columns.Add("First Mesh");
			SeamList.Columns.Add("Second Mesh");
			SeamList.Columns.Add("Verts");
		}


		public void Clear()
		{
			SeamList.Clear();
			mDevice		=null;
		}


		public void Initialize(Device dev)
		{
			mDevice		=dev;
		}


		public void SizeColumns()
		{
			FormExtensions.SizeColumns(SeamList);
		}


		public void AddSeams(List<EditorMesh.WeightSeam> seams)
		{
			Action<ListView>	addItem	=lv =>
			{
				foreach(EditorMesh.WeightSeam ws in seams)
				{
					ListViewItem	lvi	=lv.Items.Add(ws.mMeshA.Name);

					lv.Items[lvi.Index].SubItems.Add(ws.mMeshB.Name);
					lv.Items[lvi.Index].SubItems.Add(ws.mSeam.Count.ToString());
					lv.Items[lvi.Index].Tag	=ws;
				}
			};

			FormExtensions.Invoke(SeamList, addItem);
		}


		void OnUseFirst(object sender, EventArgs e)
		{
			if(SeamList.SelectedItems.Count <= 0)
			{
				return;
			}

			List<ListViewItem>	toNuke	=new List<ListViewItem>();

			for(int i=0;i < SeamList.SelectedItems.Count;i++)
			{
				ListViewItem	lvi	=SeamList.SelectedItems[i];

				EditorMesh.WeightSeam	ws	=
					lvi.Tag as EditorMesh.WeightSeam;

				ws.mMeshA.WeldOtherWeights(mDevice, ws);

				toNuke.Add(lvi);
			}

			foreach(ListViewItem lvi in toNuke)
			{
				SeamList.Items.Remove(lvi);
			}
		}


		void OnUseSecond(object sender, EventArgs e)
		{
			if(SeamList.SelectedItems.Count <= 0)
			{
				return;
			}

			List<ListViewItem>	toNuke	=new List<ListViewItem>();

			for(int i=0;i < SeamList.SelectedItems.Count;i++)
			{
				ListViewItem	lvi	=SeamList.SelectedItems[i];

				EditorMesh.WeightSeam	ws	=
					lvi.Tag as EditorMesh.WeightSeam;

				ws.mMeshA.WeldMyWeights(mDevice, ws);

				toNuke.Add(lvi);
			}

			foreach(ListViewItem lvi in toNuke)
			{
				SeamList.Items.Remove(lvi);
			}
		}


		void OnAverage(object sender, EventArgs e)
		{
			if(SeamList.SelectedItems.Count <= 0)
			{
				return;
			}

			List<ListViewItem>	toNuke	=new List<ListViewItem>();

			for(int i=0;i < SeamList.SelectedItems.Count;i++)
			{
				ListViewItem	lvi	=SeamList.SelectedItems[i];

				EditorMesh.WeightSeam	ws	=
					lvi.Tag as EditorMesh.WeightSeam;

				ws.mMeshA.WeldAverage(mDevice, ws);

				toNuke.Add(lvi);
			}

			foreach(ListViewItem lvi in toNuke)
			{
				SeamList.Items.Remove(lvi);
			}
		}


		void OnFormClosed(object sender, FormClosedEventArgs e)
		{
			Clear();
		}
	}
}
