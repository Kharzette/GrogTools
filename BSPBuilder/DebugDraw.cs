using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityLib;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MatLib	=MaterialLib.MaterialLib;
using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;


namespace BSPBuilder
{
	internal class DebugDraw
	{
		internal struct VertexPositionNormalColor
		{
			internal Vector3	Position;
			internal Vector3	Normal;
			internal Color		Color;
		}

		Buffer				mVB, mIB;
		VertexBufferBinding	mVBBinding;
		int					mNumIndexes;
		Vector3				mLightDir;
		Random				mRand	=new Random();

		MatLib	mMatLib;


		internal DebugDraw(GraphicsDevice gd)
		{
			mMatLib	=new MatLib(gd.GD, gd.GD.FeatureLevel, true);

			mLightDir	=Mathery.RandomDirection(mRand);

			Vector4	lightColor2	=Vector4.One * 0.8f;
			Vector4	lightColor3	=Vector4.One * 0.6f;

			lightColor2.W	=lightColor3.W	=1f;

			mMatLib.CreateMaterial("LevelGeometry");
			mMatLib.SetMaterialEffect("LevelGeometry", "Static.fx");
			mMatLib.SetMaterialTechnique("LevelGeometry", "TriSolidSpec");
			mMatLib.SetMaterialParameter("LevelGeometry", "mLightColor0", Vector4.One);
			mMatLib.SetMaterialParameter("LevelGeometry", "mLightColor1", lightColor2);
			mMatLib.SetMaterialParameter("LevelGeometry", "mLightColor2", lightColor3);
			mMatLib.SetMaterialParameter("LevelGeometry", "mSolidColour", Vector4.One);
			mMatLib.SetMaterialParameter("LevelGeometry", "mSpecPower", 1);
			mMatLib.SetMaterialParameter("LevelGeometry", "mSpecColor", Vector4.One);
			mMatLib.SetMaterialParameter("LevelGeometry", "mWorld", Matrix.Identity);
		}


		internal void MakeDrawStuff(Device dev,
			List<Vector3> verts,
			List<Vector3> norms,
			List<Color> colors,
			List<UInt16> inds)
		{
			VertexPositionNormalColor	[]vpnc	=new VertexPositionNormalColor[verts.Count];

			for(int i=0;i < vpnc.Length;i++)
			{
				vpnc[i].Position	=verts[i];
				vpnc[i].Normal		=norms[i];
				vpnc[i].Color		=colors[i];
			}

			BufferDescription	bd	=new BufferDescription(
				28 * verts.Count,
				ResourceUsage.Default, BindFlags.VertexBuffer,
				CpuAccessFlags.None, ResourceOptionFlags.None, 0);

			mVB	=Buffer.Create(dev, vpnc, bd);
			
			BufferDescription	id	=new BufferDescription(inds.Count * 2,
				ResourceUsage.Default, BindFlags.IndexBuffer,
				CpuAccessFlags.None, ResourceOptionFlags.None, 0);

			mIB	=Buffer.Create<UInt16>(dev, inds.ToArray(), id);

			mVBBinding	=new VertexBufferBinding(mVB, 28, 0);

			mNumIndexes	=inds.Count;
		}


		internal void Draw(GraphicsDevice gd)
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

			mMatLib.ApplyMaterialPass("LevelGeometry", gd.DC, 0);

			gd.DC.InputAssembler.SetVertexBuffers(0, mVBBinding);
			gd.DC.InputAssembler.SetIndexBuffer(mIB, Format.R16_UInt, 0);

			gd.DC.DrawIndexed(mNumIndexes, 0, 0);
		}
	}
}
