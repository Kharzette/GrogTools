using System.Numerics;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;
using Vortice.Mathematics;
using MeshLib;
using UtilityLib;

namespace ColladaConvert;

internal class ColladaData
{
	//loads an animation into an existing anim lib
	internal static bool LoadAnimDAE(string path, MeshConverter.ScaleFactor scaleDesired,
									 AnimLib alib, bool bCheckSkeleton, bool bRightHand)
	{
		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			Console.WriteLine("Null file in LoadAnim()\n");
			return	false;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Y or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			Console.WriteLine("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			Console.WriteLine("Warning!  Y up axis not supported.  Strange things may happen!\n");
		}

		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(scaleDesired);

		Matrix4x4	scaleMat	=Matrix4x4.CreateScale(Vector3.One * (1f / scaleFactor));

		Skeleton	skel	=BuildSkeleton(colladaFile);

		if(!bRightHand)
		{
			skel.ConvertToLeftHanded();
		}

		//grab visual scenes
		IEnumerable<library_visual_scenes>	lvss	=
			colladaFile.Items.OfType<library_visual_scenes>();

		library_visual_scenes	lvs	=lvss.First();

		//see if animlib has a skeleton yet
		if(alib.GetSkeleton() == null)
		{
			alib.SetSkeleton(skel);
//			Misc.SafeInvoke(eSkeletonChanged, skel);
		}
		else if(bCheckSkeleton)
		{
			//make sure they match
			if(!alib.CheckSkeleton(skel))
			{
				Console.WriteLine("Warning!  Skeleton check failed, anim load aborted!\n");
				return	false;
			}
		}

		Anim	anm	=BuildAnim(colladaFile, alib.GetSkeleton(), lvs, path);

		if(alib.HasAnim(anm.Name))
		{
			alib.NukeAnim(anm.Name);

			//if only 1 anim in the lib it nukes the skeleton
			if(alib.GetSkeleton() == null)
			{
				alib.SetSkeleton(skel);
			}
		}

		alib.AddAnim(anm);

		//need to do this again in case keyframes were added
		//for the root bone.
		anm.SetBoneRefs(alib.GetSkeleton());

		return	true;
	}

	internal static Character LoadCharacterDAE(string path, MeshConverter.ScaleFactor scaleDesired, AnimLib alib, bool bRightHand)
	{
		Character	ret	=null;

		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			Console.WriteLine("Null file in LoadCharacterDAE()\n");
			return	ret;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Y or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			Console.WriteLine("Warning!  X up axis not supported.  Strange things may happen!\n");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			Console.WriteLine("Warning!  Y up axis not supported.  Strange things may happen!\n");
		}

		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(scaleDesired);

		Matrix4x4	scaleMat	=Matrix4x4.CreateScale(Vector3.One * (1f / scaleFactor));
		
		//grab visual scenes
		IEnumerable<library_visual_scenes>	lvss	=
			colladaFile.Items.OfType<library_visual_scenes>();

		library_visual_scenes	lvs	=lvss.First();

		List<MeshConverter>	allChunks	=GetMeshChunks(colladaFile, !bRightHand, scaleDesired);
		List<MeshConverter>	chunks		=new List<MeshConverter>();

		//skip dummies
		foreach(MeshConverter mc in allChunks)
		{
			if(!mc.GetName().Contains("DummyGeometry"))
			{
				chunks.Add(mc);
//				mc.ePrint	+=OnPrintString;
			}
		}

		allChunks.Clear();

		if(!AddVertexWeightsToChunks(colladaFile, chunks))
		{
			Console.WriteLine("No vertex weights... are you trying to load static geometry as a character?\n");
			return	ret;
		}

		//build or get skeleton
		Skeleton	skel	=BuildSkeleton(colladaFile);
		if(skel ==  null)
		{
			Console.WriteLine("No skeleton... are you trying to load static geometry as a character?\n");
			return	ret;
		}

		if(!bRightHand)
		{
			skel.ConvertToLeftHanded();
		}

		//see if animlib has a skeleton yet
		if(alib.GetSkeleton() == null)
		{
			alib.SetSkeleton(skel);
//			Misc.SafeInvoke(eSkeletonChanged, skel);
		}
		else
		{
			//make sure they match
			if(!alib.CheckSkeleton(skel))
			{
				Console.WriteLine("Warning!  Skeleton check failed!  Might need to restart to clear the animation library skeleton.\n");
				return	ret;
			}

			//use old one
			skel	=alib.GetSkeleton();
		}

