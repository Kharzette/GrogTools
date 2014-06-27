using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using MeshLib;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

//ambiguous stuff
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;


namespace ColladaConvert
{
	public class MeshConverter
	{
		//keeps track of original pos index
		struct TrackedVert
		{
			public Vector3		Position0;
			public Half4		Normal0;
			public Color		BoneIndex;
			public Half4		BoneWeights;
			public Vector2		TexCoord0;
			public Vector2		TexCoord1;
			public Vector2		TexCoord2;
			public Vector2		TexCoord3;
			public Vector4		Color0;
			public Vector4		Color1;
			public Vector4		Color2;
			public Vector4		Color3;

			public int		mOriginalIndex;


			public static bool operator==(TrackedVert a, TrackedVert b)
			{
				return	(
					(a.BoneIndex == b.BoneIndex) &&
					(a.BoneWeights == b.BoneWeights) &&
					(a.Position0 == b.Position0) &&
					(a.Normal0 == b.Normal0) &&
					(a.TexCoord0 == b.TexCoord0) &&
					(a.TexCoord1 == b.TexCoord1) &&
					(a.TexCoord2 == b.TexCoord2) &&
					(a.TexCoord3 == b.TexCoord3) &&
					(a.Color0 == b.Color0) &&
					(a.Color1 == b.Color1) &&
					(a.Color2 == b.Color2) &&
					(a.Color3 == b.Color3) &&
					(a.mOriginalIndex == b.mOriginalIndex));
			}


			public static bool operator!=(TrackedVert a, TrackedVert b)
			{
				return	(
					(a.BoneIndex != b.BoneIndex) ||
					(a.BoneWeights != b.BoneWeights) ||
					(a.Position0 != b.Position0) ||
					(a.Normal0 != b.Normal0) ||
					(a.TexCoord0 != b.TexCoord0) ||
					(a.TexCoord1 != b.TexCoord1) ||
					(a.TexCoord2 != b.TexCoord2) ||
					(a.TexCoord3 != b.TexCoord3) ||
					(a.Color0 != b.Color0) ||
					(a.Color1 != b.Color1) ||
					(a.Color2 != b.Color2) ||
					(a.Color3 != b.Color3) ||
					(a.mOriginalIndex != b.mOriginalIndex));
			}


			public override bool Equals(object obj)
			{
				return	base.Equals(obj);
			}


			public override int GetHashCode()
			{
				return	base.GetHashCode();
			}
		}

		string			mName, mGeomName;
		TrackedVert		[]mBaseVerts;
		int				mNumBaseVerts;
		List<ushort>	mIndexList	=new List<ushort>();

		public string	mGeometryID;
		public int		mNumVerts, mNumTriangles, mVertSize;
		public int		mPartIndex;

		//the converted meshes
		EditorMesh	mConverted;


		public MeshConverter(string name, string geoName)
		{
			mName		=name;
			mGeomName	=geoName;
		}


		public Mesh	GetConvertedMesh()
		{
			return	mConverted;
		}


		public string GetName()
		{
			return	mName;
		}


		public string GetGeomName()
		{
			//strip off mesh, add name put mesh back on end
			string	ret	=mGeomName.Substring(0, mGeomName.Length - 4);

			ret	+=mName + "Mesh";

			return	ret;
		}


		//this will build a base list of verts
		//eventually these will need to expand
		public void CreateBaseVerts(float_array verts, bool bSkinned)
		{
			mNumBaseVerts	=(int)verts.count / 3;
			mBaseVerts		=new TrackedVert[mNumBaseVerts];

			for(int i=0;i < (int)verts.count;i+=3)
			{
				mBaseVerts[i / 3].Position0.X		=verts.Values[i];
				mBaseVerts[i / 3].Position0.Y		=verts.Values[i + 1];
				mBaseVerts[i / 3].Position0.Z		=verts.Values[i + 2];
				mBaseVerts[i / 3].mOriginalIndex	=i / 3;
			}

			//create a new meshlib mesh
			mConverted	=new EditorMesh(mName);
		}


