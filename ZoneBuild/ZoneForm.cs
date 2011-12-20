using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZoneBuild
{
	public partial class ZoneForm : Form
	{
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		public event EventHandler	eGenerateMaterials;
		public event EventHandler	eZoneGBSP;
		public event EventHandler	eSaveZone;
		public event EventHandler	eSaveEmissives;
		public event EventHandler	eMaterialVis;
		public event EventHandler	eLoadDebug;


		public ZoneForm()
		{
			InitializeComponent();

			BSPCore.CoreEvents.ePrint	+=OnPrint;
		}


		internal string NumberOfPlanes
		{
			get { return NumPlanes.Text; }
			set
			{
				Action<TextBox>	ta	=tb => tb.Text = value;
				SharedForms.FormExtensions.Invoke(NumPlanes, ta);
			}
		}

		internal string NumberOfPortals
		{
			get { return NumPortals.Text; }
			set
			{
				Action<TextBox>	ta	=tb => tb.Text = value;
				SharedForms.FormExtensions.Invoke(NumPortals, ta);
			}
		}

		internal string NumberOfVerts
		{
			get { return NumVerts.Text; }
			set
			{
				Action<TextBox>	ta	=tb => tb.Text = value;
				SharedForms.FormExtensions.Invoke(NumVerts, ta);
			}
		}

		internal string NumberOfClusters
		{
			get { return NumClusters.Text; }
			set
			{
				Action<TextBox>	ta	=tb => tb.Text = value;
				SharedForms.FormExtensions.Invoke(NumClusters, ta);
			}
		}

		internal bool SaveDebugInfo
		{
			get { return SaveDebug.Checked; }
		}



		internal void EnableFileIO(bool bOn)
		{
			Action<GroupBox>	enable	=but => but.Enabled = bOn;
			SharedForms.FormExtensions.Invoke(GroupFileIO, enable);
		}


		internal void SetZoneSaveEnabled(bool bOn)
		{
			Action<Button>	enable	=but => but.Enabled = bOn;
			SharedForms.FormExtensions.Invoke(SaveZone, enable);
		}


		void OnPrint(object sender, EventArgs ea)
		{
			string	toPrint	=sender as string;
			if(toPrint == null)
			{
				return;
			}

			Action<TextBox>	ta	=con => con.AppendText(toPrint);
			SharedForms.FormExtensions.Invoke(ConsoleOut, ta);
		}


		void OnGenerateMaterials(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eGenerateMaterials, mOFD.FileName);
		}


		void OnMaterialVis(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eMaterialVis, mOFD.FileName);
		}


		void OnZone(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eZoneGBSP, mOFD.FileName);
		}


		void OnSaveZone(object sender, EventArgs e)
		{
			//google was useless for the "file is not valid" problem
			mSFD.CheckFileExists	=false;
			mSFD.CreatePrompt		=false;
			mSFD.ValidateNames		=false;
			mSFD.DefaultExt			="*.Zone";
			mSFD.Filter				="Zone files (*.Zone)|*.Zone|All files (*.*)|*.*";

			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eSaveZone, mSFD.FileName);
		}


		internal int GetLightAtlasSize()
		{
			return	(int)AtlasSize.Value;
		}


		void OnSaveEmissives(object sender, EventArgs e)
		{
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eSaveEmissives, mSFD.FileName);
		}


		void OnLoadPortals(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.Portals";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			UtilityLib.Misc.SafeInvoke(eLoadDebug, mOFD.FileName);
		}
	}
}
