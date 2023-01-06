using System.Numerics;
using Vortice.Mathematics;

namespace ColladaConvert;

public class Animation
{
	public enum KeyPartsUsed
	{
		None		=0,
		TranslateX	=1,
		TranslateY	=2,
		TranslateZ	=4,
		RotateX		=8,
		RotateY		=16,
		RotateZ		=32,
		ScaleX		=64,
		ScaleY		=128,
		ScaleZ		=256,
		All			=511
	}

	string	?mName;

	List<SubAnimation>	mSubAnims	=new List<SubAnimation>();


	public Animation(animation anim)
	{
		if(anim.Items.OfType<animation>().Count() > 0)
		{
			foreach(object anObj in anim.Items)
			{
				animation	?anm	=anObj as animation;
				if(anm == null)
				{
					continue;
				}

				mName	=anim.name;

				SubAnimation	sa	=new SubAnimation(anm);
				mSubAnims.Add(sa);
			}
		}
		else
		{
			SubAnimation	sa	=new SubAnimation(anim);
			mSubAnims.Add(sa);

			mName	="Empty";
		}
	}


	public string? GetName()
	{
		return	mName;
	}


	internal List<MeshLib.SubAnim>	GetAnims(MeshLib.Skeleton skel, library_visual_scenes lvs,
		EventHandler ?ePrint, out KeyPartsUsed parts)
	{
		List<MeshLib.SubAnim>	ret	=new List<MeshLib.SubAnim>();

		parts	=0;

		//grab full list of bones
		List<string>	boneNames	=new List<string>();

		skel.GetBoneNames(boneNames);

		//for each bone, find any keyframe times
		foreach(string bone in boneNames)
		{
			List<float>	times	=new List<float>();

			foreach(SubAnimation sa in mSubAnims)
			{
				List<float>	saTimes	=sa.GetTimesForBone(bone, lvs);

				foreach(float t in saTimes)
				{
					if(times.Contains(t))
					{
						continue;
					}
					times.Add(t);
				}
			}

			if(times.Count == 0)
			{
				continue;
			}

			times.Sort();

			//build list of keys for times
			List<MeshLib.KeyFrame>	keys	=new List<MeshLib.KeyFrame>();
			foreach(float t in times)
			{
				keys.Add(new MeshLib.KeyFrame());
			}

			//track axis angle style keys
			List<MeshLib.KeyFrame>	axisAngleKeys	=new List<MeshLib.KeyFrame>();

			//set keys
			foreach(SubAnimation sa in mSubAnims)
			{
				parts	|=sa.SetKeys(bone, times, keys, lvs, ePrint, axisAngleKeys);
			}

			//fix axis angle keyframes
			foreach(MeshLib.KeyFrame kf in axisAngleKeys)
			{
				Matrix4x4	mat	=Matrix4x4.CreateFromAxisAngle(
					Vector3.UnitX, MathHelper.ToRadians(kf.mRotation.X));

				mat	*=Matrix4x4.CreateFromAxisAngle(
					Vector3.UnitY, MathHelper.ToRadians(kf.mRotation.Y));

				mat	*=Matrix4x4.CreateFromAxisAngle(
					Vector3.UnitZ, MathHelper.ToRadians(kf.mRotation.Z));

				kf.mRotation	=Quaternion.CreateFromRotationMatrix(mat);
			}

			ret.Add(new MeshLib.SubAnim(bone, times, keys));
		}

		return	ret;
	}
}