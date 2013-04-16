using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BSPCore;
using BSPZone;
using BSPVis;
using UtilityLib;


namespace FullBuild
{
	public class FullBuild : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mGameCM;
		ContentManager			mShaderLib;

		//forms
		SharedForms.BSPForm			mBSPForm;
		SharedForms.VisForm			mVisForm;
		SharedForms.ZoneForm		mZoneForm;
		SharedForms.Output			mOutputForm;
		SharedForms.MaterialForm	mMatForm;

		//data
		Map						mMap;
		VisMap					mVisMap;
		MaterialLib.MaterialLib	mMatLib;
		MeshLib.IndoorMesh		mIndoorMesh;
		List<string>			mAllTextures	=new List<string>();
//		List<Vector3>			mPathNodes		=new List<Vector3>();
//		PathLib.PathGrid		mPathGrid;
		Dictionary<int, Matrix>	mModelMats;

		//lighting emissives
		Dictionary<string, Microsoft.Xna.Framework.Color>	mEmissives;

		//build farm end points
		List<string>					mEndPoints	=new List<string>();
		ConcurrentQueue<MapVisClient>	mBuildFarm	=new ConcurrentQueue<MapVisClient>();

		//ordered list of fonts
		IOrderedEnumerable<KeyValuePair<string, SpriteFont>>	mFonts;

		//debug draw stuff
		Vector2			mTextPos;
		Random			mRnd	=new Random();
		Vector3			mDynamicLightPos;
		VertexBuffer	mLineVB;
		BasicEffect		mBFX;
		float			mWarpFactor;

		//control / view
		UtilityLib.GameCamera		mGameCam;
		UtilityLib.PlayerSteering	mPlayerControl;
		UtilityLib.Input			mInput;
		bool						mbWorking, mbFullBuilding;
		string						mFullBuildFileName;
		TriggerHelper				mTHelper	=new TriggerHelper();


