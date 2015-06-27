using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MaterialLib;
using UtilityLib;
using TerrainLib;


namespace TerrainEdit
{
	internal partial class TerrainAtlas : Form
	{
		GraphicsDevice	mGD;
		StuffKeeper		mSK;
		TexAtlas		mAtlas;

		BindingList<HeightMap.TexData>	mGridData	=new BindingList<HeightMap.TexData>();

		internal event EventHandler	eReBuild;


		internal TerrainAtlas(GraphicsDevice gd, StuffKeeper sk)
		{
			mGD	=gd;
			mSK	=sk;

			InitializeComponent();

			AtlasGrid.DataSource	=mGridData;

			//initial guess at data
			AutoFill();

			AtlasX.DataBindings.Add(new Binding("Value",
				Settings.Default, "AtlasX", true,
				DataSourceUpdateMode.OnPropertyChanged));
			AtlasY.DataBindings.Add(new Binding("Value",
				Settings.Default, "AtlasY", true,
				DataSourceUpdateMode.OnPropertyChanged));
			TransitionHeight.DataBindings.Add(new Binding("Value",
				Settings.Default, "TransitionHeight", true,
				DataSourceUpdateMode.OnPropertyChanged));
		}


		internal void FreeAll()
		{
			if(mAtlas != null)
			{
				mAtlas.FreeAll();
			}
		}


		void AutoFill()
		{
			//load last time's values
			if(LoadValues())
			{
				return;
			}

			//if no saved stuff, just fill in the textures
            List<string>    textures    =mSK.GetTexture2DList();

            foreach(string tex in textures)
            {
                if(tex.StartsWith("Terrain\\"))
                {
                    HeightMap.TexData    gd    =new HeightMap.TexData();

                    gd.TextureName    =tex;

                    mGridData.Add(gd);
                }
            }
		}


		void RebuildImage()
		{
			if(mAtlas != null)
			{
				mAtlas.FreeAll();
			}

			mAtlas	=new TexAtlas(mGD, (int)AtlasX.Value, (int)AtlasY.Value);

			bool	bAllWorked	=true;

			List<string>	textures	=mSK.GetTexture2DList();
			for(int i=0;i < mGridData.Count;i++)
			{
				HeightMap.TexData	gd	=mGridData[i];
				if(textures.Contains(gd.TextureName))
				{
					if(!mSK.AddTexToAtlas(mAtlas, gd.TextureName, mGD,
						out gd.mScaleU, out gd.mScaleV, out gd.mUOffs, out gd.mVOffs))
					{
						bAllWorked	=false;
						break;
					}
				}
			}

			if(!bAllWorked)
			{
				return;
			}

			AtlasPic01.Image	=mAtlas.GetAtlasImage(mGD.DC);

			mAtlas.Finish(mGD);
		}


		void OnReBuildAtlas(object sender, EventArgs e)
		{
			SaveValues();

			RebuildImage();

			Misc.SafeInvoke(eReBuild, mAtlas, new ListEventArgs<HeightMap.TexData>(mGridData.ToList()));
		}


		internal float GetTransitionHeight()
		{
			return	(float)TransitionHeight.Value;
		}

		internal void SaveAtlasInfo(string path)
		{
			if(mAtlas == null)
			{
				return;
			}

			string	noExt	=FileUtil.StripExtension(path);

			string	fname	=noExt + ".TerTexData";

			FileStream	fs	=new FileStream(fname, FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				//bummer?
				return;
			}

			BinaryWriter	bw	=new BinaryWriter(fs);

			//secret code for atlas stuffs
			UInt32	magic	=0x7EC5DA7A;
			bw.Write(magic);

			//atlas size
			bw.Write(mAtlas.Width);
			bw.Write(mAtlas.Height);

			//transition height
			bw.Write((int)TransitionHeight.Value);

			bw.Write(mGridData.Count);

			foreach(HeightMap.TexData td in mGridData)
			{
				bw.Write(td.BottomElevation);
				bw.Write(td.TopElevation);
				bw.Write(td.Steep);
				bw.Write(td.ScaleFactor);
				bw.Write(td.TextureName);

				bw.Write(td.mScaleU);
				bw.Write(td.mScaleV);
				bw.Write(td.mUOffs);
				bw.Write(td.mVOffs);
			}

			bw.Close();
			fs.Close();
		}

