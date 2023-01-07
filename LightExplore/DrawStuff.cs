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


namespace LightExplore;

public class DrawStuff
{
	Buffer				mVB;
	VertexBufferBinding	mVBBinding;
	int					mVertCount;
	Vector3				mLightDir;
	Random				mRand	=new Random();
	PrimObject			mLMPlane;
	PrimObject			mTexU, mTexV;
	Matrix				mCenter;
	Matrix				mPlaneProj, mPlaneWorld;

	MatLib	mMatLib;

	const float	AxisSize	=50f;


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

		mPlaneProj	=Matrix.OrthoOffCenterLH(0, gd.RendForm.Width, gd.RendForm.Height, 0, 0.1f, 5f);
		mPlaneWorld	=Matrix.RotationY(MathF.PI);
		mPlaneWorld	*=Matrix.Translation(Vector3.ForwardLH
			+ Vector3.UnitX * 105f + Vector3.UnitY * 530f);

		mLMPlane	=PrimFactory.CreatePlane(gd.GD, 200f);

		//axis boxes
		BoundingBox	xBox	=Misc.MakeBox(AxisSize, 1f, 1f);
		BoundingBox	yBox	=Misc.MakeBox(1f, AxisSize, 1f);
		BoundingBox	zBox	=Misc.MakeBox(1f, 1f, AxisSize);

		xBox.Minimum.X	=0;
		yBox.Minimum.Y	=0;

		mTexU	=PrimFactory.CreateCube(gd.GD, xBox);
		mTexV	=PrimFactory.CreateCube(gd.GD, yBox);

		Vector4	redColor	=Vector4.One;
		Vector4	greenColor	=Vector4.One;
		Vector4	blueColor	=Vector4.One;

		redColor.Y	=redColor.Z	=greenColor.X	=greenColor.Z	=blueColor.X	=blueColor.Y	=0f;

		//materials for axis
		mMatLib.CreateMaterial("RedAxis");
		mMatLib.SetMaterialEffect("RedAxis", "Static.fx");
		mMatLib.SetMaterialTechnique("RedAxis", "TriSolidSpec");
		mMatLib.SetMaterialParameter("RedAxis", "mLightColor0", Vector4.One);
		mMatLib.SetMaterialParameter("RedAxis", "mLightColor1", lightColor2);
		mMatLib.SetMaterialParameter("RedAxis", "mLightColor2", lightColor3);
		mMatLib.SetMaterialParameter("RedAxis", "mSolidColour", redColor);
		mMatLib.SetMaterialParameter("RedAxis", "mSpecPower", 1);
		mMatLib.SetMaterialParameter("RedAxis", "mSpecColor", Vector4.One);

		mMatLib.CloneMaterial("RedAxis", "GreenAxis");
		mMatLib.CloneMaterial("RedAxis", "BlueAxis");

		mMatLib.SetMaterialParameter("GreenAxis", "mSolidColour", blueColor);
		mMatLib.SetMaterialParameter("BlueAxis", "mSolidColour", greenColor);
	}


	public void SetLMTexture(string texName)
	{
		mMatLib.SetMaterialTexture("LMPlane", "mTexture0", texName);
	}


	public void SetTexVecs(Vector3 texOrg, Vector3 t2WU, Vector3 t2WV, Vector3 start)
	{
		Vector3	cross	=Vector3.Cross(t2WV, t2WU);

		Matrix	gack	=Matrix.Identity;

		mCenter	=new Matrix(
			t2WU.X, t2WU.Y, t2WU.Z, 0f,
			t2WV.X, t2WV.Y, t2WV.Z, 0f,
			cross.X, cross.Y, cross.Z, 0f,
			start.X, start.Y, start.Z, 1);
	}


	public void MakeDrawStuff(Device dev,
		Vector3		[]facePoints,
		bool		[]inSolid,
		GFXPlane	facePlane,
		int			numSamples)
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

		int	sampIdx		=0;
		int	j			=0;
		int	sampPoints	=facePoints.Length / numSamples;
		int	sampCounter	=0;

		for(int i=0;i < facePoints.Length;i++)
		{
			vpc[j].Position	=facePoints[i];
			vpc[j].Normal.X	=facePlane.mNormal.X;
			vpc[j].Normal.Y	=facePlane.mNormal.Y;
			vpc[j].Normal.Z	=facePlane.mNormal.Z;
			vpc[j].Normal.W	=1f;

			if(sampIdx == 0)
			{
				vpc[j++].Color0	=inSolid[i]?	Color.Red : Color.Green;
				vpc[j].Position	=facePoints[i] + facePlane.mNormal * 3f;
				vpc[j].Color0	=inSolid[i]?	Color.Red : Color.Green;
			}
			else
			{
				vpc[j++].Color0	=Color.Blue;
				vpc[j].Position	=facePoints[i] + facePlane.mNormal * 1f;
				vpc[j].Color0	=inSolid[i]?	Color.Red : Color.Blue;
			}

			vpc[j].Normal.X		=facePlane.mNormal.X;
			vpc[j].Normal.Y		=facePlane.mNormal.Y;
			vpc[j].Normal.Z		=facePlane.mNormal.Z;
			vpc[j++].Normal.W	=1f;

			sampCounter++;

			if(sampCounter >= sampPoints)
			{
				sampIdx++;
				sampCounter	=0;
			}
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
		if(mTexU != null)
		{
			mTexU.Free();
		}
		if(mTexV != null)
		{
			mTexV.Free();
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

		gd.DC.InputAssembler.PrimitiveTopology	=SharpDX.Direct3D.PrimitiveTopology.TriangleList;

		mMatLib.SetMaterialParameter("RedAxis", "mWorld", mCenter);
		mMatLib.SetMaterialParameter("GreenAxis", "mWorld", mCenter);

		//X axis red
		mMatLib.ApplyMaterialPass("RedAxis", gd.DC, 0);
		mTexU.Draw(gd.DC);

		//Y axis green
		mMatLib.ApplyMaterialPass("GreenAxis", gd.DC, 0);
		mTexV.Draw(gd.DC);

		mMatLib.SetMaterialParameter("LMPlane", "mWorld", mPlaneWorld);
		mMatLib.SetMaterialParameter("LMPlane", "mView", Matrix.Identity);
		mMatLib.SetMaterialParameter("LMPlane", "mProjection", mPlaneProj);
		mMatLib.ApplyMaterialPass("LMPlane", gd.DC, 0);
		mLMPlane.Draw(gd.DC);
	}
}