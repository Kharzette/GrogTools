using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using BSPLib;


namespace BSPBuilder
{
	public class BSPBuilder : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSharedCM;

		//build farm end points
		List<string>	mBuildFarm	=new List<string>();

		//forms
		MainForm					mMainForm;
		SharedForms.MaterialForm	mMatForm;

		//data
		Map						mMap;
		MaterialLib.MaterialLib	mMatLib;
		MeshLib.IndoorMesh		mIndoorMesh;

		//debug draw stuff
		BasicEffect		mMapEffect;
		VertexBuffer	mVB;
		IndexBuffer		mIB;
		SpriteFont		mKoot;
		Vector2			mTextPos;
		Random			mRnd	=new Random();
		Vector3			mDynamicLightPos;

		//control / view
		UtilityLib.GameCamera		mGameCam;
		UtilityLib.PlayerSteering	mPlayerControl;
		UtilityLib.Input			mInput;

		//working?
		bool	mbWorking;


		public BSPBuilder()
		{
			mGDM	=new GraphicsDeviceManager(this);
			Content.RootDirectory	="Content";

			//set window position
			if(!mGDM.IsFullScreen)
			{
				System.Windows.Forms.Control	mainWindow
					=System.Windows.Forms.Form.FromHandle(this.Window.Handle);

				//add data binding so it will save
				mainWindow.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					global::BSPBuilder.Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

				mainWindow.Location	=
					global::BSPBuilder.Settings.Default.MainWindowPos;

				IsMouseVisible	=true;
			}
		}


		protected override void Initialize()
		{
			mTextPos	=Vector2.One * 20.0f;

			mInput	=new UtilityLib.Input();

			mGameCam	=new UtilityLib.GameCamera(mGDM.GraphicsDevice.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height,
				mGDM.GraphicsDevice.Viewport.AspectRatio,
				1.0f, 8000.0f);

			mPlayerControl	=new UtilityLib.PlayerSteering(mGDM.GraphicsDevice.Viewport.Width,
				mGDM.GraphicsDevice.Viewport.Height);

			mPlayerControl.Method	=UtilityLib.PlayerSteering.SteeringMethod.Fly;

			base.Initialize();
		}


		protected override void LoadContent()
		{
			mSB			=new SpriteBatch(GraphicsDevice);
			mSharedCM	=new ContentManager(Services, "SharedContent");

			mMatLib	=new MaterialLib.MaterialLib(mGDM.GraphicsDevice, Content, mSharedCM, true);

			mIndoorMesh	=new MeshLib.IndoorMesh(GraphicsDevice, mMatLib);

			mMatForm					=new SharedForms.MaterialForm(mGDM.GraphicsDevice, mMatLib, false);
			mMatForm.Visible			=true;
			mMatForm.eMaterialNuked		+=OnMaterialNuked;
			mMatForm.eLibraryCleared	+=OnMaterialsCleared;
			mMatForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::BSPBuilder.Settings.Default,
				"MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mMatForm.Location	=
				global::BSPBuilder.Settings.Default.MaterialFormPos;

			mMainForm						=new MainForm();
			mMainForm.Visible				=true;
			mMainForm.eOpenBrushFile		+=OnOpenBrushFile;
			mMainForm.eLightGBSP			+=OnLightGBSP;
			mMainForm.eVisGBSP				+=OnVisGBSP;
			mMainForm.eGenerateMaterials	+=OnGenerateMaterials;
			mMainForm.eBuildGBSP			+=OnBuildGBSP;
			mMainForm.eSaveGBSP				+=OnSaveGBSP;
			mMainForm.eSaveZone				+=OnSaveZone;
			mMainForm.eLoadGBSP				+=OnLoadGBSP;
			mMainForm.eDrawChoiceChanged	+=OnDrawChoiceChanged;
			mMainForm.eQueryBuildFarm		+=OnQueryBuildFarm;

			mKoot	=mSharedCM.Load<SpriteFont>("Fonts/Koot20");

			mMapEffect	=new BasicEffect(mGDM.GraphicsDevice);
			mMapEffect.TextureEnabled		=false;
			mMapEffect.DiffuseColor			=Vector3.One;
			mMapEffect.VertexColorEnabled	=true;

			Map.ePrint	+=OnMapPrint;

			//load renderfarm contacts
			FileStream	fs	=new FileStream("Content/BuildFarm.txt", FileMode.Open, FileAccess.Read);
			StreamReader	sr	=new StreamReader(fs);

			while(!sr.EndOfStream)
			{
				string	url	=sr.ReadLine();
				mBuildFarm.Add(url);
			}
		}


		protected override void UnloadContent()
		{
		}


