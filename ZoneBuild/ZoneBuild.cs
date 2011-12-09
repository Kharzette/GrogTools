using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using SharedForms;


namespace ZoneBuild
{
	public class ZoneBuild : Game
	{
		GraphicsDeviceManager	mGDM;
		SpriteBatch				mSB;
		ContentManager			mSharedCM;

		ZoneForm		mZoneForm;
		MaterialForm	mMatForm;

		//data
		Map						mMap;
		VisMap					mVisMap;
		MaterialLib.MaterialLib	mMatLib;
		MeshLib.IndoorMesh		mIndoorMesh;

		//debug draw stuff
		SpriteFont		mKoot;
		Vector2			mTextPos;
		Random			mRnd	=new Random();
		Vector3			mDynamicLightPos;
		VertexBuffer	mLineVB, mPortVB;
		IndexBuffer		mPortIB;
		BasicEffect		mBFX;

		//control / view
		UtilityLib.GameCamera		mGameCam;
		UtilityLib.PlayerSteering	mPlayerControl;
		UtilityLib.Input			mInput;


		public ZoneBuild()
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
					global::ZoneBuild.ZBSettings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

				mainWindow.Location	=
					global::ZoneBuild.ZBSettings.Default.MainWindowPos;

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
			mBFX		=new BasicEffect(mGDM.GraphicsDevice);

			mBFX.VertexColorEnabled	=true;
			mBFX.LightingEnabled	=false;
			mBFX.TextureEnabled		=false;

			mMatLib	=new MaterialLib.MaterialLib(mGDM.GraphicsDevice, Content, mSharedCM, true);

			mIndoorMesh	=new MeshLib.IndoorMesh(GraphicsDevice, mMatLib);

			mMatForm					=new SharedForms.MaterialForm(mGDM.GraphicsDevice, mMatLib, false);
			mMatForm.Visible			=true;
			mMatForm.eMaterialNuked		+=OnMaterialNuked;
			mMatForm.eLibraryCleared	+=OnMaterialsCleared;
			mMatForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ZoneBuild.ZBSettings.Default,
				"MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mMatForm.Location	=
				global::ZoneBuild.ZBSettings.Default.MaterialFormPos;

			mZoneForm			=new ZoneForm();
			mZoneForm.Visible	=true;
			mZoneForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				global::ZoneBuild.ZBSettings.Default,
				"ZoneFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			mZoneForm.Location	=
				global::ZoneBuild.ZBSettings.Default.ZoneFormPos;

			mZoneForm.eGenerateMaterials	+=OnGenerateMaterials;
			mZoneForm.eMaterialVis			+=OnMaterialVis;
			mZoneForm.eSaveZone				+=OnSaveZone;
			mZoneForm.eZoneGBSP				+=OnZoneGBSP;
			mZoneForm.eSaveEmissives		+=OnSaveEmissives;
			mZoneForm.eLoadPortals			+=OnLoadPortals;

			mKoot	=mSharedCM.Load<SpriteFont>("Fonts/Koot20");
		}


		protected override void UnloadContent()
		{
		}


		protected override void Update(GameTime gameTime)
		{
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

			mMatLib.UpdateWVP(mGameCam.World, mGameCam.View, mGameCam.Projection, -mPlayerControl.Position);
			mBFX.World		=mGameCam.World;
			mBFX.View		=mGameCam.View;
			mBFX.Projection	=mGameCam.Projection;

			base.Update(gameTime);
		}


		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			GraphicsDevice	g	=mGDM.GraphicsDevice;

			if(mMap != null)
			{
				mIndoorMesh.Draw(g, mGameCam, -mPlayerControl.Position, mVisMap.IsMaterialVisibleFromPos);
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

			if(mPortVB != null)
			{
				g.SetVertexBuffer(mPortVB);
				g.Indices	=mPortIB;

				mBFX.CurrentTechnique.Passes[0].Apply();

				g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mPortVB.VertexCount, 0, mPortIB.IndexCount / 3);
			}

			mSB.Begin();

			mSB.DrawString(mKoot, "Coordinates: " + -mPlayerControl.Position, mTextPos, Color.Yellow);

			mSB.End();

