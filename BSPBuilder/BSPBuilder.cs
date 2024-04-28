using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using BSPCore;
using BSPVis;
using MeshLib;
using UtilityLib;
using MaterialLib;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using Vortice.Mathematics.PackedVector;

using MatLib = MaterialLib.MaterialLib;


namespace BSPBuilder;

internal class BSPBuilder
{
	//data
	Map							mMap;
	VisMap						mVisMap;
	IndoorMesh					mZoneDraw;
	MatLib						mMatLib;
	StuffKeeper					mSKeeper;
	bool						mbWorking, mbFullBuilding;
	string						mFullBuildFileName;
	List<string>				mAllTextures	=new List<string>();
	Dictionary<int, Matrix4x4>	mModelMats;
	string						mGameRootDir;

	//gpu
	GraphicsDevice	mGD;

	//debug draw stuff
	SharedForms.DebugDraw	mDebugDraw;

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
//		mBSPForm.eBuild					+=OnBuild;
		mBSPForm.eLight					+=OnLight;
//		mBSPForm.eOpenMap				+=OnOpenMap;
//		mBSPForm.eStaticToMap			+=OnOpenStatic;
//		mBSPForm.eMapToStatic			+=OnMapToStatic;
//		mBSPForm.eSave					+=OnSaveGBSP;
//		mBSPForm.eFullBuild				+=OnFullBuild;
//		mBSPForm.eUpdateEntities		+=OnUpdateEntities;
		mVisForm.eResumeVis				+=OnResumeVis;
		mVisForm.eStopVis				+=OnStopVis;
		mVisForm.eVis					+=OnVis;

		//core events
//		CoreEvents.eBuildDone		+=OnBuildDone;
		CoreEvents.eLightDone		+=OnLightDone;
//		CoreEvents.eGBSPSaveDone	+=OnGBSPSaveDone;
		CoreEvents.eVisDone			+=OnVisDone;
		CoreEvents.ePrint			+=OnPrint;

		//stats
//		CoreEvents.eNumPortalsChanged	+=OnNumPortalsChanged;
//		CoreEvents.eNumClustersChanged	+=OnNumClustersChanged;
//		CoreEvents.eNumPlanesChanged	+=OnNumPlanesChanged;
//		CoreEvents.eNumVertsChanged		+=OnNumVertsChanged;

		ProgressWatcher.eProgressUpdated	+=OnProgress;
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


	internal void Update(float msDelta, GraphicsDevice gd, Vector3 pos)
	{
		if(mbWorking)
		{
			Thread.Sleep(0);
			return;
		}

		CBKeeper	cbk	=mSKeeper.GetCBKeeper();

		cbk.SetView(gd.GCam.View, pos);
		cbk.SetProjection(gd.GCam.Projection);

		cbk.UpdateFrame(gd.DC);

		mZoneDraw.Update(msDelta);

//			mMatLib.UpdateWVP(Matrix.Identity,
//				gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);
	}


