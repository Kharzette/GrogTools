using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using UtilityLib;
//using XMeshCLR;


namespace ColladaConvert
{
	public partial class AnimForm : Form
	{
		//data
		AnimGridModel	mAnimGrid;
		MeshLib.AnimLib	mAnimLib;

		//file dialog
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		//events
		public event EventHandler	eLoadAnim;
		public event EventHandler	eLoadModel;
		public event EventHandler	eLoadStaticModel;
		public event EventHandler	eAnimSelectionChanged;
		public event EventHandler	eTimeScaleChanged;
		public event EventHandler	eSaveLibrary;
		public event EventHandler	eSaveCharacter;
		public event EventHandler	eLoadCharacter;
		public event EventHandler	eLoadLibrary;
		public event EventHandler	eSaveStatic;
		public event EventHandler	eLoadStatic;
		public event EventHandler	eBoundMesh;
		public event EventHandler	eShowBound;
		public event EventHandler	eShowAxis;
		public event EventHandler	ePause;

		public bool BakeTransforms
		{
			get { return BakeVerts.Checked; }
		}


		public AnimForm(MeshLib.AnimLib anlib)
		{
			InitializeComponent();

			mAnimLib	=anlib;

			ColladaConvert.eAnimsUpdated	+=OnAnimsUpdated;
		}


		void OnLoadModel(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.dae";
			mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eLoadModel(mOFD.FileName, null);
		}


		void OnAnimsUpdated(object sender, EventArgs e)
		{
			List<MeshLib.Anim>	anms	=mAnimLib.GetAnims();

			mAnimGrid	=new AnimGridModel(anms);

			AnimGrid.DataBindingComplete	+=OnDataBindingComplete;

			AnimGrid.DataSource	=mAnimGrid;
		}


		void OnDataBindingComplete(object sender, EventArgs ea)
		{
			AnimGrid.DataBindingComplete	-=OnDataBindingComplete;

			AnimGrid.Columns[0].Width	=180;
		}


		void AnimGrid_SelectionChanged(object sender, EventArgs e)
		{
			DataGridViewSelectedRowCollection	row	=AnimGrid.SelectedRows;

			if(eAnimSelectionChanged != null)
			{
				eAnimSelectionChanged(row, null);
			}
		}

		void TimeScale_ValueChanged(object sender, EventArgs e)
		{
			eTimeScaleChanged(TimeScale.Value, null);
		}


		void OnSaveLibrary(object sender, EventArgs e)
		{
			mSFD.DefaultExt		="*.AnimLib";
			mSFD.Filter			="Animation library files (*.AnimLib)|*.AnimLib|All files (*.*)|*.*";
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eSaveLibrary(mSFD.FileName, null);
		}


		void OnLoadLibrary(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.AnimLib";
			mOFD.Filter			="Animation library files (*.AnimLib)|*.AnimLib|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eLoadLibrary(mOFD.FileName, null);
		}


		void OnSaveCharacter(object sender, EventArgs e)
		{
			mSFD.DefaultExt		="*.Character";
			mSFD.Filter			="Character files (*.Character)|*.Character|All files (*.*)|*.*";
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eSaveCharacter(mSFD.FileName, null);
		}


		void OnLoadCharacter(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.Character";
			mOFD.Filter			="Character files (*.Character)|*.Character|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eLoadCharacter(mOFD.FileName, null);
		}

		void OnCellValidated(object sender, DataGridViewCellEventArgs e)
		{
			//update name?
			if(e.ColumnIndex == 0)
			{
				mAnimLib.UpdateDictionaries();
			}
		}


		void OnRowNuking(object sender, DataGridViewRowCancelEventArgs e)
		{
			MeshLib.Anim	nukeMe	=(MeshLib.Anim)e.Row.DataBoundItem;
			mAnimLib.NukeAnim(nukeMe.Name);
		}


		void OnClearAll(object sender, EventArgs e)
		{
			mAnimLib.NukeAll();

			List<MeshLib.Anim>	anms	=mAnimLib.GetAnims();

			mAnimGrid	=new AnimGridModel(anms);

			AnimGrid.DataSource	=mAnimGrid;
		}


		void OnCompress(object sender, EventArgs e)
		{
			if(AnimGrid.SelectedRows.Count <= 0)
			{
				return;
			}
			mAnimLib.Reduce(
				Convert.ToString(AnimGrid.SelectedRows[0].Cells[0].Value),
				Convert.ToSingle(MaxError.Value));
		}


		void OnLoadStaticModel(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.dae";
			mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
			mOFD.Multiselect	=false;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eLoadStaticModel(mOFD.FileName, null);
		}


		void OnLoadAnim(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.dae";
			mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
			mOFD.Multiselect	=true;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			string	[]fnames	=mOFD.FileNames;

			foreach(string fname in fnames)
			{
				eLoadAnim(fname, null);
			}
		}


		void OnSaveStatic(object sender, EventArgs e)
		{
			mSFD.DefaultExt		="*.Static";
			mSFD.Filter			="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			eSaveStatic(mSFD.FileName, null);
		}


		void OnLoadStatic(object sender, EventArgs e)
		{
			mOFD.DefaultExt		="*.Static";
			mOFD.Filter			="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";
			mOFD.Multiselect	=true;
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			string	[]fnames	=mOFD.FileNames;

			foreach(string fname in fnames)
			{
				eLoadStatic(fname, null);
			}
		}


		void OnBoundMesh(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eBoundMesh, null);
		}


		void OnBoundShowBoxChanged(object sender, EventArgs e)
		{
			if(ShowBox.Checked)
			{
				ShowSphere.Checked	=false;
			}
			Misc.SafeInvoke(eShowBound, (ShowBox.Checked)? 1 : 0);
		}


		void OnAxisDrawChanged(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eShowAxis, DrawAxis.Checked);
		}


		void OnBoundShowSphereChanged(object sender, EventArgs e)
		{
			if(ShowSphere.Checked)
			{
				ShowBox.Checked	=false;
			}
			Misc.SafeInvoke(eShowBound, (ShowSphere.Checked)? 2 : 0);
		}


		void OnOptimize(object sender, EventArgs e)
		{
//			ManagedHelloWorld	mhw	=new ManagedHelloWorld();
//			mhw.SayThis("By the mailed fist of Torm!!");
		}


		void OnPause(object sender, EventArgs e)
		{
			Misc.SafeInvoke(ePause, null);
		}
	}
}
