using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BSPCore;
using BSPVis;
using MeshLib;
using UtilityLib;
using MaterialLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MatLib	=MaterialLib.MaterialLib;


namespace BSPBuilder
{
	internal class BSPBuilder
	{
		//data
		Map						mMap;
		VisMap					mVisMap;
		IndoorMesh				mZoneDraw;
		MatLib					mMatLib;
		StuffKeeper				mSKeeper;
		bool					mbWorking, mbFullBuilding;
		string					mFullBuildFileName;
		List<string>			mAllTextures	=new List<string>();
		Dictionary<int, Matrix>	mModelMats;
		string					mGameRootDir;

		//gpu
		GraphicsDevice	mGD;
		PostProcess		mPost;

		//debug draw stuff
		DebugDraw	mDebugDraw;

		//forms
		BSPForm		mBSPForm	=new BSPForm();
		VisForm		mVisForm	=new VisForm();
		ZoneForm	mZoneForm	=new ZoneForm();
		Output		mOutForm	=new Output();

		//shared forms
		SharedForms.MaterialForm		mMatForm;
		SharedForms.CelTweakForm		mCTForm;
		SharedForms.ThreadedProgress	mSProg;


		internal BSPBuilder(GraphicsDevice gd, string gameRootDir)
		{
			mGD				=gd;
			mGameRootDir	=gameRootDir;

			mSKeeper	=new StuffKeeper();

			mSKeeper.eCompilesNeeded	+=OnCompilesNeeded;
			mSKeeper.eCompileDone		+=OnCompileDone;

			mSKeeper.Init(mGD, gameRootDir);

			mDebugDraw	=new DebugDraw(gd, mSKeeper);
			mMatLib		=new MatLib(gd, mSKeeper);

			mMatLib.InitCelShading(1);
			mMatLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);

			mZoneDraw	=new IndoorMesh(gd, mMatLib);

			RenderTargetView	[]backBuf	=new RenderTargetView[1];
			DepthStencilView	backDepth;

			backBuf	=gd.DC.OutputMerger.GetRenderTargets(1, out backDepth);