		//this totally doesn't work at all
		public void EliminateDuplicateVerts()
		{
			//throw these in a list to make it easier
			//to throw some out
			List<TrackedVert>	verts	=new List<TrackedVert>();
			for(int i=0;i < mNumBaseVerts;i++)
			{
				verts.Add(mBaseVerts[i]);
			}

			restart:
			for(int i=0;i < verts.Count;i++)
			{
				for(int j=0;j < verts.Count;j++)
				{
					if(i == j)
					{
						continue;
					}
					if(verts[i] == verts[j])
					{
						verts.RemoveAt(j);

						//search through the polygon
						//index list to remove any instances
						//of j and replace them with i
						ReplaceIndex((ushort)j, (ushort)i);
						goto restart;
					}
				}
			}
		}


		public void BakeTransformIntoVerts(Matrix mat)
		{
			for(int i=0;i < mBaseVerts.Length;i++)
			{
				mBaseVerts[i].Position0	=
					Vector3.TransformCoordinate(mBaseVerts[i].Position0, mat);
			}
		}


		public void BakeTransformIntoNormals(Matrix mat)
		{
			for(int i=0;i < mBaseVerts.Length;i++)
			{
				Vector3	norm	=Vector3.Zero;

				norm.X	=mBaseVerts[i].Normal0.X;
				norm.Y	=mBaseVerts[i].Normal0.Y;
				norm.Z	=mBaseVerts[i].Normal0.Z;

				norm	=Vector3.TransformNormal(norm, mat);
				norm.Normalize();

				mBaseVerts[i].Normal0.X	=norm.X;
				mBaseVerts[i].Normal0.Y	=norm.Y;
				mBaseVerts[i].Normal0.Z	=norm.Z;
				mBaseVerts[i].Normal0.W	=1f;
			}
		}


		public void FlipNormals()
		{
			for(int i=0;i < mBaseVerts.Length;i++)
			{
				mBaseVerts[i].Normal0.X	=-mBaseVerts[i].Normal0.X;
				mBaseVerts[i].Normal0.Y	=-mBaseVerts[i].Normal0.Y;
				mBaseVerts[i].Normal0.Z	=-mBaseVerts[i].Normal0.Z;
				mBaseVerts[i].Normal0.W	=-mBaseVerts[i].Normal0.W;
			}
		}


