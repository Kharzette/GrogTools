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
		Random	mRand			=new Random();
		Vector3	mPos			=Vector3.Zero;

		BoundingFrustum	mFrust	=new BoundingFrustum(Matrix.Identity);

		//terrain stuff
		FractalFactory	mFracFact;
		TerrainModel	mTModel;
		Terrain			mTerrain;
		PrimObject		mSkyCube;
		MatLib			mTerMats;
		int				mChunkRange, mNumStreamThreads;
		Point			mGridCoordinate;
		int				mCellGridMax, mBoundary;

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

		//debug draw
		PrimObject	mQTreeBoxes;
		MatLib		mDebugMats;

		const int	Nearby		=7;
		const float	FlySpeed	=1000f;


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

			mTerMats.CreateMaterial("Sky");
			mTerMats.SetMaterialEffect("Sky", "Terrain.fx");
			mTerMats.SetMaterialTechnique("Sky", "SkyGradient");

			mTerMats.InitCelShading(1);
			mTerMats.GenerateCelTexturePreset(mGD.GD, mGD.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
			mTerMats.SetCelTexture(0);

			mSkyCube	=PrimFactory.CreateCube(gd.GD, -5f);

			//debug draw
			mDebugMats	=new MatLib(gd, sk);

			Vector4	redColor	=Vector4.One;
			Vector4	greenColor	=Vector4.One;
			Vector4	blueColor	=Vector4.One;

			redColor.Y	=redColor.Z	=greenColor.X	=greenColor.Z	=blueColor.X	=blueColor.Y	=0f;

			mDebugMats.CreateMaterial("DebugBoxes");
			mDebugMats.SetMaterialEffect("DebugBoxes", "Static.fx");
			mDebugMats.SetMaterialTechnique("DebugBoxes", "TriSolidSpec");
			mDebugMats.SetMaterialParameter("DebugBoxes", "mLightColor0", Vector4.One);
			mDebugMats.SetMaterialParameter("DebugBoxes", "mLightColor1", lightColor2);
			mDebugMats.SetMaterialParameter("DebugBoxes", "mLightColor2", lightColor3);
			mDebugMats.SetMaterialParameter("DebugBoxes", "mSolidColour", blueColor);
			mDebugMats.SetMaterialParameter("DebugBoxes", "mSpecPower", 1);
			mDebugMats.SetMaterialParameter("DebugBoxes", "mSpecColor", Vector4.One);
		}


		internal void Update(UpdateTimer time, List<Input.InputAction> acts, PlayerSteering ps)
		{
			Vector3	moveVec		=ps.Update(mPos, mGD.GCam.Forward, mGD.GCam.Left, mGD.GCam.Up, acts);

			mPos	+=(moveVec * FlySpeed);

			bool	bWrapped	=WrapPosition(ref mPos);

			WrapGridCoordinates();

			if(bWrapped && mTerrain != null)
			{
				mTerrain.BuildGrid(mGD, mChunkRange, mNumStreamThreads);
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

			mSkyCube.World	=Matrix.Translation(mGD.GCam.Position);

			mTerMats.SetMaterialParameter("Sky", "mWorld", mSkyCube.World);
			mDebugMats.UpdateWVP(Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);
		}

		internal void Render()
		{
			mTerMats.ApplyMaterialPass("Sky", mGD.DC, 0);
			mSkyCube.Draw(mGD.DC);

			if(mTerrain != null)
			{
				mTerrain.Draw(mGD, mTerMats, mFrust);
			}

			if(mQTreeBoxes != null)
			{
				mDebugMats.ApplyMaterialPass("DebugBoxes", mGD.DC, 0);
				mQTreeBoxes.Draw(mGD.DC);
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
			float rainFall, float solubility, float evaporation, int threads)
		{
			mFracFact	=new FractalFactory(variance, medianHeight, gridSize + 1, gridSize + 1);

			mNumStreamThreads	=threads;

			float	[,]fract	=mFracFact.CreateFractal(seed);

			for(int i=0;i < smoothPasses;i++)
			{
				FractalFactory.SmoothPass(fract);
			}

			for(int i=0;i < tilingIterations;i++)
			{
				float	borderSlice	=borderSize / tilingIterations;

				FractalFactory.MakeTiled(fract, borderSlice * (i + 1));
			}

			if(erosionIterations > 0)
			{
				int	realIterations	=FractalFactory.Erode(fract, mRand,
					erosionIterations, rainFall, solubility, evaporation);

				//redo tiling if eroded
				for(int i=0;i < tilingIterations;i++)
				{
					float	borderSlice	=borderSize / tilingIterations;

					FractalFactory.MakeTiled(fract, borderSlice * (i + 1));
				}
			}

			Vector3	[,]norms	=mFracFact.BuildNormals(fract, polySize);

			if(mTModel != null)
			{
				mTModel.FreeAll();
			}
			mTModel	=new TerrainModel(fract, polySize, gridSize);

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
			mTerrain	=new Terrain(fract, norms, polySize, chunkSize, mCellGridMax);

			mTerrain.SetTextureData(tdata, transHeight);

			mBoundary	=chunkSize * polySize;

			WrapGridCoordinates();

			mTerrain.SetCellCoord(mGridCoordinate);

			mTerrain.BuildGrid(mGD, mChunkRange, mNumStreamThreads);

			mPos.Y	=mTModel.GetHeight(mPos) + 200f;

			mTerrain.UpdatePosition(mPos, mTerMats);

			//clamp box heights
			mTModel.FixBoxHeights();

			//clear existing
			if(mQTreeBoxes != null)
			{
				mQTreeBoxes.Free();
			}

			//turn on to debug quadtree
			//careful about big map sizes
//			List<BoundingBox>	boxes	=mTModel.GetAllBoxes();
//			mQTreeBoxes	=PrimFactory.CreateCubes(mGD.GD, boxes);
		}


		internal void ApplyShadingInfo(TerrainShading.ShadingInfo si)
		{
			mTerMats.SetMaterialParameter("Terrain", "mFogStart", si.mFogStart);
			mTerMats.SetMaterialParameter("Terrain", "mFogEnd", si.mFogEnd);
			mTerMats.SetMaterialParameter("Terrain", "mFogEnabled", si.mbFogEnabled? 1f : 0f);

			mTerMats.SetMaterialParameter("Sky", "mSkyGradient0", si.mSkyColor0.ToVector3());
			mTerMats.SetMaterialParameter("Sky", "mSkyGradient1", si.mSkyColor1.ToVector3());

			Viewport	vp	=mGD.GetScreenViewPort();

			mGD.GCam.Projection	=Matrix.PerspectiveFovLH(
				MathUtil.DegreesToRadians(45f),
				vp.Width / (float)vp.Height, 0.1f, si.mFogEnd);

			mGD.SetClip(0.1f, si.mFogEnd);

			mChunkRange	=si.mChunkRange;
		}


		internal void TLoad(string path)
		{
			mTerrain	=new Terrain(path);

			mChunkRange			=10;	//todo fix
			mNumStreamThreads	=2;

			mTerrain.BuildGrid(mGD, mChunkRange, mNumStreamThreads);
		}


		internal bool TSave(string path)
		{
			return	mTerrain.Save(path);
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