			//set up post processing module
			mPost	=new PostProcess(gd, mMatLib, "Post.fx");

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			mPost.MakePostTarget(gd, "SceneColor", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostDepth(gd, "SceneDepth", resx, resy,
				(gd.GD.FeatureLevel != FeatureLevel.Level_9_3)?
					Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt);
			mPost.MakePostTarget(gd, "SceneDepthMatNorm", resx, resy, Format.R16G16B16A16_Float);
			mPost.MakePostTarget(gd, "Bleach", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Outline", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Bloom1", resx/2, resy/2, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Bloom2", resx/2, resy/2, Format.R8G8B8A8_UNorm);

			mMatForm	=new SharedForms.MaterialForm(mMatLib, mSKeeper);
			mCTForm		=new SharedForms.CelTweakForm(gd.GD, mMatLib);

			SetFormPos(mBSPForm, "BSPFormPos");
			SetFormPos(mVisForm, "VisFormPos");
			SetFormPos(mZoneForm, "ZoneFormPos");
			SetFormPos(mOutForm, "OutputFormPos");
			SetFormPos(mMatForm, "MaterialFormPos");
			SetFormPos(mCTForm, "CelTweakFormPos");

			//show forms
			mBSPForm.Visible	=true;
			mVisForm.Visible	=true;
			mZoneForm.Visible	=true;
			mOutForm.Visible	=true;
			mMatForm.Visible	=true;
			mCTForm.Visible		=true;

			//form events
			mZoneForm.eMaterialVis		+=OnMaterialVis;
			mZoneForm.eSaveZone			+=OnSaveZone;
			mZoneForm.eZoneGBSP			+=OnZoneGBSP;
			mZoneForm.eLoadDebug		+=OnLoadDebug;
			mZoneForm.eDumpTextures		+=OnDumpTextures;
			mBSPForm.eBuild				+=OnBuild;
			mBSPForm.eLight				+=OnLight;
			mBSPForm.eOpenMap			+=OnOpenMap;
			mBSPForm.eSave				+=OnSaveGBSP;
			mBSPForm.eFullBuild			+=OnFullBuild;
			mBSPForm.eUpdateEntities	+=OnUpdateEntities;
			mVisForm.eResumeVis			+=OnResumeVis;
			mVisForm.eStopVis			+=OnStopVis;
			mVisForm.eVis				+=OnVis;

			//core events
			CoreEvents.eBuildDone		+=OnBuildDone;
			CoreEvents.eLightDone		+=OnLightDone;
			CoreEvents.eGBSPSaveDone	+=OnGBSPSaveDone;
			CoreEvents.eVisDone			+=OnVisDone;

			//stats
			CoreEvents.eNumPortalsChanged	+=OnNumPortalsChanged;
			CoreEvents.eNumClustersChanged	+=OnNumClustersChanged;
			CoreEvents.eNumPlanesChanged	+=OnNumPlanesChanged;
			CoreEvents.eNumVertsChanged		+=OnNumVertsChanged;
		}


		internal bool Busy()
		{
			return	mbWorking;
		}


		internal void Update(float msDelta, GraphicsDevice gd)
		{
			if(mbWorking)
			{
				Thread.Sleep(0);
				return;
			}

			mZoneDraw.Update(msDelta);

			mMatLib.SetParameterForAll("mView", gd.GCam.View);
			mMatLib.SetParameterForAll("mEyePos", gd.GCam.Position);
			mMatLib.SetParameterForAll("mProjection", gd.GCam.Projection);
		}


		internal void Render(GraphicsDevice gd)
		{
			if(mbWorking)
			{
				Thread.Sleep(0);
				return;
			}

			if(mZoneDraw == null || mVisMap == null)
			{
				mDebugDraw.Draw(mGD);
				return;
			}

/*			mPost.SetTargets(gd, "SceneDepthMatNorm", "SceneColor");

			mPost.ClearTarget(gd, "SceneDepthMatNorm", Color.White);
			mPost.ClearDepth(gd, "SceneColor");

			mZoneDraw.DrawDMN(gd, mVisMap.IsMaterialVisibleFromPos, GetModelMatrix, RenderExternalDMN);

			mPost.SetTargets(gd, "SceneColor", "SceneColor");

			mPost.ClearTarget(gd, "SceneColor", Color.CornflowerBlue);
			mPost.ClearDepth(gd, "SceneColor");*/

			mZoneDraw.Draw(gd, 0, mVisMap.IsMaterialVisibleFromPos, GetModelMatrix, RenderExternal, RenderShadows);

/*			mPost.SetTargets(gd, "Outline", "null");
			mPost.SetParameter("mNormalTex", "SceneDepthMatNorm");
			mPost.DrawStage(gd, "Outline");

			mPost.SetTargets(gd, "Bleach", "null");
			mPost.SetParameter("mColorTex", "SceneColor");
			mPost.DrawStage(gd, "BleachBypass");

			mPost.SetTargets(gd, "Bloom1", "null");
			mPost.SetParameter("mBlurTargetTex", "Bleach");
			mPost.DrawStage(gd, "BloomExtract");

			mPost.SetTargets(gd, "Bloom2", "null");
			mPost.SetParameter("mBlurTargetTex", "Bloom1");
			mPost.DrawStage(gd, "GaussianBlurX");

			mPost.SetTargets(gd, "Bloom1", "null");
			mPost.SetParameter("mBlurTargetTex", "Bloom2");
			mPost.DrawStage(gd, "GaussianBlurY");

			mPost.SetTargets(gd, "SceneColor", "null");
			mPost.SetParameter("mBlurTargetTex", "Bloom1");
			mPost.SetParameter("mColorTex", "Bleach");
			mPost.DrawStage(gd, "BloomCombine");

			mPost.SetTargets(gd, "BackColor", "BackDepth");
			mPost.SetParameter("mBlurTargetTex", "Outline");
			mPost.SetParameter("mColorTex", "SceneColor");
			mPost.DrawStage(gd, "Modulate");*/
		}


		void BuildDebugDraw(Map.DebugDrawChoice choice)
		{
			List<Vector3>	verts	=new List<Vector3>();
			List<UInt16>	inds	=new List<UInt16>();
			List<Vector3>	norms	=new List<Vector3>();
			List<Color>		cols	=new List<Color>();

			mMap.GetTriangles(verts, norms, cols, inds, choice);
			
			mDebugDraw.MakeDrawStuff(mGD.GD, verts, norms, cols, inds);
		}


		void RenderExternal(MaterialLib.AlphaPool ap, GameCamera gcam)
		{
		}


		void RenderExternalDMN(GameCamera gcam)
		{
		}


		bool RenderShadows(int shadIndex)
		{
			return	false;
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


		void SetFormPos(Form form, string posName)
		{
			form.DataBindings.Add(new Binding("Location",
				Settings.Default, posName, true,
				DataSourceUpdateMode.OnPropertyChanged));

			System.Configuration.SettingsPropertyValue	val	=			
				Settings.Default.PropertyValues[posName];

			form.Location	=(System.Drawing.Point)val.PropertyValue;
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
			mVisMap.MaterialVisGBSPFile(fileName, mGD);

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			mVisMap	=null;
		}

		void OnSaveZone(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;

			if(fileName != null)
			{
				mZoneForm.Text	=fileName;
				mMap.Write(fileName, mZoneForm.SaveDebugInfo,
					mMatLib.GetMaterialNames().Count, mVisMap.SaveVisZoneData);

				//write out the zoneDraw
				mZoneDraw.Write(fileName + "Draw");

				mOutForm.Print("Zone save complete.\n");
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
					mVisMap	=new VisMap();
					mVisMap.SetMap(mMap);
					mVisMap.LoadVisData(fileName);

					mMatLib.NukeAllMaterials();

					mMap.MakeMaterials(mGD, mMatLib, fileName);

					mZoneDraw.BuildLM(mGD, mSKeeper, mZoneForm.GetLightAtlasSize(), mMap.BuildLMRenderData, mMap.GetPlanes());
					mZoneDraw.BuildVLit(mGD, mSKeeper, mMap.BuildVLitRenderData, mMap.GetPlanes());
					mZoneDraw.BuildAlpha(mGD, mSKeeper, mMap.BuildAlphaRenderData, mMap.GetPlanes());
					mZoneDraw.BuildFullBright(mGD, mSKeeper, mMap.BuildFullBrightRenderData, mMap.GetPlanes());
					mZoneDraw.BuildMirror(mGD, mSKeeper, mMap.BuildMirrorRenderData, mMap.GetPlanes());
					mZoneDraw.BuildSky(mGD, mSKeeper, mMap.BuildSkyRenderData, mMap.GetPlanes());

					mZoneDraw.FinishAtlas(mGD, mSKeeper);

					mModelMats	=mMap.GetModelTransforms();

					mMatForm.RefreshMaterials();

					HideParametersByMaterial();

					mVisMap.SetMaterialVisBytes(mMatLib.GetMaterialNames().Count);

					mMatLib.SetLightMapsToAtlas();
				}
				mZoneForm.EnableFileIO(true);
				mBSPForm.EnableFileIO(true);
				mVisForm.EnableFileIO(true);
				mZoneForm.SetZoneSaveEnabled(true);

				mOutForm.Print("Zoning complete.\n");

//				BuildDebugDraw(Map.DebugDrawChoice.GFXFaces);
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

//			mLineVB	=new VertexBuffer(mGDM.GraphicsDevice, typeof(VertexPositionColor),
//				points.Count, BufferUsage.WriteOnly);

//			VertexPositionColor	[]normVerts	=new VertexPositionColor[points.Count];
//			for(int i=0;i < points.Count;i++)
//			{
//				normVerts[i].Position	=points[i];
//				normVerts[i].Color		=Color.Green;
//			}

//			mLineVB.SetData<VertexPositionColor>(normVerts);
		}

		void OnDumpTextures(object sender, EventArgs e)
		{
			mAllTextures.Sort();
			foreach(string tex in mAllTextures)
			{
				CoreEvents.Print("\t" + tex + "\n");
			}
		}

		void OnBuild(object sender, EventArgs ea)
		{
			mbWorking	=true;
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);
			mMap.BuildTree(mBSPForm.BSPParameters);
		}

		void OnLight(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mbWorking	=true;
//			mEmissives	=FileUtil.LoadEmissives(fileName);

			mBSPForm.SetSaveEnabled(false);
			mBSPForm.SetBuildEnabled(false);
			mZoneForm.EnableFileIO(false);
			mBSPForm.EnableFileIO(false);
			mVisForm.EnableFileIO(false);

			mMap	=new Map();

			mMap.LightGBSPFile(fileName, mBSPForm.LightParameters,
				mBSPForm.BSPParameters);
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

			BuildDebugDraw(Map.DebugDrawChoice.MapBrushes);
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

			mOutForm.Print("GBSP File Updated\n");

			OnZoneGBSP(mFullBuildFileName + ".gbsp", null);
			OnSaveZone(mFullBuildFileName + ".Zone", null);
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
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=true;
			vp.mbSortPortals	=mVisForm.bSortPortals;

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, mBSPForm.BSPParameters);
		}