		//fill baseverts with bone indices and weights
		internal void AddWeightsToBaseVerts(skin sk)
		{
			//break out vert weight counts
			List<int>	influenceCounts	=new List<int>();

			string[] tokens	=sk.vertex_weights.vcount.Split(' ','\n');

			//copy vertex weight counts
			foreach(string tok in tokens)
			{
				int numInfluences;

				if(int.TryParse(tok, out numInfluences))
				{
					influenceCounts.Add(numInfluences);
				}
			}

			//copy weight and bone indexes
			List<List<int>>	boneIndexes		=new List<List<int>>();
			List<List<int>>	weightIndexes	=new List<List<int>>();
			tokens	=sk.vertex_weights.v.Split(' ', '\n');

			int			curVert		=0;
			bool		bEven		=true;
			int			numInf		=0;
			List<int>	pvBone		=new List<int>();
			List<int>	pvWeight	=new List<int>();

			//copy float weights
			string	weightKey	="";
			foreach(InputLocalOffset ilo in sk.vertex_weights.input)
			{
				if(ilo.semantic == "WEIGHT")
				{
					weightKey	=ilo.source.Substring(1);
				}
			}
			float_array	weightArray	=null;
			foreach(source src in sk.source)
			{
				if(src.id != weightKey)
				{
					continue;
				}
				weightArray	=src.Item as float_array;
				if(weightArray == null)
				{
					continue;
				}
			}
			
			//copy vertex weight bones
			foreach(string tok in tokens)
			{
				int	val;

				if(int.TryParse(tok, out val))
				{
					if(bEven)
					{
						pvBone.Add(val);
					}
					else
					{
						pvWeight.Add(val);
						numInf++;
					}
					bEven	=!bEven;
					if(numInf >= influenceCounts[curVert])
					{
						boneIndexes.Add(pvBone);
						weightIndexes.Add(pvWeight);
						numInf		=0;
						pvBone		=new List<int>();
						pvWeight	=new List<int>();
						curVert++;
					}
				}
			}


			for(int i=0;i < mNumBaseVerts;i++)
			{
				int	numInfluences	=influenceCounts[i];

				//fix weights over 4
				List<int>	indexes	=new List<int>();
				List<float>	weights	=new List<float>();
				for(int j=0;j < numInfluences;j++)
				{
					//grab bone indices and weights
					int		boneIdx		=boneIndexes[i][j];
					int		weightIdx	=weightIndexes[i][j];
					float	boneWeight	=weightArray.Values[weightIdx];

					indexes.Add(boneIdx);
					weights.Add(boneWeight);
				}

				while(weights.Count > 4)
				{
					//find smallest weight
					float	smallest	=6969.69f;
					int		smIdx		=-1;
					for(int wt=0;wt < weights.Count;wt++)
					{
						if(weights[wt] < smallest)
						{
							smIdx		=wt;
							smallest	=weights[wt];
						}
					}

					//drop smallest weight
					weights.RemoveAt(smIdx);
					indexes.RemoveAt(smIdx);

					numInfluences--;

					//boost other weights by the amount
					//diminished by the loss of the
					//smallest weight
					float	boost	=smallest / weights.Count;

					for(int wt=0;wt < weights.Count;wt++)
					{
						weights[wt]	+=boost;
					}
				}

				Int4	index	=Int4.Zero;
				Half4	weight	=Vector4.Zero;
				for(int j=0;j < numInfluences;j++)
				{
					Debug.Assert(j < 4);

					//grab bone indices and weights
					int		boneIdx;
					float	boneWeight;

					boneIdx		=indexes[j];
					boneWeight	=weights[j];

					switch(j)
					{
						case	0:
							index.X		=boneIdx;
							weight.X	=boneWeight;
							break;
						case	1:
							index.Y		=boneIdx;
							weight.Y	=boneWeight;
							break;
						case	2:
							index.Z		=boneIdx;
							weight.Z	=boneWeight;
							break;
						case	3:
							index.W		=boneIdx;
							weight.W	=boneWeight;
							break;
					}
				}

				mBaseVerts[i].BoneIndex		=new Color((byte)index.X, (byte)index.Y, (byte)index.Z, (byte)index.W);
				mBaseVerts[i].BoneWeights	=weight;
			}
		}


