using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using BSPCore;
using Microsoft.Xna.Framework;	//xnamathery


namespace BSPBuild
{
	public partial class BuildForm : Form
	{
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		//build params
		BSPBuildParams	mBSPParams		=new BSPBuildParams();
		LightParams		mLightParams	=new LightParams();

		//lighting emissives
		Dictionary<string, Microsoft.Xna.Framework.Color>	mEmissives	=new Dictionary<string, Microsoft.Xna.Framework.Color>();

		Map	mMap;

		bool	mbWorking;


		public BuildForm()
		{
			InitializeComponent();

			CoreEvents.eNumPortalsChanged		+=OnNumPortalsChanged;
			CoreEvents.eNumClustersChanged		+=OnNumClustersChanged;
			CoreEvents.eNumPlanesChanged		+=OnNumPlanesChanged;
			CoreEvents.eNumVertsChanged			+=OnNumVertsChanged;
			CoreEvents.eBuildDone				+=OnBuildDone;
			CoreEvents.eLightDone				+=OnLightDone;
			CoreEvents.eGBSPSaveDone			+=OnGBSPSaveDone;
			CoreEvents.ePrint					+=OnPrint;
			ProgressWatcher.eProgressUpdated	+=OnProgressUpdated;
		}


		internal BSPBuildParams BSPParameters
		{
			get
			{
				mBSPParams.mbVerbose		=VerboseBSP.Checked;
				mBSPParams.mbEntityVerbose	=VerboseEntity.Checked;
				mBSPParams.mbFixTJunctions	=FixTJunctions.Checked;

				return	mBSPParams;
			}
			set { }	//donut allow settery
		}

		internal LightParams LightParameters
		{
			get
			{
				mLightParams.mbSeamCorrection	=SeamCorrection.Checked;
				mLightParams.mbRadiosity		=Radiosity.Checked;
				mLightParams.mbFastPatch		=FastPatch.Checked;
				mLightParams.mPatchSize			=(int)PatchSize.Value;
				mLightParams.mNumBounces		=(int)NumBounce.Value;
				mLightParams.mLightScale		=(float)LightScale.Value;
				mLightParams.mMinLight.X		=(float)MinLightX.Value;
				mLightParams.mMinLight.Y		=(float)MinLightY.Value;
				mLightParams.mMinLight.Z		=(float)MinLightZ.Value;
				mLightParams.mSurfaceReflect	=(float)ReflectiveScale.Value;
				mLightParams.mMaxIntensity		=(int)MaxIntensity.Value;
				mLightParams.mLightGridSize		=(int)LightGridSize.Value;
				mLightParams.mAtlasSize			=(int)4;	//doesn't matter

				return	mLightParams;
			}
			set { }	//donut allow settery
		}


		internal void PrintToConsole(string text)
		{
			AppendTextBox(ConsoleOut, text);
		}


		void OnPrint(object sender, EventArgs ea)
		{
			string	str	=sender as string;
			PrintToConsole(str);
		}


		void OnOpenBrushFile(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.vmf";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Text	=mOFD.FileName;

			mMap	=new Map();

			mMap.LoadBrushFile(mOFD.FileName);
			SetBuildEnabled(true);
			SetSaveEnabled(false);

		}

		void OnNumClustersChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;
			SetTextBoxValue(NumClusters, "" + num);
		}


