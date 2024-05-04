using System.Numerics;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using MeshLib;
using MaterialLib;
using UtilityLib;
using SharedForms;

using MatLib	=MaterialLib.MaterialLib;


namespace ColladaConvert;

public partial class AnimForm : Form
{
	//file dialog
	OpenFileDialog	mOFD	=new OpenFileDialog();
	SaveFileDialog	mSFD	=new SaveFileDialog();

	//graphics device
	ID3D11Device	mGD;

	//matlib
	MatLib	mMatLib;

	//stuff
	StuffKeeper	mSKeeper;

	//anim lib
	AnimLib	mAnimLib;

	//selected anim info
	string	?mSelectedAnim;
	float	mAnimStartTime, mAnimEndTime;
	float	mCurAnimTime;
	bool	mbPaused;

	StaticMesh		?mStatMesh;
	Character		?mChar;

	public event EventHandler	?eMeshChanged;
	public event EventHandler	?eSkeletonChanged;
	public event EventHandler	?eBoundReCompute;
	public event EventHandler	?eBoundAdjust;
	public event EventHandler	?eScaleFactorDecided;
	public event EventHandler	?ePrint;


	public AnimForm(ID3D11Device gd, MatLib mats, AnimLib alib, StuffKeeper sk)
	{
		InitializeComponent();

		mGD			=gd;
		mMatLib		=mats;
		mSKeeper	=sk;
		mAnimLib	=alib;

		AnimList.Columns.Add("Name");
		AnimList.Columns.Add("Total Time");
		AnimList.Columns.Add("Start Time");
		AnimList.Columns.Add("Looping");
		AnimList.Columns.Add("Ping Pong");
		AnimList.Columns.Add("Num Keys");
	}


	internal bool GetDrawAxis()
	{
		return	DrawAxis.Checked;
	}

	internal bool GetDrawBound()
	{
		return	ShowBound.Checked;
	}

	internal bool GetBoundChoice()
	{
		if(ChoiceSphere.Checked)
		{
			return	false;
		}
		return	true;
	}


	internal void DoneBoneAdjust()
	{
		//re-enable bound group
		BoundGroup.Enabled	=true;

		//re-enable radios
		ChoiceBox.Enabled		=true;
		ChoiceSphere.Enabled	=true;
	}


	internal void GetBoneNamesInUseByDraw(List<string> ?names)
	{
		mChar?.GetBoneNamesInUseByDraw(names);
	}


	internal void BonesChanged()
	{
		mChar?.ReBuildBones(mGD);
	}


	public void AdjustBone(string boneName)
	{
		//stub till I get things going
	}


	internal COLLADA? DeSerializeCOLLADA(string path)
	{
		FileStream		fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
		XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

		COLLADA	?ret	=xs.Deserialize(fs) as COLLADA;

		fs.Close();

		return	ret;
	}


	internal void SerializeCOLLADA(COLLADA saveyThing, string path)
	{
		FileStream		fs	=new FileStream(path, FileMode.Create, FileAccess.Write);
		XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

		xs.Serialize(fs, saveyThing);

		fs.Close();
	}


	//crude dae export
	internal void ConvertMesh(object mesh, out COLLADA col)
	{
		col	=new COLLADA();

		if(mesh == null)
		{
			return;
		}

		col.asset	=new asset();

		col.asset.created		=DateTime.Now;
		col.asset.unit			=new assetUnit();
		col.asset.unit.meter	=0.1f;
		col.asset.unit.name		="meter";
		col.asset.up_axis		=UpAxisType.Z_UP;

		col.Items	=new object[2];

		library_geometries	geom	=new library_geometries();

		StaticMesh	?sm		=mesh as StaticMesh;
		Character	?chr	=mesh as Character;

		int	partCount;
		if(sm != null)
		{
			partCount	=sm.GetPartCount();
		}
		else if(chr != null)
		{
			partCount	=chr.GetPartCount();
		}
		else
		{
			PrintToOutput("Invalid part in ConvertMesh()\n");
			return;
		}

		geom.geometry	=new geometry[partCount];

		for(int i=0;i < partCount;i++)
		{
			geometry	g	=new geometry();
			Type		vType;

			if(sm != null)
			{
				g.name	=sm.GetPartName(i);
				vType	=sm.GetPartVertexType(i);
			}
			else if(chr != null)
			{
				g.name	=chr.GetPartName(i);
				vType	=chr.GetPartVertexType(i);
			}
			else
			{
				continue;
			}

			g.id	=g.name + "-mesh";

			polylist	plist	=new polylist();

			plist.input	=MakeInputs(g.id, vType);

			plist.material	=g.id + "_mat";	//hax

			string	polys, vcounts;

			if(sm != null)
			{
				plist.count	=(ulong)sm.GetPartColladaPolys(i, out polys, out vcounts);
			}
			else if(chr != null)
			{
				plist.count	=(ulong)chr.GetPartColladaPolys(i, out polys, out vcounts);
			}
			else
			{
				continue;
			}

			plist.p			=polys;
			plist.vcount	=vcounts;

			object	[]itemses	=new object[1];

			itemses[0]	=plist;

			mesh	m	=new mesh();

			m.Items	=itemses;

			m.source	=MakeSources(g.id, mesh, i);

			m.vertices	=new vertices();

			m.vertices.id	=g.id + "-vertices";

			m.vertices.input	=new InputLocal[1];

			m.vertices.input[0]	=new InputLocal();

			m.vertices.input[0].semantic	="POSITION";
			m.vertices.input[0].source		="#" + g.id + "-positions";

			g.Item	=m;

			geom.geometry[i]	=g;
		}

		col.Items[0]	=geom;

		library_visual_scenes	lvs	=new library_visual_scenes();

		lvs.visual_scene	=new visual_scene[1];

		lvs.visual_scene[0]	=new visual_scene();

		lvs.visual_scene[0].id		="RootNode";
		lvs.visual_scene[0].name	="RootNode";

		node	[]nodes	=new node[partCount];
		for(int i=0;i < partCount;i++)
		{
			nodes[i]	=new node();

			Matrix4x4	mat			=Matrix4x4.Identity;
			string		partName	="default";

			if(sm != null)
			{
				nodes[i].id		=sm.GetPartName(i);
				nodes[i].name	=sm.GetPartName(i);
				mat				=sm.GetPartTransform(i);
				partName		=sm.GetPartName(i);
			}
			else if(chr != null)
			{
				nodes[i].id		=chr.GetPartName(i);
				nodes[i].name	=chr.GetPartName(i);
				mat				=chr.GetPartTransform(i);
				partName		=chr.GetPartName(i);
			}

			TargetableFloat3	trans	=new TargetableFloat3();

			trans.sid		="translate";
			trans.Values	=new float[3];

			trans.Values[0]	=mat.Translation.X;
			trans.Values[1]	=mat.Translation.Y;
			trans.Values[2]	=mat.Translation.Z;

			rotate	rot	=new rotate();

			rot.sid		="rotateX";
			rot.Values	=new float[4];

			rot.Values[0]	=1f;
			rot.Values[1]	=0f;
			rot.Values[2]	=0f;
			rot.Values[3]	=-90f;

			nodes[i].Items	=new object[2];

			nodes[i].ItemsElementName	=new ItemsChoiceType2[2];

			nodes[i].ItemsElementName[0]	=ItemsChoiceType2.translate;
			nodes[i].ItemsElementName[1]	=ItemsChoiceType2.rotate;

			nodes[i].Items[0]	=trans;
			nodes[i].Items[1]	=rot;

			nodes[i].instance_geometry	=new instance_geometry[1];

			nodes[i].instance_geometry[0]	=new instance_geometry();

			nodes[i].instance_geometry[0].url	="#" + partName + "-mesh";
		}

		lvs.visual_scene[0].node	=nodes;

		col.Items[1]	=lvs;

		COLLADAScene	scene	=new COLLADAScene();

		scene.instance_visual_scene	=new InstanceWithExtra();

		scene.instance_visual_scene.url	="#RootNode";

		col.scene	=scene;
	}