		public FullBuild()
		{
			mGDM	=new GraphicsDeviceManager(this);
			Content.RootDirectory	="GameContent";

			mGDM.PreferredBackBufferWidth	=848;
			mGDM.PreferredBackBufferHeight	=480;

			//set window position
			if(!mGDM.IsFullScreen)
			{
				System.Windows.Forms.Control	mainWindow
					=System.Windows.Forms.Form.FromHandle(this.Window.Handle);

				//add data binding so it will save
				mainWindow.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					global::FullBuild.FBSettings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

				mainWindow.Location	=
					global::FullBuild.FBSettings.Default.MainWindowPos;

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
			mGameCM		=new ContentManager(Services, "GameContent");
			mShaderLib	=new ContentManager(Services, "ShaderLib");
			mBFX		=new BasicEffect(mGDM.GraphicsDevice);

			Dictionary<string, SpriteFont>	fonts	=UtilityLib.FileUtil.LoadAllFonts(Content);

			mFonts	=fonts.OrderBy(fnt => fnt.Value.LineSpacing);

			mBFX.VertexColorEnabled	=true;
			mBFX.LightingEnabled	=false;
			mBFX.TextureEnabled		=false;

			mMatLib		=new MaterialLib.MaterialLib(mGDM.GraphicsDevice, mGameCM, mShaderLib, true);
			mIndoorMesh	=new MeshLib.IndoorMesh(GraphicsDevice, mMatLib);

			//set up cell shading
			mMatLib.InitCellShading(1);

			//set to worldy settings
			mMatLib.GenerateCellTexturePreset(GraphicsDevice, false, 0);
			mMatLib.SetCellTexture(0);

			mBSPForm	=new SharedForms.BSPForm();
			mVisForm	=new SharedForms.VisForm();
			mZoneForm	=new SharedForms.ZoneForm();
			mOutputForm	=new SharedForms.Output();
			mMatForm	=new SharedForms.MaterialForm(mGDM.GraphicsDevice, mMatLib, false);

			mBSPForm.Visible	=true;
			mVisForm.Visible	=true;
			mZoneForm.Visible	=true;
			mOutputForm.Visible	=true;
			mMatForm.Visible	=true;

			SetFormPos(mBSPForm, "BSPFormPos");
			SetFormPos(mVisForm, "VisFormPos");
			SetFormPos(mZoneForm, "ZoneFormPos");
			SetFormPos(mOutputForm, "OutputFormPos");
			SetFormPos(mMatForm, "MaterialFormPos");

			//form events
			mMatForm.eMaterialNuked			+=OnMaterialNuked;
			mMatForm.eLibraryCleared		+=OnMaterialsCleared;
			mMatForm.eLibrarySaved			+=OnMaterialLibSaved;
			mZoneForm.eGenerateMaterials	+=OnGenerateMaterials;
			mZoneForm.eMaterialVis			+=OnMaterialVis;
			mZoneForm.eSaveZone				+=OnSaveZone;
			mZoneForm.eZoneGBSP				+=OnZoneGBSP;
			mZoneForm.eSaveEmissives		+=OnSaveEmissives;
			mZoneForm.eLoadDebug			+=OnLoadDebug;
			mZoneForm.eDumpTextures			+=OnDumpTextures;
			mBSPForm.eBuild					+=OnBuild;
			mBSPForm.eLight					+=OnLight;
			mBSPForm.eOpenMap				+=OnOpenMap;
			mBSPForm.eSave					+=OnSaveGBSP;
			mBSPForm.eFullBuild				+=OnFullBuild;
			mBSPForm.eUpdateEntities		+=OnUpdateEntities;
			mVisForm.eQueryVisFarm			+=OnQueryVisFarm;
			mVisForm.eReloadVisFarm			+=OnReLoadVisFarm;
			mVisForm.eResumeVis				+=OnResumeVis;
			mVisForm.eStopVis				+=OnStopVis;
			mVisForm.eVis					+=OnVis;

			//core events
			CoreEvents.eBuildDone		+=OnBuildDone;
			CoreEvents.eLightDone		+=OnLightDone;
			CoreEvents.eGBSPSaveDone	+=OnGBSPSaveDone;
			CoreEvents.eVisDone			+=OnVisDone;

			//stats
			CoreEvents.eNumPortalsChanged		+=OnNumPortalsChanged;
			CoreEvents.eNumClustersChanged		+=OnNumClustersChanged;
			CoreEvents.eNumPlanesChanged		+=OnNumPlanesChanged;
			CoreEvents.eNumVertsChanged			+=OnNumVertsChanged;

			LoadBuildFarm();
		}


		protected override void UnloadContent()
		{
		}


		protected override void Update(GameTime gameTime)
		{
			if(mbWorking)
			{
				base.Update(gameTime);
				Thread.Sleep(0);
				return;
			}

			int	msDelta	=gameTime.ElapsedGameTime.Milliseconds;

			if(mModelMats != null)
			{
				if(mModelMats.Count > 0)
				{
//					mModelMats[1]	=Matrix.CreateRotationY(mTestRot);
//					mModelMats[1]	*=Matrix.CreateTranslation(mTestMover.GetPos());
				}
			}

			mWarpFactor	+=msDelta / 1000.0f;
			while(mWarpFactor > MathHelper.TwoPi)
			{
				mWarpFactor	-=MathHelper.TwoPi;
			}
			mMatLib.SetParameterOnAll("mWarpFactor", mWarpFactor);

			if(IsActive)
			{
				mInput.Update();
			}

			if(mInput.Player1.mKBS.IsKeyDown(Keys.L)
				|| mInput.Player1.mGPS.IsButtonDown(Buttons.LeftShoulder))
			{
				mDynamicLightPos	=mPlayerControl.Position;
				mMatLib.SetParameterOnAll("mLight0Position", mDynamicLightPos);
				mMatLib.SetParameterOnAll("mLight0Color", Vector3.One);
				mMatLib.SetParameterOnAll("mLightRange", 200.0f);
				mMatLib.SetParameterOnAll("mLightFalloffRange", 100.0f);
			}

			mIndoorMesh.Update(msDelta);

			mPlayerControl.Update(msDelta, mGameCam, mInput.Player1.mKBS, mInput.Player1.mMS, mInput.Player1.mGPS);

			mGameCam.Update(-mPlayerControl.Position, mPlayerControl.Pitch, mPlayerControl.Yaw, mPlayerControl.Roll);

			mBFX.World		=Matrix.Identity;
			mBFX.View		=mGameCam.View;
			mBFX.Projection	=mGameCam.Projection;

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			if(mbWorking)
			{
				base.Draw(gameTime);
				return;
			}

			GraphicsDevice.Clear(Color.CornflowerBlue);

			GraphicsDevice	g	=mGDM.GraphicsDevice;

			g.DepthStencilState	=DepthStencilState.Default;

			if(mMap != null && mVisMap != null)
			{
				mIndoorMesh.Draw(g, mGameCam, mVisMap.IsMaterialVisibleFromPos, GetModelMatrix, RenderExternal);
			}

			KeyboardState	kbstate	=Keyboard.GetState();
			if(kbstate.IsKeyDown(Keys.L))
			{
				mMatLib.DrawMap("LightMapAtlas", mSB);
			}

			if(mLineVB != null)
			{
				g.SetVertexBuffer(mLineVB);

				mBFX.CurrentTechnique.Passes[0].Apply();

				g.DrawPrimitives(PrimitiveType.LineList, 0, mLineVB.VertexCount / 2);
			}

			mSB.Begin();
			mSB.DrawString(mFonts.First().Value, "Coordinates: " + mPlayerControl.Position, mTextPos, Color.Yellow);
			mSB.End();

			base.Draw(gameTime);
		}


		void SetFormPos(System.Windows.Forms.Form form, string posName)
		{
			form.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::FullBuild.FBSettings.Default,
				posName, true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			System.Configuration.SettingsPropertyValue	val	=			
				global::FullBuild.FBSettings.Default.PropertyValues[posName];

			form.Location	=(System.Drawing.Point)val.PropertyValue;
		}


		void RenderExternal(MaterialLib.AlphaPool ap, Matrix view, Matrix proj)
		{
		}


		Vector3 EmissiveForMaterial(string matName)
		{
			if(mEmissives != null && mEmissives.ContainsKey(matName))
			{
				return	mEmissives[matName].ToVector3();
			}
			return	Vector3.One;
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


		#region Event Handlers
		void OnMaterialNuked(object sender, EventArgs ea)
		{
			//shouldn't mess with materials if something is loaded
			mMap		=null;
			mModelMats	=null;
		}


		void OnMaterialLibSaved(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				CoreEvents.Print("A material library was saved somewhere, but I have no idea where.");
			}
			else
			{
				CoreEvents.Print("Material library file " + fileName + " saved.\n");
			}
		}


		void OnMaterialsCleared(object sender, EventArgs ea)
		{
			if(mIndoorMesh != null)
			{
				Texture2D	atlas	=mIndoorMesh.GetLightMapAtlas();
				if(atlas != null)
				{
					mMatLib.AddMap("LightMapAtlas", atlas);
				}
			}
		}


		void OnMaterialVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				return;
			}

