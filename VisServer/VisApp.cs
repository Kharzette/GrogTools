using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using BSPVis;


namespace VisServer
{
	internal class VisApp
	{
		SharedForms.VisForm	mVisForm;
		SharedForms.Output	mOutForm;

		VisMap	mVisMap;

		//build farm end points
		List<string>					mEndPoints	=new List<string>();
		ConcurrentQueue<MapVisClient>	mBuildFarm	=new ConcurrentQueue<MapVisClient>();


		internal VisApp(SharedForms.VisForm visForm, SharedForms.Output outForm)
		{
			mVisForm	=visForm;
			mOutForm	=outForm;

			mVisForm.eQueryVisFarm	+=OnQueryVisFarm;
			mVisForm.eReloadVisFarm	+=OnReLoadVisFarm;
			mVisForm.eResumeVis		+=OnResumeVis;
			mVisForm.eStopVis		+=OnStopVis;
			mVisForm.eVis			+=OnVis;

			BSPCore.CoreEvents.eVisDone	+=OnVisDone;

			visForm.ControlBox	=true;

			LoadBuildFarm();
		}


		void LoadBuildFarm()
		{
			//load renderfarm contacts
			FileStream		fs	=new FileStream("BuildFarm.txt", FileMode.Open, FileAccess.Read);
			StreamReader	sr	=new StreamReader(fs);

			while(!sr.EndOfStream)
			{
				string	url	=sr.ReadLine();

				//ensure unique
				if(!mEndPoints.Contains(url))
				{
					mEndPoints.Add(url);
				}
			}

			//clear when able
			while(!mBuildFarm.IsEmpty)
			{
				MapVisClient	junx;
				mBuildFarm.TryDequeue(out junx);
			}

			//list up the endpoints
			foreach(string address in mEndPoints)
			{
				MapVisClient	amvc	=new MapVisClient("WSHttpBinding_IMapVis", address);
				mBuildFarm.Enqueue(amvc);
			}			
		}


		void OnQueryVisFarm(object sender, EventArgs e)
		{
			foreach(MapVisClient mvc in mBuildFarm)
			{
				BuildFarmCaps	bfc	=null;
				try
				{
					bfc	=mvc.QueryCapabilities();
					mvc.Close();
				}
				catch
				{
				}

				if(bfc != null)
				{
					mOutForm.Print("Build farm capabilities for " + mvc.Endpoint.Address + "\n");
					mOutForm.Print("Cpu speed in mhz:  " + bfc.mMHZ + "\n");
					mOutForm.Print("Number of cpu cores:  " + bfc.mNumCores + "\n");
					mvc.mBuildCaps	=bfc;
				}
				else
				{
					mOutForm.Print("Build farm node " + mvc.Endpoint.Address + " is not responding.\n");
					mvc.mBuildCaps	=null;
				}
			}
		}


		void OnVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}
			mVisForm.EnableFileIO(false);

			BSPCore.BSPBuildParams	bp	=new BSPCore.BSPBuildParams();
			BSPCore.VisParams		vp	=new BSPCore.VisParams();

			vp.mbDistribute		=mVisForm.bDistributed;
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=false;
			vp.mbSortPortals	=mVisForm.bSortPortals;
			bp.mbVerbose		=false;	//if you want spam

			//ensure sorted is off if going dist
			if(vp.mbDistribute)
			{
				vp.mbSortPortals	=false;
			}

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, bp, mBuildFarm);
		}


		void OnResumeVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}
			mVisForm.EnableFileIO(false);

			BSPCore.BSPBuildParams	bp	=new BSPCore.BSPBuildParams();
			BSPCore.VisParams		vp	=new BSPCore.VisParams();

			vp.mbDistribute		=mVisForm.bDistributed;
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=true;
			vp.mbSortPortals	=mVisForm.bSortPortals;

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, bp, mBuildFarm);
		}


		void OnVisDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mOutForm.UpdateProgress(0, 0, 0);
			mVisForm.EnableFileIO(true);
		}


		void OnStopVis(object sender, EventArgs ea)
		{
			//dunno what to do here yet
		}


		void OnReLoadVisFarm(object sender, EventArgs e)
		{
			LoadBuildFarm();
		}
	}
}
