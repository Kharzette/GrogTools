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
	Character		mChar;
	IArch			mArch;

	public event EventHandler	?eMeshChanged;
	public event EventHandler	?eSkeletonChanged;
	public event EventHandler	?eBoundsChanged;
	public event EventHandler	?ePrint;


	public AnimForm(ID3D11Device gd, MatLib mats, AnimLib alib, StuffKeeper sk)
	{
		InitializeComponent();

		mGD			=gd;
		mMatLib		=mats;
		mSKeeper	=sk;
		mAnimLib	=alib;
		mArch		=new CharacterArch();
		mChar		=new Character(mArch, alib);

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

	internal bool GetDrawBox()
	{
		return	ShowBox.Checked;
	}

	internal bool GetDrawSphere()
	{
		return	ShowSphere.Checked;
	}


	internal void GetBoneNamesInUseByDraw(List<string> names)
	{
		mChar.GetBoneNamesInUseByDraw(names);
	}


	internal void BonesChanged()
	{
		mChar.ReBuildBones(mGD);
	}


	internal COLLADA DeSerializeCOLLADA(string path)
	{
		FileStream		fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
		XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

		COLLADA	ret	=xs.Deserialize(fs) as COLLADA;

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
	internal void ConvertMesh(IArch ia, out COLLADA col)
	{
		col	=new COLLADA();

		col.asset	=new asset();

		col.asset.created		=DateTime.Now;
		col.asset.unit			=new assetUnit();
		col.asset.unit.meter	=0.1f;
		col.asset.unit.name		="meter";
		col.asset.up_axis		=UpAxisType.Z_UP;

		col.Items	=new object[2];

		library_geometries	geom	=new library_geometries();

		int	partCount	=ia.GetPartCount();

		geom.geometry	=new geometry[partCount];

		for(int i=0;i < partCount;i++)
		{
			geometry	g	=new geometry();

			g.name	=ia.GetPartName(i);
			g.id	=g.name + "-mesh";

			polylist	plist	=new polylist();

			Type	vType	=ia.GetPartVertexType(i);

			plist.input	=MakeInputs(g.id, vType);

			plist.material	=g.id + "_mat";	//hax

			string	polys, vcounts;
			plist.count	=(ulong)ia.GetPartColladaPolys(i, out polys, out vcounts);

			plist.p			=polys;
			plist.vcount	=vcounts;

			object	[]itemses	=new object[1];

			itemses[0]	=plist;

			mesh	m	=new mesh();

			m.Items	=itemses;

			m.source	=MakeSources(g.id, ia, i);

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

			nodes[i].id		=ia.GetPartName(i);
			nodes[i].name	=ia.GetPartName(i);

			Matrix4x4	mat	=ia.GetPartTransform(i);

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

			nodes[i].instance_geometry[0].url	="#" + ia.GetPartName(i) + "-mesh";
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
		COLLADA	colladaFile	=DeSerializeCOLLADA(path);

		//don't have a way to test this
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		//do have a way to test this, but it causes
		//the bind shape matrii to have a rotation
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			PrintToOutput("Warning!  Y up axis sort of works but might cause problems sharing anims between mesh parts!\n");
		}

		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(GetScaleFactor());

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

		TransformRoots(alib.GetSkeleton(), anm, scaleFactor);

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


	internal void LoadCharacterDAE(string path,	AnimLib alib, IArch arch)
	{
		COLLADA	colladaFile	=DeSerializeCOLLADA(path);

		//don't have a way to test this
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		//do have a way to test this, but it causes
		//the bind shape matrii to have a rotation
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			PrintToOutput("Warning!  Y up axis sort of works but might cause problems sharing anims between mesh parts!\n");
		}

		//unit conversion
		float	scaleFactor		=1f;
		if(colladaFile.asset.unit != null)
		{
			scaleFactor	=colladaFile.asset.unit.meter;
		}

		scaleFactor	*=MeshConverter.GetScaleFactor(GetScaleFactor());

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

		AddVertexWeightsToChunks(colladaFile, chunks);

		//build or get skeleton
		Skeleton	skel	=BuildSkeleton(colladaFile, ePrint);

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

		TransformRoots(alib.GetSkeleton(), anm, scaleFactor);

		//need to do this again in case keyframes were added
		//for the root bone.
		anm.SetBoneRefs(skel);

		CreateSkin(colladaFile, arch, chunks, skel, scaleFactor, ePrint);

		BuildFinalVerts(mGD, colladaFile, chunks);

		foreach(MeshConverter mc in chunks)
		{
			Mesh		conv	=mc.GetConvertedMesh();
			Matrix4x4	mat		=GetSceneNodeTransform(colladaFile, mc);

			//this might not be totally necessary
			//but it is nice to have
			if(!mat.IsIdentity)
			{
				PrintToOutput("Warning!  Mesh chunk " + conv.Name + "'s scene node is not identity!  This can make it tricksy to orient and move them in a game.\n");
			}

			conv.Name	=mc.GetGeomName();

			conv.SetTransform(Matrix4x4.Identity);

			arch.AddPart(conv);

			if(!conv.Name.EndsWith("Mesh"))
			{
				conv.Name	+="Mesh";
			}
			mc.ePrint	-=OnPrintString;
		}
	}


	internal IArch LoadStatic(string path, out StaticMesh sm)
	{
		COLLADA	colladaFile	=DeSerializeCOLLADA(path);

		//don't have a way to test this
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			PrintToOutput("Warning!  X up axis not supported.  Strange things may happen!\n");
		}

		IArch	arch	=new StaticArch();


		List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, false, GetScaleFactor());

		BuildFinalVerts(mGD, colladaFile, chunks);
		foreach(MeshConverter mc in chunks)
		{
			Mesh	m	=mc.GetConvertedMesh();
			Matrix4x4	mat	=GetSceneNodeTransform(colladaFile, mc);

			m.Name	=mc.GetGeomName();

			//set transform of each mesh
			m.SetTransform(mat);

			arch.AddPart(m);
		}

		sm	=new StaticMesh(arch);

		//this needs to be identity so the game
		//can mess with it without needing the axis info
		sm.SetTransform(Matrix4x4.Identity);

		return	arch;
	}


	void BuildFinalVerts(ID3D11Device gd, COLLADA colladaFile, List<MeshConverter> chunks)
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

					List<int>	posIdxs		=GetGeometryIndexesBySemantic(geom, "VERTEX", 0, name);
					float_array	norms		=GetGeometryFloatArrayBySemantic(geom, "NORMAL", 0, name, out normStride);
					List<int>	normIdxs	=GetGeometryIndexesBySemantic(geom, "NORMAL", 0, name);
					float_array	texCoords0	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 0, name, out tex0Stride);
					float_array	texCoords1	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 1, name, out tex1Stride);
					float_array	texCoords2	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 2, name, out tex2Stride);
					float_array	texCoords3	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 3, name, out tex3Stride);
					List<int>	texIdxs0	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 0, name);
					List<int>	texIdxs1	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 1, name);
					List<int>	texIdxs2	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 2, name);
					List<int>	texIdxs3	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 3, name);
					float_array	colors0		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 0, name, out col0Stride);
					float_array	colors1		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 1, name, out col1Stride);
					float_array	colors2		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 2, name, out col2Stride);
					float_array	colors3		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 3, name, out col3Stride);
					List<int>	colIdxs0	=GetGeometryIndexesBySemantic(geom, "COLOR", 0, name);
					List<int>	colIdxs1	=GetGeometryIndexesBySemantic(geom, "COLOR", 1, name);
					List<int>	colIdxs2	=GetGeometryIndexesBySemantic(geom, "COLOR", 2, name);
					List<int>	colIdxs3	=GetGeometryIndexesBySemantic(geom, "COLOR", 3, name);
					List<int>	vertCounts	=GetGeometryVertCount(geom, name, ePrint);

					if(vertCounts.Count == 0)
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
						vertCounts, col0Stride, col1Stride, col2Stride, col3Stride);

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


	static void BakeSceneNodesIntoVerts(COLLADA				colladaFile,
										Skeleton			skel,
										List<MeshConverter>	chunks,
										EventHandler		ePrint)
	{
		if(colladaFile.Items.OfType<library_controllers>().Count() <= 0)
		{
			return;
		}

		var	ctrlNodes	=from vss in colladaFile.Items.OfType<library_visual_scenes>().First().visual_scene
							from n in vss.node
							select n;

		var	skinControllers	=from conts in colladaFile.Items.OfType<library_controllers>().First().controller
								where conts.Item is skin select conts;
		
		foreach(controller cont in skinControllers)
		{
			string	contID	=cont.id;

			skin	?sk	=cont.Item as skin;

			string	skinSource	=sk.source1.Substring(1);

			foreach(node n in ctrlNodes)
			{
				string	nname	=GetNodeNameForInstanceController(n, cont.id);
				if(nname == "")
				{
					Misc.SafeInvoke(ePrint, "Empty node name for instance controller: " + cont.id + "!\n");
					continue;
				}
				Matrix4x4	mat	=Matrix4x4.Identity;
				if(!skel.GetMatrixForBone(nname, out mat))
				{
					Misc.SafeInvoke(ePrint, "Node: " + nname + " not found in skeleton!\n");
					continue;
				}

				foreach(MeshConverter mc in chunks)
				{
					if(mc.mGeometryID == skinSource)
					{
						mc.BakeTransformIntoVerts(mat);
					}
				}
			}
		}
	}


	static Anim BuildAnim(COLLADA colladaFile, Skeleton skel, library_visual_scenes lvs, string path, EventHandler ePrint)
	{
		//create useful anims
		List<SubAnim>	subs	=CreateSubAnims(colladaFile, skel, ePrint);

		Anim	anm	=new Anim(subs);

		FixMultipleSkeletons(lvs, anm, skel);

		anm.SetBoneRefs(skel);
		anm.Name	=NameFromPath(path);

		return	anm;
	}


	static void CreateSkin(COLLADA				colladaFile,
							IArch				arch,
							List<MeshConverter>	chunks,
							Skeleton				skel,
							float				scaleFactor,
							EventHandler			ePrint)
	{
		IEnumerable<library_controllers>	lcs	=colladaFile.Items.OfType<library_controllers>();
		if(lcs.Count() <= 0)
		{
			Misc.SafeInvoke(ePrint, "No library_controllers in CreateSkin()!\n");
			return;
		}

		//create or reuse a single master skin for the character's parts
		Skin	skin	=arch.GetSkin();
		if(skin == null)
		{
			skin	=new Skin();
		}

		Dictionary<int, Matrix4x4>	invBindPoses	=new Dictionary<int, Matrix4x4>();

		foreach(controller cont in lcs.First().controller)
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

			Matrix4x4	bindMat	=Matrix4x4.Identity;

			GetMatrixFromString(sk.bind_shape_matrix, out bindMat);

			//Blender now seems to always have a rotation stuck in here,
			//and there seems no way to get rid of it, so I compensate
			//for it elsewhere
			if(bindMat != Matrix4x4.Identity)
			{
				Misc.SafeInvoke(ePrint, "Non identity bind pose in skin: " + sk.source1 + "\n");
			}

			string	jointSrc	="";
			string	invSrc		="";
			foreach(InputLocal inp in sk.joints.input)
			{
				if(inp.semantic == "JOINT")
				{
					jointSrc	=inp.source.Substring(1);
				}
				else if(inp.semantic == "INV_BIND_MATRIX")
				{
					invSrc	=inp.source.Substring(1);
				}
			}

			Name_array	?na	=null;
			float_array	?ma	=null;

			foreach(source src in sk.source)
			{
				if(src.id == jointSrc)
				{
					na	=src.Item as Name_array;
				}
				else if(src.id == invSrc)
				{
					ma	=src.Item as float_array;
				}
			}

			List<Matrix4x4>	mats	=GetMatrixListFromFA(ma);
			List<string>	bnames	=GetBoneNamesViaSID(na.Values, colladaFile);

			Debug.Assert(mats.Count == bnames.Count);

			//add to master list
			for(int i=0;i < mats.Count;i++)
			{
				string	bname	=bnames[i];
				Matrix4x4	ibp		=mats[i];
				int		idx		=skel.GetBoneIndex(bname);

				if(idx == -1)
				{
					Misc.SafeInvoke(ePrint, "Warning!  No index in skeleton for bone: " + bname + "!\n");
					continue;
				}
				if(invBindPoses.ContainsKey(idx))
				{
					Misc.SafeInvoke(ePrint, "Warning!  Duplicate bind pose for bone: " + bname + "!\n");
					continue;
				}

				if(invBindPoses.ContainsKey(idx))
				{
					if(invBindPoses[idx] != Matrix4x4.Identity)
					{
						//if bone name already added, make sure the
						//inverse bind pose is the same for this skin
						if(!ibp.Equals(invBindPoses[idx]))
						{
							Misc.SafeInvoke(ePrint, "Warning!  Non matching bind pose for bone: " + bname + "!\n");
						}
					}
				}
				else
				{
					//inverse bind poses need the scalefactor applied
					Matrix4x4.Invert(ibp, out ibp);

					Matrix4x4	scaleMat	=Matrix4x4.CreateScale(scaleFactor);

					ibp	*=scaleMat;

					Matrix4x4.Invert(ibp, out ibp);

					invBindPoses.Add(idx, ibp);
				}
			}
		}

		skin.SetBonePoses(invBindPoses);

		arch.SetSkin(skin);

		FixBoneIndexes(colladaFile, chunks, skel);
	}


	static List<SubAnim> CreateSubAnims(COLLADA colladaFile, Skeleton skel, EventHandler ePrint)
	{
		//create useful anims
		List<SubAnim>	subs	=new List<SubAnim>();

		IEnumerable<library_visual_scenes>	lvs	=colladaFile.Items.OfType<library_visual_scenes>();
		if(lvs.Count() <= 0)
		{
			Misc.SafeInvoke(ePrint, "No library_visual_scenes in CreateSubAnims()!\n");
			return	subs;
		}

		IEnumerable<library_animations>	anims	=colladaFile.Items.OfType<library_animations>();
		if(anims.Count() <= 0)
		{
			Misc.SafeInvoke(ePrint, "No library_animations in CreateSubAnims()!\n");
			return	subs;
		}

		Animation.KeyPartsUsed	partsUsed	=Animation.KeyPartsUsed.None;
		foreach(animation anim in anims.First().animation)
		{
			if(anim.Items == null)
			{
				continue;
			}
			Animation	an	=new Animation(anim);

			subs.AddRange(an.GetAnims(skel, lvs.First(), ePrint, out partsUsed));
		}

		//TODO:  All of this merging stuff needs testing again

		//merge animations affecting a single bone
		List<SubAnim>					merged		=new List<SubAnim>();
		List<Animation.KeyPartsUsed>	mergedParts	=new List<Animation.KeyPartsUsed>();

		//grab full list of bones
		List<string>	boneNames	=new List<string>();

		skel.GetBoneNames(boneNames);
		foreach(string bone in boneNames)
		{
			List<SubAnim>					combine			=new List<SubAnim>();
			List<Animation.KeyPartsUsed>	combineParts	=new List<Animation.KeyPartsUsed>();

			for(int i=0;i < subs.Count;i++)
			{
				SubAnim	sa	=subs[i];

				if(sa.GetBoneName() == bone)
				{
					combine.Add(sa);
					combineParts.Add(partsUsed);
				}
			}

			if(combine.Count == 1)
			{
				merged.Add(combine[0]);
				mergedParts.Add(combineParts[0]);
				continue;
			}
			else if(combine.Count <= 0)
			{
				continue;
			}

			//merge together
			SubAnim		first		=combine.First();
			KeyFrame	[]firstKeys	=first.GetKeys();
			for(int i=1;i < combine.Count;i++)
			{
				KeyFrame	[]next	=combine[i].GetKeys();

				Debug.Assert(firstKeys.Length == next.Length);

				Animation.KeyPartsUsed	nextParts	=combineParts[i];

				//ensure no overlap (shouldn't be)
				Debug.Assert(((UInt32)nextParts & (UInt32)combineParts[0]) == 0);

				MergeKeys(firstKeys, next, nextParts);

				combineParts[0]	|=nextParts;
			}

			merged.Add(first);
			mergedParts.Add(combineParts[0]);
		}

		//post merge, fill in any gaps in the keyframes with
		//data from the nodes themselves
		for(int i=0;i < merged.Count;i++)
		{
			SubAnim		sub			=merged[i];
			string		boneName	=sub.GetBoneName();
			KeyFrame	boneKey		=skel.GetBoneKey(boneName);
			KeyFrame	[]keys		=sub.GetKeys();

			foreach(KeyFrame key in keys)
			{
				FillKeyGaps(key, mergedParts[i], boneKey);
			}
		}

		return	merged;
	}


	static void FillKeyGaps(KeyFrame key, Animation.KeyPartsUsed keyPartsUsed, KeyFrame boneKey)
	{
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateX))
		{
			key.mPosition.X	=boneKey.mPosition.X;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateY))
		{
			key.mPosition.Y	=boneKey.mPosition.Y;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateZ))
		{
			key.mPosition.Z	=boneKey.mPosition.Z;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleX))
		{
			key.mScale.X	=boneKey.mScale.X;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleY))
		{
			key.mScale.Y	=boneKey.mScale.Y;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleZ))
		{
			key.mScale.Z	=boneKey.mScale.Z;
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateX))
		{
			key.mRotation	=Quaternion.Multiply(key.mRotation, boneKey.mRotation);
			key.mRotation	=Quaternion.Multiply(boneKey.mRotation, key.mRotation);
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateY))
		{
			key.mRotation	=Quaternion.Multiply(key.mRotation, boneKey.mRotation);
			key.mRotation	=Quaternion.Multiply(boneKey.mRotation, key.mRotation);
		}
		if(!Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateZ))
		{
			key.mRotation	=Quaternion.Multiply(key.mRotation, boneKey.mRotation);
			key.mRotation	=Quaternion.Multiply(boneKey.mRotation, key.mRotation);
		}
	}


	static void MergeKeys(KeyFrame []first, KeyFrame []next, Animation.KeyPartsUsed nextParts)
	{
		Debug.Assert(first.Length == next.Length);

		for(int i=0;i < first.Length;i++)
		{
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateX))
			{
				first[i].mPosition.X	=next[i].mPosition.X;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateY))
			{
				first[i].mPosition.Y	=next[i].mPosition.Y;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateZ))
			{
				first[i].mPosition.Z	=next[i].mPosition.Z;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleX))
			{
				first[i].mScale.X	=next[i].mScale.X;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleY))
			{
				first[i].mScale.Y	=next[i].mScale.Y;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleZ))
			{
				first[i].mScale.Z	=next[i].mScale.Z;
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateX))
			{
				first[i].mRotation	=Quaternion.Multiply(next[i].mRotation, first[i].mRotation);
				first[i].mRotation	=Quaternion.Multiply(first[i].mRotation, next[i].mRotation);
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateY))
			{
				first[i].mRotation	=Quaternion.Multiply(next[i].mRotation, first[i].mRotation);
				first[i].mRotation	=Quaternion.Multiply(first[i].mRotation, next[i].mRotation);
			}
			if(Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateZ))
			{
				first[i].mRotation	=Quaternion.Multiply(next[i].mRotation, first[i].mRotation);
				first[i].mRotation	=Quaternion.Multiply(first[i].mRotation, next[i].mRotation);
			}
		}
	}


	internal static List<Matrix4x4> GetMatrixListFromFloatList(List<float> fa)
	{
		List<Matrix4x4>	ret	=new List<Matrix4x4>();

		Debug.Assert(fa.Count % 16 == 0);

		for(int i=0;i < (int)fa.Count;i+=16)
		{
			Matrix4x4	mat	=new Matrix4x4();

			mat.M11	=fa[i + 0];
			mat.M21	=fa[i + 1];
			mat.M31	=fa[i + 2];
			mat.M41	=fa[i + 3];
			mat.M12	=fa[i + 4];
			mat.M22	=fa[i + 5];
			mat.M32	=fa[i + 6];
			mat.M42	=fa[i + 7];
			mat.M13	=fa[i + 8];
			mat.M23	=fa[i + 9];
			mat.M33	=fa[i + 10];
			mat.M43	=fa[i + 11];
			mat.M14	=fa[i + 12];
			mat.M24	=fa[i + 13];
			mat.M34	=fa[i + 14];
			mat.M44	=fa[i + 15];

			ret.Add(mat);
		}

		return	ret;
	}


	internal static int GetNodeItemIndex(node n, string sid)
	{
		if(n.Items == null)
		{
			return	-1;
		}

		for(int i=0;i < n.Items.Length;i++)
		{
			object	item	=n.Items[i];
			Type	t		=item.GetType();

			PropertyInfo	[]pinfo	=t.GetProperties();

			foreach(PropertyInfo pi in pinfo)
			{
				if(pi.Name == "sid")
				{
					string	itemSid	=pi.GetValue(item, null) as string;
					if(itemSid == sid)
					{
						return	i;
					}
				}
			}
		}
		return	-1;
	}


	static string NameFromPath(string path)
	{
		int	lastSlash	=path.LastIndexOf('\\');
		if(lastSlash == -1)
		{
			lastSlash	=0;
		}
		else
		{
			lastSlash++;
		}

		int	extension	=path.LastIndexOf('.');

		string	name	=path.Substring(lastSlash, extension - lastSlash);

		return	name;
	}


	static void FixBoneIndexes(COLLADA colladaFile,
		List<MeshConverter> chunks,
		Skeleton skel)
	{
		if(colladaFile.Items.OfType<library_controllers>().Count() <= 0)
		{
			return;
		}

		var	skins	=from conts in colladaFile.Items.OfType<library_controllers>().First().controller
						where conts.Item is skin select conts.Item as skin;

		foreach(skin sk in skins)
		{
			string	jointSrc	="";
			foreach(InputLocal inp in sk.joints.input)
			{
				if(inp.semantic == "JOINT")
				{
					jointSrc	=inp.source.Substring(1);
				}
			}

			Name_array	na	=null;

			foreach(source src in sk.source)
			{
				if(src.id == jointSrc)
				{
					na	=src.Item as Name_array;
				}
			}

			List<string>	bnames	=GetBoneNamesViaSID(na.Values, colladaFile);
			string	skinSource	=sk.source1.Substring(1);

			foreach(MeshConverter cnk in chunks)
			{
				if(cnk.mGeometryID == skinSource)
				{
					cnk.FixBoneIndexes(skel, bnames);
				}
			}
		}
	}


	static List<string> GetBoneNamesViaSID(string []sids, COLLADA cfile)
	{
		List<string>	boneNames	=new List<string>();

		IEnumerable<library_visual_scenes>	lvs	=cfile.Items.OfType<library_visual_scenes>();

		foreach(string sid in sids)
		{
			//supposed to use sids (I think, the spec is ambiguous)
			//but if that fails use ids.  Maybe should use names I dunno
			node	n	=LookUpNodeViaSID(lvs.First(), sid);

			if(n == null)
			{
				n	=LookUpNode(lvs.First(), sid);
			}
			
			Debug.Assert(n != null);

			boneNames.Add(n.name);
		}
		return	boneNames;
	}


	internal static node LookUpNodeViaSID(library_visual_scenes lvs, string SID)
	{
		//find the node addressed
		node	addressed	=null;
		foreach(visual_scene vs in lvs.visual_scene)
		{
			foreach(node n in vs.node)
			{
				addressed	=LookUpNodeViaSID(n, SID);
				if(addressed != null)
				{
					break;
				}
			}
		}
		return	addressed;
	}


	internal static node LookUpNodeViaSID(node n, string sid)
	{
		if(n.sid == sid)
		{
			return	n;
		}

		if(n.node1 == null)
		{
			return	null;
		}

		foreach(node child in n.node1)
		{
			node	ret	=LookUpNodeViaSID(child, sid);
			if(ret != null)
			{
				return	ret;
			}
		}
		return	null;
	}


	internal static void GetMatrixFromString(string str, out Matrix4x4 mat)
	{
		string[] tokens	=str.Split(' ', '\n', '\t');

		int	tokIdx	=0;

		//transpose as we load
		//this looks very unsafe / dangerous
		while(!Single.TryParse(tokens[tokIdx++],out mat.M11));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M21));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M31));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M41));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M12));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M22));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M32));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M42));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M13));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M23));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M33));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M43));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M14));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M24));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M34));
		while(!Single.TryParse(tokens[tokIdx++],out mat.M44));
	}


	internal static List<Matrix4x4> GetMatrixListFromFA(float_array fa)
	{
		List<Matrix4x4>	ret	=new List<Matrix4x4>();

		Debug.Assert(fa.count % 16 == 0);

		for(int i=0;i < (int)fa.count;i+=16)
		{
			Matrix4x4	mat	=new Matrix4x4();

			mat.M11	=fa.Values[i + 0];
			mat.M21	=fa.Values[i + 1];
			mat.M31	=fa.Values[i + 2];
			mat.M41	=fa.Values[i + 3];
			mat.M12	=fa.Values[i + 4];
			mat.M22	=fa.Values[i + 5];
			mat.M32	=fa.Values[i + 6];
			mat.M42	=fa.Values[i + 7];
			mat.M13	=fa.Values[i + 8];
			mat.M23	=fa.Values[i + 9];
			mat.M33	=fa.Values[i + 10];
			mat.M43	=fa.Values[i + 11];
			mat.M14	=fa.Values[i + 12];
			mat.M24	=fa.Values[i + 13];
			mat.M34	=fa.Values[i + 14];
			mat.M44	=fa.Values[i + 15];

			ret.Add(mat);
		}

		return	ret;
	}


	static void FixMultipleSkeletons(library_visual_scenes lvs, Anim anm, Skeleton skel)
	{
		Debug.Assert(lvs.visual_scene.Length == 1);

		foreach(node n in lvs.visual_scene[0].node)
		{
			if(n.instance_controller != null)
			{
				Debug.Assert(n.instance_controller.Length == 1);

				string	[]skels	=n.instance_controller.First().skeleton;

				if(skels != null)
				{
					if(skels.Length > 1)
					{
						for(int i=1;i < skels.Length;i++)
						{
							string	skelName	=skels[i].Substring(1);

							node	skelNode	=LookUpNodeViaSID(lvs, skelName);
							if(skelNode == null)
							{
								skelNode	=LookUpNode(lvs, skelName);
							}

							anm.FixDetatchedSkeleton(skel, skelNode.name);
						}
					}
				}
			}
		}
	}


	static string GetNodeNameForInstanceController(node n, string ic)
	{
		if(n.instance_controller != null)
		{
			foreach(instance_controller inst in n.instance_controller)
			{
				if(inst.url.Substring(1) == ic)
				{
					return	n.name;
				}
			}
		}

		if(n.node1 == null)
		{
			return	"";
		}

		//check kids
		foreach(node kid in n.node1)
		{
			string	ret	=GetNodeNameForInstanceController(kid, ic);
			if(ret != "")
			{
				return	ret;
			}
		}
		return	"";
	}


	static string AdjustName(List<string> namesInUse, string name)
	{
		string	origName	=name;
		int		postNum		=0;

		while(namesInUse.Contains(name))
		{
			name	=origName + String.Format("{0,10:D6}", postNum);
		}
		return	name;
	}


	static void BuildSkeleton(node n, List<string> namesInUse, out GSNode gsn, EventHandler ePrint)
	{
		gsn	=new GSNode();

		if(namesInUse.Contains(n.name))
		{
			Misc.SafeInvoke(ePrint, "Warning!  Non unique bone name: " + n.name + "!\n");

			string	newName	=AdjustName(namesInUse, n.name);

			gsn.SetName(newName);
			namesInUse.Add(newName);

			//also need to adjust the collada data so searches
			//will spot the adjusted name
			n.name	=newName;
		}
		else
		{
			gsn.SetName(n.name);
			namesInUse.Add(n.name);
		}

		KeyFrame	kf	=GetKeyFromCNode(n);

		gsn.SetKey(kf);

		if(n.node1 == null)
		{
			return;
		}

		foreach(node child in n.node1)
		{
			GSNode	kid	=new GSNode();

			BuildSkeleton(child, namesInUse, out kid, ePrint);

			gsn.AddChild(kid);
		}
	}


	static Skeleton BuildSkeleton(COLLADA colMesh, EventHandler ePrint)
	{
		Skeleton	ret	=new Skeleton();

		var	nodes	=from lvs in colMesh.Items.OfType<library_visual_scenes>().First().visual_scene
						from n in lvs.node select n;

		List<string>	namesInUse	=new List<string>();
		foreach(node n in nodes)
		{
			GSNode	gsnRoot	=new GSNode();

			BuildSkeleton(n, namesInUse, out gsnRoot, ePrint);

			ret.AddRoot(gsnRoot);
		}

		ret.ComputeNameIndex();

		return	ret;
	}


	static void AddVertexWeightsToChunks(COLLADA colladaFile, List<MeshConverter> chunks)
	{
		if(colladaFile.Items.OfType<library_controllers>().Count() <= 0)
		{
			return;
		}

		var	skins	=from conts in colladaFile.Items.OfType<library_controllers>().First().controller
						where conts.Item is skin select conts.Item as skin;

		foreach(skin sk in skins)
		{
			string	skinSource	=sk.source1.Substring(1);

			foreach(MeshConverter cnk in chunks)
			{
				if(cnk.mGeometryID == skinSource)
				{
					cnk.AddWeightsToBaseVerts(sk);
				}
			}
		}
	}


	static List<int> GetGeometryVertCount(geometry geom, string material, EventHandler ePrint)
	{
		List<int>	ret	=new List<int>();

		mesh	msh	=geom.Item as mesh;
		if(msh == null || msh.Items == null)
		{
			return	null;
		}
		foreach(object polObj in msh.Items)
		{
			polygons	polys	=polObj as polygons;
			polylist	plist	=polObj as polylist;
			triangles	tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				Misc.SafeInvoke(ePrint, "Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!\n");
				continue;
			}

			if(polys != null)
			{
				if(polys.material != material)
				{
					continue;
				}
				if(polys.Items == null)
				{
					Misc.SafeInvoke(ePrint, geom.name + ", null Items!\n");
					continue;
				}

				//find the stride
				int	stride	=0;
				for(int i=0;i < polys.input.Length;i++)
				{
					InputLocalOffset	inp	=polys.input[i];
					if((int)inp.offset > stride)
					{
						stride	=(int)inp.offset;
					}
				}

				foreach(object polyObj in polys.Items)
				{
					string	pols	=polyObj as string;
					Debug.Assert(pols != null);

					pols	=pols.Trim();

					string	[]tokens	=pols.Split(' ', '\n');

					//sometimes inputs share an index, sometimes they are all separated
					ret.Add(tokens.Length / (stride + 1));
				}
			}
			else if(plist != null)
			{
				//this path is very untested now
				if(plist.material != material)
				{
					continue;
				}
				Misc.SafeInvoke(ePrint, "Warning!  PolyLists are very untested at the moment!\n");
				string	[]tokens	=plist.vcount.Split(' ', '\n');

				int	numSem	=plist.input.Length;
				foreach(string tok in tokens)
				{
					if(tok == "")
					{
						continue;	//blender exporter wierdness
					}
					
					int	vertCount;
					
					bool	bGood	=Int32.TryParse(tok, out vertCount);

					Debug.Assert(bGood);

					ret.Add(vertCount);
				}
			}
			else if(tris != null)
			{
				//this path is very untested now
				if(tris.material != material)
				{
					continue;
				}
				Misc.SafeInvoke(ePrint, "Warning!  Tris are very untested at the moment!\n");

				for(int i=0;i < (int)tris.count;i++)
				{
					ret.Add(3);
				}
			}
		}
		return	ret;
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
			mesh	m	=geom.Item as mesh;

			//check for empty geoms
			if(m.Items == null)
			{
				PrintToOutput("Empty mesh in GetMeshChunks()\n");
				continue;
			}

			foreach(object polyObj in m.Items)
			{
				polygons	polys	=polyObj as polygons;
				polylist	plist	=polyObj as polylist;
				triangles	tris	=polyObj as triangles;

				if(polys == null && plist == null && tris == null)
				{
					PrintToOutput("Unknown polygon type: " + polyObj + " in mesh: " + geom.name + "!\n");
					continue;
				}

				string	mat		=null;
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

				float_array		verts	=null;
				MeshConverter	cnk		=null;
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

				cnk.CreateBaseVerts(verts, fileUnitSize, sf);

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


	List<int> GetGeometryIndexesBySemantic(geometry geom, string sem, int set, string material)
	{
		List<int>	ret	=new List<int>();

		mesh	msh	=geom.Item as mesh;
		if(msh == null || msh.Items == null)
		{
			return	null;
		}

		string	key		="";
		int		idx		=-1;
		int		ofs		=-1;
		foreach(object polObj in msh.Items)
		{
			polygons	polys	=polObj as polygons;
			polylist	plist	=polObj as polylist;
			triangles	tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				PrintToOutput("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!\n");
				continue;
			}

			InputLocalOffset	[]inputs	=null;

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
					string	pols	=polyObj as string;
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


	float_array GetGeometryFloatArrayBySemantic(geometry geom,
		string sem, int set, string material, out int stride)
	{
		stride	=-1;

		mesh	msh	=geom.Item as mesh;
		if(msh == null)
		{
			return	null;
		}

		string	key		="";
		int		idx		=-1;
		int		ofs		=-1;
		foreach(object polObj in msh.Items)
		{
			polygons	polys	=polObj as polygons;
			polylist	plist	=polObj as polylist;
			triangles	tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				PrintToOutput("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!\n");
				continue;
			}

			InputLocalOffset	[]inputs	=null;

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
			float_array	verts	=msh.source[j].Item as float_array;
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


	geometry GetGeometryByID(COLLADA colladaFile, string id)
	{
		return	(from geoms in colladaFile.Items.OfType<library_geometries>().First().geometry
				where geoms is geometry
				where geoms.id == id select geoms).FirstOrDefault();
	}


	static void	matrixToMatrix(ref matrix cMat, ref Matrix4x4 sdxMat)
	{
		sdxMat.M11	=cMat.Values[0];
		sdxMat.M21	=cMat.Values[1];
		sdxMat.M31	=cMat.Values[2];
		sdxMat.M41	=cMat.Values[3];
		sdxMat.M12	=cMat.Values[4];
		sdxMat.M22	=cMat.Values[5];
		sdxMat.M32	=cMat.Values[6];
		sdxMat.M42	=cMat.Values[7];
		sdxMat.M13	=cMat.Values[8];
		sdxMat.M23	=cMat.Values[9];
		sdxMat.M33	=cMat.Values[10];
		sdxMat.M43	=cMat.Values[11];
		sdxMat.M14	=cMat.Values[12];
		sdxMat.M24	=cMat.Values[13];
		sdxMat.M34	=cMat.Values[14];
		sdxMat.M44	=cMat.Values[15];
	}


	static KeyFrame GetKeyFromCNode(node n)
	{
		KeyFrame	key	=new KeyFrame();

		if(n.Items == null)
		{
			return	key;
		}

		Matrix4x4	mat	=Matrix4x4.Identity;
		for(int i=0;i < n.Items.Length;i++)
		{
			if(n.ItemsElementName[i] == ItemsChoiceType2.matrix)
			{
				matrix	cmat	=n.Items[i] as matrix;

				Debug.Assert(cmat != null);

				matrixToMatrix(ref cmat, ref mat);
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.rotate)
			{
				rotate	rot	=n.Items[i] as rotate;

				Debug.Assert(rot != null);

				Vector3	axis	=Vector3.Zero;
				axis.X			=rot.Values[0];
				axis.Y			=rot.Values[1];
				axis.Z			=rot.Values[2];
				float	angle	=MathHelper.ToRadians(rot.Values[3]);

				mat	=Matrix4x4.CreateFromAxisAngle(axis, angle)
					* mat;
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.translate)
			{
				TargetableFloat3	trans	=n.Items[i] as TargetableFloat3;

				Vector3	t	=Vector3.Zero;
				t.X	=trans.Values[0];
				t.Y	=trans.Values[1];
				t.Z	=trans.Values[2];

				mat	=Matrix4x4.CreateTranslation(t)
					* mat;
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.scale)
			{
				TargetableFloat3	scl	=n.Items[i] as TargetableFloat3;

				Vector3	t	=Vector3.Zero;
				t.X	=scl.Values[0];
				t.Y	=scl.Values[1];
				t.Z	=scl.Values[2];

				mat	=Matrix4x4.CreateScale(t)
					* mat;
			}
		}

		bool	bOK	=Matrix4x4.Decompose(mat,
				out key.mScale, out key.mRotation, out key.mPosition);
		
		Debug.Assert(bOK);

		return	key;
	}


	internal static node LookUpNode(library_visual_scenes lvs, string nodeID)
	{
		//find the node addressed
		node	addressed	=null;
		foreach(visual_scene vs in lvs.visual_scene)
		{
			foreach(node n in vs.node)
			{
				addressed	=LookUpNode(n, nodeID);
				if(addressed != null)
				{
					break;
				}
			}
		}
		return	addressed;
	}


	internal static node LookUpNode(node n, string id)
	{
		if(n.id == id)
		{
			return	n;
		}

		if(n.node1 == null)
		{
			return	null;
		}

		foreach(node child in n.node1)
		{
			node	ret	=LookUpNode(child, id);
			if(ret != null)
			{
				return	ret;
			}
		}
		return	null;
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
		geometry	g	=GetGeometryByID(colFile, chunk.mGeometryID);
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

					KeyFrame.RightHandToLeft(ref mat);
								
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

							KeyFrame.RightHandToLeft(ref parentMat);
						}

						if(CNodeHasKeyData(sn))
						{
							KeyFrame	kf	=GetKeyFromCNode(sn);

							mat	=Matrix4x4.CreateScale(kf.mScale) *
								Matrix4x4.CreateFromQuaternion(kf.mRotation) *
								Matrix4x4.CreateTranslation(kf.mPosition);

							KeyFrame.RightHandToLeft(ref mat);
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


	internal void NukeVertexElement(List<int> partIndexes, List<int> vertElementIndexes)
	{
		mArch.NukeVertexElements(mGD, partIndexes, vertElementIndexes);
	}


	internal void NukeMeshPart(List<int> indexes)
	{
		if(mArch != null)
		{
			mArch.NukeParts(indexes);
		}
		if(mStatMesh != null)
		{
			mStatMesh.NukeParts(indexes);
		}
		else
		{
			mChar.NukeParts(indexes);
		}
	}


	source	[]MakeSources(string sourcePrefix, IArch arch, int partIndex)
	{
		List<source>	ret	=new List<source>();

		Type	vType	=arch.GetPartVertexType(partIndex);

		FieldInfo	[]fi	=vType.GetFields();
		for(int i=0;i < fi.Length;i++)
		{
			source	src	=new source();

			if(fi[i].Name == "Position")
			{
				src.id	=sourcePrefix + "-positions";

				float_array	fa	=new float_array();

				float	[]positions;

				arch.GetPartColladaPositions(partIndex, out positions);

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

				arch.GetPartColladaNormals(partIndex, out normals);

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


	//for adjusting coordinate systems
	void TransformRoots(Skeleton skel, Anim anm, float scaleFactor)
	{
		//this will undo the scaling from the bind shape
		Matrix4x4	scaleMat	=Matrix4x4.CreateScale(Vector3.One * scaleFactor);

		List<string>	roots	=new List<string>();

		skel.GetRootNames(roots);

		//rotate the animation to match our coord system
		foreach(string root in roots)
		{
			anm.TransformBoneAnim(root, scaleMat);
		}
	}


	void PrintToOutput(string stuff)
	{
		OnPrintString(stuff, null);
	}


	#region FormEvents
	void OnPrintString(object sender, EventArgs ea)
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

		Misc.SafeInvoke(eSkeletonChanged, mAnimLib.GetSkeleton());

		RefreshAnimList();
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

		mArch	=new CharacterArch();
		mChar	=new Character(mArch, mAnimLib);

		Mesh.MeshAndArch	mea	=new Mesh.MeshAndArch();

		mea.mMesh	=mChar;
		mea.mArch	=mArch;

		mArch.ReadFromFile(mOFD.FileName, mGD, true);
		mChar.ReadFromFile(mOFD.FileName + "Instance");

		Misc.SafeInvoke(eMeshChanged, mea);
	}


	void OnSaveCharacter(object sender, EventArgs e)
	{
		mSFD.DefaultExt		="*.Character";
		mSFD.Filter			="Character files (*.Character)|*.Character|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mArch.SaveToFile(mSFD.FileName);
		mChar.SaveToFile(mSFD.FileName + "Instance");
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

		mArch		=new StaticArch();
		mStatMesh	=new StaticMesh(mArch);

		Mesh.MeshAndArch	mea	=new Mesh.MeshAndArch();

		mea.mMesh	=mStatMesh;
		mea.mArch	=mArch;

		mArch.ReadFromFile(mOFD.FileName, mGD, true);
		mStatMesh.ReadFromFile(mOFD.FileName + "Instance");

		Misc.SafeInvoke(eMeshChanged, mea);
	}


	void OnSaveStatic(object sender, EventArgs e)
	{
		mSFD.DefaultExt		="*.Static";
		mSFD.Filter			="Static mesh files (*.Static)|*.Static|All files (*.*)|*.*";
		DialogResult	dr	=mSFD.ShowDialog();

		if(dr == DialogResult.Cancel)
		{
			return;
		}

		mArch.SaveToFile(mSFD.FileName);
		mStatMesh.SaveToFile(mSFD.FileName + "Instance");
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

		mArch	=LoadStatic(mOFD.FileName, out mStatMesh);

		Mesh.MeshAndArch	mea	=new Mesh.MeshAndArch();

		mea.mMesh	=mStatMesh;
		mea.mArch	=mArch;

		Misc.SafeInvoke(eMeshChanged, mea);
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
			LoadCharacterDAE(fileName, mAnimLib, mArch);
		}

		RefreshAnimList();

		Mesh.MeshAndArch	mea	=new Mesh.MeshAndArch();

		mChar	=new Character(mArch, mAnimLib);

		mea.mMesh	=mChar;
		mea.mArch	=mArch;

		Misc.SafeInvoke(eMeshChanged, mea);
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
		ConvertMesh(mArch, out col);

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


	void OnCalcBounds(object sender, EventArgs e)
	{
		mArch.UpdateBounds();

		if(mChar != null)
		{
			if(!mChar.IsEmpty())
			{
				mChar.UpdateBounds();
				Misc.SafeInvoke(eBoundsChanged, mChar);
			}
		}
	}


	void OnShowSphereChanged(object sender, EventArgs e)
	{
		if(ShowSphere.Checked)
		{
			ShowBox.Checked	=false;
		}
	}


	void OnShowBoxChanged(object sender, EventArgs e)
	{
		if(ShowBox.Checked)
		{
			ShowSphere.Checked	=false;
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