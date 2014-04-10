using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ColladaStartSmall
{
	public partial class MaterialForm : Form
	{
		MaterialLib.MaterialLib	mMatLib;


		public MaterialForm(MaterialLib.MaterialLib matLib)
		{
			InitializeComponent();

			mMatLib	=matLib;

			MaterialList.Columns.Add("Name");
			MaterialList.Columns.Add("Effect");
			MaterialList.Columns.Add("Technique");

			List<string>	names	=mMatLib.GetMaterialNames();

			foreach(string name in names)
			{
				MaterialList.Items.Add(name);
			}

			for(int i=0;i < MaterialList.Items.Count;i++)
			{
				MaterialList.Items[i].Tag	="MaterialName";

				MaterialList.Items[i].SubItems.Add(
					mMatLib.GetMaterialEffect(MaterialList.Items[i].Text));
				MaterialList.Items[i].SubItems.Add(
					mMatLib.GetMaterialTechnique(MaterialList.Items[i].Text));

				MaterialList.Items[i].SubItems[1].Tag	="MaterialEffect";
				MaterialList.Items[i].SubItems[2].Tag	="MaterialTechnique";
			}

			SizeColumns();
		}


		void SizeColumns()
		{
			//set to header size first
			MaterialList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

			List<int>	sizes	=new List<int>();
			for(int i=0;i < 3;i++)
			{
				sizes.Add(MaterialList.Columns[i].Width);
			}

			for(int i=0;i < 3;i++)
			{
				MaterialList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

				if(MaterialList.Columns[i].Width < sizes[i])
				{
					MaterialList.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
				}
			}
		}


		void SpawnEffectComboBox(string matName, ListViewItem.ListViewSubItem sub)
		{
			List<string>	effects	=mMatLib.GetEffects();

			ListBox	lbox	=new ListBox();

			lbox.Parent		=MaterialList;
			lbox.Location	=sub.Bounds.Location;
			lbox.Tag		=matName;

			string	current	=mMatLib.GetMaterialEffect(matName);

			foreach(string fx in effects)
			{
				lbox.Items.Add(fx);
			}

			if(current != null)
			{
				lbox.SelectedItem	=current;
			}

			lbox.Visible		=true;

			lbox.MouseClick	+=OnEffectListBoxClick;
			lbox.Leave		+=OnEffectListBoxEscaped;
			lbox.KeyPress	+=OnEffectListBoxKey;
			lbox.Focus();
		}


		void SpawnTechniqueComboBox(string matName, ListViewItem.ListViewSubItem sub)
		{
			List<string>	techs	=mMatLib.GetMaterialTechniques(matName);

			ListBox	lbox	=new ListBox();

			lbox.Parent		=MaterialList;
			lbox.Location	=sub.Bounds.Location;
			lbox.Tag		=matName;

			foreach(string tn in techs)
			{
				lbox.Items.Add(tn);
			}

			string	current	=mMatLib.GetMaterialTechnique(matName);
			if(current != null)
			{
				lbox.SelectedItem	=current;
			}

			lbox.Visible		=true;

			lbox.Leave		+=OnTechListBoxEscaped;
			lbox.KeyPress	+=OnTechListBoxKey;
			lbox.MouseClick	+=OnTechListBoxClick;
			lbox.Focus();
		}


		void SetListEffect(string mat, string fx)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Text == mat)
				{
					lvi.SubItems[1].Text	=fx;
					return;
				}
			}
		}


		void SetListTechnique(string mat, string tech)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Text == mat)
				{
					lvi.SubItems[2].Text	=tech;
					return;
				}
			}
		}


		void OnTechListBoxKey(object sender, KeyPressEventArgs kpea)
		{
			ListBox	lb	=sender as ListBox;

			if(kpea.KeyChar == 27)	//escape
			{
				lb.Leave	-=OnTechListBoxEscaped;
				lb.KeyPress	-=OnTechListBoxKey;
				lb.Dispose();
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialTechnique(lb.Tag as string, lb.SelectedItem as string);
					SetListTechnique(lb.Tag as string, lb.SelectedItem as string);
				}
				lb.Leave		-=OnTechListBoxEscaped;
				lb.KeyPress		-=OnTechListBoxKey;
				lb.MouseClick	-=OnTechListBoxClick;
				lb.Dispose();
			}
		}


		void OnEffectListBoxKey(object sender, KeyPressEventArgs kpea)
		{
			ListBox	lb	=sender as ListBox;

			if(kpea.KeyChar == 27)	//escape
			{
				lb.Leave	-=OnEffectListBoxEscaped;
				lb.KeyPress	-=OnEffectListBoxKey;
				lb.Dispose();
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialEffect(lb.Tag as string, lb.SelectedItem as string);
					SetListEffect(lb.Tag as string, lb.SelectedItem as string);
				}
				lb.Leave		-=OnEffectListBoxEscaped;
				lb.KeyPress		-=OnEffectListBoxKey;
				lb.MouseClick	-=OnEffectListBoxClick;
				lb.Dispose();
			}
		}


		void OnTechListBoxClick(object sender, MouseEventArgs mea)
		{
			ListBox	lb	=sender as ListBox;

			if(lb.SelectedIndex != -1)
			{
				mMatLib.SetMaterialTechnique(lb.Tag as string, lb.SelectedItem as string);
				SetListTechnique(lb.Tag as string, lb.SelectedItem as string);
			}
			lb.Leave		-=OnTechListBoxEscaped;
			lb.KeyPress		-=OnTechListBoxKey;
			lb.MouseClick	-=OnTechListBoxClick;
			lb.Dispose();
		}


		void OnEffectListBoxClick(object sender, MouseEventArgs mea)
		{
			ListBox	lb	=sender as ListBox;

			if(lb.SelectedIndex != -1)
			{
				mMatLib.SetMaterialEffect(lb.Tag as string, lb.SelectedItem as string);
				SetListEffect(lb.Tag as string, lb.SelectedItem as string);
			}
			lb.Leave		-=OnEffectListBoxEscaped;
			lb.KeyPress		-=OnEffectListBoxKey;
			lb.MouseClick	-=OnEffectListBoxClick;
			lb.Dispose();
		}


		void OnTechListBoxEscaped(object sender, EventArgs ea)
		{
			ListBox	lb	=sender as ListBox;

			lb.Leave		-=OnTechListBoxEscaped;
			lb.KeyPress		-=OnTechListBoxKey;
			lb.MouseClick	-=OnTechListBoxClick;
			lb.Dispose();
		}


		void OnEffectListBoxEscaped(object sender, EventArgs ea)
		{
			ListBox	lb	=sender as ListBox;

			lb.Leave		-=OnEffectListBoxEscaped;
			lb.KeyPress		-=OnEffectListBoxKey;
			lb.MouseClick	-=OnEffectListBoxClick;
			lb.Dispose();
		}


		void OnMaterialSelectionChanged(object sender, EventArgs e)
		{
		}

		
		void OnMaterialRename(object sender, LabelEditEventArgs e)
		{
			if(!mMatLib.RenameMaterial(MaterialList.Items[e.Item].Text, e.Label))
			{
				e.CancelEdit	=true;
			}
			else
			{
				SizeColumns();	//this doesn't work, still has the old value
			}
		}


		void OnMatListClick(object sender, MouseEventArgs e)
		{
			foreach(ListViewItem lvi in MaterialList.Items)
			{
				if(lvi.Bounds.Contains(e.Location))
				{
					foreach(ListViewItem.ListViewSubItem sub in lvi.SubItems)
					{
						if(sub.Bounds.Contains(e.Location))
						{
							if(sub.Tag == "MaterialEffect")
							{
								SpawnEffectComboBox(lvi.Text, sub);
							}
							else if(sub.Tag == "MaterialTechnique")
							{
								SpawnTechniqueComboBox(lvi.Text, sub);
							}
						}
					}
				}
			}
		}
	}
}
