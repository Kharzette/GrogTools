using System;
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
			List<string>	textures	=mSK.GetTexture2DList();

			foreach(string tex in textures)
			{
				if(tex.StartsWith("Terrain\\"))
				{
					HeightMap.TexData	gd	=new HeightMap.TexData();

					gd.TextureName	=tex;

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
			RebuildImage();

			Misc.SafeInvoke(eReBuild, mAtlas, new ListEventArgs<HeightMap.TexData>(mGridData.ToList()));
		}


		internal float GetTransitionHeight()
		{
			return	(float)TransitionHeight.Value;
		}
	}
}
