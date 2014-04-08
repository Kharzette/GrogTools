using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MeshLib;
using UtilityLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

//ambiguous stuff
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;


namespace ColladaStartSmall
{
	public partial class StartSmall : Form
	{
		//file dialog
		OpenFileDialog	mOFD	=new OpenFileDialog();
		SaveFileDialog	mSFD	=new SaveFileDialog();

		//graphics device
		Device	mGD;

		StaticMesh	mStatic;


		public StartSmall(Device gd)
		{
			InitializeComponent();

			mGD	=gd;
		}

		internal COLLADA DeSerializeCOLLADA(string path)
		{
			FileStream		fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
			XmlSerializer	xs	=new XmlSerializer(typeof(COLLADA));

			COLLADA	ret	=xs.Deserialize(fs) as COLLADA;

			fs.Close();

			return	ret;
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

			mStatic	=LoadStatic(mOFD.FileName);
		}


		internal StaticMesh LoadStatic(string path)
		{
			COLLADA	colladaFile	=DeSerializeCOLLADA(path);

			//don't have a way to test this
			Debug.Assert(colladaFile.asset.up_axis != UpAxisType.X_UP);

			StaticMesh			smo		=new StaticMesh();
			List<MeshConverter>	chunks	=GetMeshChunks(colladaFile, false);

			//adjust coordinate system
			Matrix	shiftMat	=Matrix.Identity;
			if(colladaFile.asset.up_axis == UpAxisType.Z_UP)
			{
				shiftMat	=Matrix.RotationX(-MathUtil.PiOverTwo);
			}

			smo.SetTransform(shiftMat);

			BuildFinalVerts(mGD, colladaFile, chunks);
			foreach(MeshConverter mc in chunks)
			{
				Mesh	m	=mc.GetConvertedMesh();
				Matrix	mat	=GetSceneNodeTransform(colladaFile, mc);

				m.Name	=mc.GetGeomName();

				//set transform of each mesh
				m.SetTransform(mat);
				smo.AddMeshPart(m);

				//temp
				m.Visible	=true;
			}
			return	smo;
		}


		void BuildFinalVerts(Device gd, COLLADA colladaFile, List<MeshConverter> chunks)
		{
			IEnumerable<library_geometries>		geoms	=colladaFile.Items.OfType<library_geometries>();
			IEnumerable<library_controllers>	conts	=colladaFile.Items.OfType<library_controllers>();

			Debug.Assert(geoms.Count() == 1);

			foreach(object geomItem in geoms.First().geometry)
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


		List<MeshConverter> GetMeshChunks(COLLADA colladaFile, bool bSkinned)
		{
			List<MeshConverter>	chunks	=new List<MeshConverter>();

			var	geoms	=from g in colladaFile.Items.OfType<library_geometries>().First().geometry
						 where g.Item is mesh select g;

			var	polyObjs	=from g in geoms
							 let m = g.Item as mesh
							 from pols in m.Items
							 select pols;

			foreach(geometry geom in geoms)
			{
				mesh	m	=geom.Item as mesh;

				foreach(object polyObj in m.Items)
				{
					polygons	polys	=polyObj as polygons;
					polylist	plist	=polyObj as polylist;
					triangles	tris	=polyObj as triangles;

					if(polys == null && plist == null && tris == null)
					{
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
						continue;
					}

					float_array		verts	=null;
					MeshConverter	cnk		=null;
					int				stride	=0;

					verts	=GetGeometryFloatArrayBySemantic(geom, "VERTEX", 0, mat, out stride);
					if(verts == null)
					{
						continue;
					}

					Debug.Assert(mat != null);

					if(mat == null)
					{
						//return an empty list
						return	new List<MeshConverter>();
					}

					cnk	=new MeshConverter(mat, geom.name);

					cnk.CreateBaseVerts(verts, bSkinned);

					cnk.mPartIndex	=-1;
					cnk.SetGeometryID(geom.id);
						
					chunks.Add(cnk);
				}
			}
			return	chunks;
		}


		void ParseIndexes(string []tokens, int offset, int numSemantics, List<int> indexes)
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
					float	angle	=MathUtil.DegreesToRadians(rot.Values[3]);

					mat	=Matrix.RotationAxis(axis, angle)
						* mat;
				}
				else if(n.ItemsElementName[i] == ItemsChoiceType2.translate)
				{
					TargetableFloat3	trans	=n.Items[i] as TargetableFloat3;

					Vector3	t	=Vector3.Zero;
					t.X	=trans.Values[0];
					t.Y	=trans.Values[1];
					t.Z	=trans.Values[2];

					mat	=Matrix.Translation(t)
						* mat;
				}
				else if(n.ItemsElementName[i] == ItemsChoiceType2.scale)
				{
					TargetableFloat3	scl	=n.Items[i] as TargetableFloat3;

					Vector3	t	=Vector3.Zero;
					t.X	=scl.Values[0];
					t.Y	=scl.Values[1];
					t.Z	=scl.Values[2];

					mat	=Matrix.Scaling(t)
						* mat;
				}
			}

			mat.Decompose(out key.mScale, out key.mRotation, out key.mPosition);

			return	key;
		}


		bool CNodeHasKeyData(node n)
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


		Matrix GetSceneNodeTransform(COLLADA colFile, MeshConverter chunk)
		{
			geometry	g	=GetGeometryByID(colFile, chunk.mGeometryID);
			if(g == null)
			{
				return	Matrix.Identity;
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
							continue;
						}
						KeyFrame	kf	=GetKeyFromCNode(n);

						Matrix	mat	=Matrix.Scaling(kf.mScale) *
							Matrix.RotationQuaternion(kf.mRotation) *
							Matrix.Translation(kf.mPosition);
									
						return	mat;
					}
				}
			}
			return	Matrix.Identity;
		}


		internal void Render(DeviceContext dc, EffectPass pass, EffectMatrixVariable fxWorld)
		{
			if(mStatic == null)
			{
				return;
			}

			mStatic.TempDraw(dc, pass, fxWorld);
		}
	}
}
