using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MeshLib;


namespace ColladaConvert
{
	internal class ColladaFileUtils
	{
		internal static COLLADA DeSerializeCOLLADA(string path)
		{
			FileStream		fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
			XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

			COLLADA	ret	=xs.Deserialize(fs) as COLLADA;

			fs.Close();

			return	ret;
		}


		//loads an animation into an existing anim lib
		internal static bool LoadAnim(string path, AnimLib alib)
		{
			COLLADA	colladaFile	=DeSerializeCOLLADA(path);
			Skeleton	skel	=BuildSkeleton(colladaFile);

			//see if animlib has a skeleton yet
			if(alib.GetSkeleton() == null)
			{
				alib.SetSkeleton(skel);
			}
			else
			{
				//make sure they match
				if(!alib.CheckSkeleton(skel))
				{
					return	false;
				}
			}
			List<SubAnim>	subs	=GetSubAnimList(colladaFile, alib.GetSkeleton());
			Anim	anm	=new Anim(subs);

			anm.SetBoneRefs(alib.GetSkeleton());

			anm.Name	=NameFromPath(path);
			alib.AddAnim(anm);

			return	true;
		}


		internal static Character LoadCharacter(string					path,
												GraphicsDevice			gd,
												MaterialLib.MaterialLib	matLib,
												AnimLib					alib)
		{
			COLLADA	colladaFile	=DeSerializeCOLLADA(path);

			Character	chr	=new Character(matLib, alib);

			List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, true);

			AddVertexWeightsToChunks(colladaFile, chunks);

			//build skeleton
			Skeleton	skel	=BuildSkeleton(colladaFile);

			//bake scene node modifiers into controllers
			BakeSceneNodesIntoVerts(colladaFile, skel, chunks);

			alib.SetSkeleton(skel);

			BuildFinalVerts(colladaFile, gd, chunks);

			//create useful anims
			List<SubAnim>	subs	=GetSubAnimList(colladaFile, skel);
			Anim	anm	=new Anim(subs);

			//see if there are multiple skeletons
			IEnumerable<library_visual_scenes>	lvss	=colladaFile.Items.OfType<library_visual_scenes>();

			library_visual_scenes	lvs	=lvss.First();

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

			anm.SetBoneRefs(skel);
			anm.Name	=NameFromPath(path);
			alib.AddAnim(anm);

			CreateSkins(colladaFile, chr, chunks);

			foreach(MeshConverter mc in chunks)
			{
				chr.AddMeshPart(mc.GetConvertedMesh());
			}

			return	chr;
		}


		internal static StaticMeshObject LoadStatic(string					path,
													GraphicsDevice			gd,
													MaterialLib.MaterialLib	matLib,
													bool					bBake)
		{
			COLLADA	colladaFile	=DeSerializeCOLLADA(path);

			//don't have a way to test this
			Debug.Assert(colladaFile.asset.up_axis != UpAxisType.X_UP);

			StaticMeshObject	smo		=new StaticMeshObject(matLib);
			List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, false);

			if(bBake)
			{
				BakeSceneNodesIntoVerts(colladaFile, chunks);
				BuildFinalVerts(colladaFile, gd, chunks);

				foreach(MeshConverter mc in chunks)
				{
					Mesh	m	=mc.GetConvertedMesh();

					m.SetTransform(Matrix.Identity);

					smo.AddMeshPart(m);
				}
			}
			else
			{
				//adjust coordinate system
				Matrix	shiftMat	=Matrix.Identity;
				if(colladaFile.asset.up_axis == UpAxisType.Z_UP)
				{
					shiftMat	=Matrix.CreateRotationX(-MathHelper.PiOver2);
				}

				BuildFinalVerts(colladaFile, gd, chunks);
				foreach(MeshConverter mc in chunks)
				{
					Mesh	m	=mc.GetConvertedMesh();
					Matrix	mat	=GetSceneNodeTransform(colladaFile, mc);

					//set transform of each mesh
					m.SetTransform(mat * shiftMat);
					smo.AddMeshPart(m);
				}
			}