	internal void Render(GraphicsDevice gd)
	{
		if(mbWorking)
		{
			Thread.Sleep(0);
			return;
		}

		if(mZoneDraw == null)// || mVisMap == null)
		{
			mDebugDraw.Draw(mGD);
			return;
		}

//		mZoneDraw.Draw(gd, mVisMap.IsMaterialVisibleFromPos, GetModelMatrix);
		mZoneDraw.Draw(gd, null, null);
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


	void RenderExternal(GameCamera gcam)
	{
	}


	void RenderExternalDMN(GameCamera gcam)
	{
	}


	bool RenderShadows(int shadIndex)
	{
		return	false;
	}


	Matrix4x4 GetModelMatrix(int modelIndex)
	{
		if(mModelMats == null)
		{
			return	Matrix4x4.Identity;
		}

		if(!mModelMats.ContainsKey(modelIndex))
		{
			return	Matrix4x4.Identity;
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

		mDebugDraw	=new SharedForms.DebugDraw(mGD, mSKeeper);
		mMatLib		=new MatLib(mSKeeper);

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
//		mBSPForm.EnableFileIO(false);
//		mVisForm.EnableFileIO(false);

		mVisMap	=new VisMap();
		mVisMap.MaterialVisGBSPFile(fileName, mGD);

		mZoneForm.EnableFileIO(true);
//		mBSPForm.EnableFileIO(true);
//		mVisForm.EnableFileIO(true);

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
//			mBSPForm.EnableFileIO(false);
//			mVisForm.EnableFileIO(false);

			QBSPFile	qfile	=new QBSPFile(fileName);

			List<Vector3>	verts	=new List<Vector3>();
			List<Vector3>	norms	=new List<Vector3>();
			List<Color>		cols	=new List<Color>();
			List<UInt16>	inds	=new List<ushort>();

			qfile.GetDrawData(verts, norms, cols, inds);

//			mDebugDraw.MakeDrawStuff(mGD.GD, verts, norms, cols, inds);

			mMatLib.NukeAllMaterials();

			MapGrinder	mg	=new MapGrinder(mGD, mSKeeper, mMatLib,
				qfile.mTexInfos, qfile.mVerts, qfile.mEdges, qfile.mSurfEdges,
				qfile.mFaces, qfile.mPlanes, qfile.mModels, qfile.mLightData,
				mZoneForm.GetLightAtlasSize());

			mg.BuildLMData();

/*
			mMap.MakeLMData(mg);
			mMap.MakeLMAData(mg);
			mMap.MakeVLitData(mg);
			mMap.MakeLMAnimData(mg);
			mMap.MakeLMAAnimData(mg);
			mMap.MakeAlphaData(mg);
			mMap.MakeFullBrightData(mg);
			mMap.MakeSkyData(mg);
*/
			int		typeIndex;
			Array	vertArray;
			UInt16	[]indArray;

			//feed data into meshlib
			mg.GetLMGeometry(out typeIndex, out vertArray, out indArray);
			mZoneDraw.SetLMData(mGD.GD, typeIndex, vertArray, indArray, mg.GetLMDrawCalls());
/*
			mg.GetLMAGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetLMAData(mGD.GD, typeIndex, verts, inds, mg.GetLMADrawCalls());

			mg.GetVLitGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetVLitData(mGD.GD, typeIndex, verts, inds, mg.GetVLitDrawCalls());

			mg.GetLMAnimGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetLMAnimData(mGD.GD, typeIndex, verts, inds, mg.GetLMAnimDrawCalls());

			mg.GetLMAAnimGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetLMAAnimData(mGD.GD, typeIndex, verts, inds, mg.GetLMAAnimDrawCalls());

			mg.GetAlphaGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetAlphaData(mGD.GD, typeIndex, verts, inds, mg.GetVLitAlphaDrawCalls());

			mg.GetFullBrightGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetFullBrightData(mGD.GD, typeIndex, verts, inds, mg.GetFullBrightDrawCalls());

			mg.GetSkyGeometry(out typeIndex, out verts, out inds);
			mZoneDraw.SetSkyData(mGD.GD, typeIndex, verts, inds, mg.GetSkyDrawCalls());
*/
			mZoneDraw.SetLMAtlas(mg.GetLMAtlas());


//				mZoneDraw.BuildLM(mGD, mSKeeper, mZoneForm.GetLightAtlasSize(), mMap.BuildLMRenderData, mMap.GetPlanes(), bPerPlaneAlpha);
//				mZoneDraw.BuildVLit(mGD, mSKeeper, mMap.BuildVLitRenderData, mMap.GetPlanes());
//				mZoneDraw.BuildAlpha(mGD, mSKeeper, mMap.BuildAlphaRenderData, mMap.GetPlanes());
//				mZoneDraw.BuildFullBright(mGD, mSKeeper, mMap.BuildFullBrightRenderData, mMap.GetPlanes());
//				mZoneDraw.BuildMirror(mGD, mSKeeper, mMap.BuildMirrorRenderData, mMap.GetPlanes());
//				mZoneDraw.BuildSky(mGD, mSKeeper, mMap.BuildSkyRenderData, mMap.GetPlanes());

			mZoneDraw.FinishAtlas(mGD, mSKeeper);

//			mModelMats	=mMap.GetModelTransforms();

			mMatForm.SetMesh(mZoneDraw);

			mMatForm.RefreshMaterials();

//			mVisMap.SetMaterialVisBytes(mMatLib.GetMaterialNames().Count);

//					mMatLib.SetLightMapsToAtlas();

			mZoneForm.EnableFileIO(true);
//			mBSPForm.EnableFileIO(true);
//			mVisForm.EnableFileIO(true);
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

/*	void OnBuild(object sender, EventArgs ea)
	{
		mbWorking	=true;
		mZoneForm.EnableFileIO(false);
		mBSPForm.EnableFileIO(false);
		mVisForm.EnableFileIO(false);
		mMap.BuildTree(mBSPForm.BSPParameters);
	}
*/
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
/*
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

		string	path	=FileUtil.StripFileName(fileName);

		Dictionary<string, Mesh>	meshes	=new Dictionary<string, Mesh>();

		//load all .mesh 
		Mesh.LoadAllMeshes(path, mGD.GD, meshes);

		StaticMesh	sm	=new StaticMesh(fileName, meshes);

		mMap	=new Map();

		int	count	=sm.GetPartCount();
		for(int i=0;i < count;i++)
		{
			Bounds	bnd	=new Bounds();

			List<Vector3>	pos;
			List<int>		inds;
			sm.GetPartPositions(i, out pos, out inds);

			for(int j=0;j < inds.Count;j+=3)
			{
				bnd.AddPointToBounds(pos[inds[j]]);
				bnd.AddPointToBounds(pos[inds[j + 1]]);
				bnd.AddPointToBounds(pos[inds[j + 2]]);
			}

			mMap.PrepareTriTree(bnd);

			mOutForm.Print("Convexizing part " + i + ": " + sm.GetPartName(i) + "\n");

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

		//free .meshes
		foreach(KeyValuePair<string, Mesh> m in meshes)
		{
			m.Value.FreeAll();
		}
		meshes.Clear();

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
			Half4	h4norm	=new Half4(norm.X, norm.Y, norm.Z, 1f);

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

		Mesh	bspMesh	=new Mesh("blort");

		Type	vtype	=VertexTypes.GetMatch(true, true, false, false, false, false, 0, 1);

		Array	varray	=Array.CreateInstance(vtype, maxVerts.Count);

		for(int i=0;i < maxVerts.Count;i++)
		{
			VertexTypes.SetArrayField(varray, i, "Position", maxVerts[i]);
			VertexTypes.SetArrayField(varray, i, "Normal", h4norms[i]);
			VertexTypes.SetArrayField(varray, i, "Color0", cols[i]);
		}

		int	vertSize	=VertexTypes.GetSizeForType(vtype);

		UInt16	[]iarray	=inds.ToArray();

		ID3D11Buffer	vb	=VertexTypes.BuildABuffer(mGD.GD, varray, vtype);
		ID3D11Buffer	ib	=VertexTypes.BuildAnIndexBuffer(mGD.GD, iarray);

		bspMesh.SetEditorData(varray, iarray);
		bspMesh.SetIndexBuffer(ib);
		bspMesh.SetNumTriangles(inds.Count / 3);
		bspMesh.SetNumVerts(maxVerts.Count);
		bspMesh.SetTypeIndex(VertexTypes.GetIndex(vtype));
		bspMesh.SetVertexBuffer(vb);
		bspMesh.SetVertSize(vertSize);

		mOutForm.Print(fileName + " opened and " + maxVerts.Count + " verts converted.\n");

		StaticMesh	saveMesh	=new StaticMesh();

		saveMesh.SetTransform(Matrix4x4.CreateRotationX((float)(Math.PI / 4)));

		saveMesh.AddPart(bspMesh, Matrix4x4.Identity, "Flort");

		saveMesh.SaveToFile(FileUtil.StripExtension(fileName) + ".Static");

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
*/
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
/*
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
*/
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
/*
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
*/
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
/*
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
	}*/
}