	//loads an animation into an existing anim lib
	internal bool LoadAnimDAE(string path, AnimLib alib, bool bCheckSkeleton)
	{
		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			PrintToOutput("Null file in LoadAnim()\n");
			return	false;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Y or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			PrintToOutput("Warning!  Y up axis not supported.  Strange things may happen!\n");
		}

		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(GetScaleFactor());

		Misc.SafeInvoke(eScaleFactorDecided, scaleFactor);

		Skeleton	skel	=BuildSkeleton(colladaFile, ePrint);

		//grab visual scenes
		IEnumerable<library_visual_scenes>	lvss	=
			colladaFile.Items.OfType<library_visual_scenes>();

		library_visual_scenes	lvs	=lvss.First();

		//see if animlib has a skeleton yet
		if(alib.GetSkeleton() == null)
		{
			alib.SetSkeleton(skel);
			Misc.SafeInvoke(eSkeletonChanged, skel);
		}
		else if(bCheckSkeleton)
		{
			//make sure they match
			if(!alib.CheckSkeleton(skel))
			{
				PrintToOutput("Warning!  Skeleton check failed, anim load aborted!\n");
				return	false;
			}
		}

		Anim	anm	=BuildAnim(colladaFile, alib.GetSkeleton(), lvs, path, ePrint);

		alib.AddAnim(anm);

		//need to do this again in case keyframes were added
		//for the root bone.
		anm.SetBoneRefs(alib.GetSkeleton());

