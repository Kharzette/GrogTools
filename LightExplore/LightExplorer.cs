using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using BSPCore;
using SharedForms;
using UtilityLib;
using MaterialLib;
using InputLib;

using MatLib = MaterialLib.MaterialLib;


namespace LightExplore
{
	internal class LightExplorer
	{
		Map			mMap;
		LightData	mLD;

		//gpu
		GraphicsDevice	mGD;
		StuffKeeper		mSK;

		//draw stuff
		DebugDraw	mDBDraw;
		DrawStuff	mDS;
		int			mFaceIndex;
		bool		mbDrawWorld	=true;

		//text
		ScreenText		mST;
		MatLib			mFontMats;
		Matrix			mTextProj;
		Mover2			mTextMover	=new Mover2();
		int				mResX, mResY;
		List<string>	mFonts	=new List<string>();


		ExploreForm	mEForm	=new ExploreForm();
		Output		mOForm	=new Output();


		internal LightExplorer(GraphicsDevice gd, StuffKeeper sk)
		{
			mGD	=gd;
			mSK	=sk;

			mResX	=gd.RendForm.ClientRectangle.Width;
			mResY	=gd.RendForm.ClientRectangle.Height;

			mGD.eDeviceLost	+=OnDeviceLost;

			mEForm.Visible	=true;
			mOForm.Visible	=true;

			mEForm.eOpenGBSP	+=OnOpenGBSP;

			mFontMats	=new MatLib(gd, sk);

			mFontMats.CreateMaterial("Text");
			mFontMats.SetMaterialEffect("Text", "2D.fx");
			mFontMats.SetMaterialTechnique("Text", "Text");

			mFonts	=sk.GetFontList();

			mST	=new ScreenText(gd.GD, mFontMats, mFonts[0], 1000);

			mTextProj	=Matrix.OrthoOffCenterLH(0, mResX, mResY, 0, 0.1f, 5f);

			Vector4	color	=Vector4.UnitY + (Vector4.UnitW * 0.15f);

			mST.AddString(mFonts[0], "Face Index: " + mFaceIndex, "FaceIndex",
				color, Vector2.UnitX * 20f + Vector2.UnitY * 400f, Vector2.One);
		}


		internal void UpdateActions(List<Input.InputAction> acts)
		{
			int	fidx	=mFaceIndex;
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(Program.MyActions.ToggleWorld))
				{
					mbDrawWorld	=!mbDrawWorld;
				}
				else if(act.mAction.Equals(Program.MyActions.IncrementFaceIndex))
				{
					mFaceIndex++;
				}
				else if(act.mAction.Equals(Program.MyActions.DecrementFaceIndex))
				{
					mFaceIndex--;
				}
				else if(act.mAction.Equals(Program.MyActions.BigIncrementFaceIndex))
				{
					mFaceIndex	+=100;
				}
				else if(act.mAction.Equals(Program.MyActions.BigDecrementFaceIndex))
				{
					mFaceIndex	-=100;
				}
			}

			if(mLD != null)
			{
				mLD.ClampIndex(ref mFaceIndex);
			}

			if(mFaceIndex != fidx)
			{
				mST.ModifyStringText(mFonts[0], "Face Index: " + mFaceIndex, "FaceIndex");

				BuildFaceDrawData();
			}
		}


		internal void Update(float msDelta, GraphicsDevice gd)
		{
//			mZoneDraw.Update(msDelta);

//			mMatLib.UpdateWVP(Matrix.Identity,
//				gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);

			mST.Update(gd.DC);
		}


		internal void Render(GraphicsDevice gd)
		{
//			if(mZoneDraw == null || mVisMap == null)
//			{
//				mDebugDraw.Draw(mGD);
//				return;
//			}

//			mZoneDraw.Draw(gd, 0, mVisMap.IsMaterialVisibleFromPos,
//				GetModelMatrix, RenderExternal, RenderShadows, SetUpAlphaRenderTargets);

			if(mDBDraw != null && mbDrawWorld)
			{
				mDBDraw.Draw(gd);
			}
			if(mDS != null)
			{
				mDS.SetLMTexture("LightMap" + mFaceIndex.ToString("D8"));
				mDS.Draw(gd);
			}
			mST.Draw(mGD.DC, Matrix.Identity, mTextProj);
		}


		internal void FreeAll()
		{
			if(mMap != null)
			{
				mMap.FreeGBSPFile();
			}
			if(mLD != null)
			{
				mLD.FreeAll();
			}
			if(mDBDraw != null)
			{
				mDBDraw.FreeAll();
			}
			if(mDS != null)
			{
				mDS.FreeAll();
			}
		}


		void OnDeviceLost(object sender, EventArgs ea)
		{
			mOForm.Print("Graphics device lost, rebuilding stuffs...\n");
		}


		void OnOpenGBSP(object sender, EventArgs ea)
		{
			string	fileName	=sender as string;
			if(fileName == null || fileName == "")
			{
				mOForm.Print("Bad filename\n");
				return;
			}

			mMap	=new Map();

			mMap.LoadGBSPFile(fileName);

			List<Vector3>	verts	=new List<Vector3>();
			List<UInt16>	inds	=new List<UInt16>();
			List<Vector3>	norms	=new List<Vector3>();
			List<Color>		cols	=new List<Color>();

			mMap.GetTriangles(verts, norms, cols, inds, Map.DebugDrawChoice.GFXFaces);

			if(mDBDraw == null)
			{
				mDBDraw	=new DebugDraw(mGD, mSK);
			}
			else
			{
				mDBDraw.FreeAll();
			}

			mDBDraw.MakeDrawStuff(mGD.GD, verts, norms, cols, inds);

			string	expFile	=FileUtil.StripExtension(fileName);

			expFile	+=".LightExplore";

			FileStream	fs	=new FileStream(expFile, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				mOForm.Print("Couldn't find light explore file: " + expFile + "\n");
				return;
			}

			BinaryReader	br	=new BinaryReader(fs);
			if(br == null)
			{
				fs.Close();
				mOForm.Print("Couldn't open light explore file: " + expFile + "\n");
				return;
			}

			mLD	=new LightData(br);

			mOForm.Print(expFile + " loaded...\n");

			br.Close();
			fs.Close();

			if(mDS == null)
			{
				mDS	=new DrawStuff(mGD, mSK);
			}

			BuildFaceDrawData();
		}


		void BuildFaceDrawData()
		{
			Debug.Assert(mDS != null);

			mLD.MakeDrawStuff(mGD, mDS, mFaceIndex);

			int	w, h;
			Color	[]lm	=mMap.GetLightMapForFace(mFaceIndex, out w, out h);

			if(lm == null)
			{
				return;
			}

			mSK.AddTex(mGD.GD, "LightMap" + mFaceIndex.ToString("D8"), lm, w, h);
		}
	}
}