using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		List<int>	mIndexes;

		public event EventHandler	eDeleteElement;
		public event EventHandler	eEscape;


		public StripElements()
		{
			InitializeComponent();
		}


		public List<int> GetIndexes()
		{
			return	mIndexes;
		}


		public void Populate(ArchEventArgs aea)
		{
			if(aea == null)
			{
				VertElements.Clear();
				MeshName.Text	="";
				return;
			}

			mIndexes	=aea.mIndexes;

			if(aea.mIndexes.Count == 1)
			{
				MeshName.Text	=aea.mArch.GetPartName(aea.mIndexes[0]);
			}
			else
			{
				MeshName.Text	="Multiple...";
			}

			//only affect those matching the first
			Type	t	=aea.mArch.GetPartVertexType(aea.mIndexes[0]);

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