		Anim	anm	=BuildAnim(colladaFile, alib.GetSkeleton(), lvs, path);

		if(alib.HasAnim(anm.Name))
		{
			alib.NukeAnim(anm.Name);
		}
		
		alib.AddAnim(anm);

		//need to do this again in case keyframes were added
		//for the root bone.
		anm.SetBoneRefs(skel);

		FixBoneIndexes(colladaFile, chunks, skel);

		BuildFinalVerts(colladaFile, chunks, !bRightHand);

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
				Console.WriteLine("Warning!  Mesh chunk " + conv.Name + "'s scene node is not identity!  This can make it tricksy to orient and move them in a game.\n");
			}

			conv.Name	=mc.GetGeomName();

			converted.Add(conv);

			if(!conv.Name.EndsWith("Mesh"))
			{
				conv.Name	+="Mesh";
			}
//			mc.ePrint	-=OnPrintString;
		}

		Skin	?sk	=null;
		CreateSkin(colladaFile, ref sk, chunks, skel, scaleFactor);

		ret	=new Character(converted, sk, alib);

		if(bRightHand)
		{
			SetSkinRootTransformBlenderRightHand(ret);
		}
		else
		{
			SetSkinRootTransformBlenderLeftHand(ret);
		}

		return	ret;
	}

	internal static void LoadStaticDAE(string path, MeshConverter.ScaleFactor scaleDesired, out StaticMesh ?sm, bool bRightHand)
	{
		COLLADA	?colladaFile	=DeSerializeCOLLADA(path);

		if(colladaFile == null)
		{
			Console.WriteLine("Null file in LoadStatic()");
			sm	=null;
			return;
		}

		//Blender's collada exporter always outputs Z up.
		//If you select Z or X it simply rotates the model before export
		if(colladaFile.asset.up_axis == UpAxisType.X_UP)
		{
			Console.WriteLine("Warning!  X up axis not supported.  Strange things may happen!");
		}
		else if(colladaFile.asset.up_axis == UpAxisType.Y_UP)
		{
			Console.WriteLine("Warning!  Y up axis not supported.  Strange things may happen!");
		}
		
		//unit conversion
		float	scaleFactor	=colladaFile.asset.unit.meter;

		scaleFactor	*=MeshConverter.GetScaleFactor(scaleDesired);

		sm	=new StaticMesh();

		//if not right handed flip X
		List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, !bRightHand, scaleDesired);

		//and this flips the winding order if left handed
		BuildFinalVerts(colladaFile, chunks, !bRightHand);
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

	static bool CNodeHasKeyData(node n)
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

	static geometry ?GetGeometryByID(COLLADA colladaFile, string? id)
	{
		if(colladaFile == null)
		{
			return	null;
		}

		return	(from geoms in colladaFile.Items.OfType<library_geometries>().First().geometry
				where geoms is geometry
				where geoms.id == id select geoms).FirstOrDefault();
	}

	static List<MeshConverter> GetMeshChunks(COLLADA colladaFile, bool bXFlip, MeshConverter.ScaleFactor sf)
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
				Console.WriteLine("Empty mesh in GetMeshChunks()");
				continue;
			}

			foreach(object polyObj in m.Items)
			{
				polygons	?polys	=polyObj as polygons;
				polylist	?plist	=polyObj as polylist;
				triangles	?tris	=polyObj as triangles;

				if(polys == null && plist == null && tris == null)
				{
					Console.WriteLine("Unknown polygon type: " + polyObj + " in mesh: " + geom.name + "!");
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
						Console.WriteLine("Empty polygon in GetMeshChunks for material: " + mat + "");
					}
					else
					{
						Console.WriteLine("Empty polygon in GetMeshChunks!");
					}
					continue;
				}

				float_array		?verts	=null;
				MeshConverter	?cnk	=null;
				int				stride	=0;

				verts	=GetGeometryFloatArrayBySemantic(geom, "VERTEX", 0, mat, out stride);
				if(verts == null)
				{
					Console.WriteLine("Empty verts for geom: " + geom.name + ", material: " + mat + "");
					continue;
				}


				if(mat == null)
				{
					Console.WriteLine("No material for geom: " + geom.name + "");

					//return an empty list
					return	new List<MeshConverter>();
				}

				cnk	=new MeshConverter(mat, geom.name);

				float	fileUnitSize	=1f;
				if(colladaFile.asset.unit != null)
				{
					fileUnitSize	=colladaFile.asset.unit.meter;
				}

				cnk.CreateBaseVerts(verts, bXFlip);

				cnk.mPartIndex	=-1;
				cnk.SetGeometryID(geom.id);
					
				chunks.Add(cnk);
			}
		}
		return	chunks;
	}

	static void ParseIndexes(string []tokens, int offset, int inputStride, List<int> indexes)
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

	static List<int> ?GetGeometryIndexesBySemantic(geometry geom, string sem, int set, string material)
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
				Console.WriteLine("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!");
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
				Console.WriteLine("Warning!  PolyLists are very untested at the moment!");
				string	[]tokens	=plist.p.Split(' ', '\n');
				ParseIndexes(tokens, ofs, stride, ret);
			}
			else if(tris != null)
			{
				//this path is very untested now
				Console.WriteLine("Warning!  Tris are very untested at the moment!");
				string	[]tokens	=tris.p.Split(' ', '\n');
				ParseIndexes(tokens, ofs, stride, ret);
			}
		}
		return	ret;
	}

	static float_array ?GetGeometryFloatArrayBySemantic(geometry geom,
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
				Console.WriteLine("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!");
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

	static void BuildFinalVerts(COLLADA colladaFile, List<MeshConverter> chunks, bool bFlipTri)
	{
		IEnumerable<library_geometries>		geoms	=colladaFile.Items.OfType<library_geometries>();
		IEnumerable<library_controllers>	conts	=colladaFile.Items.OfType<library_controllers>();

		Console.WriteLine("geoms.Count() is: " + geoms.Count() + " in BuildFinalVerts()");

		foreach(object geomItem in geoms.First().geometry)
		{
			geometry	?geom	=geomItem as geometry;
			if(geom == null)
			{
				Console.WriteLine("Null geometry in BuildFinalVerts()!");
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
					List<int>	?vertCounts	=GetGeometryVertCount(geom, name);

					if(vertCounts == null || vertCounts.Count == 0)
					{
						Console.WriteLine("Empty geometry chunk in BuildFinalVerts()!");
						toNuke.Add(cnk);
						continue;
					}

					cnk.AddNormTexByPoly(posIdxs, norms, normIdxs,
						texCoords0, texIdxs0, texCoords1, texIdxs1,
						texCoords2, texIdxs2, texCoords3, texIdxs3,
						colors0, colIdxs0, colors1, colIdxs1,
						colors2, colIdxs2, colors3, colIdxs3,
						vertCounts, col0Stride, col1Stride, col2Stride, col3Stride,
						bFlipTri);

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
					cnk.BuildBuffers(bPos, bNorm, bBone,
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

	static COLLADA? DeSerializeCOLLADA(string path)
	{
		FileStream		fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
		XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

		COLLADA	?ret	=xs.Deserialize(fs) as COLLADA;

		fs.Close();

		return	ret;
	}

	static Matrix4x4 GetSceneNodeTransform(COLLADA colFile, MeshConverter chunk)
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
				matrix	?cmat	=n.Items[i] as matrix;

				Debug.Assert(cmat != null);

				matrixToMatrix(ref cmat, ref mat);
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.rotate)
			{
				rotate	?rot	=n.Items[i] as rotate;

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
				TargetableFloat3	?trans	=n.Items[i] as TargetableFloat3;

				if(trans != null)
				{
					Vector3	t	=Vector3.Zero;
					t.X	=trans.Values[0];
					t.Y	=trans.Values[1];
					t.Z	=trans.Values[2];

					mat	=Matrix4x4.CreateTranslation(t)
						* mat;
				}
			}
			else if(n.ItemsElementName[i] == ItemsChoiceType2.scale)
			{
				TargetableFloat3	?scl	=n.Items[i] as TargetableFloat3;

				if(scl != null)
				{
					Vector3	t	=Vector3.Zero;
					t.X	=scl.Values[0];
					t.Y	=scl.Values[1];
					t.Z	=scl.Values[2];

					mat	=Matrix4x4.CreateScale(t)
						* mat;
				}
			}
		}

		bool	bOK	=Matrix4x4.Decompose(mat,
				out key.mScale, out key.mRotation, out key.mPosition);
		
		Debug.Assert(bOK);

		return	key;
	}


	internal static node ?LookUpNode(library_visual_scenes lvs, string nodeID)
	{
		//find the node addressed
		node	?addressed	=null;
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


	internal static node ?LookUpNode(node n, string id)
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
			node	?ret	=LookUpNode(child, id);
			if(ret != null)
			{
				return	ret;
			}
		}
		return	null;
	}


	static void BakeSceneNodesIntoVerts(COLLADA				colladaFile,
										Skeleton			skel,
										List<MeshConverter>	chunks)
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
			if(sk == null)
			{
				continue;
			}

			string	skinSource	=sk.source1.Substring(1);

			foreach(node n in ctrlNodes)
			{
				string	nname	=GetNodeNameForInstanceController(n, cont.id);
				if(nname == "")
				{
					Console.WriteLine("Empty node name for instance controller: " + cont.id + "!");
					continue;
				}
				Matrix4x4	mat	=Matrix4x4.Identity;
				if(!skel.GetMatrixForBone(nname, out mat))
				{
					Console.WriteLine("Node: " + nname + " not found in skeleton!");
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


	static Anim BuildAnim(COLLADA colladaFile, Skeleton skel, library_visual_scenes lvs, string path)
	{
		//create useful anims
		List<SubAnim>	subs	=CreateSubAnims(colladaFile, skel);

		Anim	anm	=new Anim(subs);

		FixMultipleSkeletons(lvs, anm, skel);

		anm.SetBoneRefs(skel);
		anm.Name	=NameFromPath(path);

		return	anm;
	}


	static void CreateSkin(COLLADA				colladaFile,
							ref Skin?			skin,
							List<MeshConverter>	chunks,
							Skeleton			skel,
							float				scaleFactor)
	{
		IEnumerable<library_controllers>	lcs	=colladaFile.Items.OfType<library_controllers>();
		if(lcs.Count() <= 0)
		{
			Console.WriteLine("No library_controllers in CreateSkin()!");
			return;
		}

		//create or reuse a single master skin for the character's parts
		if(skin == null)
		{
			skin	=new Skin(scaleFactor);
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
				Console.WriteLine("Non identity bind pose in skin: " + sk.source1 + "");
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

			if(ma == null || na == null)
			{
				continue;
			}

			List<Matrix4x4>	mats	=GetMatrixListFromFA(ma);
			List<string>	bnames	=GetBoneNamesViaSID(na.Values, colladaFile);

			Debug.Assert(mats.Count == bnames.Count);

			//add to master list
			for(int i=0;i < mats.Count;i++)
			{
				string		bname	=bnames[i];
				Matrix4x4	ibp		=mats[i];
				int			idx		=skel.GetBoneIndex(bname);

				if(idx == -1)
				{
					Console.WriteLine("Warning!  No index in skeleton for bone: " + bname + "!");
					continue;
				}
				if(invBindPoses.ContainsKey(idx))
				{
					Console.WriteLine("Warning!  Duplicate bind pose for bone: " + bname + "!");
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
							Console.WriteLine("Warning!  Non matching bind pose for bone: " + bname + "!");
						}
					}
				}
				else
				{
					invBindPoses.Add(idx, ibp);
				}
			}
		}

		skin.SetBonePoses(invBindPoses);
	}


	static List<SubAnim> CreateSubAnims(COLLADA colladaFile, Skeleton skel)
	{
		//create useful anims
		List<SubAnim>	subs	=new List<SubAnim>();

		IEnumerable<library_visual_scenes>	lvs	=colladaFile.Items.OfType<library_visual_scenes>();
		if(lvs.Count() <= 0)
		{
			Console.WriteLine("No library_visual_scenes in CreateSubAnims()!");
			return	subs;
		}

		IEnumerable<library_animations>	anims	=colladaFile.Items.OfType<library_animations>();
		if(anims.Count() <= 0)
		{
			Console.WriteLine("No library_animations in CreateSubAnims()!");
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

			subs.AddRange(an.GetAnims(skel, lvs.First(), out partsUsed));
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
					string	?itemSid	=pi.GetValue(item, null) as string;
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

			Name_array	?na	=null;

			foreach(source src in sk.source)
			{
				if(src.id == jointSrc)
				{
					na	=src.Item as Name_array;
				}
			}

			if(na == null)
			{
				continue;
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
			node	?n	=LookUpNodeViaSID(lvs.First(), sid);

			if(n == null)
			{
				n	=LookUpNode(lvs.First(), sid);
			}
			
			Debug.Assert(n != null);

			boneNames.Add(n.name);
		}
		return	boneNames;
	}


	internal static node? LookUpNodeViaSID(library_visual_scenes lvs, string SID)
	{
		//find the node addressed
		node	?addressed	=null;
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


	internal static node? LookUpNodeViaSID(node n, string sid)
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
			node?	ret	=LookUpNodeViaSID(child, sid);
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

							node	?skelNode	=LookUpNodeViaSID(lvs, skelName);
							if(skelNode == null)
							{
								skelNode	=LookUpNode(lvs, skelName);
							}

							if(skelNode != null)
							{
								anm.FixDetatchedSkeleton(skel, skelNode.name);
							}
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


	static void BuildSkeleton(node n, List<string> namesInUse, out GSNode gsn)
	{
		gsn	=new GSNode();

		if(namesInUse.Contains(n.name))
		{
			Console.WriteLine("Warning!  Non unique bone name: " + n.name + "!");

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

			BuildSkeleton(child, namesInUse, out kid);

			gsn.AddChild(kid);
		}
	}


	static Skeleton BuildSkeleton(COLLADA colMesh)
	{
		Skeleton	ret	=new Skeleton();

		var	nodes	=from lvs in colMesh.Items.OfType<library_visual_scenes>().First().visual_scene
						from n in lvs.node select n;

		List<string>	namesInUse	=new List<string>();
		foreach(node n in nodes)
		{
			GSNode	gsnRoot	=new GSNode();

			BuildSkeleton(n, namesInUse, out gsnRoot);

			ret.AddRoot(gsnRoot);
		}

		ret.ComputeNameIndex();

		return	ret;
	}


	static bool AddVertexWeightsToChunks(COLLADA colladaFile, List<MeshConverter> chunks)
	{
		if(colladaFile.Items.OfType<library_controllers>().Count() <= 0)
		{
			return	false;
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
		return	true;
	}


	static List<int>? GetGeometryVertCount(geometry geom, string material)
	{
		List<int>	ret	=new List<int>();

		mesh	?msh	=geom.Item as mesh;
		if(msh == null || msh.Items == null)
		{
			return	null;
		}
		foreach(object polObj in msh.Items)
		{
			polygons	?polys	=polObj as polygons;
			polylist	?plist	=polObj as polylist;
			triangles	?tris	=polObj as triangles;

			if(polys == null && plist == null && tris == null)
			{
				Console.WriteLine("Unknown polygon type: " + polObj + " in mesh: " + geom.name + "!");
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
					Console.WriteLine(geom.name + ", null Items!");
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
					string	?pols	=polyObj as string;
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
				Console.WriteLine("Warning!  PolyLists are very untested at the moment!");
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
				Console.WriteLine("Warning!  Tris are very untested at the moment!");

				for(int i=0;i < (int)tris.count;i++)
				{
					ret.Add(3);
				}
			}
		}
		return	ret;
	}
	
	//this is for the standard unchanged export of z up and y forward
	static void SetSkinRootTransformBlenderRightHand(Character c)
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

		c.GetSkin().SetRootTransform(accum);
	}

	//this is for the standard unchanged export of z up and y forward
	static void SetSkinRootTransformBlenderLeftHand(Character c)
	{
		//need a couple rotations to go from blender which is:
		//z up, y forward, x right
		//to
		//z forward, y up, x right
		Matrix4x4	tiltYToForward	=Matrix4x4.CreateRotationX(-MathHelper.PiOver2);

		Plane	xPlane;

		xPlane.Normal	=Vector3.UnitX;
		xPlane.D		=0;

		Matrix4x4	accum	=Matrix4x4.CreateReflection(xPlane);

		accum	*=tiltYToForward;

//		KeyFrame.RightHandToLeft(ref accum);

		c.GetSkin().SetRootTransform(accum);
	}
}