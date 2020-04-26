using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using BSPCore;
using BSPVis;
using MeshLib;
using UtilityLib;
using MaterialLib;
using SharpDX;
using SharpDX.Direct3D;

using MatLib = MaterialLib.MaterialLib;


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

		//shader parameter stuff
		List<string>	mCommonIgnores	=new List<string>();
		List<string>	mCommonHides	=new List<string>();

		//gpu
		GraphicsDevice	mGD;

		//debug draw stuff
		DebugDraw	mDebugDraw;

		//forms
		BSPForm		mBSPForm	=new BSPForm();
		VisForm		mVisForm	=new VisForm();
		ZoneForm	mZoneForm	=new ZoneForm();

		//shared forms
		SharedForms.MaterialForm	mMatForm;
		SharedForms.CelTweakForm	mCTForm;
		SharedForms.Output			mOutForm	=new SharedForms.Output();


		internal BSPBuilder(GraphicsDevice gd, string gameRootDir)
		{
			mGD				=gd;
			mGameRootDir	=gameRootDir;

			mGD.eDeviceLost	+=OnDeviceLost;

			mSKeeper	=new StuffKeeper();

			mSKeeper.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			mSKeeper.eCompileDone	+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			LoadStuff();

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			mMatForm	=new SharedForms.MaterialForm(mMatLib, mSKeeper);
			mCTForm		=new SharedForms.CelTweakForm(gd.GD, mMatLib);

			SetFormPos(mBSPForm, "BSPFormPos");
			SetFormPos(mVisForm, "VisFormPos");
			SetFormPos(mZoneForm, "ZoneFormPos");
			SetFormPos(mOutForm, "OutputFormPos");
			SetFormPos(mMatForm, "MaterialFormPos");
			SetFormPos(mCTForm, "CelTweakFormPos");
			SetFormSize(mMatForm, "MaterialFormSize");

			//show forms
			mBSPForm.Visible	=true;
			mVisForm.Visible	=true;
			mZoneForm.Visible	=true;
			mOutForm.Visible	=true;
			mMatForm.Visible	=true;
			mCTForm.Visible		=true;

			//form events
			mZoneForm.eMaterialVis			+=OnMaterialVis;
			mZoneForm.eSaveZone				+=OnSaveZone;
			mZoneForm.eZoneGBSP				+=OnZoneGBSP;
			mZoneForm.eLoadDebug			+=OnLoadDebug;
			mZoneForm.eDumpTextures			+=OnDumpTextures;
			mBSPForm.eBuild					+=OnBuild;
			mBSPForm.eLight					+=OnLight;
			mBSPForm.eOpenMap				+=OnOpenMap;
			mBSPForm.eStaticToMap			+=OnOpenStatic;
			mBSPForm.eMapToStatic			+=OnMapToStatic;
			mBSPForm.eSave					+=OnSaveGBSP;
			mBSPForm.eFullBuild				+=OnFullBuild;
			mBSPForm.eUpdateEntities		+=OnUpdateEntities;
			mVisForm.eResumeVis				+=OnResumeVis;
			mVisForm.eStopVis				+=OnStopVis;
			mVisForm.eVis					+=OnVis;
			mMatForm.eMatLibNotReadyToSave	+=OnMatLibNotReadyToSave;
			mMatForm.eMatTechniqueChanged	+=OnMatTechChanged;

			//core events
			CoreEvents.eBuildDone		+=OnBuildDone;
			CoreEvents.eLightDone		+=OnLightDone;
			CoreEvents.eGBSPSaveDone	+=OnGBSPSaveDone;
			CoreEvents.eVisDone			+=OnVisDone;
			CoreEvents.ePrint			+=OnPrint;

			//stats
			CoreEvents.eNumPortalsChanged	+=OnNumPortalsChanged;
			CoreEvents.eNumClustersChanged	+=OnNumClustersChanged;
			CoreEvents.eNumPlanesChanged	+=OnNumPlanesChanged;
			CoreEvents.eNumVertsChanged		+=OnNumVertsChanged;

			ProgressWatcher.eProgressUpdated	+=OnProgress;

			SetUpCommonIgnores();
		}


		internal void FreeAll()
		{
			if(mMap != null)
			{
				mMap.FreeGBSPFile();
			}

			if(mVisMap != null)
			{
				mVisMap.FreeFileVisData();
			}

			if(mZoneDraw != null)
			{
				mZoneDraw.FreeAll();
			}

			mMatLib.FreeAll();
			mDebugDraw.FreeAll();

			mSKeeper.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			mSKeeper.eCompileDone	-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
			mSKeeper.FreeAll();
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

			mMatLib.UpdateWVP(Matrix.Identity,
				gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);
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

			mZoneDraw.Draw(gd, 0, mVisMap.IsMaterialVisibleFromPos,
				GetModelMatrix, RenderExternal, RenderShadows, SetUpAlphaRenderTargets);
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


		void SetUpAlphaRenderTargets()
		{
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


		void SetFormSize(Form form, string sizeName)
		{
			form.DataBindings.Add(new Binding("Size",
				Settings.Default, sizeName, true,
				DataSourceUpdateMode.OnPropertyChanged));
		}


		void LoadStuff()
		{
			mSKeeper.Init(mGD, mGameRootDir);

			mDebugDraw	=new DebugDraw(mGD, mSKeeper);
			mMatLib		=new MatLib(mGD, mSKeeper);

			mMatLib.InitCelShading(1);
			mMatLib.GenerateCelTexturePreset(mGD.GD,
				mGD.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);

			mZoneDraw	=new IndoorMesh(mGD, mMatLib);
		}


		void OnProgress(object sender, EventArgs ea)
		{
			ProgressEventArgs pea	=ea as ProgressEventArgs;

			if(pea != null)
			{
				mOutForm.UpdateProgress(pea.mMin, pea.mMax, pea.mCurrent);
			}
		}


		void OnPrint(object sender, EventArgs ea)
		{
			string	sz	=sender as string;
			if(sz != null)
			{
				mOutForm.Print(sz);
			}
		}


		void OnDeviceLost(object sender, EventArgs ea)
		{
			mOutForm.Print("Graphics device lost, rebuilding stuffs...\n");

			mSKeeper.FreeAll();
			mDebugDraw.FreeAll();
			mMatLib.FreeAll();
			mZoneDraw.FreeAll();

			LoadStuff();
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

					bool	bPerPlaneAlpha	=false;

					mZoneDraw.BuildLM(mGD, mSKeeper, mZoneForm.GetLightAtlasSize(), mMap.BuildLMRenderData, mMap.GetPlanes(), bPerPlaneAlpha);
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

			BSPBuildParams	bbp	=mBSPForm.BSPParameters;
			bbp.mMapName		=Path.GetFileName(fileName);

			bbp.mMapType	=MapType.Quake1;

			mMap.LoadBrushFile(fileName, mBSPForm.BSPParameters);

			mBSPForm.SetBuildEnabled(true);
			mBSPForm.SetSaveEnabled(false);

			if(!mbFullBuilding)
			{
				BuildDebugDraw(Map.DebugDrawChoice.MapBrushes);
			}
		}

		void OnOpenStatic(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			IArch	sa	=new StaticArch();

			sa.ReadFromFile(fileName, mGD.GD, true);

			mMap	=new Map();

			int	count	=sa.GetPartCount();
			for(int i=0;i < count;i++)
			{
				Bounds	bnd	=new Bounds();

				List<Vector3>	pos;
				List<int>		inds;
				sa.GetPartPositions(i, out pos, out inds);

				for(int j=0;j < inds.Count;j+=3)
				{
					bnd.AddPointToBounds(pos[inds[j]]);
					bnd.AddPointToBounds(pos[inds[j + 1]]);
					bnd.AddPointToBounds(pos[inds[j + 2]]);
				}

				mMap.PrepareTriTree(bnd);

				mOutForm.Print("Convexizing part " + i + ": " + sa.GetPartName(i) + "\n");

				List<Vector3>	tri	=new List<Vector3>();
				for(int j=0;j < inds.Count;j+=3)
				{
					tri.Add(pos[inds[j]]);
					tri.Add(pos[inds[j + 1]]);
					tri.Add(pos[inds[j + 2]]);

					mMap.AddBSPTriangle(tri);

					tri.Clear();
				}

				mMap.AddBSPVolume(bnd);

				mMap.AccumulateVolumes();
			}

			if(count <= 0)
			{
				mOutForm.Print(fileName + " didn't have anything usable.\n");
				mMap	=null;
				return;
			}

			BuildDebugDraw(Map.DebugDrawChoice.TriTree);

			int	numDumped	=mMap.DumpBSPVolumesToQuarkMap(FileUtil.StripExtension(fileName));

			mOutForm.Print(fileName + " opened and " + numDumped + " brushes extracted.\n");

			mMap	=null;
		}

		void OnMapToStatic(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null)
			{
				return;
			}

			mMap	=new Map();

			mMap.LoadBrushFile(fileName, mBSPForm.BSPParameters);

			List<Vector3>	verts	=new List<Vector3>();
			List<UInt16>	inds	=new List<UInt16>();
			List<Vector3>	norms	=new List<Vector3>();
			List<Color>		cols	=new List<Color>();

			mMap.GetTriangles(verts, norms, cols, inds, Map.DebugDrawChoice.MapBrushes);

			if(verts.Count <= 0)
			{
				mOutForm.Print(fileName + " didn't have anything usable.\n");
				mMap	=null;
				return;
			}

			//convert normals to half4
			List<Half4>	h4norms	=new List<Half4>();
			foreach(Vector3 norm in norms)
			{
				Half4	h4norm;

				h4norm.X	=norm.X;
				h4norm.Y	=norm.Y;
				h4norm.Z	=norm.Z;
				h4norm.W	=1f;

				h4norms.Add(h4norm);
			}

			//convert to funky max coordinate system
			List<Vector3>	maxVerts	=new List<Vector3>();
			foreach(Vector3 vert in verts)
			{
				Vector3	maxVert;

				maxVert.X	=vert.X;
				maxVert.Y	=vert.Y;
				maxVert.Z	=vert.Z;

				maxVerts.Add(maxVert);
			}

			EditorMesh	em	=new EditorMesh("MapMesh");

			Type	vtype	=VertexTypes.GetMatch(true, true, false, false, false, false, 0, 1);

			Array	varray	=Array.CreateInstance(vtype, maxVerts.Count);

			for(int i=0;i < maxVerts.Count;i++)
			{
				VertexTypes.SetArrayField(varray, i, "Position", maxVerts[i]);
				VertexTypes.SetArrayField(varray, i, "Normal", h4norms[i]);
				VertexTypes.SetArrayField(varray, i, "Color0", cols[i]);
			}

			SharpDX.Direct3D11.Buffer	vb	=VertexTypes.BuildABuffer(mGD.GD, varray, vtype);

			int	vertSize	=VertexTypes.GetSizeForType(vtype);

			em.SetVertSize(vertSize);
			em.SetNumVerts(maxVerts.Count);
			em.SetNumTriangles(inds.Count / 3);
			em.SetTypeIndex(VertexTypes.GetIndex(vtype));
			em.SetVertexBuffer(vb);

			UInt16	[]iarray	=inds.ToArray();

			SharpDX.Direct3D11.Buffer	ib	=VertexTypes.BuildAnIndexBuffer(mGD.GD, iarray);

			em.SetIndexBuffer(ib);

			em.SetData(varray, iarray);			

			mOutForm.Print(fileName + " opened and " + maxVerts.Count + " verts converted.\n");

			IArch	saveArch	=new StaticArch();

			em.SetTransform(Matrix.RotationX((float)(Math.PI / 4)));

			saveArch.AddPart(em);

			saveArch.SaveToFile(FileUtil.StripExtension(fileName) + ".Static");

			mMap	=null;
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

		void OnMatLibNotReadyToSave(object sender, EventArgs ea)
		{
			mOutForm.Print("Material lib has some sort of problem that is preventing saving!\n");
		}


		void OnMatTechChanged(object sender, EventArgs ea)
		{
			string	matName	=sender as string;
			if(matName == null || matName == "")
			{
				return;
			}

			mOutForm.Print("Material " + matName + " changed techniques...\n");

			//reset hides
			HideParameters(matName);
		}


		void SetUpCommonIgnores()
		{
			//some stuff isn't used by most bsp stuff
			mCommonIgnores.Add("mSpecColor");
			mCommonIgnores.Add("mSpecPower");
			mCommonIgnores.Add("mSolidColour");

			//renderstates are never used as a variable
			mCommonIgnores.Add("AlphaBlending");
			mCommonIgnores.Add("NoBlending");
			mCommonIgnores.Add("ShadowBlending");
			mCommonIgnores.Add("EnableDepth");
			mCommonIgnores.Add("DisableDepth");
			mCommonIgnores.Add("DisableDepthWrite");
			mCommonIgnores.Add("DisableDepthTest");
			mCommonIgnores.Add("NoCull");
			mCommonIgnores.Add("LinearClamp");
			mCommonIgnores.Add("LinearWrap");
			mCommonIgnores.Add("PointClamp");
			mCommonIgnores.Add("PointWrap");
			mCommonIgnores.Add("LinearClampCube");
			mCommonIgnores.Add("LinearWrapCube");
			mCommonIgnores.Add("PointClampCube");
			mCommonIgnores.Add("PointWrapCube");
			mCommonIgnores.Add("PointClamp1D");

			//common hidey stuff
			mCommonHides.Add("mWorld");
			mCommonHides.Add("mView");
			mCommonHides.Add("mProjection");
			mCommonHides.Add("mLightViewProj");
			mCommonHides.Add("mEyePos");
			mCommonHides.Add("mCelTable");
			mCommonHides.Add("mShadowTexture");
			mCommonHides.Add("mShadowCube");
			mCommonHides.Add("mShadowLightPos");
			mCommonHides.Add("mbDirectional");
			mCommonHides.Add("mAniIntensities");
			mCommonHides.Add("mDynLights");
			mCommonHides.Add("mShadowAtten");
			mCommonHides.Add("mSpecColor");	//eventually want these
			mCommonHides.Add("mSpecPower");	//for future
			mCommonHides.Add("mMaterialID");	//idkeeper takes care of this
		}


		void HideParameters(string matName)
		{
			//look for material specific stuff to hide / ignore
			List<string>	matIgnores	=new List<string>();
			List<string>	matHides	=new List<string>();

			string	tech	=mMatLib.GetMaterialTechnique(matName);

			if(tech == null)
			{
				return;	//no shaders loaded?
			}

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
			matHides.AddRange(mCommonHides);
			matIgnores.AddRange(mCommonIgnores);

			mMatLib.IgnoreMaterialVariables(matName, matIgnores, true);
			mMatLib.HideMaterialVariables(matName, matHides, true);
		}


		void HideParametersByMaterial()
		{
			List<string>	mats	=mMatLib.GetMaterialNames();

			//hide stuff that the user doesn't care about
			//these are for all indoormesh materials
			foreach(string m in mats)
			{
				HideParameters(m);
			}
		}
	}
}
