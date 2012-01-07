using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using BSPVis;


namespace BuildWCF
{
	class WorkState
	{
		public bool	mbVisInProgress;
	}


	public class MapVisService : IMapVis
	{
		VisState	mState;
		WorkState	mWorkState	=new WorkState();

		//stuff completed
		List<VisState>	mFinishedData	=new List<VisState>();


		public bool PortalFlowCB(object state)
		{
			return	PortalFlowBytes(mState.mVisData, mState.mStartPort, mState.mEndPort);
		}

		public bool PortalFlowBytes(byte []visData, int startPort, int endPort)
		{
			bool	bWorking	=false;
			lock(mWorkState)
			{
				bWorking	=mWorkState.mbVisInProgress;
				mWorkState.mbVisInProgress	=true;
			}

			if(bWorking)
			{
				Console.WriteLine("Already working on another work unit... returning false...");
				return	false;
			}

			Console.WriteLine("Beginning Vis work unit of size " + (endPort - startPort));

			byte	[]result	=null;

			try
			{
				result	=VisMap.PortalFlow(visData, startPort, endPort);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);

				return	false;
			}

			Console.WriteLine("Finished work unit from portal " + startPort
				+ " to portal " + endPort + " for " + result.Length + " bytes of data.");

			lock(mWorkState)
			{
				mWorkState.mbVisInProgress	=false;
			}

			return	true;
		}

		public bool HasPortals(object visState)
		{
			if(mState == null)
			{
				Console.WriteLine("HasPortals: Ready for Portals!");
				return	false;
			}

			VisState	vs	=visState as VisState;

			if(vs.mTotalPorts != mState.mTotalPorts)
			{
				Console.WriteLine("HasPortals: Server Portals is " + vs.mEndPort + " and Client Portals are " + mState.mTotalPorts);
				return	false;
			}

			return	true;
		}

		public bool ReadPortals(object visState)
		{
			if(mState != null)
			{
				Console.WriteLine("Already a vis state in place!  ReadPortals fail");
				return	false;
			}

			mState	=visState as VisState;

			Console.WriteLine("Received " + mState.mVisData.Length + " bytes of portals");

			VisMap.eFlowChunkComplete	+=OnFlowVisPieceDone;

			return	true;
		}

		public bool FreePortals()
		{
			Console.WriteLine("Freeing Portals...");

			mState	=null;

			VisMap.eFlowChunkComplete	-=OnFlowVisPieceDone;

			return	true;
		}

		public byte []IsFinished(object visState)
		{
			VisState	inState	=visState as VisState;

			VisState	found	=null;

			lock(mFinishedData)
			{
				foreach(VisState vs in mFinishedData)
				{
					if(vs.mStartPort == inState.mStartPort
						&& vs.mEndPort == inState.mEndPort)
					{
						found	=vs;
						break;
					}
				}
			}

			if(found == null)
			{
				return	null;
			}
			return	found.mVisData;
		}

		void OnFlowVisPieceDone(object sender, EventArgs ea)
		{
			VisState	vs	=sender as VisState;

			if(vs == null)
			{
				Console.WriteLine("Had a slow piece arrive in a strange format: " + sender.GetType().ToString());
				return;
			}

			lock(mFinishedData)
			{
				mFinishedData.Add(vs);
			}
		}

		public bool PortalFlow(object visState)
		{
			VisState	vs	=visState as VisState;

			if(mState == null || mState.mVisData == null || mState.mVisData.Length == 0)
			{
				Console.WriteLine("No valid data to crunch!");
				return	false;
			}

			Console.WriteLine("Flood begun on portals " + vs.mStartPort + " to " + vs.mEndPort + "...");

			mState.mStartPort	=vs.mStartPort;
			mState.mEndPort		=vs.mEndPort;

			var	task	=Task<bool>.Factory.StartNew(PortalFlowCB, null);

			return	true;
		}

		public BuildFarmCaps QueryCapabilities()
		{
			BuildFarmCaps	ret	=new BuildFarmCaps();
			
			int	coreCount	=0;
			foreach(var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
			{
				coreCount	+=int.Parse(item["NumberOfCores"].ToString());
			}
			ret.mNumCores	=coreCount;

			System.Management.ManagementObject	Mo	=
				new System.Management.ManagementObject("Win32_Processor.DeviceID='CPU0'");
			
			ret.mMHZ	=(uint)(Mo["CurrentClockSpeed"]);
			Mo.Dispose();
			
			return	ret;
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			//Step 1 of the address configuration procedure: Create a URI to serve as the base address.
			Uri	baseAddr	=new Uri("http://localhost:8000/");
			
			//Step 2 of the hosting procedure: Create ServiceHost
			ServiceHost selfHost = new ServiceHost(typeof(MapVisService), baseAddr);

			try
			{
				// Step 3 of the hosting procedure: Add a service endpoint.
				selfHost.AddServiceEndpoint(
					typeof(IMapVis),
					new WSHttpBinding("WSHttpBinding_MapVisService"),
					"MapVisService");
				
				//Step 4 of the hosting procedure: Enable metadata exchange.
				ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
				
				smb.HttpGetEnabled = true;
				
				selfHost.Description.Behaviors.Add(smb);
				
				// Step 5 of the hosting procedure: Start (and then stop) the service.
				selfHost.Open();
				Console.WriteLine("The service is ready.");
				Console.WriteLine("Press <ENTER> to terminate service.");
				Console.WriteLine();
				Console.ReadLine();
				
				// Close the ServiceHostBase to shutdown the service.
				selfHost.Close();
			}
			catch (CommunicationException ce)
			{
				Console.WriteLine("An exception occurred: {0}", ce.Message);
				selfHost.Abort();
			}
		}
	}
}
