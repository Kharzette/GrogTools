using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaConvert
{
	//animations on individual controllers
	public class SubAnimation
	{
		Dictionary<string, source>	mSources	=new Dictionary<string, source>();
		Dictionary<string, sampler>	mSamplers	=new Dictionary<string, sampler>();
		List<channel>				mChannels	=new List<channel>();


		public SubAnimation(animation anim)
		{
			foreach(object anObj in anim.Items)
			{
				if(anObj is source)
				{
					source	src	=anObj as source;
					mSources.Add(src.id, src);
				}
				else if(anObj is sampler)
				{
					sampler	samp	=anObj as sampler;
					mSamplers.Add(samp.id, samp);
				}
				else if(anObj is channel)
				{
					channel	chan	=anObj as channel;
					mChannels.Add(chan);
				}
			}
		}


		string GetSourceForSemantic(sampler samp, string sem)
		{
			string	srcInp	="";
			foreach(InputLocal inp in samp.input)
			{
				if(inp.semantic == sem)
				{
					srcInp	=inp.source.Substring(1);
				}
			}
			return	srcInp;
		}


		internal List<float> GetTimesForBone(string bone, library_visual_scenes lvs)
		{
			List<float>	ret	=new List<float>();

			foreach(channel chan in mChannels)
			{
				//extract the node name and address
				int		sidx	=chan.target.IndexOf('/');
				string	sid		=chan.target.Substring(0, sidx);

				//ok this is tricky, the spec says that the <source>
				//referenced by the input with the JOINT semantic
				//should contain a <Name_array> that contains sids
				//to identify the joint nodes.  sids are used instead
				//of IDREFs to allow a skin controller to be instantiated
				//multiple times, where each instance can be animated
				//independently.
				//
				//So max's default collada exporter doesn't even give the
				//bones sids at all, and the other one whose name escapes
				//me gives the bones sids, but then the address element
				//says Name (note the case), so I guess you need to try
				//to match via sid first and if that fails, use name?
				node	n	=ColladaFileUtils.LookUpNode(lvs, sid);

				Debug.Assert(n != null);

				if(bone != n.name)
				{
					continue;
				}

				//grab sampler key
				string	sampKey	=chan.source;

				//strip #
				sampKey	=sampKey.Substring(1);

				sampler	samp	=mSamplers[sampKey];
				string	srcInp	=GetSourceForSemantic(samp, "INPUT");

				float_array	srcTimes	=mSources[srcInp].Item as float_array;

				foreach(float time in srcTimes.Values)
				{
					float	t	=time;
					if(ret.Contains(t))
					{
						continue;
					}
					ret.Add(t);
				}
			}

			return	ret;
		}


		List<float> LerpValue(float time, float_array chanTimes, float_array chanValues, int stride)
		{
			List<float>	ret	=new List<float>();

			//calc totaltime
			float	totalTime	=chanTimes.Values[chanTimes.Values.Length - 1]
				- chanTimes.Values[0];

			//make sure the time is not before our start
			Debug.Assert(time >= chanTimes.Values[0]);

			//bring the passed in time value into
			//the space of our animation
			float	animTime	=time % totalTime;

			//Bring to start
			animTime	+=chanTimes.Values[0];

			//locate the key index to start with
			int	startIndex;
			for(startIndex = 0;startIndex < chanTimes.Values.Length;startIndex++)
			{
				if(animTime < chanTimes.Values[startIndex])
				{
					//back up one
					startIndex--;
					break;	//found
				}
			}

			//figure out the percentage between pos1 and pos2
			//get the deltatime
			float	percentage	=chanTimes.Values[startIndex + 1]
				- chanTimes.Values[startIndex];

			//convert to percentage
			percentage	=1.0f / percentage;

			//multiply by amount beyond p1
			percentage	*=(animTime - chanTimes.Values[startIndex]);

			Debug.Assert(percentage >= 0.0f && percentage <= 1.0f);

			for(int i=0;i < stride;i++)
			{
				float value	=MathHelper.Lerp(chanValues.Values[startIndex],
					chanValues.Values[startIndex + 1], percentage);

				ret.Add(value);
			}
			return	ret;
		}


		internal Animation.KeyPartsUsed SetKeys(string bone,
			List<float> times, List<MeshLib.KeyFrame> keys,
			library_visual_scenes scenes,
			List<MeshLib.KeyFrame> axisAngleKeys)
		{
			Animation.KeyPartsUsed	ret	=0;

			foreach(channel chan in mChannels)
			{
				//extract the node name and address
				int		sidx	=chan.target.IndexOf('/');
				string	sid		=chan.target.Substring(0, sidx);

				node	n	=ColladaFileUtils.LookUpNode(scenes, sid);

				Debug.Assert(n != null);

				if(bone != n.name)
				{
					continue;
				}

				//grab sampler key
				string	sampKey	=chan.source;

				//strip #
				sampKey	=sampKey.Substring(1);

				sampler	samp	=mSamplers[sampKey];

				string	srcInp	=GetSourceForSemantic(samp, "INPUT");
				string	srcOut	=GetSourceForSemantic(samp, "OUTPUT");
				string	srcC1	=GetSourceForSemantic(samp, "IN_TANGENT");
				string	srcC2	=GetSourceForSemantic(samp, "OUT_TANGENT");

				float_array	chanTimes	=mSources[srcInp].Item as float_array;
				float_array	chanValues	=mSources[srcOut].Item as float_array;
				List<float>	outValues	=new List<float>();

				int	numChanKeys	=chanValues.Values.Length;

				numChanKeys	/=(int)mSources[srcOut].technique_common.accessor.stride;

				Debug.Assert(numChanKeys == (int)chanTimes.count);

				//grab values for this channel
				//along the overall list of times
				for(int tidx=0;tidx < times.Count;tidx++)
				{
					outValues.AddRange(LerpValue(times[tidx], chanTimes,
						chanValues,
						(int)mSources[srcOut].technique_common.accessor.stride));
				}

				int		slashIndex	=chan.target.IndexOf("/");
				string	nodeID		=chan.target.Substring(0, slashIndex);
				string	nodeElement	=chan.target.Substring(slashIndex + 1);

				//see if the element has an additional address
				string	addr	=null;
				int		dotIdx	=nodeElement.IndexOf('.');
				if(dotIdx != -1)
				{
					addr		=nodeElement.Substring(dotIdx + 1);
					nodeElement	=nodeElement.Substring(0, dotIdx);
				}

				node	targeted	=ColladaFileUtils.LookUpNode(scenes, nodeID);
				int		idx			=ColladaFileUtils.GetNodeItemIndex(targeted, nodeElement);

				if(targeted.ItemsElementName[idx] == ItemsChoiceType2.lookat)
				{
					Debug.Assert(false);	//haven't dealt with this one yet
				}
				else if(targeted.ItemsElementName[idx] == ItemsChoiceType2.matrix)
				{
					//this doesn't really work yet
					List<Matrix>	mats	=ColladaFileUtils.GetMatrixListFromFloatList(outValues);
					for(int v=0;v < mats.Count;v++)
					{
						mats[v].Decompose(out keys[v].mScale, out keys[v].mRotation, out keys[v].mScale);
					}
					ret	|=Animation.KeyPartsUsed.All;
				}
				else if(targeted.ItemsElementName[idx] == ItemsChoiceType2.rotate)
				{
					if(addr == null)
					{
						//I'm guessing these would be true quaternions
						//I don't really support that, as I store the
						//usual axis angle stuff I've seen in a quaternion
						//and then later fix it up to be a real quaternion
						Debug.Assert(false);
					}
					else if(addr == "ANGLE")
					{
						Debug.Assert(targeted.Items[idx] is rotate);

						rotate	rot	=targeted.Items[idx] as rotate;

						if(rot.Values[0] > 0.999f)
						{
							for(int v=0;v < outValues.Count;v++)
							{
								float	val	=outValues[v];
								keys[v].mRotation.X	=val;
								if(!axisAngleKeys.Contains(keys[v]))
								{
									axisAngleKeys.Add(keys[v]);
								}
							}
							ret	|=Animation.KeyPartsUsed.RotateX;
						}
						else if(rot.Values[1] > 0.999f)
						{
							for(int v=0;v < outValues.Count;v++)
							{
								float	val	=outValues[v];
								keys[v].mRotation.Y	=val;
								if(!axisAngleKeys.Contains(keys[v]))
								{
									axisAngleKeys.Add(keys[v]);
								}
							}
							ret	|=Animation.KeyPartsUsed.RotateY;
						}
						else if(rot.Values[2] > 0.999f)
						{
							for(int v=0;v < outValues.Count;v++)
							{
								float	val	=outValues[v];
								keys[v].mRotation.Z	=val;
								if(!axisAngleKeys.Contains(keys[v]))
								{
									axisAngleKeys.Add(keys[v]);
								}
							}
							ret	|=Animation.KeyPartsUsed.RotateZ;
						}
						else
						{
							Debug.Assert(false);	//broken!
						}
					}
				}
				else if(targeted.ItemsElementName[idx] == ItemsChoiceType2.scale)
				{
					if(addr == null)
					{
						//I haven't seen this happen, but I'm guessing it
						//would be vector3s
						for(int v=0;v < outValues.Count;v+=3)
						{
							keys[v / 3].mScale.X	=outValues[v];
							keys[v / 3].mScale.Y	=outValues[v + 1];
							keys[v / 3].mScale.Z	=outValues[v + 2];
						}
						ret	|=Animation.KeyPartsUsed.ScaleX;
						ret	|=Animation.KeyPartsUsed.ScaleY;
						ret	|=Animation.KeyPartsUsed.ScaleZ;
					}
					else if(addr == "X")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mScale.X	=val;
						}
						ret	|=Animation.KeyPartsUsed.ScaleX;
					}
					else if(addr == "Y")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mScale.Y	=val;
						}
						ret	|=Animation.KeyPartsUsed.ScaleY;
					}
					else if(addr == "Z")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mScale.Z	=val;
						}
						ret	|=Animation.KeyPartsUsed.ScaleZ;
					}
				}
				else if(targeted.ItemsElementName[idx] == ItemsChoiceType2.skew)
				{
					Debug.Assert(false);	//haven't dealt with this one yet
				}
				else if(targeted.ItemsElementName[idx] == ItemsChoiceType2.translate)
				{
					if(addr == null)
					{
						//the values are vector3s in this case
						for(int v=0;v < outValues.Count;v+=3)
						{
							keys[v / 3].mPosition.X	=outValues[v];
							keys[v / 3].mPosition.Y	=outValues[v + 1];
							keys[v / 3].mPosition.Z	=outValues[v + 2];
						}
						ret	|=Animation.KeyPartsUsed.TranslateX;
						ret	|=Animation.KeyPartsUsed.TranslateY;
						ret	|=Animation.KeyPartsUsed.TranslateZ;
					}
					else if(addr == "X")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mPosition.X	=val;
						}
						ret	|=Animation.KeyPartsUsed.TranslateX;
					}
					else if(addr == "Y")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mPosition.Y	=val;
						}
						ret	|=Animation.KeyPartsUsed.TranslateY;
					}
					else if(addr == "Z")
					{
						for(int v=0;v < outValues.Count;v++)
						{
							float	val	=outValues[v];
							keys[v].mPosition.Z	=val;
						}
						ret	|=Animation.KeyPartsUsed.TranslateZ;
					}
				}
			}
			return	ret;
		}
	}
}