		return	true;
	}


	internal MeshConverter.ScaleFactor	GetScaleFactor()
	{
		if(UnitsCentimeters.Checked)
		{
			return	MeshConverter.ScaleFactor.Centimeters;
		}
		else if(UnitsGrog.Checked)
		{
			return	MeshConverter.ScaleFactor.Grog;
		}
		else if(UnitsMeters.Checked)
		{
			return	MeshConverter.ScaleFactor.Meters;
		}
		else if(UnitsQuake.Checked)
		{
			return	MeshConverter.ScaleFactor.Quake;
		}
		else if(UnitsValve.Checked)
		{
			return	MeshConverter.ScaleFactor.Valve;
		}

		PrintToOutput("Warning!  Desired Units could not be determined, using meters...\n");

		return	MeshConverter.ScaleFactor.Meters;
	}


	internal void LoadCharacterDAE(string path,	AnimLib alib)
	{
		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			PrintToOutput("Null file in LoadCharacterDAE()\n");
			return;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Y or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			PrintToOutput("Warning!  Y up axis not supported.  Strange things may happen!\n");
		}

		//unit conversion
		float	scaleFactor		=1f;
		if(colladaFile.asset.unit != null)
		{
			scaleFactor	=colladaFile.asset.unit.meter;
		}

		scaleFactor	*=MeshConverter.GetScaleFactor(GetScaleFactor());

		Misc.SafeInvoke(eScaleFactorDecided, scaleFactor);

		Matrix4x4	scaleMat	=Matrix4x4.CreateScale(Vector3.One * (1f / scaleFactor));
		
		//grab visual scenes
		IEnumerable<library_visual_scenes>	lvss	=
			colladaFile.Items.OfType<library_visual_scenes>();

		library_visual_scenes	lvs	=lvss.First();

		List<MeshConverter>	allChunks	=GetMeshChunks(colladaFile, true, GetScaleFactor());
		List<MeshConverter>	chunks		=new List<MeshConverter>();

		//skip dummies
		foreach(MeshConverter mc in allChunks)
		{
			if(!mc.GetName().Contains("DummyGeometry"))
			{
				chunks.Add(mc);
				mc.ePrint	+=OnPrintString;
			}
		}

		allChunks.Clear();

		if(!AddVertexWeightsToChunks(colladaFile, chunks))
		{
			PrintToOutput("No vertex weights... are you trying to load static geometry as a character?\n");
			return;
		}

		//build or get skeleton
		Skeleton	skel	=BuildSkeleton(colladaFile, ePrint);
		if(skel ==  null)
		{
			PrintToOutput("No skeleton... are you trying to load static geometry as a character?\n");
			return;
		}

		//see if animlib has a skeleton yet
		if(alib.GetSkeleton() == null)
		{
			alib.SetSkeleton(skel);
			Misc.SafeInvoke(eSkeletonChanged, skel);
		}
		else
		{
			//make sure they match
			if(!alib.CheckSkeleton(skel))
			{
				PrintToOutput("Warning!  Skeleton check failed!  Might need to restart to clear the animation library skeleton.\n");
				return;
			}

			//use old one
			skel	=alib.GetSkeleton();
		}

		Anim	anm	=BuildAnim(colladaFile, alib.GetSkeleton(), lvs, path, ePrint);

		alib.AddAnim(anm);

		//need to do this again in case keyframes were added
		//for the root bone.
		anm.SetBoneRefs(skel);

		FixBoneIndexes(colladaFile, chunks, skel);

		BuildFinalVerts(mGD, colladaFile, chunks, false);

		List<Mesh>		converted	=new List<Mesh>();

		foreach(MeshConverter mc in chunks)
		{
			Mesh	?conv	=mc.GetConvertedMesh();
			if(conv == null)
			{
				continue;
			}

			Matrix4x4	mat		=GetSceneNodeTransform(colladaFile, mc);

			//this might not be totally necessary
			//but it is nice to have
			if(!mat.IsIdentity)
			{
				PrintToOutput("Warning!  Mesh chunk " + conv.Name + "'s scene node is not identity!  This can make it tricksy to orient and move them in a game.\n");
			}

			conv.Name	=mc.GetGeomName();

			converted.Add(conv);

			if(!conv.Name.EndsWith("Mesh"))
			{
				conv.Name	+="Mesh";
			}
			mc.ePrint	-=OnPrintString;
		}

		if(mChar != null)
		{
			Skin	?sk	=mChar.GetSkin();
			CreateSkin(colladaFile, ref sk, chunks, skel, scaleFactor, ePrint);

			foreach(Mesh part in converted)
			{
				mChar.AddPart(part, "default");
			}
		}
		else
		{
			Skin	?sk	=null;
			CreateSkin(colladaFile, ref sk, chunks, skel, scaleFactor, ePrint);

			mChar	=new Character(converted, sk, alib);
		}

		SetSkinRootTransformBlender();
	}


	internal void LoadStatic(string path, out StaticMesh ?sm)
	{
		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			PrintToOutput("Null file in LoadStatic()\n");
			sm	=null;
			return;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Z or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			PrintToOutput("Warning!  Y up axis not supported.  Strange things may happen!\n");
		}
		
		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(GetScaleFactor());

		Misc.SafeInvoke(eScaleFactorDecided, scaleFactor);

		sm	=new StaticMesh();

		List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, false, GetScaleFactor());

		BuildFinalVerts(mGD, colladaFile, chunks, true);
		foreach(MeshConverter mc in chunks)
		{
			Mesh	?m	=mc.GetConvertedMesh();
			if(m == null)
			{
				continue;
			}
			Matrix4x4	mat	=GetSceneNodeTransform(colladaFile, mc);

			m.Name	=mc.GetGeomName();

			sm.AddPart(m, mat, null);
		}

		//this needs to be identity so the game
		//can mess with it without needing the axis info
		sm.SetTransform(Matrix4x4.Identity);
	}


	void BuildFinalVerts(ID3D11Device gd, COLLADA colladaFile, List<MeshConverter> chunks, bool bStatic)
	{
		IEnumerable<library_geometries>		geoms	=colladaFile.Items.OfType<library_geometries>();
		IEnumerable<library_controllers>	conts	=colladaFile.Items.OfType<library_controllers>();

		PrintToOutput("geoms.Count() is: " + geoms.Count() + " in BuildFinalVerts()\n");

		foreach(object geomItem in geoms.First().geometry)
		{
			geometry	?geom	=geomItem as geometry;
			if(geom == null)
			{
				PrintToOutput("Null geometry in BuildFinalVerts()!\n");
				continue;
			}

			//blast any chunks with no verts (happens with max collada)
			List<MeshConverter>	toNuke	=new List<MeshConverter>();

			foreach(MeshConverter cnk in chunks)
			{
				string	name	=cnk.GetName();
				if(cnk.mGeometryID == geom.id)
				{
					int	normStride, tex0Stride, tex1Stride, tex2Stride, tex3Stride;
					int	col0Stride, col1Stride, col2Stride, col3Stride;

					List<int>	?posIdxs	=GetGeometryIndexesBySemantic(geom, "VERTEX", 0, name);
					float_array	?norms		=GetGeometryFloatArrayBySemantic(geom, "NORMAL", 0, name, out normStride);
					List<int>	?normIdxs	=GetGeometryIndexesBySemantic(geom, "NORMAL", 0, name);
					float_array	?texCoords0	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 0, name, out tex0Stride);
					float_array	?texCoords1	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 1, name, out tex1Stride);
					float_array	?texCoords2	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 2, name, out tex2Stride);
					float_array	?texCoords3	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 3, name, out tex3Stride);
					List<int>	?texIdxs0	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 0, name);
					List<int>	?texIdxs1	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 1, name);
					List<int>	?texIdxs2	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 2, name);
					List<int>	?texIdxs3	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 3, name);
					float_array	?colors0	=GetGeometryFloatArrayBySemantic(geom, "COLOR", 0, name, out col0Stride);
					float_array	?colors1	=GetGeometryFloatArrayBySemantic(geom, "COLOR", 1, name, out col1Stride);
					float_array	?colors2	=GetGeometryFloatArrayBySemantic(geom, "COLOR", 2, name, out col2Stride);
					float_array	?colors3	=GetGeometryFloatArrayBySemantic(geom, "COLOR", 3, name, out col3Stride);
					List<int>	?colIdxs0	=GetGeometryIndexesBySemantic(geom, "COLOR", 0, name);
					List<int>	?colIdxs1	=GetGeometryIndexesBySemantic(geom, "COLOR", 1, name);
					List<int>	?colIdxs2	=GetGeometryIndexesBySemantic(geom, "COLOR", 2, name);
					List<int>	?colIdxs3	=GetGeometryIndexesBySemantic(geom, "COLOR", 3, name);
					List<int>	?vertCounts	=GetGeometryVertCount(geom, name, ePrint);

					if(vertCounts == null || vertCounts.Count == 0)
					{
						PrintToOutput("Empty geometry chunk in BuildFinalVerts()!\n");
						toNuke.Add(cnk);
						continue;
					}

					cnk.AddNormTexByPoly(posIdxs, norms, normIdxs,
						texCoords0, texIdxs0, texCoords1, texIdxs1,
						texCoords2, texIdxs2, texCoords3, texIdxs3,
						colors0, colIdxs0, colors1, colIdxs1,
						colors2, colIdxs2, colors3, colIdxs3,
						vertCounts, col0Stride, col1Stride, col2Stride, col3Stride, bStatic);

					bool	bPos	=(posIdxs != null && posIdxs.Count > 0);
					bool	bNorm	=(norms != null && norms.count > 0);
					bool	bTex0	=(texCoords0 != null && texCoords0.count > 0);
					bool	bTex1	=(texCoords1 != null && texCoords1.count > 0);
					bool	bTex2	=(texCoords2 != null && texCoords2.count > 0);
					bool	bTex3	=(texCoords3 != null && texCoords3.count > 0);
					bool	bCol0	=(colors0 != null && colors0.count > 0);
					bool	bCol1	=(colors1 != null && colors1.count > 0);
					bool	bCol2	=(colors2 != null && colors2.count > 0);
					bool	bCol3	=(colors3 != null && colors3.count > 0);
					bool	bBone	=false;

					//see if any skins reference this geometry
					if(conts.Count() > 0)
					{
						foreach(controller cont in conts.First().controller)
						{
							skin	?sk	=cont.Item as skin;
							if(sk == null)
							{
								continue;
							}
							string	skinSource	=sk.source1.Substring(1);
							if(skinSource == null || skinSource == "")
							{
								continue;
							}
							if(skinSource == geom.id)
							{
								bBone	=true;
								break;
							}
						}
					}

					//todo obey stride on everything
					cnk.BuildBuffers(gd, bPos, bNorm, bBone,
						bBone, bTex0, bTex1, bTex2, bTex3,
						bCol0, bCol1, bCol2, bCol3);
				}
			}

			//blast empty chunks
			foreach(MeshConverter nuke in toNuke)
			{
				chunks.Remove(nuke);
			}
			toNuke.Clear();
		}
	}


	List<MeshConverter> GetMeshChunks(COLLADA colladaFile, bool bSkinned, MeshConverter.ScaleFactor sf)
	{
		List<MeshConverter>	chunks	=new List<MeshConverter>();

		//if you crash and land here, there are no geoms in the file
		var	geoms	=from g in colladaFile.Items.OfType<library_geometries>().First().geometry
						where g.Item is mesh select g;

		var	polyObjs	=from g in geoms
							let m = g.Item as mesh
							from pols in m.Items
							select pols;

		foreach(geometry geom in geoms)
		{
			mesh	?m	=geom.Item as mesh;

			//check for empty geoms
			if(m == null || m.Items == null)
			{
				PrintToOutput("Empty mesh in GetMeshChunks()\n");
				continue;
			}

			foreach(object polyObj in m.Items)
			{
				polygons	?polys	=polyObj as polygons;
				polylist	?plist	=polyObj as polylist;
				triangles	?tris	=polyObj as triangles;

				if(polys == null && plist == null && tris == null)
				{
					PrintToOutput("Unknown polygon type: " + polyObj + " in mesh: " + geom.name + "!\n");
					continue;
				}

				string	?mat	=null;
				UInt64	count	=0;
				if(polys != null)
				{
					mat		=polys.material;
					count	=polys.count;
				}
				else if(plist != null)
				{
					mat		=plist.material;
					count	=plist.count;
				}
				else if(tris != null)
				{
					mat		=tris.material;
					count	=tris.count;
				}

				if(count <= 0)
				{
					if(mat != null)
					{
						PrintToOutput("Empty polygon in GetMeshChunks for material: " + mat + "\n");
					}
					else
					{
						PrintToOutput("Empty polygon in GetMeshChunks!\n");
					}
					continue;
				}

				float_array		?verts	=null;
				MeshConverter	?cnk	=null;
				int				stride	=0;

				verts	=GetGeometryFloatArrayBySemantic(geom, "VERTEX", 0, mat, out stride);
				if(verts == null)
				{
					PrintToOutput("Empty verts for geom: " + geom.name + ", material: " + mat + "\n");
					continue;
				}


				if(mat == null)
				{
					PrintToOutput("No material for geom: " + geom.name + "\n");

					//return an empty list
					return	new List<MeshConverter>();
				}

				cnk	=new MeshConverter(mat, geom.name);

				float	fileUnitSize	=1f;
				if(colladaFile.asset.unit != null)
				{
					fileUnitSize	=colladaFile.asset.unit.meter;
				}

				cnk.CreateBaseVerts(verts);

				cnk.mPartIndex	=-1;
				cnk.SetGeometryID(geom.id);
					
				chunks.Add(cnk);
			}
		}
		return	chunks;
	}


	void ParseIndexes(string []tokens, int offset, int inputStride, List<int> indexes)
	{
		int	curIdx	=0;
		foreach(string tok in tokens)
		{
			if(curIdx == offset)
			{
				int	val	=0;
				if(int.TryParse(tok, out val))
				{
					indexes.Add(val);
				}
			}
			curIdx++;
			if(curIdx > inputStride)
			{
				curIdx	=0;
			}
		}
	}


	List<int> ?GetGeometryIndexesBySemantic(geometry geom, string sem, int set, string material)
	{
		List<int>	ret	=new List<int>();

		mesh	msh	=(mesh)geom.Item;
		if(msh == null || msh.Items == null)
		{
			return	null;
		}

		string	key		="";
		int		idx		=-1;
		int		ofs		=-1;
		foreach(object polObj in msh.Items)
		{
			polygons	?polys	=polObj as polygons;
			polylist	?plist	=polObj as polylist;
			triangles	?tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				PrintToOutput("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!\n");
				continue;
			}

			InputLocalOffset	[]?inputs	=null;

			if(polys != null)
			{
				inputs	=polys.input;
				if(polys.material != material)
				{
					continue;
				}
			}
			else if(plist != null)
			{
				inputs	=plist.input;
				if(plist.material != material)
				{
					continue;
				}
			}
			else if(tris != null)
			{
				inputs	=tris.input;
				if(tris.material != material)
				{
					continue;
				}
			}

			if(inputs == null)
			{
				continue;
			}

			//find the key, idx, and offset for passed in sem
			for(int i=0;i < inputs.Length;i++)
			{
				InputLocalOffset	inp	=inputs[i];
				if(inp.semantic == sem && set == (int)inp.set)
				{
					//strip #
					key		=inp.source.Substring(1);
					idx		=i;
					ofs		=(int)inp.offset;
					break;
				}
			}

			if(key == "")
			{
				//this is not a big deal, just means this particular
				//semantic doesn't exist, like if there's no vertex
				//colors or texcoords etc
				continue;
			}

			//find the stride to the matched sem, by checking max offset
			int	stride	=0;
			for(int i=0;i < inputs.Length;i++)
			{
				InputLocalOffset	inp	=inputs[i];
				if((int)inp.offset > stride)
				{
					stride	=(int)inp.offset;
				}
			}

			if(polys != null && polys.Items != null)
			{
				foreach(object polyObj in polys.Items)
				{
					string	?pols	=polyObj as string;
					Debug.Assert(pols != null);

					//better collada adds some annoying whitespace
					pols	=pols.Trim();

					string	[]tokens	=pols.Split(' ', '\n');
					ParseIndexes(tokens, ofs, stride, ret);
				}
			}
			else if(plist != null)
			{
				//this path is very untested now
				PrintToOutput("Warning!  PolyLists are very untested at the moment!\n");
				string	[]tokens	=plist.p.Split(' ', '\n');
				ParseIndexes(tokens, ofs, stride, ret);
			}
			else if(tris != null)
			{
				//this path is very untested now
				PrintToOutput("Warning!  Tris are very untested at the moment!\n");
				string	[]tokens	=tris.p.Split(' ', '\n');
				ParseIndexes(tokens, ofs, stride, ret);
			}
		}
		return	ret;
	}


	float_array ?GetGeometryFloatArrayBySemantic(geometry geom,
		string sem, int set, string ?material, out int stride)
	{
		stride	=-1;

		mesh	?msh	=geom.Item as mesh;
		if(msh == null)
		{
			return	null;
		}

		string	key		="";
		int		idx		=-1;
		int		ofs		=-1;
		foreach(object polObj in msh.Items)
		{
			polygons	?polys	=polObj as polygons;
			polylist	?plist	=polObj as polylist;
			triangles	?tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				PrintToOutput("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!\n");
				continue;
			}

			InputLocalOffset	[]?inputs	=null;

			string	polyMat	="";

			if(polys != null)
			{
				polyMat	=polys.material;
				inputs	=polys.input;
			}
			else if(plist != null)
			{
				polyMat	=plist.material;
				inputs	=plist.input;
			}
			else if(tris != null)
			{
				polyMat	=tris.material;
				inputs	=tris.input;
			}

			if(polyMat != material)
			{
				continue;
			}

			if(inputs == null)
			{
				continue;
			}

			for(int i=0;i < inputs.Length;i++)
			{
				InputLocalOffset	inp	=inputs[i];
				if(inp.semantic == sem && set == (int)inp.set)
				{
					//strip #
					key		=inp.source.Substring(1);
					idx		=i;
					ofs		=(int)inp.offset;
					break;
				}
			}
		}

		if(key == "")
		{
			//this is not a big deal, just means this particular
			//semantic doesn't exist, like if there's no vertex
			//colors or texcoords etc
			return	null;
		}

		//check vertices
		if(msh.vertices != null && msh.vertices.id == key)
		{
			key	=msh.vertices.input[0].source.Substring(1);
		}

		for(int j=0;j < msh.source.Length;j++)
		{
			float_array	?verts	=msh.source[j].Item as float_array;
			if(verts == null || msh.source[j].id != key)
			{
				continue;
			}

			stride	=(int)msh.source[j].technique_common.accessor.stride;

			return	verts;
		}

		stride	=-1;

		return	null;
	}


	geometry ?GetGeometryByID(COLLADA colladaFile, string? id)
	{
		if(colladaFile == null)
		{
			return	null;
		}

		return	(from geoms in colladaFile.Items.OfType<library_geometries>().First().geometry
				where geoms is geometry
				where geoms.id == id select geoms).FirstOrDefault();
	}


	bool CNodeHasKeyData(node n)
	{
		if(n.Items == null)
		{
			return	false;
		}

		Matrix4x4	mat	=Matrix4x4.Identity;
		for(int i=0;i < n.Items.Length;i++)
		{
			if(n.ItemsElementName[i] == ItemsChoiceType2.matrix)
			{
				return	true;
			}
			if(n.ItemsElementName[i] == ItemsChoiceType2.rotate)
			{
				return	true;
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.translate)
			{
				return	true;
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.scale)
			{
				return	true;
			}
		}
		return	false;
	}


	Matrix4x4 GetSceneNodeTransform(COLLADA colFile, MeshConverter chunk)
	{
		geometry	?g	=GetGeometryByID(colFile, chunk.mGeometryID);
		if(g == null)
		{
			return	Matrix4x4.Identity;
		}

		var	geomNodes	=from lvs in colFile.Items.OfType<library_visual_scenes>().First().visual_scene
							from n in lvs.node
							where n.instance_geometry != null
							select n;

		foreach(node n in geomNodes)
		{
			foreach(instance_geometry ig in n.instance_geometry)
			{
				if(ig.url.Substring(1) == g.id)
				{
					if(!CNodeHasKeyData(n))
					{
						//no transform needed
						return	Matrix4x4.Identity;
					}

					KeyFrame	kf	=GetKeyFromCNode(n);

					Matrix4x4	mat	=Matrix4x4.CreateScale(kf.mScale) *
						Matrix4x4.CreateFromQuaternion(kf.mRotation) *
						Matrix4x4.CreateTranslation(kf.mPosition);

					return	mat;
				}
			}
		}

		//might have a max pivot
		geomNodes	=from lvs in colFile.Items.OfType<library_visual_scenes>().First().visual_scene
							from n in lvs.node
							where n.instance_geometry == null && n.node1 != null
							select n;

		foreach(node n in geomNodes)
		{
			var subNodes	=from nd in n.node1 where nd.instance_geometry != null select nd;

			foreach(node sn in subNodes)
			{
				foreach(instance_geometry ig in sn.instance_geometry)
				{
					if(ig.url.Substring(1) == g.id)
					{
						if(!CNodeHasKeyData(sn) && !CNodeHasKeyData(n))
						{
							//no transform needed
							return	Matrix4x4.Identity;
						}

						Matrix4x4	parentMat	=Matrix4x4.Identity;
						Matrix4x4	mat			=Matrix4x4.Identity;

						if(CNodeHasKeyData(n))
						{
							KeyFrame	kfParent	=GetKeyFromCNode(n);

							parentMat	=Matrix4x4.CreateScale(kfParent.mScale) *
								Matrix4x4.CreateFromQuaternion(kfParent.mRotation) *
								Matrix4x4.CreateTranslation(kfParent.mPosition);
						}

						if(CNodeHasKeyData(sn))
						{
							KeyFrame	kf	=GetKeyFromCNode(sn);

							mat	=Matrix4x4.CreateScale(kf.mScale) *
								Matrix4x4.CreateFromQuaternion(kf.mRotation) *
								Matrix4x4.CreateTranslation(kf.mPosition);
						}
						return	parentMat * mat;// * parentMat;
					}
				}
			}
		}

		//none found, not necessarily bad
		//skinned stuff doesn't have this
		return	Matrix4x4.Identity;
	}


	internal void RenderUpdate(float msDelta)
	{
		if(mChar == null)
		{
			return;
		}

		if(mSelectedAnim != null && mSelectedAnim != "")
		{
			if(!mbPaused)
			{
				mCurAnimTime	+=msDelta * (float)AnimTimeScale.Value;
			}

			if(mAnimStartTime == 0f && mAnimEndTime == 0)
			{
				mCurAnimTime	=0;
			}
			else
			{
				if(mCurAnimTime > mAnimEndTime)
				{
					mCurAnimTime	%=mAnimEndTime;
				}

				if(mCurAnimTime < mAnimStartTime)
				{
					mCurAnimTime	=mAnimStartTime;
				}
			}

			Debug.Assert(!float.IsNaN(mCurAnimTime));

			mChar.Animate(mSelectedAnim, mCurAnimTime);
		}
	}


	internal void Render(ID3D11DeviceContext dc)
	{
		if(mStatMesh == null && mChar == null)
		{
			return;
		}

		if(mStatMesh != null)
		{
			mStatMesh.Draw(mMatLib);
		}

		if(mChar != null)
		{
			mChar.Draw(mMatLib);
		}
	}


	internal void RenderDMN(ID3D11DeviceContext dc)
	{
		if(mStatMesh == null && mChar == null)
		{
			return;
		}

		if(mStatMesh != null)
		{
			mStatMesh.DrawDMN(mMatLib);
		}

		if(mChar != null)
		{
			mChar.DrawDMN(mMatLib);
		}
	}


	internal void NukeVertexElement(List<int> ?partIndexes, List<int> vertElementIndexes)
	{
		if(mChar != null)
		{
			mChar.NukeVertexElements(mGD, partIndexes, vertElementIndexes);
		}
		else if(mStatMesh != null)
		{
			mStatMesh.NukeVertexElements(mGD, partIndexes, vertElementIndexes);
		}
	}


	internal void NukeMeshPart(List<int> indexes)
	{
		int	partsLeft	=0;

		if(mStatMesh != null)
		{
			mStatMesh.NukeParts(indexes);
			partsLeft	=mStatMesh.GetPartCount();
		}
		else if(mChar != null)
		{
			mChar.NukeParts(indexes);
			partsLeft	=mChar.GetPartCount();
		}

		if(partsLeft > 0)
		{
			return;
		}

		//if all parts are gone, just blast the skin and skel and all
		if(mStatMesh != null)
		{
			mStatMesh.FreeAll();
			mStatMesh	=null;
		}
		else if(mChar != null)
		{
			mChar.FreeAll();
			mChar	=null;
		}

		//Misc.SafeInvoke(eMeshChanged, mea);
	}


	source	[]?MakeSources(string sourcePrefix, object mesh, int partIndex)
	{
		List<source>	ret	=new List<source>();

		StaticMesh	?sm		=mesh as StaticMesh;
		Character	?chr	=mesh as Character;

		Type	vType;

		if(sm != null)
		{
			vType	=sm.GetPartVertexType(partIndex);
		}
		else if(chr != null)
		{
			vType	=chr.GetPartVertexType(partIndex);
		}
		else
		{
			return	null;
		}

		FieldInfo	[]fi	=vType.GetFields();
		for(int i=0;i < fi.Length;i++)
		{
			source	src	=new source();

			if(fi[i].Name == "Position")
			{
				src.id	=sourcePrefix + "-positions";

				float_array	fa	=new float_array();

				float	[]positions;

				if(sm != null)
				{
					sm.GetPartColladaPositions(partIndex, out positions);
				}
				else if(chr != null)
				{
					chr.GetPartColladaPositions(partIndex, out positions);
				}
				else
				{
					continue;
				}

				fa.count		=(ulong)positions.Length;
				fa.id			=src.id + "-array";
				fa.magnitude	=38;	//default?
				fa.Values		=positions;

				src.Item	=fa;

				src.technique_common			=new sourceTechnique_common();
				src.technique_common.accessor	=MakeAccessor(fa.id, positions.Length);
			}
			else if(fi[i].Name == "Normal")
			{
				src.id	=sourcePrefix + "-normals";

				float_array	fa	=new float_array();

				float	[]normals;

				if(sm != null)
				{
					sm.GetPartColladaNormals(partIndex, out normals);
				}
				else if(chr != null)
				{
					chr.GetPartColladaNormals(partIndex, out normals);
				}
				else
				{
					continue;
				}

				fa.count		=(ulong)normals.Length;
				fa.id			=src.id + "-array";
				fa.magnitude	=38;	//default?
				fa.Values		=normals;

				src.Item	=fa;

				src.technique_common			=new sourceTechnique_common();
				src.technique_common.accessor	=MakeAccessor(fa.id, normals.Length);
			}
			else
			{
				continue;
			}

			ret.Add(src);
		}
		return	ret.ToArray();
	}


	accessor	MakeAccessor(string src, int count)
	{
		accessor	acc	=new accessor();

		acc.count	=(ulong)(count / 3);
		acc.stride	=3;
		acc.source	="#" + src;

		acc.param	=new param[3];

		for(int i=0;i < 3;i++)
		{
			acc.param[i]		=new param();
			acc.param[i].type	="float";
		}
		acc.param[0].name	="X";
		acc.param[1].name	="Y";
		acc.param[2].name	="Z";

		return	acc;
	}


	InputLocalOffset	[]MakeInputs(string sourcePrefix, Type vType)
	{
		FieldInfo	[]fi	=vType.GetFields();

		List<InputLocalOffset>	ret	=new List<InputLocalOffset>();

		int	usedIndex	=0;
		for(int i=0;i < fi.Length;i++)
		{
			InputLocalOffset	inp	=new InputLocalOffset();

			inp.offset	=(ulong)usedIndex;

			if(fi[i].Name == "Position")
			{
				inp.semantic	="VERTEX";
				inp.source		="#" + sourcePrefix + "-vertices";
				usedIndex++;
			}
			else if(fi[i].Name == "Normal")
			{
				inp.semantic	="NORMAL";
				inp.source		="#" + sourcePrefix + "-normals";
				usedIndex++;
			}
			else
			{
				continue;
			}
//				else if(fi[i].Name == "TexCoord0")
//				{
//					inp.semantic	="TEXCOORD";
//					inp.source		=sourcePrefix + "-normals";
//				}
			ret.Add(inp);
		}
		return	ret.ToArray();
	}


	void SizeColumns(ListView lv)
	{
		//set to header size first
		Action<ListView>	autoResize	=lvar => lvar.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		FormExtensions.Invoke(lv, autoResize);

		List<int>	sizes	=new List<int>();
		for(int i=0;i < lv.Columns.Count;i++)
		{
			Action<ListView>	addWidth	=lvar => sizes.Add(lvar.Columns[i].Width);
			FormExtensions.Invoke(lv, addWidth);
		}

		for(int i=0;i < lv.Columns.Count;i++)
		{
			Action<ListView>	arHeader	=lvar => {
				lvar.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);

				if(lvar.Columns[i].Width < sizes[i])
				{
					lvar.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
				}
			};

			FormExtensions.Invoke(lv, arHeader);
		}
	}


	void RefreshAnimList()
	{
		Action<ListView>	clear	=lv => lv.Items.Clear();

		FormExtensions.Invoke(AnimList, clear);

		List<Anim>	anims	=mAnimLib.GetAnims();

		foreach(Anim anm in anims)
		{
			Action<ListView>	addItem	=lv => lv.Items.Add(anm.Name);

			FormExtensions.Invoke(AnimList, addItem);
		}

		for(int i=0;i < AnimList.Items.Count;i++)
		{
			Action<ListView>	tagAndSub	=lv =>
			{
				lv.Items[i].SubItems.Add(anims[i].TotalTime.ToString());
				lv.Items[i].SubItems.Add(anims[i].StartTime.ToString());
				lv.Items[i].SubItems.Add(anims[i].Looping.ToString());
				lv.Items[i].SubItems.Add(anims[i].PingPong.ToString());
				lv.Items[i].SubItems.Add(anims[i].NumKeyFrames.ToString());
			};

			FormExtensions.Invoke(AnimList, tagAndSub);
		}

		SizeColumns(AnimList);
	}


	//this is for the standard unchanged export of z up and y forward
	void SetSkinRootTransformBlender()
	{
		//need a couple rotations to go from blender which is:
		//z up, y forward, x right
		//to
		//z forward, y up, x left
		Matrix4x4	spinXToLeft		=Matrix4x4.CreateRotationZ(MathHelper.Pi);
		Matrix4x4	tiltYToForward	=Matrix4x4.CreateRotationX(-MathHelper.PiOver2);
		Matrix4x4	spinYToFront	=Matrix4x4.CreateRotationY(MathHelper.Pi);

		Matrix4x4	accum	=Matrix4x4.Identity;

		accum	*=spinXToLeft;
		accum	*=tiltYToForward;
		accum	*=spinYToFront;

		mChar.GetSkin().SetRootTransform(accum);
	}


	void PrintToOutput(string stuff)
	{
		OnPrintString(stuff, null);
	}


	#region FormEvents
	void OnPrintString(object ?sender, EventArgs ?ea)
	{
		//pass it along
		Misc.SafeInvoke(ePrint, sender, ea);
	}


	void OnSaveAnimLib(object sender, EventArgs e)
	{
		mSFD.DefaultExt		="*.AnimLib";
		mSFD.Filter			="Animation library files (*.AnimLib)|*.AnimLib|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mAnimLib.SaveToFile(mSFD.FileName);
	}


	void OnLoadAnimLib(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.AnimLib";
		mOFD.Filter			="Animation library files (*.AnimLib)|*.AnimLib|All files (*.*)|*.*";
		mOFD.Multiselect	=false;
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mAnimLib.ReadFromFile(mOFD.FileName);

		Skeleton	skel	=mAnimLib.GetSkeleton();

		Misc.SafeInvoke(eSkeletonChanged, skel);

		BonesChanged();

		RefreshAnimList();

		List<Anim>	anims	=mAnimLib.GetAnims();

		int	animCount	=anims.Count;
		int	boneCount	=skel.GetBoneCount();

		PrintToOutput("Animation library: " + FileUtil.StripPath(mOFD.FileName) + " loaded with " +
			 animCount + " animations and a skeleton with " + boneCount + " bones.\n");
	}


	void SetUnitsByScaleFactor(float scaleFactor)
	{
		if(Mathery.CompareFloatEpsilon(Mesh.MetersToCentiMeters, scaleFactor, 0.1f))
		{
			UnitsCentimeters.Checked	=true;
		}
		else if(Mathery.CompareFloatEpsilon(Mesh.MetersToGrogUnits, scaleFactor, 0.1f))
		{
			UnitsGrog.Checked	=true;
		}
		else if(Mathery.CompareFloatEpsilon(Mesh.MetersToQuakeUnits, scaleFactor, 0.1f))
		{
			UnitsQuake.Checked	=true;
		}
		else
		{
			UnitsMeters.Checked	=true;
		}
	}


	void OnLoadCharacter(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.Character";
		mOFD.Filter			="Character files (*.Character)|*.Character|All files (*.*)|*.*";
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		//load up submeshes in the folder
		Dictionary<string, Mesh>	meshes	=new Dictionary<string, Mesh>();
		Mesh.LoadAllMeshes(FileUtil.StripFileName(mOFD.FileName), mGD, meshes);

		mChar	=new Character(mOFD.FileName, meshes, mAnimLib);

		Skin	sk	=mChar.GetSkin();

		float	scaleFactor	=sk.GetScaleFactor();

		Misc.SafeInvoke(eScaleFactorDecided, scaleFactor);
		Misc.SafeInvoke(eMeshChanged, mChar);

		SetUnitsByScaleFactor(scaleFactor);

		int	partCount	=mChar.GetPartCount();

		PrintToOutput("Character: " + FileUtil.StripPath(mOFD.FileName) + " loaded with " +
			 partCount + " submesh parts.\n");
	}


	void OnSaveCharacter(object sender, EventArgs e)
	{
		if(mChar == null)
		{
			PrintToOutput("No character to save!");
			return;
		}

		mSFD.DefaultExt		="*.Character";
		mSFD.Filter			="Character files (*.Character)|*.Character|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mChar.SaveToFile(mSFD.FileName);
		mChar.SaveParts(FileUtil.StripFileName(mSFD.FileName));
	}


	void OnLoadStatic(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.Static";
		mOFD.Filter			="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		//load up submeshes in the folder
		Dictionary<string, Mesh>	meshes	=new Dictionary<string, Mesh>();
		Mesh.LoadAllMeshes(FileUtil.StripFileName(mOFD.FileName), mGD, meshes);

		mStatMesh	=new StaticMesh(mOFD.FileName, meshes);

		Misc.SafeInvoke(eMeshChanged, mStatMesh);
	}


	void OnSaveStatic(object sender, EventArgs e)
	{
		if(mStatMesh == null)
		{
			PrintToOutput("No static to save!");
			return;
		}

		mSFD.DefaultExt		="*.Static";
		mSFD.Filter			="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mStatMesh.SaveToFile(mSFD.FileName + "Instance");
		mStatMesh.SaveParts(FileUtil.StripFileName(mSFD.FileName));
	}


	void OnOpenStaticDAE(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.dae";
		mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
		mOFD.Multiselect	=false;
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		LoadStatic(mOFD.FileName, out mStatMesh);

		Misc.SafeInvoke(eMeshChanged, mStatMesh);
	}


	void OnLoadCharacterDAE(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.dae";
		mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
		mOFD.Multiselect	=true;	//individual parts now
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		foreach(string fileName in mOFD.FileNames)
		{
			LoadCharacterDAE(fileName, mAnimLib);
		}

		RefreshAnimList();

		Misc.SafeInvoke(eMeshChanged, mChar);
	}


	void OnLoadAnimDAE(object sender, EventArgs e)
	{
		mOFD.DefaultExt		="*.dae";
		mOFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
		mOFD.Multiselect	=false;
		DialogResult	dr	=mOFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		LoadAnimDAE(mOFD.FileName, mAnimLib, CheckSkeleton.Checked);

		RefreshAnimList();
	}


	void OnAnimListSelectionChanged(object sender, EventArgs e)
	{
		if(AnimList.SelectedIndices.Count != 1)
		{
			return;
		}

		string	selAnim	=AnimList.SelectedItems[0].Text;

		Anim	anm	=mAnimLib.GetAnim(selAnim);
		if(anm == null)
		{
			return;
		}

		mSelectedAnim	=anm.Name;
		mAnimStartTime	=anm.StartTime;
		mAnimEndTime	=anm.TotalTime + anm.StartTime;
	}


	void OnReCollada(object sender, EventArgs e)
	{
		mSFD.DefaultExt		="*.dae";
		mSFD.Filter			="DAE Collada files (*.dae)|*.dae|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		COLLADA	col;

		if(mStatMesh != null)
		{
			ConvertMesh(mStatMesh, out col);
		}
		else if(mChar != null)
		{
			ConvertMesh(mChar, out col);
		}
		else
		{
			return;
		}

		SerializeCOLLADA(col, mSFD.FileName);
	}


	void OnPauseAnim(object sender, EventArgs e)
	{
		mbPaused	=!mbPaused;

		if(mbPaused)
		{
			PauseButton.Text	="Paused";
		}
		else
		{
			PauseButton.Text	="Pause";
		}
	}


	void OnAnimCellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if(e.ColumnIndex == 0)
		{
			mAnimLib.FixRename();
		}
	}


	void OnComputeRoughBounds(object sender, EventArgs e)
	{
		if(mStatMesh != null)
		{
			Misc.SafeInvoke(eBoundReCompute, mStatMesh);
		}
		else if(mChar != null)
		{
			Misc.SafeInvoke(eBoundReCompute, mChar);
		}
	}


	void OnAnimRename(object sender, LabelEditEventArgs e)
	{
		if(!mAnimLib.RenameAnim(AnimList.Items[e.Item].Text, e.Label))
		{
			e.CancelEdit	=true;
		}
		else
		{
			SizeColumns(AnimList);
		}
	}

	void OnEditBound(object sender, EventArgs e)
	{
		BoundGroup.Enabled		=false;
		ChoiceBox.Enabled		=false;
		ChoiceSphere.Enabled	=false;

		string	msg		="Adjusting bound...\n";

		if(ChoiceBox.Checked)
		{
			msg	+="Use R / Shift-R to adjust width, Y / Shift-Y to adjust depth,\n"
				+ "T / Shift-T to adjust length along the bone axis,\n"
				+ ", and X when finished.\n";
		}
		else
		{
			msg	+="Use R / Shift-R to adjust radius, X when finished.\n";
		}

		PrintToOutput(msg);

		Misc.SafeInvoke(eBoundAdjust, null);
	}

	void OnAnimListKeyUp(object sender, KeyEventArgs e)
	{
		if(e.KeyValue == 46)	//delete
		{
			if(AnimList.SelectedItems.Count < 1)
			{
				return;	//nothing to do
			}

			foreach(ListViewItem lvi in AnimList.SelectedItems)
			{
				mAnimLib.NukeAnim(lvi.Text);
			}

			RefreshAnimList();
		}
	}
	#endregion
}