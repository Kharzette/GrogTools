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
		bool		[][]mInSolid;
		GFXPlane	[]mPlanes;
		FInfo		[]mFInfos;
		int			mNumSamples;


		internal LightData(BinaryReader br, Map map, SharedForms.Output outForm)
		{
			mNumSamples		=br.ReadInt32();
			int	numFaces	=br.ReadInt32();

			mLightPoints	=new Vector3[numFaces][];
			mInSolid		=new bool[numFaces][];
			mPlanes			=new GFXPlane[numFaces];
			mFInfos			=new FInfo[numFaces];

			outForm.Print("Reading " + numFaces + " faces...\n");
			outForm.UpdateProgress(0, numFaces, 0);

			int	numPointsTotal	=0;

			for(int i=0;i < numFaces;i++)
			{
				int	numPoints	=br.ReadInt32();

				mLightPoints[i]	=new Vector3[numPoints];
				mInSolid[i]		=new bool[numPoints];

				for(int j=0;j < numPoints;j++)
				{
					mLightPoints[i][j]	=FileUtil.ReadVector3(br);
					mInSolid[i][j]		=map.IsPointInSolidSpace(mLightPoints[i][j]);
					numPointsTotal++;
				}

				mPlanes[i]	=new GFXPlane();
				mPlanes[i].Read(br);

				bool	bFInfo	=br.ReadBoolean();
				if(bFInfo)
				{
					mFInfos[i]	=new FInfo();
					mFInfos[i].ReadVecs(br);
				}
				outForm.UpdateProgress(0, numFaces, i);
			}

			outForm.UpdateProgress(0, numFaces, 0);
			outForm.Print("Read " + numPointsTotal + " total points.\n");
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
				mInSolid[i]		=null;
			}

			mLightPoints	=null;
			mInSolid		=null;
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

			ds.MakeDrawStuff(gd.GD, mLightPoints[index], mInSolid[index], mPlanes[index], mNumSamples);
		}
	}
}