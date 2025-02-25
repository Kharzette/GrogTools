﻿using System;
using System.Windows.Forms;

namespace BSPBuilder;

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
	public event EventHandler	eDumpTextures;


	public ZoneForm() : base()
	{
		InitializeComponent();
	}


	public bool SaveDebugInfo
	{
		get { return SaveDebug.Checked; }
	}


	public void EnableFileIO(bool bOn)
	{
		Action<GroupBox>	enable	=but => but.Enabled = bOn;
		SharedForms.FormExtensions.Invoke(GroupFileIO, enable);
	}


	public void SetZoneSaveEnabled(bool bOn)
	{
		Action<Button>	enable	=but => but.Enabled = bOn;
		SharedForms.FormExtensions.Invoke(SaveZone, enable);
	}


	void OnGenerateMaterials(object sender, EventArgs e)
	{
		mOFD.DefaultExt	="*.bsp";
		mOFD.Filter		="Quake 2 bsp files (*.bsp)|*.bsp|All files (*.*)|*.*";
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
		mOFD.Filter		="Genesis bsp files (*.gbsp)|*.gbsp|All files (*.*)|*.*";
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		BSPCore.CoreEvents.Print("Material vising " + mOFD.FileName + "\n");

		UtilityLib.Misc.SafeInvoke(eMaterialVis, mOFD.FileName);
	}


	void OnZone(object sender, EventArgs e)
	{
		mOFD.DefaultExt	="*.bsp";
		mOFD.Filter		="Quake 2 bsp files (*.bsp)|*.bsp|All files (*.*)|*.*";
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		BSPCore.CoreEvents.Print("Zoning " + mOFD.FileName + "\n");

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

		BSPCore.CoreEvents.Print("Saving Zone " + mSFD.FileName + "\n");

		UtilityLib.Misc.SafeInvoke(eSaveZone, mSFD.FileName);
	}


	public int GetLightAtlasSize()
	{
		return	(int)AtlasSize.Value;
	}


	void OnSaveEmissives(object sender, EventArgs e)
	{
		mSFD.DefaultExt	="*.Emissives";
		mSFD.Filter		="Emissives files (*.Emissives)|*.Emissives|All files (*.*)|*.*";

		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		BSPCore.CoreEvents.Print("Saving Emissives " + mSFD.FileName + "\n");

		UtilityLib.Misc.SafeInvoke(eSaveEmissives, mSFD.FileName);
	}


	void OnLoadDebug(object sender, EventArgs e)
	{
		mOFD.DefaultExt	="*.Portals";
		mOFD.Filter		="Portals files (*.Portals)|*.Portals|All files (*.*)|*.*";

		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		BSPCore.CoreEvents.Print("Loading debug file " + mSFD.FileName + "\n");

		UtilityLib.Misc.SafeInvoke(eLoadDebug, mOFD.FileName);
	}


	void OnDumpTextures(object sender, EventArgs e)
	{
		UtilityLib.Misc.SafeInvoke(eDumpTextures, null);
	}
}