﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using MeshLib;
using UtilityLib;

using Device = SharpDX.Direct3D11.Device;
using MatLib = MaterialLib.MaterialLib;


namespace ColladaConvert
{
	internal class ExtraPrims
	{
		MatLib	mMatLib;

		PrimObject	mXAxis, mYAxis, mZAxis;
		PrimObject	mBoxBound, mSphereBound;

		const float	AxisSize	=50f;


		internal ExtraPrims(Device gd, MatLib.ShaderModel sm)
		{
			//extra material lib for prim stuff
			mMatLib	=new MatLib(gd, sm, true);

			//axis boxes
			BoundingBox	xBox	=Misc.MakeBox(AxisSize, 1f, 1f);
			BoundingBox	yBox	=Misc.MakeBox(1f, AxisSize, 1f);
			BoundingBox	zBox	=Misc.MakeBox(1f, 1f, AxisSize);

			mXAxis	=PrimFactory.CreateCube(gd, xBox);
			mYAxis	=PrimFactory.CreateCube(gd, yBox);
			mZAxis	=PrimFactory.CreateCube(gd, zBox);

			Vector4	redColor	=Vector4.One;
			Vector4	greenColor	=Vector4.One;
			Vector4	blueColor	=Vector4.One;
			Vector4	lightColor2	=Vector4.One * 0.8f;
			Vector4	lightColor3	=Vector4.One * 0.6f;

			lightColor2.W	=lightColor3.W	=1f;

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

			mMatLib.SetParameterForAll("mWorld", Matrix.Identity);

			//material for bound primitives
			mMatLib.CreateMaterial("BoundMat");
			mMatLib.SetMaterialEffect("BoundMat", "Static.fx");
			mMatLib.SetMaterialTechnique("BoundMat", "TriSolidSpecAlpha");
			mMatLib.SetMaterialParameter("BoundMat", "mLightColor0", Vector4.One);
			mMatLib.SetMaterialParameter("BoundMat", "mLightColor1", lightColor2);
			mMatLib.SetMaterialParameter("BoundMat", "mLightColor2", lightColor3);
			mMatLib.SetMaterialParameter("BoundMat", "mSolidColour", Vector4.One * 0.5f);
			mMatLib.SetMaterialParameter("BoundMat", "mSpecPower", 4);
			mMatLib.SetMaterialParameter("BoundMat", "mSpecColor", Vector4.One);
		}


		internal void Update(GameCamera gcam, Vector3 lightDir)
		{
			mMatLib.SetParameterForAll("mLightDirection", -lightDir);
			mMatLib.SetParameterForAll("mView", gcam.View);
			mMatLib.SetParameterForAll("mEyePos", gcam.Position);
			mMatLib.SetParameterForAll("mProjection", gcam.Projection);
		}


		internal void DrawAxis(DeviceContext dc)
		{
			//X axis red
			mMatLib.ApplyMaterialPass("RedAxis", dc, 0);
			mXAxis.Draw(dc);

			//Y axis green
			mMatLib.ApplyMaterialPass("GreenAxis", dc, 0);
			mYAxis.Draw(dc);

			//Z axis blue
			mMatLib.ApplyMaterialPass("BlueAxis", dc, 0);
			mZAxis.Draw(dc);
		}


		internal void DrawBox(DeviceContext dc)
		{
			if(mBoxBound == null)
			{
				return;
			}

			mMatLib.ApplyMaterialPass("BoundMat", dc, 0);
			mBoxBound.Draw(dc);
		}


		internal void DrawSphere(DeviceContext dc)
		{
			if(mSphereBound == null)
			{
				return;
			}

			mMatLib.ApplyMaterialPass("BoundMat", dc, 0);
			mSphereBound.Draw(dc);
		}


		internal void ReBuildBoundsDrawData(Device gd, object mesh)
		{
			BoundingBox		box;
			BoundingSphere	sphere;

			box.Minimum		=Vector3.Zero;
			box.Maximum		=Vector3.Zero;
			sphere.Center	=Vector3.Zero;
			sphere.Radius	=0.0f;

			if(mesh is Character)
			{
				Character	chr	=mesh as Character;

				box		=chr.GetBoxBound();
				sphere	=chr.GetSphereBound();
			}
			else
			{
				StaticMesh	sm	=mesh as StaticMesh;

				box		=sm.GetBoxBound();
				sphere	=sm.GetSphereBound();
			}

			mBoxBound		=PrimFactory.CreateCube(gd, box);
			mSphereBound	=PrimFactory.CreateSphere(gd, sphere.Center, sphere.Radius);
		}
	}
}
