using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace ColladaConvert
{
	public partial class StripElements : Form
	{
		List<Mesh>	mMeshes;

		public event EventHandler	eDeleteElement;
		public event EventHandler	eEscape;


		public StripElements()
		{
			InitializeComponent();
		}


		public List<Mesh> GetMeshes()
		{
			return	mMeshes;
		}


		public void Populate(List<Mesh> meshes)
		{
			mMeshes	=meshes;

			if(meshes == null)
			{
				VertElements.Clear();
				MeshName.Text	="";
				return;
			}

			if(meshes.Count == 1)
			{
				MeshName.Text	=meshes[0].Name;
			}
			else
			{
				MeshName.Text	="Multiple...";
			}

			//only affect those matching the first
			Type	t	=meshes[0].VertexType;

			FieldInfo	[]fis	=t.GetFields();

			foreach(FieldInfo fi in fis)
			{
				ListViewItem	lvi	=new ListViewItem();

				lvi.Text	=fi.Name;

				VertElements.Items.Add(lvi);
			}

			Visible	=true;
		}


		void OnVertElementsKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Delete)
			{
				if(VertElements.SelectedIndices.Count == 0)
				{
					return;
				}

				List<int>	sels	=new List<int>();
				foreach(int index in VertElements.SelectedIndices)
				{
					sels.Add(index);
				}

				Misc.SafeInvoke(eDeleteElement, sels);
			}
			else if(e.KeyCode == Keys.Escape)
			{
				Misc.SafeInvoke(eEscape, null);
			}
		}
	}
}