			return	smo;
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


		static void CreateSkins(COLLADA				colladaFile,
								Character			chr,
								List<MeshConverter>	chunks)
		{
			List<Skin>	skinList	=new List<Skin>();
			foreach(object item in colladaFile.Items)
			{
				library_controllers	conts	=item as library_controllers;
				if(conts == null)
				{
					continue;
				}
				foreach(controller cont in conts.controller)
				{
					skin	sk	=cont.Item as skin;
					if(sk == null)
					{
						continue;
					}
					string	skinSource	=sk.source1.Substring(1);
					if(skinSource == null || skinSource == "")
					{
						continue;
					}
					Skin	skin	=new Skin();

					Matrix	mat	=Matrix.Identity;

					GetMatrixFromString(sk.bind_shape_matrix, out mat);

					skin.SetBindShapeMatrix(mat);

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

					foreach(source src in sk.source)
					{
						if(src.id == jointSrc)
						{
							Name_array	na	=src.Item as Name_array;

							skin.SetBoneNames(GetBoneNamesViaSID(na.Values, colladaFile));
						}
						else if(src.id == invSrc)
						{
							float_array	ma	=src.Item as float_array;

							List<Matrix>	mats	=GetMatrixListFromFA(ma);

							skin.SetInverseBindPoses(mats);
						}
					}
					chr.AddSkin(skin);
					skinList.Add(skin);

					//set mesh pointers
					foreach(MeshConverter mc in chunks)
					{
						if(mc.mGeometryID == sk.source1.Substring(1))
						{
							SkinnedMesh	msh	=mc.GetConvertedMesh() as SkinnedMesh;
							msh.SetSkin(skin);
							msh.SetSkinIndex(skinList.IndexOf(skin));
						}
					}
				}
			}
		}