			Action<System.Windows.Forms.Form>	setText	=frm => frm.Text = fileName;
			SharedForms.FormExtensions.Invoke(mZoneForm, setText);
			mZoneForm.SetZoneSaveEnabled(false);
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			mVisMap	=new VisMap();
			mVisMap.MaterialVisGBSPFile(fileName);

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			mVisMap	=null;
		}


		void OnGenerateMaterials(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}
			mZoneForm.Text	=fileName;
			mZoneForm.SetZoneSaveEnabled(false);
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);
			mMap	=new Map();

			mMatLib.NukeAllMaterials();
			List<MaterialLib.Material>	mats	=mMap.GenerateMaterials(fileName);

			foreach(MaterialLib.Material mat in mats)
			{
				mMatLib.AddMaterial(mat);
			}
			mMatLib.RefreshShaderParameters();
			mMatForm.UpdateMaterials();

			mVisMap	=new VisMap();
			mVisMap.LoadVisData(fileName);

			mZoneForm.EnableFileIO(true);	//not threaded
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			mOutputForm.Print("Materials generated.\n");

			//store a list of textures used
			foreach(MaterialLib.Material m in mats)
			{
				string	texName	="";
				int		starPos	=m.Name.LastIndexOf('*');
				if(starPos == -1)
				{
					texName	=m.Name;
				}
				else
				{
					texName	=m.Name.Substring(0, m.Name.LastIndexOf('*'));
				}

				//lower case
				texName	=texName.ToLower();

				if(mAllTextures.Contains(texName))
				{
					continue;
				}

				mAllTextures.Add(texName);
			}
		}


		void OnSaveEmissives(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				return;
			}

			mMatLib.SaveEmissives(fileName);

			mOutputForm.Print("Emissive save complete.\n");
		}


		void OnSaveZone(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mZoneForm.Text	=fileName;
				mMap.Write(fileName, mZoneForm.SaveDebugInfo,
					mMatLib.GetMaterials().Count, mVisMap.SaveVisZoneData);

				//write out the zoneDraw
				mIndoorMesh.Write(fileName + "Draw");

				mOutputForm.Print("Zone save complete.\n");
			}
		}


		void OnZoneGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				Action<System.Windows.Forms.Form>	setText	=frm => frm.Text = fileName;
				SharedForms.FormExtensions.Invoke(mZoneForm, setText);
				mZoneForm.EnableFileIO(false);
				mBSPForm.EnableFileIO(false);
				mVisForm.EnableFileIO(false);
				mMap	=new Map();

				GFXHeader	hdr	=mMap.LoadGBSPFile(fileName);

				if(hdr == null)
				{
					CoreEvents.Print("Load failed\n");
				}
				else
				{
/*					List<Vector3>	lines	=mMap.GetFaceNormals();

					mLineVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor), lines.Count, BufferUsage.WriteOnly);

					VertexPositionColor	[]normVerts	=new VertexPositionColor[lines.Count];
					for(int i=0;i < lines.Count;i++)
					{
						normVerts[i].Position	=lines[i];
						normVerts[i].Color		=Color.Green;
					}

					mLineVB.SetData<VertexPositionColor>(normVerts);
*/

					//generate pathing information
//					mPathNodes	=mMap.GetGoodPathPoints();
/*
					mLineVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor), mPathNodes.Count * 2, BufferUsage.WriteOnly);

					VertexPositionColor	[]normVerts	=new VertexPositionColor[mPathNodes.Count * 2];
					for(int i=0;i < mPathNodes.Count;i++)
					{
						normVerts[i * 2].Position	=mPathNodes[i];
						normVerts[i * 2].Color		=Color.Green;

						normVerts[(i * 2) + 1].Position	=mPathNodes[i] + Vector3.UnitY * 20.0f;
						normVerts[(i * 2) + 1].Color	=Color.Green;
					}

					mLineVB.SetData<VertexPositionColor>(normVerts);

					mPathGrid	=PathLib.PathGrid.CreatePathGrid(false);

					mPathGrid.GenerateFromPoints(mPathNodes);
					*/
					mVisMap	=new VisMap();
					mVisMap.SetMap(mMap);
					mVisMap.LoadVisData(fileName);
					GraphicsDevice	g	=mGDM.GraphicsDevice;

					mMatLib.NukeAllMaterials();

					List<MaterialLib.Material>	mats	=mMap.GetMaterials();

					mIndoorMesh.BuildLM(g, mZoneForm.GetLightAtlasSize(), mMap.BuildLMRenderData, mMap.GetPlanes());
					mIndoorMesh.BuildVLit(g, mMap.BuildVLitRenderData, mMap.GetPlanes());
					mIndoorMesh.BuildAlpha(g, mMap.BuildAlphaRenderData, mMap.GetPlanes());
					mIndoorMesh.BuildFullBright(g, mMap.BuildFullBrightRenderData, mMap.GetPlanes());
					mIndoorMesh.BuildMirror(g, mMap.BuildMirrorRenderData, mMap.GetPlanes());
					mIndoorMesh.BuildSky(g, mMap.BuildSkyRenderData, mMap.GetPlanes());

					mModelMats	=mMap.GetModelTransforms();

					foreach(MaterialLib.Material mat in mats)
					{
						mMatLib.AddMaterial(mat);
					}
					mMatLib.RefreshShaderParameters();
					mMatForm.UpdateMaterials();

					mVisMap.SetMaterialVisBytes(mats.Count);
				}
				mZoneForm.EnableFileIO(true);
				mBSPForm.EnableFileIO(true);
				mVisForm.EnableFileIO(true);
				mZoneForm.SetZoneSaveEnabled(true);

				mOutputForm.Print("Zoning complete.\n");
			}
		}


		void OnFullBuild(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mbFullBuilding		=true;
			mFullBuildFileName	=FileUtil.StripExtension(fileName);

			OnOpenMap(sender, ea);
			OnBuild(sender, ea);
		}


		void OnUpdateEntities(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mFullBuildFileName	=FileUtil.StripExtension(fileName);

			//load the update entities from the .map
			Map	updatedMap	=new Map();
			updatedMap.LoadBrushFile(fileName, mBSPForm.BSPParameters);

			updatedMap.SaveUpdatedEntities(fileName);

			mOutputForm.Print("GBSP File Updated\n");

			OnZoneGBSP(mFullBuildFileName + ".gbsp", null);
			OnSaveZone(mFullBuildFileName + ".Zone", null);
		}


		void OnOpenMap(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mMap	=new Map();

			mMap.LoadBrushFile(fileName, mBSPForm.BSPParameters);

			mBSPForm.SetBuildEnabled(true);
			mBSPForm.SetSaveEnabled(false);
		}


		void OnBuild(object sender, EventArgs ea)
		{
			mbWorking	=true;
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);
			mMap.BuildTree(mBSPForm.BSPParameters);
		}


		void OnSaveGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mbWorking	=true;
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			mMap.SaveGBSPFile(fileName, mBSPForm.BSPParameters);
		}


		void OnLight(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mbWorking	=true;
			mEmissives	=UtilityLib.FileUtil.LoadEmissives(fileName);

			mBSPForm.SetSaveEnabled(false);
			mBSPForm.SetBuildEnabled(false);
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			mMap	=new Map();

			mMap.LightGBSPFile(fileName, EmissiveForMaterial,
				mBSPForm.LightParameters, mBSPForm.BSPParameters);
		}


		void OnBuildDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mBSPForm.SetSaveEnabled(true);
			mBSPForm.SetBuildEnabled(false);
			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);
			mbWorking	=false;

			if(mbFullBuilding)
			{
				OnSaveGBSP(mFullBuildFileName + ".gbsp", null);
			}
		}


		void OnLightDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);
			mbWorking	=false;

			if(mbFullBuilding)
			{
				OnMaterialVis(mFullBuildFileName + ".gbsp", null);

				mbWorking	=true;
				OnZoneGBSP(mFullBuildFileName + ".gbsp", null);
				mbFullBuilding	=false;
				mbWorking		=false;
			}
		}


		void OnGBSPSaveDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);
			mbWorking	=false;

			if(mbFullBuilding)
			{
				if(mBSPForm.BSPParameters.mbBuildAsBModel)
				{
					OnLight(mFullBuildFileName + ".gbsp", null);
				}
				else
				{
					OnVis(mFullBuildFileName + ".gbsp", null);
				}
			}
		}


		void OnQueryVisFarm(object sender, EventArgs e)
		{
			mbWorking	=true;
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
					mOutputForm.Print("Build farm capabilities for " + mvc.Endpoint.Address + "\n");
					mOutputForm.Print("Cpu speed in mhz:  " + bfc.mMHZ + "\n");
					mOutputForm.Print("Number of cpu cores:  " + bfc.mNumCores + "\n");
					mvc.mBuildCaps	=bfc;
				}
				else
				{
					mOutputForm.Print("Build farm node " + mvc.Endpoint.Address + " is not responding.\n");
					mvc.mBuildCaps	=null;
				}
			}
			mbWorking	=false;
		}


		void OnVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}
			mbWorking	=true;
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			VisParams	vp		=new VisParams();
			vp.mbDistribute		=mVisForm.bDistributed;
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=false;
			vp.mbSortPortals	=mVisForm.bSortPortals;

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, mBSPForm.BSPParameters, mBuildFarm);
		}


		void OnResumeVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}
			mbWorking	=true;
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			VisParams	vp		=new VisParams();
			vp.mbDistribute		=mVisForm.bDistributed;
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=true;
			vp.mbSortPortals	=mVisForm.bSortPortals;

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, mBSPForm.BSPParameters, mBuildFarm);
		}


		void OnVisDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mOutputForm.UpdateProgress(0, 0, 0);
			mbWorking	=false;
			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			if(mbFullBuilding)
			{
				OnLight(mFullBuildFileName + ".gbsp", null);
			}
		}


		void OnStopVis(object sender, EventArgs ea)
		{
			//dunno what to do here yet
		}


		void OnReLoadVisFarm(object sender, EventArgs e)
		{
			LoadBuildFarm();
		}


		void OnDumpTextures(object sender, EventArgs e)
		{
			mAllTextures.Sort();
			foreach(string tex in mAllTextures)
			{
				CoreEvents.Print("\t" + tex + "\n");
			}
		}


		void OnLoadDebug(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				return;
			}

			FileStream		fs	=new FileStream(fileName, FileMode.Open, FileAccess.Read);
			BinaryReader	br	=new BinaryReader(fs);

			List<Vector3>	points	=new List<Vector3>();

			int	numPoints	=br.ReadInt32();
			for(int i=0;i < numPoints;i++)
			{
				Vector3	p	=Vector3.Zero;

				p.X	=br.ReadSingle();
				p.Y	=br.ReadSingle();
				p.Z	=br.ReadSingle();

				points.Add(p);
			}

			br.Close();
			fs.Close();

			mLineVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor),
				points.Count, BufferUsage.WriteOnly);

			VertexPositionColor	[]normVerts	=new VertexPositionColor[points.Count];
			for(int i=0;i < points.Count;i++)
			{
				normVerts[i].Position	=points[i];
				normVerts[i].Color		=Color.Green;
			}

			mLineVB.SetData<VertexPositionColor>(normVerts);
		}


		Matrix GetModelMatrix(int modelIndex)
		{
			if(mModelMats == null)
			{
				return	Matrix.Identity;
			}

			if(!mModelMats.ContainsKey(modelIndex))
			{
				return	Matrix.Identity;
			}

			return	mModelMats[modelIndex];
		}


		void OnNumClustersChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

//			mBSPForm.NumberOfClusters	="" + num;
		}


		void OnNumVertsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

//			mBSPForm.NumberOfVerts	="" + num;
		}


		void OnNumPortalsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

//			mBSPForm.NumberOfPortals	="" + num;
		}


		void OnNumPlanesChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

//			mBSPForm.NumberOfPlanes	="" + num;
		}
		#endregion
	}
}
