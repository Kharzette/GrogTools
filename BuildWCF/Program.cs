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
using BSPCore;


namespace BuildWCF
{
	public class MapVisService : IMapVis
	{
		VisState	mState;

		List<VisState>	mAbandonedData	=new List<VisState>();


		public byte []FloodPortalsSlow(object state)
		{
			return	FloodPortalsSlow(mState.mVisData, mState.mStartPort, mState.mEndPort);
		}

		public byte []FloodPortalsSlow(byte []visData, int startPort, int endPort)
		{
			bool	bWorking	=false;
			lock(mState)
			{
				bWorking	=mState.mbVisInProgress;
				mState.mbVisInProgress	=true;
			}

			if(bWorking)
			{
				Console.WriteLine("Already working on another work unit... returning null...");
				return	null;
			}

			Console.WriteLine("Beginning Vis work unit of size " + (endPort - startPort));

			byte	[]result	=null;

			try
			{
				result	=Map.FloodPortalsSlow(visData, startPort, endPort);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);

				return	null;
			}

			Console.WriteLine("Finished work unit from portal " + startPort
				+ " to portal " + endPort + " for " + result.Length + " bytes of data.");

			lock(mState)
			{
				mState.mbVisInProgress	=false;
			}

			return	result;
		}

		public bool HasPortals(object visState)
		{
			if(mState == null)
			{
				Console.WriteLine("HasPortals: Null state");
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

			Console.WriteLine("Received " + mState.mVisData.Length + " portals");

			//in case connection is broken
			Map.eSlowVisPieceDone	+=OnSlowVisPieceDone;

			return	true;
		}

		public bool FreePortals()
		{
			Console.WriteLine("Freeing Portals...");

			mState	=null;

			Map.eSlowVisPieceDone	-=OnSlowVisPieceDone;

			return	true;
		}

		public VisState GetAbandoned(int startPort, int endPort)
		{
			VisState	ret	=null;

			lock(mAbandonedData)
			{
				foreach(VisState vs in mAbandonedData)
				{
					if(vs.mStartPort == startPort && vs.mEndPort == endPort)
					{
						ret	=vs;
						break;
					}
				}
			}

			return	ret;
		}

		void OnSlowVisPieceDone(object sender, EventArgs ea)
		{
			VisState	vs	=sender as VisState;

			if(vs == null)
			{
				Console.WriteLine("Had a slow piece arrive in a strange format: " + sender.GetType().ToString());
				return;
			}

			lock(mAbandonedData)
			{
				mAbandonedData.Add(vs);
			}
		}

		public IAsyncResult BeginFloodPortalsSlow(object visState, AsyncCallback callBack, object aSyncState)
		{
			VisState	vs	=visState as VisState;

			Console.WriteLine("Flood begun on portals " + vs.mStartPort + " to " + vs.mEndPort + "...");

			mState.mStartPort	=vs.mStartPort;
			mState.mEndPort		=vs.mEndPort;

			var	task	=Task<byte []>.Factory.StartNew(FloodPortalsSlow, aSyncState);

			return	task.ContinueWith(res => callBack(task));
		}

		public byte []EndFloodPortalsSlow(IAsyncResult res)
		{
			return	((Task<byte []>)res).Result;
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


	public class HaxdServiceHost : ServiceHost
	{
		public HaxdServiceHost(Type type, Uri baseAddr) : base(type, baseAddr)
		{
		}

		protected override void ApplyConfiguration()
		{
			// workaround for passing a custom configFilename
			string configFilename = (string)CallContext.GetData("_config");

			configFilename	=AppDomain.CurrentDomain.BaseDirectory + "App.config";

			Console.WriteLine(configFilename);
			
			ExeConfigurationFileMap filemap = new ExeConfigurationFileMap();
			
			filemap.ExeConfigFilename =
				string.IsNullOrEmpty(configFilename) ?
				AppDomain.CurrentDomain.SetupInformation.ConfigurationFile : configFilename;
			
			Configuration config =
				ConfigurationManager.OpenMappedExeConfiguration(filemap, ConfigurationUserLevel.None);
			
			ServiceModelSectionGroup serviceModel = ServiceModelSectionGroup.GetSectionGroup(config);
			foreach (ServiceElement se in serviceModel.Services.Services)
			{
				if (se.Name == this.Description.ConfigurationName)
				{
					base.LoadConfigurationSection(se);
					return;
				}
			}
			throw new ArgumentException("ServiceElement doesn't exist");
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			//Step 1 of the address configuration procedure: Create a URI to serve as the base address.
			Uri	baseAddr	=new Uri("http://localhost:8000/ServiceModelSamples/Service");
			
			//Step 2 of the hosting procedure: Create ServiceHost
//			HaxdServiceHost selfHost = new HaxdServiceHost(typeof(MapVisService), baseAddr);
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