			base.Draw(gameTime);
		}


		void RegisterMapEvents(bool bReg)
		{
			if(bReg)
			{
				CoreEvents.eNumPortalsChanged		+=OnNumPortalsChanged;
				CoreEvents.eNumClustersChanged		+=OnNumClustersChanged;
				CoreEvents.eNumPlanesChanged		+=OnNumPlanesChanged;
				CoreEvents.eNumVertsChanged			+=OnNumVertsChanged;
			}
			else
			{
				CoreEvents.eNumPortalsChanged		-=OnNumPortalsChanged;
				CoreEvents.eNumClustersChanged		-=OnNumClustersChanged;
				CoreEvents.eNumPlanesChanged		-=OnNumPlanesChanged;
				CoreEvents.eNumVertsChanged			-=OnNumVertsChanged;
			}
		}


		void OnMaterialNuked(object sender, EventArgs ea)
		{
			if(mMap != null)
			{
				//rebuild material vis
				mVisMap.VisMaterials();
			}
		}


		void OnMaterialsCleared(object sender, EventArgs ea)
		{
			//might need to readd lightmap tex
		}


		void OnLoadPortals(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				return;
			}

			FileStream		fs	=new FileStream(fileName, FileMode.Open, FileAccess.Read);
			BinaryReader	br	=new BinaryReader(fs);

			List<GBSPPoly>	ports	=new List<GBSPPoly>();

			int	numPorts	=br.ReadInt32();
			for(int i=0;i < numPorts;i++)
			{
				GBSPPoly	p	=new GBSPPoly(0);

				p.Read(br);

				ports.Add(p);
			}

			br.Close();
			fs.Close();

			List<Vector3>	verts	=new List<Vector3>();
			List<UInt32>	inds	=new List<UInt32>();

			foreach(GBSPPoly p in ports)
			{
				p.GetTriangles(verts, inds, false);
			}

			mPortVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);

			VertexPositionColor	[]vpc	=new VertexPositionColor[verts.Count];

			for(int i=0;i < verts.Count;i++)
			{
				vpc[i].Position	=verts[i];
				vpc[i].Color	=UtilityLib.Mathery.RandomColor(mRnd);
			}

			mPortVB.SetData<VertexPositionColor>(vpc);

			mPortIB	=new IndexBuffer(mGDM.GraphicsDevice, IndexElementSize.ThirtyTwoBits, inds.Count, BufferUsage.WriteOnly);

			mPortIB.SetData<UInt32>(inds.ToArray());
		}


		void OnMaterialVis(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName == null)
			{
				return;
			}

			mZoneForm.Text	=fileName;
			if(mMap != null)
			{
				//unregister old events
				RegisterMapEvents(false);
				mMap	=null;
			}
			mZoneForm.SetZoneSaveEnabled(false);
			mZoneForm.EnableFileIO(false);

			mVisMap	=new VisMap();
			mVisMap.MaterialVisGBSPFile(fileName);

			mZoneForm.EnableFileIO(true);	//not threaded

			mVisMap	=null;
		}


		void OnGenerateMaterials(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mZoneForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mZoneForm.SetZoneSaveEnabled(false);
				mZoneForm.EnableFileIO(false);
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

				mVisMap	=new VisMap();
				mVisMap.LoadVisData(fileName);

				mZoneForm.EnableFileIO(true);	//not threaded
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
		}


		void OnSaveZone(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mZoneForm.Text	=fileName;
				mMap.Write(fileName, mMatLib.GetMaterials().Count, mVisMap.SaveVisZoneData);

				//write out the zoneDraw
				mIndoorMesh.Write(fileName + "Draw");
			}
		}


		void OnZoneGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mZoneForm.Text	=fileName;
				if(mMap != null)
				{
					//unregister old events
					RegisterMapEvents(false);
				}
				mZoneForm.EnableFileIO(false);
				mMap	=new Map();

				RegisterMapEvents(true);

				GFXHeader	hdr	=mMap.LoadGBSPFile(fileName);

				if(hdr == null)
				{
					CoreEvents.Print("Load failed\n");
				}
				else
				{
					List<Vector3>	lines	=mMap.GetFaceNormals();

					mLineVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor), lines.Count, BufferUsage.WriteOnly);

					VertexPositionColor	[]normVerts	=new VertexPositionColor[lines.Count];
					for(int i=0;i < lines.Count;i++)
					{
						normVerts[i].Position	=lines[i];
						normVerts[i].Color		=Color.Green;
					}

					mLineVB.SetData<VertexPositionColor>(normVerts);

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

					foreach(MaterialLib.Material mat in mats)
					{
						mMatLib.AddMaterial(mat);
					}
					mMatLib.RefreshShaderParameters();
					mMatForm.UpdateMaterials();
				}
				mZoneForm.EnableFileIO(true);
				mZoneForm.SetZoneSaveEnabled(true);
			}
		}


		void OnNumClustersChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mZoneForm.NumberOfClusters	="" + num;
		}


		void OnNumVertsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mZoneForm.NumberOfVerts	="" + num;
		}


		void OnNumPortalsChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mZoneForm.NumberOfPortals	="" + num;
		}


		void OnNumPlanesChanged(object sender, EventArgs ea)
		{
			int	num	=(int)sender;

			mZoneForm.NumberOfPlanes	="" + num;
		}
	}
}