		protected override void Update(GameTime gameTime)
		{
			if(mbWorking && !mMainForm.DrawNWork)
			{
				base.Update(gameTime);
				Thread.Sleep(10);
				return;
			}

			float	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

			mInput.Update(msDelta);

			KeyboardState	kbs	=Keyboard.GetState();

			if(mInput.Player1.mKBS.IsKeyDown(Keys.L)
				|| mInput.Player1.mGPS.IsButtonDown(Buttons.LeftShoulder))
			{
				mDynamicLightPos	=-mPlayerControl.Position;
				mMatLib.SetParameterOnAll("mLight0Position", mDynamicLightPos);
				mMatLib.SetParameterOnAll("mLight0Color", Vector3.One);
				mMatLib.SetParameterOnAll("mLightRange", 200.0f);
				mMatLib.SetParameterOnAll("mLightFalloffRange", 100.0f);
			}

			mIndoorMesh.Update(msDelta);

			mPlayerControl.Update(msDelta, mGameCam.View, mInput.Player1.mKBS, mInput.Player1.mMS, mInput.Player1.mGPS);

			mGameCam.Update(msDelta, mPlayerControl.Position, mPlayerControl.Pitch, mPlayerControl.Yaw, mPlayerControl.Roll);

			mMapEffect.World		=mGameCam.World;
			mMapEffect.View			=mGameCam.View;
			mMapEffect.Projection	=mGameCam.Projection;

			mMatLib.UpdateWVP(mGameCam.World, mGameCam.View, mGameCam.Projection, -mPlayerControl.Position);

			base.Update(gameTime);
		}


		float StyleVal(string szVal)
		{
			char	first	=szVal[0];
			char	topVal	='z';

			//get from zero to 25
			float	val	=topVal - first;

			//scale up to 0 to 255
			val	*=(255.0f / 25.0f);

			Debug.Assert(val >= 0.0f);
			Debug.Assert(val <= 255.0f);

			return	(255.0f - val) / 255.0f;
		}