		//this copies all pertinent per polygon information
		//into the trackedverts.  Every vert indexed by a
		//polygon will be duplicated as the normals and
		//texcoords can vary on a particular position in a mesh
		//depending on which polygon is being drawn.
		//This also constructs a list of indices
		internal void AddNormTexByPoly(List<int>		posIdxs,
									   float_array		norms,
									   List<int>		normIdxs,
									   float_array		texCoords0,
									   List<int>		texIdxs0,
									   float_array		texCoords1,
									   List<int>		texIdxs1,
									   float_array		texCoords2,
									   List<int>		texIdxs2,
									   float_array		texCoords3,
									   List<int>		texIdxs3,
									   float_array		colors0,
									   List<int>		colIdxs0,
									   float_array		colors1,
									   List<int>		colIdxs1,
									   float_array		colors2,
									   List<int>		colIdxs2,
									   float_array		colors3,
									   List<int>		colIdxs3,
									   List<int>		vertCounts,
									   int				col0Stride,
									   int				col1Stride,
									   int				col2Stride,
									   int				col3Stride)
		{
			//make sure there are at least positions and vertCounts
			if(posIdxs == null || vertCounts == null)
			{
				return;
			}

			List<TrackedVert>	verts	=new List<TrackedVert>();

			//track the polygon in use
			int	polyIndex	=0;
			int	curVert		=0;
			int	vCnt		=vertCounts[polyIndex];
			for(int i=0;i < posIdxs.Count;i++)
			{
				int	pidx, nidx;
				int	tidx0, tidx1, tidx2, tidx3;
				int	cidx0, cidx1, cidx2, cidx3;

				pidx	=posIdxs[i];
				nidx	=0;
				tidx0	=tidx1	=tidx2	=tidx3	=0;
				cidx0	=cidx1	=cidx2	=cidx3	=0;

				if(normIdxs != null && norms != null)
				{
					nidx	=normIdxs[i];
				}
				if(texIdxs0 != null && texCoords0 != null)
				{
					tidx0	=texIdxs0[i];
				}
				if(texIdxs1 != null && texCoords1 != null)
				{
					tidx1	=texIdxs1[i];
				}
				if(texIdxs2 != null && texCoords2 != null)
				{
					tidx2	=texIdxs2[i];
				}
				if(texIdxs3 != null && texCoords3 != null)
				{
					tidx3	=texIdxs3[i];
				}
				if(colIdxs0 != null && colors0 != null)
				{
					cidx0	=colIdxs0[i];
				}
				if(colIdxs1 != null && colors1 != null)
				{
					cidx1	=colIdxs1[i];
				}
				if(colIdxs2 != null && colors2 != null)
				{
					cidx2	=colIdxs2[i];
				}
				if(colIdxs3 != null && colors3 != null)
				{
					cidx3	=colIdxs3[i];
				}

				TrackedVert	tv	=new TrackedVert();

				//copy the basevertex, this will ensure we
				//get the right position and bone indexes
				//and vertex weights
				tv	=mBaseVerts[pidx];

				//copy normal if exists
				if(normIdxs != null && norms != null)
				{
					tv.Normal0.X	=norms.Values[nidx * 3];
					tv.Normal0.Y	=norms.Values[1 + nidx * 3];
					tv.Normal0.Z	=norms.Values[2 + nidx * 3];
				}
				//copy texcoords
				if(texIdxs0 != null && texCoords0 != null)
				{
					tv.TexCoord0.X	=texCoords0.Values[tidx0 * 2];
					tv.TexCoord0.Y	=-texCoords0.Values[1 + tidx0 * 2];
				}
				if(texIdxs1 != null && texCoords1 != null)
				{
					tv.TexCoord1.X	=texCoords1.Values[tidx1 * 2];
					tv.TexCoord1.Y	=-texCoords1.Values[1 + tidx1 * 2];
				}
				if(texIdxs2 != null && texCoords2 != null)
				{
					tv.TexCoord2.X	=texCoords2.Values[tidx2 * 2];
					tv.TexCoord2.Y	=-texCoords2.Values[1 + tidx2 * 2];
				}
				if(texIdxs3 != null && texCoords3 != null)
				{
					tv.TexCoord3.X	=texCoords3.Values[tidx3 * 2];
					tv.TexCoord3.Y	=-texCoords3.Values[1 + tidx3 * 2];
				}
				if(colIdxs0 != null && colors0 != null)
				{
					tv.Color0.X	=colors0.Values[cidx0 * col0Stride];
					tv.Color0.Y	=colors0.Values[1 + cidx0 * col0Stride];
					tv.Color0.Z	=colors0.Values[2 + cidx0 * col0Stride];
					if(col0Stride > 3)
					{
						tv.Color0.W	=colors0.Values[3 + cidx0 * col0Stride];
					}
					else
					{
						tv.Color0.W	=1.0f;
					}
				}
				if(colIdxs1 != null && colors1 != null)
				{
					tv.Color1.X	=colors1.Values[cidx1 * col1Stride];
					tv.Color1.Y	=colors1.Values[1 + cidx1 * col1Stride];
					tv.Color1.Z	=colors1.Values[2 + cidx1 * col1Stride];
					if(col1Stride > 3)
					{
						tv.Color1.W	=colors1.Values[3 + cidx1 * col1Stride];
					}
					else
					{
						tv.Color1.W	=1.0f;
					}
				}
				if(colIdxs2 != null && colors2 != null)
				{
					tv.Color2.X	=colors2.Values[cidx2 * col2Stride];
					tv.Color2.Y	=colors2.Values[1 + cidx2 * col2Stride];
					tv.Color2.Z	=colors2.Values[2 + cidx2 * col2Stride];
					if(col2Stride > 3)
					{
						tv.Color2.W	=colors2.Values[3 + cidx2 * col2Stride];
					}
					else
					{
						tv.Color2.W	=1.0f;
					}
				}
				if(colIdxs3 != null && colors3 != null)
				{
					tv.Color3.X	=colors3.Values[cidx3 * col3Stride];
					tv.Color3.Y	=colors3.Values[1 + cidx3 * col3Stride];
					tv.Color3.Z	=colors3.Values[2 + cidx3 * col3Stride];
					if(col3Stride > 3)
					{
						tv.Color3.W	=colors3.Values[3 + cidx3 * col3Stride];
					}
					else
					{
						tv.Color3.W	=1.0f;
					}
				}

				verts.Add(tv);
				mIndexList.Add((ushort)(verts.Count - 1));
				curVert++;

				if(curVert >= vCnt)
				{
					polyIndex++;
					if(polyIndex >= vertCounts.Count)
					{
						break;
					}
					vCnt	=vertCounts[polyIndex];
					curVert	=0;
				}
			}

			//dump verts back into baseverts
			mBaseVerts		=new TrackedVert[verts.Count];
			mNumBaseVerts	=verts.Count;
			for(int i=0;i < verts.Count;i++)
			{
				mBaseVerts[i]	=verts[i];
			}
			//EliminateDuplicateVerts();

			Triangulate(vertCounts);

			mNumVerts		=verts.Count;
			mNumTriangles	=mIndexList.Count / 3;
		}


