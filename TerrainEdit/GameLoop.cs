using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.IO;

using MeshLib;
using UtilityLib;
using MaterialLib;
using InputLib;
using PathLib;
using TerrainLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MatLib	=MaterialLib.MaterialLib;



namespace TerrainEdit
{
	class GameLoop
	{
		MatLib		mTerMats;

		Random	mRand			=new Random();
		Vector3	mPos			=Vector3.Zero;
		Point	mGridCoordinate;
		int		mCellGridMax, mBoundary;

		BoundingFrustum	mFrust	=new BoundingFrustum(Matrix.Identity);

		//terrain stuff
		FractalFactory	mFracFact;
		TerrainModel	mTModel;
		Terrain			mTerrain;
//		HeightMap		mHeight;

		//gpu
		GraphicsDevice	mGD;
		StuffKeeper		mSK;

		//Fonts / UI
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		Mover2			mTextMover	=new Mover2();
		int				mResX, mResY;
		List<string>	mFonts	=new List<string>();

		const int	Nearby		=7;


		internal GameLoop(GraphicsDevice gd, StuffKeeper sk, string gameRootDir)
		{
			mGD		=gd;
			mSK		=sk;
			mResX	=gd.RendForm.ClientRectangle.Width;
			mResY	=gd.RendForm.ClientRectangle.Height;

			mFontMats	=new MatLib(gd, sk);

			mFontMats.CreateMaterial("Text");
			mFontMats.SetMaterialEffect("Text", "2D.fx");
			mFontMats.SetMaterialTechnique("Text", "Text");

			mFonts	=sk.GetFontList();

			mST	=new ScreenText(gd.GD, mFontMats, mFonts[0], 1000);

			mTextProj	=Matrix.OrthoOffCenterLH(0, mResX, mResY, 0, 0.1f, 5f);

			Vector4	color	=Vector4.UnitY + (Vector4.UnitW * 0.15f);

			//string indicators for various statusy things
			mST.AddString(mFonts[0], "Stuffs", "PosStatus",
				color, Vector2.UnitX * 20f + Vector2.UnitY * 580f, Vector2.One);
			mST.AddString(mFonts[0], "Thread Status...", "ThreadStatus",
				color, Vector2.UnitX * 20f + Vector2.UnitY * 560f, Vector2.One);

			mTerMats	=new MatLib(mGD, sk);

			Vector3	lightDir	=Mathery.RandomDirection(mRand);

			Vector4	lightColor2	=Vector4.One * 0.4f;
			Vector4	lightColor3	=Vector4.One * 0.1f;

			lightColor2.W	=lightColor3.W	=1f;

			mTerMats.CreateMaterial("Terrain");
			mTerMats.SetMaterialEffect("Terrain", "Terrain.fx");
			mTerMats.SetMaterialTechnique("Terrain", "TriTerrain");
			mTerMats.SetMaterialParameter("Terrain", "mLightColor0", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mLightColor1", lightColor2);
			mTerMats.SetMaterialParameter("Terrain", "mLightColor2", lightColor3);
			mTerMats.SetMaterialParameter("Terrain", "mLightDirection", lightDir);
			mTerMats.SetMaterialParameter("Terrain", "mSolidColour", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mSpecPower", 1);
			mTerMats.SetMaterialParameter("Terrain", "mSpecColor", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mWorld", Matrix.Identity);

			mTerMats.InitCelShading(1);
			mTerMats.GenerateCelTexturePreset(mGD.GD, mGD.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
			mTerMats.SetCelTexture(0);
		}


		internal void Update(UpdateTimer time, List<Input.InputAction> acts, PlayerSteering ps)
		{
			Vector3	moveVec		=ps.Update(mPos, mGD.GCam.Forward, mGD.GCam.Left, mGD.GCam.Up, acts);

			mPos	+=(moveVec * 100f);

			bool	bWrapped	=WrapPosition(ref mPos);

			WrapGridCoordinates();

			if(bWrapped && mTerrain != null)
			{
				mTerrain.BuildGrid(mGD, Nearby);
			}

			if(mTerrain != null)
			{
				mTerrain.SetCellCoord(mGridCoordinate);
				mTerrain.UpdatePosition(mPos, mTerMats);
			}

			mGD.GCam.Update(-mPos, ps.Pitch, ps.Yaw, ps.Roll);

			mST.ModifyStringText(mFonts[0], "Grid: " + mGridCoordinate.ToString() +
				", Position: " + " : "
				+ mGD.GCam.Position.IntStr(), "PosStatus");

			if(mTerrain != null)
			{
				mST.ModifyStringText(mFonts[0], "Threads Active: " + mTerrain.GetThreadsActive()
					+ ", Thread Counter: " + mTerrain.GetThreadCounter(), "ThreadStatus");
			}

			mST.Update(mGD.DC);
		}

		internal void RenderUpdate(float deltaMS)
		{
			mTerMats.UpdateWVP(Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);

			mFrust.Matrix	=mGD.GCam.View * mGD.GCam.Projection;
		}

		internal void Render()
		{
			if(mTerrain != null)
			{
				mTerrain.Draw(mGD, mTerMats, mFrust);
			}

			mST.Draw(mGD.DC, Matrix.Identity, mTextProj);
		}

		internal void FreeAll()
		{
			mFontMats.FreeAll();
		}


		internal void Texture(TexAtlas texAtlas, List<HeightMap.TexData> texInfo, float transHeight)
		{
			mSK.AddMap("TerrainAtlas", texAtlas.GetAtlasSRV());
			mTerMats.SetMaterialTexture("Terrain", "mTexture0", "TerrainAtlas");

			Vector4	[]scaleofs	=new Vector4[16];
			float	[]scale		=new float[16];

			for(int i=0;i < texInfo.Count;i++)
			{
				if(i > 15)
				{
					break;
				}

				scaleofs[i]	=new Vector4(
					(float)texInfo[i].mScaleU,
					(float)texInfo[i].mScaleV,
					(float)texInfo[i].mUOffs,
					(float)texInfo[i].mVOffs);

				//basically a divisor
				scale[i]	=1.0f / texInfo[i].ScaleFactor;
			}
			mTerMats.SetMaterialParameter("Terrain", "mAtlasUVData", scaleofs);
			mTerMats.SetMaterialParameter("Terrain", "mAtlasTexScale", scale);

			if(mTerrain != null)
			{
				mTerrain.SetTextureData(texInfo, transHeight);
			}

			Vector3	lightDir	=Mathery.RandomDirection(mRand);
			mTerMats.SetMaterialParameter("Terrain", "mLightDirection", lightDir);
		}


		internal void TBuild(int gridSize, int chunkSize, float medianHeight,
			float variance, int polySize, int tilingIterations, float borderSize,
			int smoothPasses, int seed, int erosionIterations,
			float rainFall, float solubility, float evaporation)
		{
			mFracFact	=new FractalFactory(variance, medianHeight, gridSize + 1, gridSize + 1);

			float[,]	fract	=mFracFact.CreateFractal(seed, gridSize + 1);

			for(int i=0;i < smoothPasses;i++)
			{
				FractalFactory.SmoothPass(fract);
			}

			if(erosionIterations > 0)
			{
				int	realIterations	=FractalFactory.Erode(fract, mRand,
					erosionIterations, rainFall, solubility, evaporation);
			}

			for(int i=0;i < tilingIterations;i++)
			{
				float	borderSlice	=borderSize / tilingIterations;

				FractalFactory.MakeTiled(fract, borderSlice * (i + 1));
			}

			if(mTModel != null)
			{
				mTModel.FreeAll();
			}
			mTModel	=new TerrainModel(fract, polySize, gridSize + 1);

			mCellGridMax	=gridSize / chunkSize;

			List<HeightMap.TexData>	tdata	=new List<HeightMap.TexData>();

			float	transHeight	=0f;
			if(mTerrain != null)
			{
				//grab a copy of the old texture data if any
				List<HeightMap.TexData>	texOld	=mTerrain.GetTextureData(out transHeight);

				//clone it because it is about to all get nuked
				foreach(HeightMap.TexData td in texOld)
				{
					HeightMap.TexData	td2	=new HeightMap.TexData(td);
					tdata.Add(td2);
				}
				mTerrain.FreeAll();
			}
			mTerrain	=new Terrain(fract, polySize, chunkSize, mCellGridMax);

			mTerrain.SetTextureData(tdata, transHeight);

			//start off in the middle
			mGridCoordinate.X	=mCellGridMax / 2;
			mGridCoordinate.Y	=mCellGridMax / 2;

			mBoundary	=chunkSize * polySize;

			mTerrain.SetCellCoord(mGridCoordinate);

			mTerrain.BuildGrid(mGD, Nearby);

			mTerrain.UpdatePosition(Vector3.Zero, mTerMats);
		}


		bool WrapPosition(ref Vector3 pos)
		{
			bool	bWrapped	=false;

			if(pos.X > mBoundary)
			{
				pos.X	-=mBoundary;
				mGridCoordinate.X++;
				bWrapped	=true;
			}
			else if(pos.X < 0f)
			{
				pos.X	+=mBoundary;
				mGridCoordinate.X--;
				bWrapped	=true;
			}

			if(pos.Z > mBoundary)
			{
				pos.Z	-=mBoundary;
				mGridCoordinate.Y++;
				bWrapped	=true;
			}
			else if(pos.Z < 0f)
			{
				pos.Z	+=mBoundary;
				mGridCoordinate.Y--;
				bWrapped	=true;
			}

			return	bWrapped;
		}


		bool WrapGridCoordinates()
		{
			bool	bWrapped	=false;

			if(mGridCoordinate.X >= mCellGridMax)
			{
				mGridCoordinate.X	=0;
				bWrapped	=true;
			}
			else if(mGridCoordinate.X < 0)
			{
				mGridCoordinate.X	=mCellGridMax - 1;
				bWrapped	=true;
			}

			if(mGridCoordinate.Y >= mCellGridMax)
			{
				mGridCoordinate.Y	=0;
				bWrapped	=true;
			}
			else if(mGridCoordinate.Y < 0)
			{
				mGridCoordinate.Y	=mCellGridMax - 1;
				bWrapped	=true;
			}

			return	bWrapped;
		}
	}
}