		void OnStopVis(object sender, EventArgs ea)
		{
			//dunno what to do here yet
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
			vp.mbFullVis		=!mVisForm.bRough;
			vp.mbResume			=false;
			vp.mbSortPortals	=mVisForm.bSortPortals;

			mVisMap	=new VisMap();

			mVisMap.VisGBSPFile(fileName, vp, mBSPForm.BSPParameters);
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

			if(bSuccess)
			{
				if(mbFullBuilding)
				{
					OnSaveGBSP(mFullBuildFileName + ".gbsp", null);
				}
				else
				{
				}
			}
			else
			{
				CoreEvents.Print("Halting full build due to a bsp build failure.\n");
				mbFullBuilding	=false;
			}
		}

		void OnLightDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);
			mbWorking	=false;

			if(bSuccess)
			{
				if(mbFullBuilding)
				{
					mbWorking	=true;
					OnMaterialVis(mFullBuildFileName + ".gbsp", null);

					OnZoneGBSP(mFullBuildFileName + ".gbsp", null);
					mbFullBuilding	=false;
					mbWorking		=false;
				}
			}
			else
			{
				CoreEvents.Print("Halting full build due to a light failure.\n");
				mbFullBuilding	=false;
			}
		}

		void OnGBSPSaveDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);
			mbWorking	=false;

			if(bSuccess)
			{
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
			else
			{
				CoreEvents.Print("Halting full build due to a gbsp save failure.\n");
				mbFullBuilding	=false;
			}
		}

		void OnVisDone(object sender, EventArgs ea)
		{
			bool	bSuccess	=(bool)sender;

			mOutForm.UpdateProgress(0, 0, 0);
			mbWorking	=false;
			mZoneForm.EnableFileIO(true);
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			if(bSuccess)
			{
				if(mbFullBuilding)
				{
					OnLight(mFullBuildFileName + ".gbsp", null);
				}
			}
			else
			{
				CoreEvents.Print("Halting full build due to a vis failure.\n");
				mbFullBuilding	=false;
			}
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


		void HideParametersByMaterial()
		{
			List<string>	mats	=mMatLib.GetMaterialNames();

			//some stuff isn't used by most bsp stuff
			List<string>	toIgnore	=new List<string>();
			toIgnore.Add("mSpecColor");
			toIgnore.Add("mSpecPower");
			toIgnore.Add("mSolidColour");

			//renderstates are never used as a variable
			toIgnore.Add("AlphaBlending");
			toIgnore.Add("NoBlending");
			toIgnore.Add("ShadowBlending");
			toIgnore.Add("EnableDepth");
			toIgnore.Add("DisableDepth");
			toIgnore.Add("DisableDepthWrite");
			toIgnore.Add("DisableDepthTest");
			toIgnore.Add("NoCull");
			toIgnore.Add("LinearClamp");
			toIgnore.Add("LinearWrap");
			toIgnore.Add("PointClamp");
			toIgnore.Add("PointWrap");
			toIgnore.Add("LinearClampCube");
			toIgnore.Add("LinearWrapCube");
			toIgnore.Add("PointClampCube");
			toIgnore.Add("PointWrapCube");
			toIgnore.Add("PointClamp1D");

			//common hidey stuff
			List<string>	toHide	=new List<string>();
			toHide.Add("mWorld");
			toHide.Add("mView");
			toHide.Add("mProjection");
			toHide.Add("mLightViewProj");
			toHide.Add("mEyePos");
			toHide.Add("mCelTable");
			toHide.Add("mShadowTexture");
			toHide.Add("mShadowCube");
			toHide.Add("mShadowLightPos");
			toHide.Add("mbDirectional");
			toHide.Add("mAniIntensities");
			toHide.Add("mDynLights");
			toHide.Add("mShadowAtten");
			toHide.Add("mSpecColor");	//eventually want these
			toHide.Add("mSpecPower");	//for future
			toHide.Add("mMaterialID");	//idkeeper takes care of this

			//hide stuff that the user doesn't care about
			//these are for all indoormesh materials
			foreach(string m in mats)
			{
				//look for material specific stuff to hide / ignore
				List<string>	matIgnores	=new List<string>();
				List<string>	matHides	=new List<string>();

				string	tech	=mMatLib.GetMaterialTechnique(m);

				if(tech == "FullBright"
					|| tech == "VertexLightingCel"
					|| tech == "VertexLighting"
					|| tech == "VertexLightingAlpha"
					|| tech == "VertexLightingAlphaCel")
				{
					matIgnores.Add("mLightMap");
					matIgnores.Add("mEyePos");
					matIgnores.Add("mSkyGradient0");
					matIgnores.Add("mSkyGradient1");
					matHides.Add("mLightMap");
					matHides.Add("mEyePos");
					matHides.Add("mSkyGradient0");
					matHides.Add("mSkyGradient1");
				}
				else if(tech.StartsWith("LightMap"))
				{
					matIgnores.Add("mEyePos");
					matIgnores.Add("mSkyGradient0");
					matIgnores.Add("mSkyGradient1");
					matHides.Add("mSkyGradient0");
					matHides.Add("mSkyGradient1");
					matHides.Add("mEyePos");
					matHides.Add("mLightMap");	//autohandled
				}
				else if(tech == "Sky")
				{
					matIgnores.Add("mLightMap");
					matHides.Add("mLightMap");
				}

				if(!tech.Contains("Anim"))
				{
					matIgnores.Add("mAniIntensities");
				}

				if(!tech.Contains("Cel"))
				{
					matIgnores.Add("mCelTable");
				}

				//add common stuff
				matHides.AddRange(toHide);
				matIgnores.AddRange(toIgnore);

				mMatLib.IgnoreMaterialVariables(m, matIgnores);
				mMatLib.HideMaterialVariables(m, matHides);
			}
		}


		void OnCompilesNeeded(object sender, EventArgs ea)
		{
			Thread	uiThread	=new Thread(() =>
				{
					mSProg	=new SharedForms.ThreadedProgress("Compiling Shaders...");
					Application.Run(mSProg);
				});

			uiThread.SetApartmentState(ApartmentState.STA);
			uiThread.Start();

			while(mSProg == null)
			{
				Thread.Sleep(0);
			}

			mSProg.SetSizeInfo(0, (int)sender);
		}


		void OnCompileDone(object sender, EventArgs ea)
		{
			mSProg.SetCurrent((int)sender);

			if((int)sender == mSProg.GetMax())
			{
				mSProg.Nuke();
			}
		}
	}
}
