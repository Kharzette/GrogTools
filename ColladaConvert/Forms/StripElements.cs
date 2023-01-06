using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace ColladaConvert
{
	public partial class StripElements : Form
	{
		List<int>	?mIndexes;

		public event EventHandler	?eDeleteElement;
		public event EventHandler	?eEscape;


		public StripElements()
		{
			InitializeComponent();
		}


		public List<int>? GetIndexes()
		{
			return	mIndexes;
		}


		public void Populate(object ?mesh, List<int> ?indexes)
		{
			if(mesh == null || indexes == null)
			{
				VertElements.Clear();
				MeshName.Text	="";
				return;
			}

			mIndexes	=indexes;

			StaticMesh	?sm		=mesh as StaticMesh;
			Character	?chr	=mesh as Character;

			if(indexes.Count == 1)
			{
				if(sm != null)
				{
					MeshName.Text	=sm.GetPartName(indexes[0]);
				}
				else if(chr != null)
				{
					MeshName.Text	=chr.GetPartName(indexes[0]);
				}
			}
			else
			{
				MeshName.Text	="Multiple...";
			}

			//only affect those matching the first
			Type	t;
			if(sm != null)
			{
				t	=sm.GetPartVertexType(indexes[0]);
			}
			else if(chr != null)
			{
				t	=chr.GetPartVertexType(indexes[0]);
			}
			else
			{
				return;
			}

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