		void OnNumVertsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;
			SetTextBoxValue(NumVerts, "" + num);
		}


		void OnNumPortalsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;
			SetTextBoxValue(NumPortals, "" + num);
		}


		void OnNumPlanesChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;
			SetTextBoxValue(NumPlanes, "" + num);
		}


		void OnBuildDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mbWorking	=false;
			EnableFileIO(true);
			SetSaveEnabled(true);
			SetBuildEnabled(false);
		}


		void OnLightDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			ClearProgress();
			mbWorking	=false;
			EnableFileIO(true);
		}


		void OnGBSPSaveDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mbWorking	=false;
			EnableFileIO(true);
		}


		internal void SetBuildEnabled(bool bOn)
		{
			EnableControl(BuildGBSP, bOn);
		}


		internal void SetSaveEnabled(bool bOn)
		{
			EnableControl(SaveGBSP, bOn);
		}


		void OnProgressUpdated(object sender, EventArgs ea)
		{
			ProgressEventArgs	pea	=ea as ProgressEventArgs;

			UpdateProgress(pea);
		}


		delegate void EnableControlCB(Control c, bool bOn);

		void EnableControl(Control control, bool bOn)
		{
			if(control.InvokeRequired)
			{
				EnableControlCB	enable	=delegate(Control c, bool bEn) { c.Enabled = bEn; };

				object	[]pms	=new object[2];

				pms[0]	=control;
				pms[1]	=bOn;

				control.Invoke(enable, pms);
			}
			else
			{
				control.Enabled	=bOn;
			}
		}

		internal void EnableFileIO(bool bOn)
		{
			EnableControl(GroupFileIO, bOn);
		}


		internal void UpdateProgress(ProgressEventArgs pea)
		{
			UpdateProgressBar(Progress1, pea.mMin, pea.mMax, pea.mCurrent);
		}


		internal void ClearProgress()
		{
			UpdateProgressBar(Progress1, 0, 0, 0);
		}
		delegate void SetTextDel(TextBox tb, string txt);

		void SetTextBoxValue(TextBox tbox, string str)
		{
			if(tbox.InvokeRequired)
			{
				SetTextDel	setText	=delegate(TextBox tb, string txt) {	tb.Text = txt; };

				object	[]pms	=new object[2];

				pms[0]	=tbox;
				pms[1]	=str;

				tbox.Invoke(setText, pms);
			}
			else
			{
				tbox.Text	=str;
			}
		}


		delegate void AppendTextDel(TextBox tb, string txt);

		void AppendTextBox(TextBox tbox, string str)
		{
			if(tbox.InvokeRequired)
			{
				AppendTextDel	appText	=delegate(TextBox tb, string txt) { tb.AppendText(txt); };

				object	[]pms	=new object[2];

				pms[0]	=tbox;
				pms[1]	=str;

				tbox.Invoke(appText, pms);
			}
			else
			{
				tbox.AppendText(str);
			}
		}


		delegate void UpdateProgressBarDel(ProgressBar pb, int min, int max, int cur);

		void UpdateProgressBar(ProgressBar pb, int min, int max, int cur)
		{
			if(pb.InvokeRequired)
			{
				UpdateProgressBarDel	updel	=delegate(ProgressBar prb, int mn, int mx, int cr)
							{ prb.Minimum	=mn; prb.Maximum =mx; prb.Value = cr; };

				object	[]pms	=new object[4];

				pms[0]	=pb;
				pms[1]	=min;
				pms[2]	=max;
				pms[3]	=cur;

				pb.Invoke(updel, pms);
			}
			else
			{
				pb.Minimum	=min;
				pb.Maximum	=max;
				pb.Value	=cur;
			}
		}

		void OnLightGBSP(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}
			Text	="Loading emissives for " + mOFD.FileName;

			LoadEmissives(mOFD.FileName);

			Text	="Lighting " + mOFD.FileName;

			SetSaveEnabled(false);
			SetBuildEnabled(false);
			mbWorking	=true;
			EnableFileIO(false);

			mMap	=new Map();

			mMap.LightGBSPFile(mOFD.FileName, EmissiveForMaterial,
				LightParameters, BSPParameters);
		}


		void LoadEmissives(string fileName)
		{
			string	emmName	=UtilityLib.FileUtil.StripExtension(fileName);

			emmName	+=".Emissives";

			if(!File.Exists(emmName))
			{
				//not a big deal, just use white
				return;
			}

			FileStream		fs	=new FileStream(emmName, FileMode.Open, FileAccess.Read);
			BinaryReader	br	=new BinaryReader(fs);

			UInt32	magic	=br.ReadUInt32();
			if(magic != 0xED1551BE)
			{
				CoreEvents.Print("Bad magic number for emissive file\n");
			}

			Microsoft.Xna.Framework.Color	tempColor	=new Microsoft.Xna.Framework.Color();

			mEmissives.Clear();

			int	count	=br.ReadInt32();
			for(int i=0;i < count;i++)
			{
				string	matName	=br.ReadString();

				tempColor.PackedValue	=br.ReadUInt32();

				mEmissives.Add(matName, tempColor);
			}

			br.Close();
			fs.Close();
		}


		Vector3 EmissiveForMaterial(string matName)
		{
			if(mEmissives.ContainsKey(matName))
			{
				return	mEmissives[matName].ToVector3();
			}
			return	Vector3.One;
		}


		void OnBuildGBSP(object sender, EventArgs e)
		{
			mbWorking	=true;
			EnableFileIO(false);
			mMap.BuildTree(BSPParameters);
		}

		private void OnSaveGBSP(object sender, EventArgs e)
		{
			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Text		="Saving " + mSFD.FileName;
			mbWorking	=true;
			EnableFileIO(false);
			mMap.SaveGBSPFile(mSFD.FileName, BSPParameters);
		}


		void OnRadiosityChanged(object sender, EventArgs e)
		{
			FastPatch.Enabled		=Radiosity.Checked;
			PatchSize.Enabled		=Radiosity.Checked;
			ReflectiveScale.Enabled	=Radiosity.Checked;
		}
	}
}
