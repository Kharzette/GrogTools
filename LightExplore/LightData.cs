using System;
using System.IO;
using System.Diagnostics;
using SharpDX;
using BSPCore;
using UtilityLib;


namespace LightExplore
{
	internal class LightData
	{
		Vector3		[][]mLightPoints;
		GFXPlane	[]mPlanes;


		internal LightData(BinaryReader br)
		{
			int	numFaces	=br.ReadInt32();

			mLightPoints	=new Vector3[numFaces][];
			mPlanes			=new GFXPlane[numFaces];

			for(int i=0;i < numFaces;i++)
			{
				int	numPoints	=br.ReadInt32();

				mLightPoints[i]	=new Vector3[numPoints];

				for(int j=0;j < numPoints;j++)
				{
					mLightPoints[i][j]	=FileUtil.ReadVector3(br);
				}

				mPlanes[i]	=new GFXPlane();
				mPlanes[i].Read(br);
			}
		}


		internal void FreeAll()
		{
			for(int i=0;i < mLightPoints.Length;i++)
			{
				mLightPoints[i]	=null;
			}

			mLightPoints	=null;
			mPlanes			=null;
		}


		internal void ClampIndex(ref int index)
		{
			if(mLightPoints == null)
			{
				index	=0;
				return;
			}

			if(index < 0)
			{
				index	=mLightPoints.Length - 1;
			}
			else if(index >= mLightPoints.Length)
			{
				index	=0;
			}
		}


		internal void MakeDrawStuff(GraphicsDevice gd, DrawStuff ds, int index)
		{
			Debug.Assert(index >= 0 && index < mLightPoints.Length);

			ds.MakeDrawStuff(gd.GD, mLightPoints[index], mPlanes[index]);
		}
	}
}