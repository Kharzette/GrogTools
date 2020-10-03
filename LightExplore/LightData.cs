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
		FInfo		[]mFInfos;


		internal LightData(BinaryReader br)
		{
			int	numFaces	=br.ReadInt32();

			mLightPoints	=new Vector3[numFaces][];
			mPlanes			=new GFXPlane[numFaces];
			mFInfos			=new FInfo[numFaces];

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

				bool	bFInfo	=br.ReadBoolean();
				if(bFInfo)
				{
					mFInfos[i]	=new FInfo();
					mFInfos[i].ReadVecs(br);
				}
			}
		}


		internal void GetFInfoVecs(int fIdx, out Vector3 texOrg,
			out Vector3 t2WU, out Vector3 t2WV, out Vector3 start)
		{
			FInfo	fi	=mFInfos[fIdx];
			if(fi == null)
			{
				texOrg	=t2WU	=t2WV	=start	=Vector3.One;
				return;
			}

			Vector3	center;
			mFInfos[fIdx].GetVecs(out texOrg, out t2WU, out t2WV, out center);

			start	=mLightPoints[fIdx][0];	//should be reasonably close
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