		public void SetGeometryID(string id)
		{
			mGeometryID	=id;
		}


		void ReplaceIndex(ushort find, ushort replace)
		{
			for(int i=0;i < mIndexList.Count;i++)
			{
				if(mIndexList[i] == find)
				{
					mIndexList[i]	=replace;
				}
			}
		}


		void Triangulate(List<int> vertCounts)
		{
			List<ushort>	newIdxs	=new List<ushort>();

			int	curIdx	=0;
			for(int i=0;i < vertCounts.Count;i++)
			{
				//see how many verts in this polygon
				int	vCount	=vertCounts[i];

				for(int j=1;j < (vCount - 1);j++)
				{
					//backwards for new directx
					newIdxs.Add(mIndexList[j + 1 + curIdx]);
					newIdxs.Add(mIndexList[j + curIdx]);
					newIdxs.Add(mIndexList[curIdx]);
				}
				curIdx	+=vCount;
			}

			//dump back into regular list
			mIndexList.Clear();
			for(int i=newIdxs.Count - 1;i >= 0;i--)
			{
				mIndexList.Add(newIdxs[i]);
			}
		}


		//take the munged data and stuff it into
		//the vertex and index buffers
		public void BuildBuffers(Device gd,
			bool bPositions, bool bNormals, bool bBoneIndices,
			bool bBoneWeights, bool bTexCoord0, bool bTexCoord1,
			bool bTexCoord2, bool bTexCoord3, bool bColor0,
			bool bColor1, bool bColor2, bool bColor3)
		{
			int	numTex		=0;
			int	numColor	=0;

			if(bTexCoord0)	numTex++;
			if(bTexCoord1)	numTex++;
			if(bTexCoord2)	numTex++;
			if(bTexCoord3)	numTex++;
			if(bColor0)		numColor++;
			if(bColor1)		numColor++;
			if(bColor2)		numColor++;
			if(bColor3)		numColor++;
			Type vtype	=VertexTypes.GetMatch(bPositions, bNormals, bBoneIndices, bBoneWeights, false, false, numTex, numColor);

			Array	verts	=Array.CreateInstance(vtype, mNumBaseVerts);

			for(int i=0;i < mNumBaseVerts;i++)
			{
				if(bPositions)
				{
					VertexTypes.SetArrayField(verts, i, "Position", mBaseVerts[i].Position0);
				}
				if(bNormals)
				{
					VertexTypes.SetArrayField(verts, i, "Normal", mBaseVerts[i].Normal0);
				}
				if(bBoneIndices)
				{
					VertexTypes.SetArrayField(verts, i, "BoneIndex", mBaseVerts[i].BoneIndex);
				}
				if(bBoneWeights)
				{
					VertexTypes.SetArrayField(verts, i, "BoneWeights", mBaseVerts[i].BoneWeights);
				}
				if(bTexCoord0)
				{
					VertexTypes.SetArrayField(verts, i, "TexCoord0",
						new Half2(mBaseVerts[i].TexCoord0.X, mBaseVerts[i].TexCoord0.Y));
				}
				if(bTexCoord1)
				{
					VertexTypes.SetArrayField(verts, i, "TexCoord1",
						new Half2(mBaseVerts[i].TexCoord1.X, mBaseVerts[i].TexCoord1.Y));
				}
				if(bTexCoord2)
				{
					VertexTypes.SetArrayField(verts, i, "TexCoord2",
						new Half2(mBaseVerts[i].TexCoord2.X, mBaseVerts[i].TexCoord2.Y));
				}
				if(bTexCoord3)
				{
					VertexTypes.SetArrayField(verts, i, "TexCoord3",
						new Half2(mBaseVerts[i].TexCoord3.X, mBaseVerts[i].TexCoord3.Y));
				}
				if(bColor0)
				{
					VertexTypes.SetArrayField(verts, i, "Color0", new Color(mBaseVerts[i].Color0));
				}
				if(bColor1)
				{
					VertexTypes.SetArrayField(verts, i, "Color1", new Color(mBaseVerts[i].Color1));
				}
				if(bColor2)
				{
					VertexTypes.SetArrayField(verts, i, "Color2", new Color(mBaseVerts[i].Color2));
				}
				if(bColor3)
				{
					VertexTypes.SetArrayField(verts, i, "Color3", new Color(mBaseVerts[i].Color3));
				}
			}

			Buffer	vb	=VertexTypes.BuildABuffer(gd, verts, vtype);

			int	vertSize	=VertexTypes.GetSizeForType(vtype);

			mConverted.SetVertSize(vertSize);
			mConverted.SetNumVerts(mNumBaseVerts);
			mConverted.SetNumTriangles(mNumTriangles);
			mConverted.SetTypeIndex(VertexTypes.GetIndex(vtype));
			mConverted.SetVertexBuffer(vb);


			ushort	[]idxs	=new ushort[mIndexList.Count];

			for(int i=0;i < mIndexList.Count;i++)
			{
				idxs[i]	=mIndexList[i];
			}

			Buffer	inds	=VertexTypes.BuildAnIndexBuffer(gd, idxs);

			mConverted.SetIndexBuffer(inds);

			mConverted.SetData(verts, idxs);
		}


		//individual mesh parts index into a skin of bones
		//that might not match the overall character...
		//this will fix them so they do
		internal void FixBoneIndexes(Skeleton skel, List<string> bnames)
		{
			for(int i=0;i < mNumBaseVerts;i++)
			{
				Color	inds	=mBaseVerts[i].BoneIndex;

				int	idx0	=(int)inds.R;
				int	idx1	=(int)inds.G;
				int	idx2	=(int)inds.B;
				int	idx3	=(int)inds.A;

				Debug.Assert(idx0 >= 0);

				string	bname	=bnames[idx0];

				idx0	=skel.GetBoneIndex(bname);
				Debug.Assert(idx0 >= 0);

				bname	=bnames[idx1];
				idx1	=skel.GetBoneIndex(bname);
				Debug.Assert(idx1 >= 0);

				bname	=bnames[idx2];
				idx2	=skel.GetBoneIndex(bname);
				Debug.Assert(idx2 >= 0);

				bname	=bnames[idx3];
				idx3	=skel.GetBoneIndex(bname);
				Debug.Assert(idx3 >= 0);

				mBaseVerts[i].BoneIndex	=new Color((byte)idx0, (byte)idx1, (byte)idx2, (byte)idx3);
			}
		}
	}
}