using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BSPCore;
using Microsoft.Xna.Framework;


namespace BSPBuild
{
	internal class BSPBuild
	{
		SharedForms.BSPForm	mBSPForm;

		Map	mMap;

		//lighting emissives
		Dictionary<string, Microsoft.Xna.Framework.Color>	mEmissives;


		internal BSPBuild(SharedForms.BSPForm bspForm)
		{
			mBSPForm	=bspForm;

			mBSPForm.eBuild		+=OnBuild;
			mBSPForm.eLight		+=OnLight;
			mBSPForm.eOpenMap	+=OnOpenMap;
			mBSPForm.eSave		+=OnSave;

			CoreEvents.eBuildDone		+=OnBuildDone;
			CoreEvents.eGBSPSaveDone	+=OnSaveDone;
			CoreEvents.eLightDone		+=OnLightDone;

			CoreEvents.eNumClustersChanged	+=OnNumClustersChanged;
			CoreEvents.eNumPlanesChanged	+=OnNumPlanesChanged;
			CoreEvents.eNumPortalsChanged	+=OnNumPortalsChanged;
			CoreEvents.eNumVertsChanged		+=OnNumVertsChanged;

			bspForm.ControlBox	=true;
		}


		void OnOpenMap(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mMap	=new Map();

			mMap.LoadBrushFile(fileName,
				mBSPForm.BSPParameters.mbSlickAsGouraud,
				mBSPForm.BSPParameters.mbWarpAsMirror);

			mBSPForm.SetBuildEnabled(true);
			mBSPForm.SetSaveEnabled(false);
		}


		void OnBuild(object sender, EventArgs ea)
		{
			mBSPForm.EnableFileIO(false);
			mMap.BuildTree(mBSPForm.BSPParameters);
		}


		void OnSave(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mBSPForm.EnableFileIO(false);
			mMap.SaveGBSPFile(fileName, mBSPForm.BSPParameters);
		}


		void OnLight(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mEmissives	=UtilityLib.FileUtil.LoadEmissives(fileName);

			mBSPForm.SetSaveEnabled(false);
			mBSPForm.SetBuildEnabled(false);
			mBSPForm.EnableFileIO(false);

			mMap	=new Map();

			mMap.LightGBSPFile(fileName, EmissiveForMaterial,
				mBSPForm.LightParameters, mBSPForm.BSPParameters);
		}


		void OnBuildDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mBSPForm.SetSaveEnabled(true);
			mBSPForm.SetBuildEnabled(false);
			mBSPForm.EnableFileIO(true);
		}


		void OnLightDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mBSPForm.EnableFileIO(true);
		}


		void OnSaveDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mBSPForm.EnableFileIO(true);
		}


		void OnNumClustersChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mBSPForm.NumberOfClusters	="" + num;
		}


		void OnNumVertsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mBSPForm.NumberOfVerts	="" + num;
		}


		void OnNumPortalsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mBSPForm.NumberOfPortals	="" + num;
		}


		void OnNumPlanesChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mBSPForm.NumberOfPlanes	="" + num;
		}


		Vector3 EmissiveForMaterial(string matName)
		{
			if(mEmissives != null && mEmissives.ContainsKey(matName))
			{
				return	mEmissives[matName].ToVector3();
			}
			return	Vector3.One;
		}


		void LoadEmissives(string fileName)
		{
			string	emmName	=UtilityLib.FileUtil.StripExtension(fileName);

			emmName	+=".Emissives";

			if(!File.Exists(emmName))
			{
				//not a big deal, just use white
				CoreEvents.Print("No emissives, just using solid white.\n");
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

			CoreEvents.Print("Loaded " + mEmissives.Count + " emissive colors.\n");
		}
	}
}
