﻿using System;
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
		bool					mbWorking, mbFullBuilding;
		string					mFullBuildFileName;
		List<string>			mAllTextures	=new List<string>();
		Dictionary<int, Matrix>	mModelMats;

		//gpu
		GraphicsDevice	mGD;
		PostProcess		mPost;

		//forms
		BSPForm		mBSPForm	=new BSPForm();
		VisForm		mVisForm	=new VisForm();
		ZoneForm	mZoneForm	=new ZoneForm();
		Output		mOutForm	=new Output();

		//shared forms
		SharedForms.MaterialForm	mMatForm;
		SharedForms.CelTweakForm	mCTForm;


		internal BSPBuilder(GraphicsDevice gd)
		{
			mGD	=gd;

			MatLib.ShaderModel	shaderModel;

			switch(gd.GD.FeatureLevel)
			{
				case	FeatureLevel.Level_11_0:
					shaderModel	=MatLib.ShaderModel.SM5;
					break;
				case	FeatureLevel.Level_10_1:
					shaderModel	=MatLib.ShaderModel.SM41;
					break;
				case	FeatureLevel.Level_10_0:
					shaderModel	=MatLib.ShaderModel.SM4;
					break;
				case	FeatureLevel.Level_9_3:
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
				default:
					Debug.Assert(false);	//only support the above
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
			}

			mMatLib		=new MatLib(gd.GD, shaderModel, true);

			mMatLib.InitCelShading(1);
			mMatLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);

			mZoneDraw	=new IndoorMesh(gd, mMatLib);

			RenderTargetView	[]backBuf	=new RenderTargetView[1];
			DepthStencilView	backDepth;

			backBuf	=gd.DC.OutputMerger.GetRenderTargets(1, out backDepth);

			//set up post processing module
			mPost	=new PostProcess(gd, mMatLib.GetEffect("Post.fx"),
				gd.RendForm.ClientRectangle.Width, gd.RendForm.ClientRectangle.Height,
				backBuf[0], backDepth);

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			mPost.MakePostTarget(gd, "SceneColor", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostDepth(gd, "SceneColor", resx, resy,
				(gd.GD.FeatureLevel != FeatureLevel.Level_9_3)?
					Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt);
			mPost.MakePostTarget(gd, "SceneDepthMatNorm", resx, resy, Format.R16G16B16A16_Float);
			mPost.MakePostTarget(gd, "Bleach", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Outline", resx, resy, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Bloom1", resx/2, resy/2, Format.R8G8B8A8_UNorm);
			mPost.MakePostTarget(gd, "Bloom2", resx/2, resy/2, Format.R8G8B8A8_UNorm);

			mMatForm	=new SharedForms.MaterialForm(mMatLib);
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
			mZoneForm.eGenerateMaterials	+=OnGenerateMaterials;
			mZoneForm.eMaterialVis			+=OnMaterialVis;
			mZoneForm.eSaveZone				+=OnSaveZone;
			mZoneForm.eZoneGBSP				+=OnZoneGBSP;
			mZoneForm.eLoadDebug			+=OnLoadDebug;
			mZoneForm.eDumpTextures			+=OnDumpTextures;
			mBSPForm.eBuild					+=OnBuild;
			mBSPForm.eLight					+=OnLight;
			mBSPForm.eOpenMap				+=OnOpenMap;
			mBSPForm.eSave					+=OnSaveGBSP;
			mBSPForm.eFullBuild				+=OnFullBuild;
			mBSPForm.eUpdateEntities		+=OnUpdateEntities;
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
		}


		internal void Update(float msDelta, GraphicsDevice gd)
		{
			if(mbWorking)
			{
				Thread.Sleep(0);
				return;
			}

			mMatLib.SetParameterForAll("mView", gd.GCam.View);
			mMatLib.SetParameterForAll("mEyePos", gd.GCam.Position);
			mMatLib.SetParameterForAll("mProjection", gd.GCam.Projection);
//			mMatLib.SetCelTexture(0);
		}


		internal void Render(GraphicsDevice gd)
		{
			if(mbWorking)
			{
				Thread.Sleep(0);
				return;
			}

			mPost.SetTargets(gd, "SceneDepthMatNorm", "SceneColor");

			mPost.ClearTarget(gd, "SceneDepthMatNorm", Color.White);
			mPost.ClearDepth(gd, "SceneColor");

//			ss.RenderDMN(gd.DC);

			mPost.SetTargets(gd, "SceneColor", "SceneColor");

			mPost.ClearTarget(gd, "SceneColor", Color.CornflowerBlue);
			mPost.ClearDepth(gd, "SceneColor");

//			ss.Render(gd.DC);

			mPost.SetTargets(gd, "Outline", "null");
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
			mPost.DrawStage(gd, "Modulate");
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


		//events
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
			List<string>	mats	=mMap.GenerateMaterials(fileName);

//			mMatLib.RefreshShaderParameters();
//			mMatForm.UpdateMaterials();

			mVisMap	=new VisMap();
			mVisMap.LoadVisData(fileName);

			mZoneForm.EnableFileIO(true);	//not threaded
			mBSPForm.EnableFileIO(true);
			mVisForm.EnableFileIO(true);

			mOutForm.Print("Materials generated.\n");

			//store a list of textures used
			foreach(string m in mats)
			{
				string	texName	=mMatLib.GetMaterialValue(m, "mTexture") as string;
				if(texName == null || texName == "")
				{
					continue;
				}

				int		starPos	=texName.LastIndexOf('*');
				if(starPos != -1)
				{
					texName	=texName.Substring(0, texName.LastIndexOf('*'));
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
//					GraphicsDevice	gd	=mGDM.GraphicsDevice;

					mMatLib.NukeAllMaterials();

					List<string>	mats	=mMap.GetMaterials();

					mZoneDraw.BuildLM(mGD, mZoneForm.GetLightAtlasSize(), mMap.BuildLMRenderData, mMap.GetPlanes());
					mZoneDraw.BuildVLit(mGD, mMap.BuildVLitRenderData, mMap.GetPlanes());
					mZoneDraw.BuildAlpha(mGD, mMap.BuildAlphaRenderData, mMap.GetPlanes());
					mZoneDraw.BuildFullBright(mGD, mMap.BuildFullBrightRenderData, mMap.GetPlanes());
					mZoneDraw.BuildMirror(mGD, mMap.BuildMirrorRenderData, mMap.GetPlanes());
					mZoneDraw.BuildSky(mGD, mMap.BuildSkyRenderData, mMap.GetPlanes());

					mModelMats	=mMap.GetModelTransforms();

//					mMatLib.RefreshShaderParameters();
//					mMatForm.UpdateMaterials();

					//this avoids altering form stuff on another thread
//					mMatForm.ReWireParameters(false);
					HideParametersByMaterial();
//					mMatForm.ReWireParameters(true);

					mVisMap.SetMaterialVisBytes(mats.Count);
				}
				mZoneForm.EnableFileIO(true);
				mBSPForm.EnableFileIO(true);
				mVisForm.EnableFileIO(true);
				mZoneForm.SetZoneSaveEnabled(true);

				mOutForm.Print("Zoning complete.\n");
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
					OnMaterialVis(mFullBuildFileName + ".gbsp", null);

					mbWorking	=true;
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
			toIgnore.Add("mDanglyForce");

			//common hidey stuff
			List<string>	toHide	=new List<string>();
			toHide.Add("mWorld");
			toHide.Add("mView");
			toHide.Add("mProjection");
			toHide.Add("mLightViewProj");
			toHide.Add("mEyePos");
			toHide.Add("mCelTable");
			toHide.Add("mShadowTexture");
			toHide.Add("mShadowLightPos");
			toHide.Add("mbDirectional");
			toHide.Add("mAniIntensities");
			toHide.Add("mWarpFactor");
			toHide.Add("mDanglyForce");
			toHide.Add("mDynLights");
			toHide.Add("mShadowAtten");
			toHide.Add("mYRangeMax");
			toHide.Add("mYRangeMin");
			toHide.Add("mSpecColor");	//eventually want these
			toHide.Add("mSpecPower");	//for future

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
					|| tech == "Alpha")
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

				mMatLib.IgnoreMaterialVariables(m, toIgnore);
				mMatLib.HideMaterialVariables(m, toHide);
			}
		}
	}
}
