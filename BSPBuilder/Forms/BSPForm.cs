using System;
using System.Windows.Forms;
using BSPCore;
using UtilityLib;


namespace BSPBuilder
{
	public partial class BSPForm : Form
	{
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		//build params
		BSPBuildParams	mBSPParams		=new BSPBuildParams();
		LightParams		mLightParams	=new LightParams();

		public event EventHandler	eOpenMap;
		public event EventHandler	eBuild;
		public event EventHandler	eSave;
		public event EventHandler	eLight;
		public event EventHandler	eFullBuild;
		public event EventHandler	eUpdateEntities;
		public event EventHandler	eStaticToMap;
		public event EventHandler	eMapToStatic;


		public BSPBuildParams BSPParameters
		{
			get
			{
				mBSPParams.mMaxThreads			=(int)MaxThreads.Value;
				mBSPParams.mbVerbose			=VerboseBSP.Checked;
				mBSPParams.mbBuildAsBModel		=BuildAsBModel.Checked;
				mBSPParams.mbFixTJunctions		=FixTJunctions.Checked;
				mBSPParams.mMapType				=GetMapType();

				return	mBSPParams;
			}
			set { }	//donut allow settery
		}

		public LightParams LightParameters
		{
			get
			{
				mLightParams.mbSeamCorrection		=SeamCorrection.Checked;
				mLightParams.mMinLight.X			=(float)MinLightX.Value;
				mLightParams.mMinLight.Y			=(float)MinLightY.Value;
				mLightParams.mMinLight.Z			=(float)MinLightZ.Value;
				mLightParams.mMaxIntensity			=(int)MaxIntensity.Value;
				mLightParams.mLightGridSize			=(int)LightGridSize.Value;
				mLightParams.mNumSamples			=(int)NumSamples.Value;

				return	mLightParams;
			}
			set { }	//donut allow settery
		}


		public BSPForm() : base()
		{
			InitializeComponent();
		}


		MapType	GetMapType()
		{
			if(GrogEnts.Checked)
			{
				return	MapType.GrogLibs;	
			}
			else if(Quake1Ents.Checked)
			{
				return	MapType.Quake1;	
			}
			else if(Quake2Ents.Checked)
			{
				return	MapType.Quake2;	
			}
			else if(Quake3Ents.Checked)
			{
				return	MapType.Quake3;	
			}
			else if(ValveEnts.Checked)
			{
				return	MapType.Valve;	
			}
			return	MapType.GrogLibs;
		}


		void OnOpenMap(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.map";
			mOFD.Filter		="QuArK map files (*.map)|*.map|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			CoreEvents.Print("Opening map " + mOFD.FileName + "\n");
			CoreEvents.Print("Map is a " + BSPParameters.mMapType + " map.\n");

			Misc.SafeInvoke(eOpenMap, mOFD.FileName);
		}


		public void SetBuildEnabled(bool bOn)
		{
			Action<Button>	enable	=but => but.Enabled = bOn;
			SharedForms.FormExtensions.Invoke(BuildGBSP, enable);
		}


		public void SetSaveEnabled(bool bOn)
		{
			Action<Button>	enable	=but => but.Enabled = bOn;
			SharedForms.FormExtensions.Invoke(SaveGBSP, enable);
		}


		public void EnableFileIO(bool bOn)
		{
			Action<GroupBox>	enable	=but => but.Enabled = bOn;
			SharedForms.FormExtensions.Invoke(GroupFileIO, enable);
		}


		void OnLightGBSP(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			mOFD.Filter		="GBSP files (*.gbsp)|*.gbsp|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			CoreEvents.Print("Lighting gbsp " + mOFD.FileName + "\n");

			Misc.SafeInvoke(eLight, mOFD.FileName);
		}


		void OnBuildGBSP(object sender, EventArgs e)
		{
			Misc.SafeInvoke(eBuild, null);
		}


		void OnSaveGBSP(object sender, EventArgs e)
		{
			mSFD.DefaultExt	="*.gbsp";
			mSFD.Filter		="GBSP files (*.gbsp)|*.gbsp|All files (*.*)|*.*";

			DialogResult	dr	=mSFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			CoreEvents.Print("Saving gbsp " + mSFD.FileName + "\n");

			Misc.SafeInvoke(eSave, mSFD.FileName);
		}


		void OnVerbose(object sender, EventArgs e)
		{
			if(VerboseBSP.Checked)
			{
				CoreEvents.Print("Note that verbosity can adversely affect performance, especially in vis.\n");
			}
		}


		void OnUpdateEntities(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.map";
			mOFD.Filter		="QuArK map files (*.map)|*.map|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Misc.SafeInvoke(eUpdateEntities, mOFD.FileName);
		}


		void OnFullBuild(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.map";
			mOFD.Filter		="QuArK map files (*.map)|*.map|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Misc.SafeInvoke(eFullBuild, mOFD.FileName);
		}


		void OnOpenStatic(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.Static";
			mOFD.Filter		="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

			Misc.SafeInvoke(eStaticToMap, mOFD.FileName);
		}


		void OnMapToStatic(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.map";
			mOFD.Filter		="QuArK map files (*.map)|*.map|All files (*.*)|*.*";

			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}
			Misc.SafeInvoke(eMapToStatic, mOFD.FileName);
		}
	}
}
