using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using BSPVis;
using BSPCore;


namespace VisServer
{
	public partial class VisServer : Form
	{
		VisMap	mMap	=new VisMap();

		OpenFileDialog			mOFD	=new OpenFileDialog();
		SaveFileDialog			mSFD	=new SaveFileDialog();

		//build params
		BSPBuildParams	mBSPParams		=new BSPBuildParams();
		VisParams		mVisParams		=new VisParams();

		//build farm end points
		List<string>					mEndPoints	=new List<string>();
		ConcurrentQueue<MapVisClient>	mBuildFarm	=new ConcurrentQueue<MapVisClient>();


		public VisServer()
		{
			InitializeComponent();

			CoreEvents.ePrint					+=OnMapPrint;

			LoadBuildFarm();
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

		void OnMapPrint(object sender, EventArgs ea)
		{
			string	str	=sender as string;

			AppendTextBox(ConsoleOut, str);
		}

		void PrintToConsole(string str)
		{
			OnMapPrint(str, null);
		}

		void OnProgressUpdated(object sender, EventArgs ea)
		{
			ProgressEventArgs	pea	=ea as ProgressEventArgs;

			UpdateProgressBar(Progress1, pea.mMin, pea.mMax, pea.mCurrent);
		}

		void OnVisDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			ProgressWatcher.eProgressUpdated	-=OnProgressUpdated;

			UpdateProgressBar(Progress1, 0, 0, 0);
		}

		void OnResumeVis(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

//			Enabled	=false;

			StatusBottom.Text	="Resuming vis of " + mOFD.FileName;

			mVisParams.mbDistribute		=true;
			mVisParams.mbFullVis		=true;
			mVisParams.mbSortPortals	=true;
			mVisParams.mbResume			=true;
			mBSPParams.mbVerbose		=Verbose.Checked;

			//register for events
			ProgressWatcher.eProgressUpdated	+=OnProgressUpdated;
			CoreEvents.eVisDone					+=OnVisDone;

			mMap.VisGBSPFile(mOFD.FileName, mVisParams, mBSPParams, mBuildFarm);
		}

		void OnVisGBSP(object sender, EventArgs e)
		{
			mOFD.DefaultExt	="*.gbsp";
			DialogResult	dr	=mOFD.ShowDialog();

			if(dr == DialogResult.Cancel)
			{
				return;
			}

//			Enabled	=false;

			StatusBottom.Text	="Firing up vis of " + mOFD.FileName;

			mVisParams.mbDistribute		=Distributed.Checked;
			mVisParams.mbFullVis		=!RoughVis.Checked;
			mVisParams.mbSortPortals	=false;
			mVisParams.mbResume			=false;
			mBSPParams.mbVerbose		=Verbose.Checked;

			//register for events
			ProgressWatcher.eProgressUpdated	+=OnProgressUpdated;
			CoreEvents.eVisDone					+=OnVisDone;

			mMap.VisGBSPFile(mOFD.FileName, mVisParams, mBSPParams, mBuildFarm);
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
					PrintToConsole("Build farm capabilities for " + mvc.Endpoint.Address + "\n");
					PrintToConsole("Cpu speed in mhz:  " + bfc.mMHZ + "\n");
					PrintToConsole("Number of cpu cores:  " + bfc.mNumCores + "\n");
					mvc.mBuildCaps	=bfc;
				}
				else
				{
					PrintToConsole("Build farm node " + mvc.Endpoint.Address + " is not responding.\n");
					mvc.mBuildCaps	=null;
				}
			}
		}


		void OnReLoadBuildFarm(object sender, EventArgs e)
		{
			LoadBuildFarm();
		}
	}
}
