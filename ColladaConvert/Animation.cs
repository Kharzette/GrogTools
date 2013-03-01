using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaConvert
{
	public class Animation
	{
		public enum KeyPartsUsed
		{
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

		string	mName;

		List<SubAnimation>	mSubAnims	=new List<SubAnimation>();


		public Animation(animation anim)
		{
			if(anim.Items.OfType<animation>().Count() > 0)
			{
				foreach(object anObj in anim.Items)
				{
					animation	anm	=anObj as animation;
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
			}
		}


		public string GetName()
		{
			return	mName;
		}


		internal MeshLib.SubAnim	GetAnims(MeshLib.Skeleton skel, library_visual_scenes lvs, out KeyPartsUsed parts)
		{
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
					parts	|=sa.SetKeys(bone, times, keys, lvs, axisAngleKeys);
				}

				//fix axis angle keyframes
				foreach(MeshLib.KeyFrame kf in axisAngleKeys)
				{
					Matrix	mat	=Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(kf.mRotation.X));
					mat	*=Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(kf.mRotation.Y));
					mat	*=Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(kf.mRotation.Z));

					kf.mRotation	=Quaternion.CreateFromRotationMatrix(mat);
				}

				//find and set bone key reference
				MeshLib.KeyFrame	boneKey	=skel.GetBoneKey(bone);

				//patch up the keys with any missing channel data
				/*
				foreach(MeshLib.KeyFrame kf in keys)
				{
					//fill in missing parts with original bone
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.TranslateX))
					{
						kf.mPosition.X	=boneKey.mPosition.X;
					}
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.TranslateY))
					{
						kf.mPosition.Y	=boneKey.mPosition.Y;
					}
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.TranslateZ))
					{
						kf.mPosition.Z	=boneKey.mPosition.Z;
					}
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.ScaleX))
					{
						kf.mScale.X	=boneKey.mScale.X;
					}
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.ScaleY))
					{
						kf.mScale.Y	=boneKey.mScale.Y;
					}
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.ScaleZ))
					{
						kf.mScale.Z	=boneKey.mScale.Z;
					}

					//rotation is trickier since these are now quaternions
					//if any are set, I'd think all would be set
					if(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateX)
						|| !UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateY)
						|| !UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateZ))
					{
						Debug.Assert(!UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateX)
							&& !UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateY)
							&& !UtilityLib.Misc.bFlagSet((UInt32)parts, (UInt32)KeyPartsUsed.RotateZ));

						//set the whole quaternion
						kf.mRotation.X	=boneKey.mRotation.X;
						kf.mRotation.Y	=boneKey.mRotation.Y;
						kf.mRotation.Z	=boneKey.mRotation.Z;
						kf.mRotation.W	=boneKey.mRotation.W;
					}
				}*/

				return	new MeshLib.SubAnim(bone, times, keys);
			}

			return	null;
		}
	}
}