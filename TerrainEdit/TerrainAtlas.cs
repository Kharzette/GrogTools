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


namespace TerrainEdit
{
	public partial class TerrainAtlas : Form
	{
		class GridData
		{
			float	mBottomElevation;
			float	mTopElevation;
			bool	mbSteep;
			string	mTextureName;

			internal double	mScaleU, mScaleV;
			internal double mUOffs, mVOffs;

			public float	BottomElevation
			{
				get {	return	mBottomElevation;	}
				set {	mBottomElevation	=value;	}
			}

			public float	TopElevation
			{
				get {	return	mTopElevation;	}
				set	{	mTopElevation	=value; }
			}

			public bool	Steep
			{
				get {	return mbSteep;	}
				set {	mbSteep	=value;	}
			}

			public string	TextureName
			{
				get {	return mTextureName;	}
				set {	mTextureName	=value;	}
			}
		}

		GraphicsDevice	mGD;
		StuffKeeper		mSK;
		TexAtlas		mAtlas;

		BindingList<GridData>	mGridData	=new BindingList<GridData>();


		public TerrainAtlas(GraphicsDevice gd, StuffKeeper sk)
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


		void AutoFill()
		{
			List<string>	textures	=mSK.GetTexture2DList();

			foreach(string tex in textures)
			{
				if(tex.StartsWith("Terrain\\"))
				{
					GridData	gd	=new GridData();

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
				GridData	gd	=mGridData[i];
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
		}

		private void OnReBuildAtlas(object sender, EventArgs e)
		{
			RebuildImage();
		}
	}
}