		static List<SubAnim> GetSubAnimList(COLLADA colladaFile, Skeleton skel)
		{
			//create useful anims
			List<SubAnim>	subs	=new List<SubAnim>();

			IEnumerable<library_visual_scenes>	lvs	=colladaFile.Items.OfType<library_visual_scenes>();
			if(lvs.Count() <= 0)
			{
				return	subs;
			}

			IEnumerable<library_animations>	anims	=colladaFile.Items.OfType<library_animations>();
			if(anims.Count() <= 0)
			{
				return	subs;
			}

			List<Animation.KeyPartsUsed>	partsUsed	=new List<Animation.KeyPartsUsed>();
			foreach(animation anim in anims.First().animation)
			{
				Animation	an	=new Animation(anim);

				Animation.KeyPartsUsed	parts;

				SubAnim	sa	=an.GetAnims(skel, lvs.First(), out parts);
				if(sa != null)
				{
					subs.Add(sa);
					partsUsed.Add(parts);
				}
			}

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
						combineParts.Add(partsUsed[i]);
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
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateX))
			{
				key.mPosition.X	=boneKey.mPosition.X;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateY))
			{
				key.mPosition.Y	=boneKey.mPosition.Y;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.TranslateZ))
			{
				key.mPosition.Z	=boneKey.mPosition.Z;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleX))
			{
				key.mScale.X	=boneKey.mScale.X;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleY))
			{
				key.mScale.Y	=boneKey.mScale.Y;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.ScaleZ))
			{
				key.mScale.Z	=boneKey.mScale.Z;
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateX))
			{
				key.mRotation	=Quaternion.Concatenate(key.mRotation, boneKey.mRotation);
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateY))
			{
				key.mRotation	=Quaternion.Concatenate(key.mRotation, boneKey.mRotation);
			}
			if(!UtilityLib.Misc.bFlagSet((UInt32)keyPartsUsed, (UInt32)Animation.KeyPartsUsed.RotateZ))
			{
				key.mRotation	=Quaternion.Concatenate(key.mRotation, boneKey.mRotation);
			}
		}


		static void MergeKeys(KeyFrame []first, KeyFrame []next, Animation.KeyPartsUsed nextParts)
		{
			Debug.Assert(first.Length == next.Length);

			for(int i=0;i < first.Length;i++)
			{
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateX))
				{
					first[i].mPosition.X	=next[i].mPosition.X;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateY))
				{
					first[i].mPosition.Y	=next[i].mPosition.Y;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.TranslateZ))
				{
					first[i].mPosition.Z	=next[i].mPosition.Z;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleX))
				{
					first[i].mScale.X	=next[i].mScale.X;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleY))
				{
					first[i].mScale.Y	=next[i].mScale.Y;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.ScaleZ))
				{
					first[i].mScale.Z	=next[i].mScale.Z;
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateX))
				{
					first[i].mRotation	=Quaternion.Concatenate(next[i].mRotation, first[i].mRotation);
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateY))
				{
					first[i].mRotation	=Quaternion.Concatenate(next[i].mRotation, first[i].mRotation);
				}
				if(UtilityLib.Misc.bFlagSet((UInt32)nextParts, (UInt32)Animation.KeyPartsUsed.RotateZ))
				{
					first[i].mRotation	=Quaternion.Concatenate(next[i].mRotation, first[i].mRotation);
				}
			}
		}


		static void BuildFinalVerts(COLLADA				colladaFile,
									GraphicsDevice		gd,
									List<MeshConverter>	chunks)
		{
			foreach(object item in colladaFile.Items)
			{
				library_geometries	geoms	=item as library_geometries;
				if(geoms == null)
				{
					continue;
				}
				foreach(object geomItem in geoms.geometry)
				{
					geometry	geom	=geomItem as geometry;
					if(geom == null)
					{
						continue;
					}

					//blast any chunks with no verts (happens with max collada)
					List<MeshConverter>	toNuke	=new List<MeshConverter>();

					foreach(MeshConverter cnk in chunks)
					{
						string	name	=cnk.GetName();
						if(cnk.mGeometryID == geom.id)
						{
							List<int>	posIdxs		=GetGeometryIndexesBySemantic(geom, "VERTEX", 0, name);
							float_array	norms		=GetGeometryFloatArrayBySemantic(geom, "NORMAL", 0, name);
							List<int>	normIdxs	=GetGeometryIndexesBySemantic(geom, "NORMAL", 0, name);
							float_array	texCoords0	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 0, name);
							float_array	texCoords1	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 1, name);
							float_array	texCoords2	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 2, name);
							float_array	texCoords3	=GetGeometryFloatArrayBySemantic(geom, "TEXCOORD", 3, name);
							List<int>	texIdxs0	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 0, name);
							List<int>	texIdxs1	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 1, name);
							List<int>	texIdxs2	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 2, name);
							List<int>	texIdxs3	=GetGeometryIndexesBySemantic(geom, "TEXCOORD", 3, name);
							float_array	colors0		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 0, name);
							float_array	colors1		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 1, name);
							float_array	colors2		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 2, name);
							float_array	colors3		=GetGeometryFloatArrayBySemantic(geom, "COLOR", 3, name);
							List<int>	colIdxs0	=GetGeometryIndexesBySemantic(geom, "COLOR", 0, name);
							List<int>	colIdxs1	=GetGeometryIndexesBySemantic(geom, "COLOR", 1, name);
							List<int>	colIdxs2	=GetGeometryIndexesBySemantic(geom, "COLOR", 2, name);
							List<int>	colIdxs3	=GetGeometryIndexesBySemantic(geom, "COLOR", 3, name);
							List<int>	vertCounts	=GetGeometryVertCount(geom, name);

							if(vertCounts.Count == 0)
							{
								toNuke.Add(cnk);
								continue;
							}

							cnk.AddNormTexByPoly(posIdxs, norms, normIdxs,
								texCoords0, texIdxs0, texCoords1, texIdxs1,
								texCoords2, texIdxs2, texCoords3, texIdxs3,
								colors0, colIdxs0, colors1, colIdxs1,
								colors2, colIdxs2, colors3, colIdxs3,
								vertCounts);

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
							foreach(object itm in colladaFile.Items)
							{
								library_controllers	conts	=itm as library_controllers;
								if(conts == null)
								{
									continue;
								}
								foreach(controller cont in conts.controller)
								{
									skin	sk	=cont.Item as skin;
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
		}


		static Matrix GetSceneNodeTransform(COLLADA colFile, MeshConverter chunk)
		{
			geometry	g	=GetGeometryByID(colFile, chunk.mGeometryID);
			if(g == null)
			{
				return	Matrix.Identity;
			}

			foreach(object item2 in colFile.Items)
			{
				library_visual_scenes	lvs	=item2 as library_visual_scenes;
				if(lvs == null)
				{
					continue;
				}
				foreach(visual_scene vs in lvs.visual_scene)
				{
					foreach(node n in vs.node)
					{
						if(n.instance_geometry == null)
						{
							continue;
						}
						foreach(instance_geometry ig in n.instance_geometry)
						{
							if(ig.url.Substring(1) == g.id)
							{
								if(!CNodeHasKeyData(n))
								{
									continue;
								}
								KeyFrame	kf	=GetKeyFromCNode(n);

								Matrix	mat	=Matrix.CreateScale(kf.mScale) *
									Matrix.CreateFromQuaternion(kf.mRotation) *
									Matrix.CreateTranslation(kf.mPosition);
									
								return	mat;
							}
						}
					}
				}
			}
			return	Matrix.Identity;
		}


		static void BakeSceneNodesIntoVerts(COLLADA				colladaFile,
											List<MeshConverter>	chunks)
		{
			//bake scene node modifiers into controllers
			foreach(MeshConverter mc in chunks)
			{
				Matrix	mat	=GetSceneNodeTransform(colladaFile, mc);
				mc.BakeTransformIntoVerts(mat);
			}
		}


		static void BakeSceneNodesIntoVerts(COLLADA				colladaFile,
											Skeleton			skel,
											List<MeshConverter>	chunks)
		{
			//bake scene node modifiers into controllers
			foreach(object item in colladaFile.Items)
			{
				library_controllers	conts	=item as library_controllers;
				if(conts == null)
				{
					continue;
				}
				foreach(controller cont in conts.controller)
				{
					skin	sk	=cont.Item as skin;
					if(sk == null)
					{
						continue;
					}
					string	skinSource	=sk.source1.Substring(1);
					if(skinSource == null || skinSource == "")
					{
						continue;
					}					

					foreach(object item2 in colladaFile.Items)
					{
						library_visual_scenes	lvs	=item2 as library_visual_scenes;
						if(lvs == null)
						{
							continue;
						}
						foreach(visual_scene vs in lvs.visual_scene)
						{
							foreach(node n in vs.node)
							{
								string	nname	=GetNodeNameForInstanceController(n, cont.id);
								if(nname == "")
								{
									continue;
								}
								Matrix	mat	=Matrix.Identity;
								if(!skel.GetMatrixForBone(nname, out mat))
								{
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
				}
			}
		}


		static void AddVertexWeightsToChunks(COLLADA colladaFile, List<MeshConverter> chunks)
		{
			foreach(object item in colladaFile.Items)
			{
				library_controllers	conts	=item as library_controllers;
				if(conts == null)
				{
					continue;
				}
				foreach(controller cont in conts.controller)
				{
					skin	sk	=cont.Item as skin;
					if(sk == null)
					{
						continue;
					}

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
		}


		static List<MeshConverter> GetMeshChunks(COLLADA colladaFile, bool bSkinned)
		{
			List<MeshConverter>	chunks	=new List<MeshConverter>();

			foreach(object item in colladaFile.Items)
			{
				library_geometries	geoms	=item as library_geometries;
				if(geoms == null)
				{
					continue;
				}
				foreach(object geomItem in geoms.geometry)
				{
					geometry	geom	=geomItem as geometry;
					if(geom == null || geom.Item == null)
					{
						continue;
					}

					mesh	msh	=geom.Item as mesh;
					if(msh == null || msh.Items == null)
					{
						continue;
					}

					foreach(object polyObj in msh.Items)
					{
						polygons	polys	=polyObj as polygons;
						polylist	plist	=polyObj as polylist;
						triangles	tris	=polyObj as triangles;

						if(polys == null && plist == null && tris == null)
						{
							continue;
						}

						string	mat	=null;
						if(polys != null)
						{
							mat	=polys.material;
						}
						else if(plist != null)
						{
							mat	=plist.material;
						}
						else if(tris != null)
						{
							mat	=tris.material;
						}

						float_array		verts	=null;
						MeshConverter	cnk		=null;

						verts	=GetGeometryFloatArrayBySemantic(geom, "VERTEX", 0, mat);
						if(verts == null)
						{
							continue;
						}
						
						cnk	=new MeshConverter(mat, geom.name);

						cnk.CreateBaseVerts(verts, bSkinned);

						cnk.mPartIndex	=-1;
						cnk.SetGeometryID(geom.id);
						
						chunks.Add(cnk);
					}
				}
			}
			return	chunks;
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


		static List<int> GetGeometryIndexesBySemantic(geometry geom, string sem, int set, string material)
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
					continue;
				}

				if(polys != null && polys.Items != null)
				{
					foreach(object polyObj in polys.Items)
					{
						string	pols	=polyObj as string;
						Debug.Assert(pols != null);

						int		numSem		=polys.input.Length;
						string	[]tokens	=pols.Split(' ', '\n');
						ParseIndexes(tokens, ofs, numSem, ret);
					}
				}
				else if(plist != null)
				{
					int		numSem		=plist.input.Length;
					string	[]tokens	=plist.p.Split(' ', '\n');
					ParseIndexes(tokens, ofs, numSem, ret);
				}
				else if(tris != null)
				{
					int		numSem		=tris.input.Length;
					string	[]tokens	=tris.p.Split(' ', '\n');
					ParseIndexes(tokens, ofs, numSem, ret);
				}
			}
			return	ret;
		}


		static void ParseIndexes(string []tokens, int offset, int numSemantics, List<int> indexes)
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
				if(curIdx >= numSemantics)
				{
					curIdx	=0;
				}
			}
		}


		static List<int> GetGeometryVertCount(geometry geom, string material)
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
					continue;
				}

				if(polys != null)
				{
					if(polys.material != material || polys.Items == null)
					{
						continue;
					}
					foreach(object polyObj in polys.Items)
					{
						string	pols	=polyObj as string;
						Debug.Assert(pols != null);

						int	numSem	=polys.input.Length;

						string	[]tokens	=pols.Split(' ', '\n');
						ret.Add(tokens.Length / numSem);
					}
				}
				else if(plist != null)
				{
					if(plist.material != material)
					{
						continue;
					}
					string	[]tokens	=plist.vcount.Split(' ', '\n');

					int	numSem	=plist.input.Length;
					foreach(string tok in tokens)
					{
						int	vertCount;
						
						bool	bGood	=Int32.TryParse(tok, out vertCount);

						Debug.Assert(bGood);

						ret.Add(vertCount);
					}
				}
				else if(tris != null)
				{
					if(tris.material != material)
					{
						continue;
					}

					for(int i=0;i < (int)tris.count;i++)
					{
						ret.Add(3);
					}
				}
			}
			return	ret;
		}


		static float_array GetGeometryFloatArrayBySemantic(geometry geom, string sem, int set, string material)
		{
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
					continue;
				}

				InputLocalOffset	[]inputs	=null;

				if(polys != null)
				{
					inputs	=polys.input;
				}
				else if(plist != null)
				{
					inputs	=plist.input;
				}
				else if(tris != null)
				{
					inputs	=tris.input;
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
				return	verts;
			}

			return	null;
		}


		static Skeleton BuildSkeleton(COLLADA colMesh)
		{
			Skeleton	ret	=new Skeleton();

			foreach(object item2 in colMesh.Items)
			{
				library_visual_scenes	lvs	=item2 as library_visual_scenes;
				if(lvs == null)
				{
					continue;
				}
				foreach(visual_scene vs in lvs.visual_scene)
				{
					foreach(node n in vs.node)
					{
						GSNode	gsnRoot	=new GSNode();

						BuildSkeleton(n, out gsnRoot);

						ret.AddRoot(gsnRoot);
					}
				}
			}
			return	ret;
		}


		static bool CNodeHasKeyData(node n)
		{
			if(n.Items == null)
			{
				return	false;
			}

			Matrix	mat	=Matrix.Identity;
			for(int i=0;i < n.Items.Length;i++)
			{
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


		static KeyFrame GetKeyFromCNode(node n)
		{
			KeyFrame	key	=new KeyFrame();

			if(n.Items == null)
			{
				return	key;
			}

			Matrix	mat	=Matrix.Identity;
			for(int i=0;i < n.Items.Length;i++)
			{
				if(n.ItemsElementName[i] == ItemsChoiceType2.rotate)
				{
					rotate	rot	=n.Items[i] as rotate;

					Debug.Assert(rot != null);

					Vector3	axis	=Vector3.Zero;
					axis.X			=rot.Values[0];
					axis.Y			=rot.Values[1];
					axis.Z			=rot.Values[2];
					float	angle	=MathHelper.ToRadians(rot.Values[3]);

					mat	=Matrix.CreateFromAxisAngle(axis, angle)
						* mat;
				}
				else if(n.ItemsElementName[i] == ItemsChoiceType2.translate)
				{
					TargetableFloat3	trans	=n.Items[i] as TargetableFloat3;

					Vector3	t	=Vector3.Zero;
					t.X	=trans.Values[0];
					t.Y	=trans.Values[1];
					t.Z	=trans.Values[2];

					mat	=Matrix.CreateTranslation(t)
						* mat;
				}
				else if(n.ItemsElementName[i] == ItemsChoiceType2.scale)
				{
					TargetableFloat3	scl	=n.Items[i] as TargetableFloat3;

					Vector3	t	=Vector3.Zero;
					t.X	=scl.Values[0];
					t.Y	=scl.Values[1];
					t.Z	=scl.Values[2];

					mat	=Matrix.CreateScale(t)
						* mat;
				}
			}

			mat.Decompose(out key.mScale, out key.mRotation, out key.mPosition);

			return	key;
		}


		static void BuildSkeleton(node n, out GSNode gsn)
		{
			gsn	=new GSNode();

			gsn.SetName(n.name);

			KeyFrame	kf	=GetKeyFromCNode(n);

			gsn.SetKey(kf);

			if(n.node1 == null)
			{
				return;
			}

			foreach(node child in n.node1)
			{
				GSNode	kid	=new GSNode();

				BuildSkeleton(child, out kid);

				gsn.AddChild(kid);
			}
		}


		static geometry GetGeometryByID(COLLADA colladaFile, string id)
		{
			foreach(object item in colladaFile.Items)
			{
				library_geometries	geoms	=item as library_geometries;
				if(geoms == null)
				{
					continue;
				}
				foreach(object geomItem in geoms.geometry)
				{
					geometry	geom	=geomItem as geometry;
					if(geom == null)
					{
						continue;
					}
					if(geom.id == id)
					{
						return	geom;
					}
				}
			}
			return	null;
		}


		internal static List<Matrix> GetMatrixListFromFA(float_array fa)
		{
			List<Matrix>	ret	=new List<Matrix>();

			Debug.Assert(fa.count % 16 == 0);

			for(int i=0;i < (int)fa.count;i+=16)
			{
				Matrix	mat	=new Matrix();

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


		internal static List<Matrix> GetMatrixListFromFloatList(List<float> fa)
		{
			List<Matrix>	ret	=new List<Matrix>();

			Debug.Assert(fa.Count % 16 == 0);

			for(int i=0;i < (int)fa.Count;i+=16)
			{
				Matrix	mat	=new Matrix();

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


		internal static void GetMatrixFromString(string str, out Matrix mat)
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


		internal static int GetNodeItemIndex(node n, string sid)
		{
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
	}
}