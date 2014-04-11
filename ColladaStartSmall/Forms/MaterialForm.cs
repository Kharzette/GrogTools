﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MeshLib;
using UtilityLib;


namespace ColladaStartSmall
{
	public partial class MaterialForm : Form
	{
		MaterialLib.MaterialLib	mMatLib;

		public event EventHandler	eNukedMeshPart;


		public MaterialForm(MaterialLib.MaterialLib matLib)
		{
			InitializeComponent();

			mMatLib	=matLib;

			MaterialList.Columns.Add("Name");
			MaterialList.Columns.Add("Effect");
			MaterialList.Columns.Add("Technique");

			RefreshMaterials();

			MeshPartList.Columns.Add("Name");
			MeshPartList.Columns.Add("Material Name");
			MeshPartList.Columns.Add("Vertex Format");
			MeshPartList.Columns.Add("Visible");
		}


		internal void RefreshMaterials()
		{
			MaterialList.Items.Clear();

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

			SizeColumns(MaterialList);
		}


		internal void RefreshMeshPartList()
		{
			StaticMesh	sm	=MeshPartList.Tag as StaticMesh;
			if(sm == null)
			{
				return;
			}

			List<Mesh>	partList	=sm.GetMeshPartList();

			MeshPartList.Items.Clear();

			foreach(Mesh m in partList)
			{
				ListViewItem	lvi	=MeshPartList.Items.Add(m.Name);

				lvi.Tag	=m;

				lvi.SubItems.Add(m.MaterialName);
				lvi.SubItems.Add(m.VertexType.ToString());
				lvi.SubItems.Add(m.Visible.ToString());
			}

			SizeColumns(MeshPartList);
		}


		void SizeColumns(ListView lv)
		{
			//set to header size first
			lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

			List<int>	sizes	=new List<int>();
			for(int i=0;i < lv.Columns.Count;i++)
			{
				sizes.Add(lv.Columns[i].Width);
			}

			for(int i=0;i < lv.Columns.Count;i++)
			{
				lv.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

				if(lv.Columns[i].Width < sizes[i])
				{
					lv.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
				}
			}
		}


		void SpawnEffectComboBox(string matName, ListViewItem.ListViewSubItem sub)
		{
			List<string>	effects	=mMatLib.GetEffects();
			if(effects.Count <= 0)
			{
				return;
			}

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
			if(techs.Count <= 0)
			{
				return;
			}

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
				lb.Leave		-=OnTechListBoxEscaped;
				lb.KeyPress		-=OnTechListBoxKey;
				lb.MouseClick	-=OnTechListBoxClick;
				lb.Dispose();
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialTechnique(lb.Tag as string, lb.SelectedItem as string);
					SetListTechnique(lb.Tag as string, lb.SelectedItem as string);
					OnMaterialSelectionChanged(null, null);
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
				lb.Leave		-=OnEffectListBoxEscaped;
				lb.KeyPress		-=OnEffectListBoxKey;
				lb.MouseClick	-=OnEffectListBoxClick;
				lb.Dispose();
			}
			else if(kpea.KeyChar == '\r')
			{
				if(lb.SelectedIndex != -1)
				{
					mMatLib.SetMaterialEffect(lb.Tag as string, lb.SelectedItem as string);
					SetListEffect(lb.Tag as string, lb.SelectedItem as string);
					OnMaterialSelectionChanged(null, null);
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
				OnMaterialSelectionChanged(null, null);
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
				OnMaterialSelectionChanged(null, null);
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
			if(MaterialList.SelectedIndices.Count < 1
				|| MaterialList.SelectedIndices.Count > 1)
			{
				VariableList.DataSource	=null;
				return;
			}

			string	matName	=MaterialList.Items[MaterialList.SelectedIndices[0]].Text;

			BindingList<MaterialLib.EffectVariableValue>	vars	=
				mMatLib.GetMaterialVariables(matName);

			if(vars.Count > 0)
			{
				VariableList.DataSource	=vars;
			}
			else
			{
				VariableList.DataSource	=null;
			}
		}

		
		void OnMaterialRename(object sender, LabelEditEventArgs e)
		{
			if(!mMatLib.RenameMaterial(MaterialList.Items[e.Item].Text, e.Label))
			{
				e.CancelEdit	=true;
			}
			else
			{
				SizeColumns(MaterialList);	//this doesn't work, still has the old value
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
							if((string)sub.Tag == "MaterialEffect")
							{
								SpawnEffectComboBox(lvi.Text, sub);
							}
							else if((string)sub.Tag == "MaterialTechnique")
							{
								SpawnTechniqueComboBox(lvi.Text, sub);
							}
						}
					}
				}
			}
		}

		void OnNewMaterial(object sender, EventArgs e)
		{
			List<string>	names	=mMatLib.GetMaterialNames();

			string	tryName	="default";
			bool	bFirst	=true;
			int		cnt		=1;
			while(names.Contains(tryName))
			{
				if(bFirst)
				{
					tryName	+="000";
					bFirst	=false;
				}
				else
				{
					tryName	="default" + String.Format("{0:000}", cnt);
					cnt++;
				}
			}

			mMatLib.CreateMaterial(tryName);

			RefreshMaterials();
		}


		internal void SetMesh(object sender)
		{
			StaticMesh	sm	=sender as StaticMesh;
			if(sm == null)
			{
				return;
			}

			MeshPartList.Tag	=sm;

			RefreshMeshPartList();
		}


		void OnFormSizeChanged(object sender, EventArgs e)
		{
			//get the mesh part grid out of the material
			//grid's junk
			int	adjust	=MeshPartGroup.Top - 6;

			adjust	-=(MeshPartList.Top + MeshPartList.Size.Height);

			MeshPartList.SetBounds(MeshPartList.Left,
				MeshPartList.Top + adjust,
				MeshPartList.Width,
				MeshPartList.Height);
		}


		void OnMeshPartNuking(object sender, DataGridViewRowCancelEventArgs e)
		{
			if(e.Row.DataBoundItem.GetType().BaseType == typeof(Mesh))
			{
				Mesh	nukeMe	=(Mesh)e.Row.DataBoundItem;
				Misc.SafeInvoke(eNukedMeshPart, nukeMe);
			}
		}


		void OnMatListKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyValue == 46)	//delete
			{
				if(MaterialList.SelectedItems.Count < 1)
				{
					return;	//nothing to do
				}

				foreach(ListViewItem lvi in MaterialList.SelectedItems)
				{
					mMatLib.NukeMaterial(lvi.Text);
				}

				RefreshMaterials();
			}
		}


		void OnMeshPartListKeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyValue == 46)	//delete
			{
				if(MeshPartList.SelectedItems.Count < 1)
				{
					return;	//nothing to do
				}

				List<object>	toNuke	=new List<object>();

				foreach(ListViewItem lvi in MeshPartList.SelectedItems)
				{
					toNuke.Add(lvi.Tag);
				}

				MeshPartList.Items.Clear();

				foreach(object o in toNuke)
				{
					Misc.SafeInvoke(eNukedMeshPart, o);
				}

				RefreshMeshPartList();
			}
		}


		void OnApplyMaterial(object sender, EventArgs e)
		{
			if(MaterialList.SelectedItems.Count != 1)
			{
				return;	//nothing to do
			}

			string	matName	=MaterialList.SelectedItems[0].Text;

			foreach(ListViewItem lvi in MeshPartList.SelectedItems)
			{
				Mesh	m	=lvi.Tag as Mesh;
				if(m == null)
				{
					continue;
				}

				m.MaterialName			=matName;
				lvi.SubItems[1].Text	=matName;
			}
		}
	}
}