		protected override void Draw(GameTime gameTime)
		{
			if(mbWorking && !mMainForm.DrawNWork)
			{
				return;
			}

			GraphicsDevice	g	=mGDM.GraphicsDevice;

			if(mMap != null && mVB == null)
			{
				mIndoorMesh.Draw(g, mGameCam, -mPlayerControl.Position, mMap.IsMaterialVisibleFromPos);
			}

			if(mVB != null)
			{
				g.SetVertexBuffer(mVB);
				g.Indices	=mIB;

				g.BlendState		=BlendState.Opaque;
				g.DepthStencilState	=DepthStencilState.Default;
				g.RasterizerState	=RasterizerState.CullCounterClockwise;

				mMapEffect.CurrentTechnique.Passes[0].Apply();

				g.DrawIndexedPrimitives(PrimitiveType.TriangleList,
						0, 0, mVB.VertexCount, 0, mIB.IndexCount / 3);
			}

			KeyboardState	kbstate	=Keyboard.GetState();
			if(kbstate.IsKeyDown(Keys.L))
			{
				mMatLib.DrawMap("LightMapAtlas", mSB);
			}

			mSB.Begin();

			mSB.DrawString(mKoot, "Coordinates: " + -mPlayerControl.Position, mTextPos, Color.Yellow);

			mSB.End();

			base.Draw(gameTime);
		}

		
		void MakeDrawData(string drawChoice)
		{
			if(drawChoice == "None")
			{
				mVB	=null;
				mIB	=null;

				return;
			}

			if(mMap == null)
			{
				return;
			}

			List<Vector3>	verts		=new List<Vector3>();
			List<UInt32>	indexes		=new List<UInt32>();
			List<Int32>		mergeIndex	=new List<Int32>();

			mMap.GetTriangles(mPlayerControl.Position, verts, indexes, mergeIndex, drawChoice);
			if(verts.Count <= 0)
			{
				return;
			}

			mVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColorTexture),
				verts.Count, BufferUsage.WriteOnly);
			mIB	=new IndexBuffer(mGDM.GraphicsDevice, IndexElementSize.ThirtyTwoBits,
				indexes.Count, BufferUsage.WriteOnly);

			VertexPositionColorTexture	[]vpnt	=new VertexPositionColorTexture[verts.Count];

			int		mi	=-1;
			Color	col	=Color.White;
			for(int i=0;i < verts.Count;i++)
			{
				Vector3	vert	=verts[i];
				if(mergeIndex.Count == verts.Count && mi != mergeIndex[i])
				{
					col	=UtilityLib.Mathery.RandomColor(mRnd);
					mi	=mergeIndex[i];
				}

				vpnt[i].Position			=vert;
				vpnt[i].Color				=col;
				vpnt[i].TextureCoordinate	=Vector2.Zero;
			}

			//slightly vary triangle colours to show tris
			for(int i=0;i < indexes.Count;i+=3)
			{
				vpnt[indexes[i + 1]].Color.R	+=5;
				vpnt[indexes[i + 2]].Color.R	+=15;
			}

			mVB.SetData<VertexPositionColorTexture>(vpnt);
			mIB.SetData<UInt32>(indexes.ToArray());
		}


		//temporary making this merge
		void OnOpenBrushFile(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				if(mMap == null)
				{
					mMap	=new Map();
					RegisterMapEvents(true);
				}

//				mMap.LoadBuggeryBrushes(fileName);

//				mMap.MergeBrushFile(fileName);
				mMap.LoadBrushFile(fileName);
				mMainForm.SetBuildEnabled(true);
				mMainForm.SetZoneSaveEnabled(false);
				mMainForm.SetSaveEnabled(false);
			}
		}


		void OnBuildGBSP(object sender, EventArgs ea)
		{
			mbWorking	=true;
			mMainForm.EnableFileIO(false);
			mMap.BuildTree(mMainForm.BSPParameters);
		}


		Vector3 EmissiveForMaterial(string matName)
		{
			MaterialLib.Material	mat	=mMatLib.GetMaterial(matName);
			if(mat == null)
			{
				return	Vector3.One;
			}
			return	mat.Emissive.ToVector3();
		}


		void RegisterMapEvents(bool bReg)
		{
			if(bReg)
			{
				mMap.eNumPortalsChanged			+=OnNumPortalsChanged;
				mMap.eNumClustersChanged		+=OnNumClustersChanged;
				mMap.eNumPlanesChanged			+=OnNumPlanesChanged;
				mMap.eNumVertsChanged			+=OnNumVertsChanged;
				mMap.eBuildDone					+=OnBuildDone;
				mMap.eLightDone					+=OnLightDone;
				mMap.eVisDone					+=OnVisDone;
				mMap.eGBSPSaveDone				+=OnGBSPSaveDone;
			}
			else
			{
				mMap.eNumPortalsChanged			-=OnNumPortalsChanged;
				mMap.eNumClustersChanged		-=OnNumClustersChanged;
				mMap.eNumPlanesChanged			-=OnNumPlanesChanged;
				mMap.eNumVertsChanged			-=OnNumVertsChanged;
				mMap.eBuildDone					-=OnBuildDone;
				mMap.eLightDone					-=OnLightDone;
				mMap.eVisDone					-=OnVisDone;
				mMap.eGBSPSaveDone				-=OnGBSPSaveDone;
			}
		}


		void OnLightGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mMainForm.SetSaveEnabled(false);
				mMainForm.SetBuildEnabled(false);
				mMainForm.SetZoneSaveEnabled(false);
				mbWorking	=true;
				mMainForm.EnableFileIO(false);
				mMap	=new Map();
				RegisterMapEvents(true);

				ProgressWatcher.eProgressUpdated	+=OnProgressUpdated;

				mMap.LightGBSPFile(fileName, EmissiveForMaterial,
					mMainForm.LightParameters,
					mMainForm.BSPParameters,
					mMainForm.VisParameters);
			}
		}


		void OnVisGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mMainForm.SetSaveEnabled(false);
				mMainForm.SetBuildEnabled(false);
				mMainForm.SetZoneSaveEnabled(false);
				mbWorking	=true;
				mMainForm.EnableFileIO(false);
				mMainForm.EnableVisGroup(false);
				mMap	=new Map();
				RegisterMapEvents(true);

				ProgressWatcher.eProgressUpdated	+=OnProgressUpdated;

				if(mMainForm.VisParameters.mbDistribute)
				{
					mMap.VisGBSPFile(fileName, mMainForm.VisParameters, mMainForm.BSPParameters, mBuildFarm);
				}
				else
				{
					mMap.VisGBSPFile(fileName, mMainForm.VisParameters, mMainForm.BSPParameters);
				}
			}
		}


		void OnGenerateMaterials(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mMainForm.SetSaveEnabled(false);
				mMainForm.SetBuildEnabled(false);
				mMainForm.SetZoneSaveEnabled(false);
				mbWorking	=true;
				mMainForm.EnableFileIO(false);
				mMap	=new Map();
				RegisterMapEvents(true);

				mMatLib.NukeAllMaterials();
				List<MaterialLib.Material>	mats	=mMap.GenerateMaterials(fileName);

				foreach(MaterialLib.Material mat in mats)
				{
					mMatLib.AddMaterial(mat);
				}
				mMatLib.RefreshShaderParameters();
				mMatForm.UpdateMaterials();

				mbWorking	=false;
				mMainForm.EnableFileIO(true);	//not threaded
			}
		}


		void OnLoadGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mMainForm.SetSaveEnabled(false);
				mMainForm.SetBuildEnabled(false);
				mMainForm.SetZoneSaveEnabled(false);
				mbWorking	=true;
				mMainForm.EnableFileIO(false);
				mMap	=new Map();

				RegisterMapEvents(true);

				GFXHeader	hdr	=mMap.LoadGBSPFile(fileName);

				if(hdr == null)
				{
					OnMapPrint("Load failed\n", null);
				}
				else
				{
					GraphicsDevice	g	=mGDM.GraphicsDevice;

					mMatLib.NukeAllMaterials();

					List<MaterialLib.Material>	mats	=mMap.GetMaterials();

//					mIndoorMesh.BuildLM(g, mMainForm.LightParameters.mAtlasSize, mMap.BuildLMRenderData);
//					mIndoorMesh.BuildVLit(g, mMap.BuildVLitRenderData);
//					mIndoorMesh.BuildAlpha(g, mMap.BuildAlphaRenderData);
//					mIndoorMesh.BuildFullBright(g, mMap.BuildFullBrightRenderData);
//					mIndoorMesh.BuildMirror(g, mMap.BuildMirrorRenderData);
//					mIndoorMesh.BuildSky(g, mMap.BuildSkyRenderData);

					foreach(MaterialLib.Material mat in mats)
					{
						mMatLib.AddMaterial(mat);
					}
					mMatLib.RefreshShaderParameters();
					mMatForm.UpdateMaterials();
					mMainForm.SetZoneSaveEnabled(true);
				}
				mbWorking	=false;
				mMainForm.EnableFileIO(true);
			}
		}


		void OnSaveGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				mbWorking		=true;
				mMainForm.EnableFileIO(false);
				mMap.SaveGBSPFile(fileName,	mMainForm.BSPParameters);
			}
		}


		void OnSaveZone(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mMainForm.Text	=fileName;
				mMap.Write(fileName);

				//write out the zoneDraw
				mIndoorMesh.Write(fileName + "Draw");
			}
		}


		void OnMapPrint(object sender, EventArgs ea)
		{
			string	str	=sender as string;

			mMainForm.PrintToConsole(str);
		}


		void OnMaterialNuked(object sender, EventArgs ea)
		{
			if(mMap != null)
			{
				//rebuild material vis
				mMap.VisMaterials();
			}
		}


		void OnMaterialsCleared(object sender, EventArgs ea)
		{
			//might need to readd lightmap tex
		}


		void OnDrawChoiceChanged(object sender, EventArgs ea)
		{
			string	choice	=sender as string;
			MakeDrawData(choice);
		}


		void OnNumClustersChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mMainForm.NumberOfClusters	="" + num;
		}


		void OnNumVertsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mMainForm.NumberOfVerts	="" + num;
		}


		void OnNumPortalsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mMainForm.NumberOfPortals	="" + num;
		}


		void OnNumPlanesChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mMainForm.NumberOfPlanes	="" + num;
		}


		void OnBuildDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mbWorking	=false;
			mMainForm.EnableFileIO(true);
			mMainForm.SetSaveEnabled(true);
			mMainForm.SetBuildEnabled(false);
		}


		void OnLightDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			ProgressWatcher.eProgressUpdated	-=OnProgressUpdated;
			mMainForm.ClearProgress();
			mbWorking	=false;
			mMainForm.EnableFileIO(true);
		}


		void OnVisDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			ProgressWatcher.eProgressUpdated	-=OnProgressUpdated;
			mMainForm.ClearProgress();
			mbWorking	=false;
			mMainForm.EnableFileIO(true);
			mMainForm.EnableVisGroup(true);
		}


		void OnGBSPSaveDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mbWorking	=false;
			mMainForm.EnableFileIO(true);
		}


		void OnProgressUpdated(object sender, EventArgs ea)
		{
			ProgressEventArgs	pea	=ea as ProgressEventArgs;

			mMainForm.UpdateProgress(pea);
		}


		void OnQueryBuildFarm(object sender, EventArgs ea)
		{
			for(int i=0;i < mBuildFarm.Count;i++)
			{
				MapVisClient	mvc	=new MapVisClient("WSHttpBinding_IMapVis", mBuildFarm[i]);
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
					mMainForm.PrintToConsole("Build farm capabilities for " + mvc.Endpoint.Address + "\n");
					mMainForm.PrintToConsole("Cpu speed in mhz:  " + bfc.mMHZ + "\n");
					mMainForm.PrintToConsole("Number of cpu cores:  " + bfc.mNumCores + "\n");
					mvc.mbActive	=true;
					mvc.mBuildCaps	=bfc;
				}
				else
				{
					mMainForm.PrintToConsole("Build farm node " + mvc.Endpoint.Address + " is not responding.\n");
					mvc.mbActive	=false;
					mvc.mBuildCaps	=null;
				}
			}
		}
	}
}