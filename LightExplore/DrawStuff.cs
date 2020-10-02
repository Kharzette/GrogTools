using System;
using System.Collections.Generic;
using UtilityLib;
using MeshLib;
using BSPCore;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

using MatLib = MaterialLib.MaterialLib;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;


namespace LightExplore
{
	public class DrawStuff
	{
		Buffer				mVB;
		VertexBufferBinding	mVBBinding;
		int					mVertCount;
		Vector3				mLightDir;
		Random				mRand	=new Random();
		PrimObject			mLMPlane;

		MatLib	mMatLib;


		public DrawStuff(GraphicsDevice gd, MaterialLib.StuffKeeper sk)
		{
			mMatLib	=new MatLib(gd, sk);

			mLightDir	=Mathery.RandomDirection(mRand);

			Vector4	lightColor2	=Vector4.One * 0.8f;
			Vector4	lightColor3	=Vector4.One * 0.6f;

			lightColor2.W	=lightColor3.W	=1f;

			mMatLib.CreateMaterial("FacePoints");
			mMatLib.SetMaterialEffect("FacePoints", "Static.fx");
			mMatLib.SetMaterialTechnique("FacePoints", "TriVColorSolidSpec");
			mMatLib.SetMaterialParameter("FacePoints", "mLightColor0", Vector4.One);
			mMatLib.SetMaterialParameter("FacePoints", "mLightColor1", lightColor2);
			mMatLib.SetMaterialParameter("FacePoints", "mLightColor2", lightColor3);
			mMatLib.SetMaterialParameter("FacePoints", "mSolidColour", Vector4.One);
			mMatLib.SetMaterialParameter("FacePoints", "mSpecPower", 1);
			mMatLib.SetMaterialParameter("FacePoints", "mSpecColor", Vector4.One);
			mMatLib.SetMaterialParameter("FacePoints", "mWorld", Matrix.Identity);

			mMatLib.CreateMaterial("LMPlane");
			mMatLib.SetMaterialEffect("LMPlane", "Static.fx");
			mMatLib.SetMaterialTechnique("LMPlane", "TriTex0");
			mMatLib.SetMaterialParameter("LMPlane", "mLightColor0", Vector4.One);
			mMatLib.SetMaterialParameter("LMPlane", "mLightColor1", lightColor2);
			mMatLib.SetMaterialParameter("LMPlane", "mLightColor2", lightColor3);

			mLMPlane	=PrimFactory.CreatePlane(gd.GD, 100f);
		}


		public void SetLMTexture(string texName)
		{
			mMatLib.SetMaterialTexture("LMPlane", "mTexture0", texName);
		}


		public void MakeDrawStuff(Device dev,
			Vector3		[]facePoints,
			GFXPlane	facePlane)
		{
			if(facePoints.Length <= 0)
			{
				return;
			}

			//free existing if any
			if(mVB != null)
			{
				mVB.Dispose();
			}

			VPosNormCol0	[]vpc	=new VPosNormCol0[facePoints.Length * 2];

			int	j=0;
			for(int i=0;i < facePoints.Length;i++)
			{
				vpc[j].Position	=facePoints[i];
				vpc[j].Normal.X	=facePlane.mNormal.X;
				vpc[j].Normal.Y	=facePlane.mNormal.Y;
				vpc[j].Normal.Z	=facePlane.mNormal.Z;
				vpc[j].Normal.W	=1f;
				vpc[j++].Color0	=Color.Red;

				vpc[j].Position	=facePoints[i] + facePlane.mNormal * 3f;
				vpc[j].Normal.X	=facePlane.mNormal.X;
				vpc[j].Normal.Y	=facePlane.mNormal.Y;
				vpc[j].Normal.Z	=facePlane.mNormal.Z;
				vpc[j].Normal.W	=1f;
				vpc[j++].Color0	=Color.Green;
			}

			mVertCount	=j;
			mVB			=VertexTypes.BuildABuffer(dev, vpc, vpc[0].GetType());
			mVBBinding	=VertexTypes.BuildAVBB(VertexTypes.GetIndex(vpc[0].GetType()), mVB);
		}


		public void FreeAll()
		{
			mMatLib.FreeAll();
			if(mVB != null)
			{
				mVB.Dispose();
			}
			if(mLMPlane != null)
			{
				mLMPlane.Free();
			}
		}


		public void Draw(GraphicsDevice gd)
		{
			if(gd.DC == null)
			{
				return;
			}
			if(mVB == null)
			{
				return;
			}

			mMatLib.SetParameterForAll("mLightDirection", -mLightDir);
			mMatLib.SetParameterForAll("mView", gd.GCam.View);
			mMatLib.SetParameterForAll("mEyePos", gd.GCam.Position);
			mMatLib.SetParameterForAll("mProjection", gd.GCam.Projection);

			mMatLib.ApplyMaterialPass("FacePoints", gd.DC, 0);

			gd.DC.InputAssembler.SetVertexBuffers(0, mVBBinding);
			gd.DC.InputAssembler.PrimitiveTopology	=SharpDX.Direct3D.PrimitiveTopology.LineList;

			gd.DC.Draw(mVertCount, 0);

			mMatLib.ApplyMaterialPass("LMPlane", gd.DC, 0);

			gd.DC.InputAssembler.PrimitiveTopology	=SharpDX.Direct3D.PrimitiveTopology.TriangleList;

			mLMPlane.Draw(gd.DC);
		}
	}
}
