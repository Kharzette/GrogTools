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

		Random	mRand	=new Random();
		Vector3	mPos	=Vector3.Zero;

		//terrain stuff
		HeightMap	mHeight;

		//gpu
		GraphicsDevice	mGD;

		//Fonts / UI
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		Mover2			mTextMover	=new Mover2();
		int				mResX, mResY;
		List<string>	mFonts	=new List<string>();


		internal GameLoop(GraphicsDevice gd, StuffKeeper sk, string gameRootDir)
		{
			mGD				=gd;
			mResX			=gd.RendForm.ClientRectangle.Width;
			mResY			=gd.RendForm.ClientRectangle.Height;

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
				color, Vector2.UnitX * 20f + Vector2.UnitY * 640f, Vector2.One);

			float	[,]chunk	=new float[67, 67];

			for(int y=0;y < 67;y++)
			{
				for(int x=0;x < 67;x++)
				{
					chunk[y, x]	=Mathery.RandomFloatNext(mRand, -6f, 6f);
				}
			}

			mHeight	=new HeightMap(chunk, Point.Zero, 67, 67, 65, 65, 0, 0, 16f, mGD);

			mTerMats	=new MatLib(mGD, sk);

			Vector3	lightDir	=Mathery.RandomDirection(mRand);

			Vector4	lightColor2	=Vector4.One * 0.6f;
			Vector4	lightColor3	=Vector4.One * 0.3f;

			lightColor2.W	=lightColor3.W	=1f;

			mTerMats.CreateMaterial("Terrain");
			mTerMats.SetMaterialEffect("Terrain", "Static.fx");
			mTerMats.SetMaterialTechnique("Terrain", "TriSolid");
			mTerMats.SetMaterialParameter("Terrain", "mLightColor0", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mLightColor1", lightColor2);
			mTerMats.SetMaterialParameter("Terrain", "mLightColor2", lightColor3);
			mTerMats.SetMaterialParameter("Terrain", "mLightDirection", lightDir);
			mTerMats.SetMaterialParameter("Terrain", "mSolidColour", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mSpecPower", 1);
			mTerMats.SetMaterialParameter("Terrain", "mSpecColor", Vector4.One);
			mTerMats.SetMaterialParameter("Terrain", "mWorld", Matrix.Identity);
		}


		internal void Update(UpdateTimer time, List<Input.InputAction> acts, PlayerSteering ps)
		{
			Vector3	moveVec		=ps.Update(mPos, mGD.GCam.Forward, mGD.GCam.Left, mGD.GCam.Up, acts);

			mPos	-=(moveVec * 100f);

			mGD.GCam.Update(mPos, ps.Pitch, ps.Yaw, ps.Roll);

			mST.ModifyStringText(mFonts[0], "Position: " + " : "
				+ mGD.GCam.Position.IntStr(), "PosStatus");

			mST.Update(mGD.DC);
		}

		internal void RenderUpdate(float deltaMS)
		{
			mTerMats.UpdateWVP(Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection, mGD.GCam.Position);
		}

		internal void Render()
		{
			mHeight.Draw(mGD.DC, mTerMats, Matrix.Identity, mGD.GCam.View, mGD.GCam.Projection);
			mST.Draw(mGD.DC, Matrix.Identity, mTextProj);
		}

		internal void FreeAll()
		{
			mFontMats.FreeAll();
		}
	}
}
