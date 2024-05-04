using System.Numerics;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;
using MeshLib;
using MaterialLib;
using UtilityLib;

namespace ColladaConvert;

internal class ColladaData
{
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
				float	angle	=Mathery.ToRadians(rot.Values[3]);
				

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


	static Anim BuildAnim(COLLADA colladaFile, Skeleton skel, library_visual_scenes lvs, string path, EventHandler ?ePrint)
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
							ref Skin?			skin,
							List<MeshConverter>	chunks,
							Skeleton			skel,
							float				scaleFactor,
							EventHandler		?ePrint)
	{
		IEnumerable<library_controllers>	lcs	=colladaFile.Items.OfType<library_controllers>();
		if(lcs.Count() <= 0)
		{
			Misc.SafeInvoke(ePrint, "No library_controllers in CreateSkin()!\n");
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
					invBindPoses.Add(idx, ibp);
				}
			}
		}

		skin.SetBonePoses(invBindPoses);
	}


	static List<SubAnim> CreateSubAnims(COLLADA colladaFile, Skeleton skel, EventHandler ?ePrint)
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


	static void BuildSkeleton(node n, List<string> namesInUse, out GSNode gsn, EventHandler ?ePrint)
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


	static Skeleton BuildSkeleton(COLLADA colMesh, EventHandler ?ePrint)
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


	static List<int>? GetGeometryVertCount(geometry geom, string material, EventHandler ?ePrint)
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
}