		internal void LoadAtlasInfo(string path)
		{
			string	noExt	=FileUtil.StripExtension(path);

			string	fname	=noExt + ".TerTexData";

			FileStream	fs	=new FileStream(fname, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				//bummer?
				return;
			}

			BinaryReader	br	=new BinaryReader(fs);

			UInt32	magic	=br.ReadUInt32();
			if(magic != 0x7EC5DA7A)
			{
				br.Close();
				fs.Close();
				return;
			}

			AtlasX.Value	=br.ReadInt32();
			AtlasY.Value	=br.ReadInt32();

			TransitionHeight.Value	=br.ReadInt32();

			//load into a temp list first
			List<HeightMap.TexData>	gridStuffs	=new List<HeightMap.TexData>();

			int	count	=br.ReadInt32();

			for(int i=0;i < count;i++)
			{
				HeightMap.TexData	td	=new HeightMap.TexData();

				td.BottomElevation	=br.ReadSingle();
				td.TopElevation		=br.ReadSingle();
				td.Steep			=br.ReadBoolean();
				td.ScaleFactor		=br.ReadSingle();
				td.TextureName		=br.ReadString();
				td.mScaleU			=br.ReadDouble();
				td.mScaleV			=br.ReadDouble();
				td.mUOffs			=br.ReadDouble();
				td.mVOffs			=br.ReadDouble();

				gridStuffs.Add(td);
			}

			br.Close();
			fs.Close();

			//use data that matches existing stuffs
			List<string>	textures	=mSK.GetTexture2DList();
			foreach(HeightMap.TexData td in gridStuffs)
			{
				if(textures.Contains(td.TextureName))
				{
					mGridData.Add(td);
				}
			}

			gridStuffs.Clear();
		}


		void SaveValues()
		{
			FileStream	fs	=new FileStream("GridValues.Save", FileMode.Create, FileAccess.Write);
			if(fs == null)
			{
				//bummer?
				return;
			}

			BinaryWriter	bw	=new BinaryWriter(fs);

			//secret code for atlas griddy stuffs
			UInt32	magic	=0xA71A5517;
			bw.Write(magic);

			bw.Write(mGridData.Count);

			foreach(HeightMap.TexData td in mGridData)
			{
				bw.Write(td.BottomElevation);
				bw.Write(td.TopElevation);
				bw.Write(td.Steep);
				bw.Write(td.ScaleFactor);
				bw.Write(td.TextureName);

				bw.Write(td.mScaleU);
				bw.Write(td.mScaleV);
				bw.Write(td.mUOffs);
				bw.Write(td.mVOffs);
			}

			bw.Close();
			fs.Close();
		}

		bool LoadValues()
		{
			if(!File.Exists("GridValues.Save"))
			{
				//no save yet, no big deal
				return	false;
			}

			FileStream	fs	=new FileStream("GridValues.Save", FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				return	false;
			}

			BinaryReader	br	=new BinaryReader(fs);

			UInt32	magic	=br.ReadUInt32();
			if(magic != 0xA71A5517)
			{
				br.Close();
				fs.Close();
				return	false;
			}

			//load into a temp list first
			List<HeightMap.TexData>	gridStuffs	=new List<HeightMap.TexData>();

			int	count	=br.ReadInt32();

			for(int i=0;i < count;i++)
			{
				HeightMap.TexData	td	=new HeightMap.TexData();

				td.BottomElevation	=br.ReadSingle();
				td.TopElevation		=br.ReadSingle();
				td.Steep			=br.ReadBoolean();
				td.ScaleFactor		=br.ReadSingle();
				td.TextureName		=br.ReadString();
				td.mScaleU			=br.ReadDouble();
				td.mScaleV			=br.ReadDouble();
				td.mUOffs			=br.ReadDouble();
				td.mVOffs			=br.ReadDouble();

				gridStuffs.Add(td);
			}

			br.Close();
			fs.Close();

			//use data that matches existing stuffs
			List<string>	textures	=mSK.GetTexture2DList();
			foreach(HeightMap.TexData td in gridStuffs)
			{
				if(textures.Contains(td.TextureName))
				{
					mGridData.Add(td);
				}
			}

			gridStuffs.Clear();

			return	true;
		}
	}
}
