﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InputLib;
using MaterialLib;
using UtilityLib;
using MeshLib;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;
using MapFlags	=SharpDX.Direct3D11.MapFlags;
using MatLib	=MaterialLib.MaterialLib;


namespace ColladaConvert
{
	internal static class Program
	{
		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLookOn, ToggleMouseLookOff,
			BoostSpeedOn, BoostSpeedOff
		};


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GraphicsDevice	gd	=new GraphicsDevice("Collada Conversion Tool",
				FeatureLevel.Level_9_3);

			//save renderform position
			gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;
			
			StuffKeeper	sk		=new StuffKeeper(gd, "C:\\Games\\CurrentGame");
			MatLib		matLib	=new MatLib(gd, sk);

			matLib.InitCelShading(1);
			matLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
			matLib.SetCelTexture(0);

			RenderTargetView	[]backBuf	=new RenderTargetView[1];
			DepthStencilView	backDepth;

			backBuf	=gd.DC.OutputMerger.GetRenderTargets(1, out backDepth);

			//set up post processing module
			PostProcess	post	=new PostProcess(gd, matLib.GetEffect("Post.fx"));

			PlayerSteering	pSteering	=SetUpSteering();
			Input			inp			=SetUpInput();
			Random			rand		=new Random();
			CommonPrims		comPrims	=new CommonPrims(gd, sk);

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			post.MakePostTarget(gd, "SceneColor", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostDepth(gd, "SceneDepth", resx, resy,
				(gd.GD.FeatureLevel != FeatureLevel.Level_9_3)?
					Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt);
			post.MakePostTarget(gd, "SceneDepthMatNorm", resx, resy, Format.R16G16B16A16_Float);
			post.MakePostTarget(gd, "Bleach", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Outline", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Bloom1", resx/2, resy/2, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Bloom2", resx/2, resy/2, Format.R8G8B8A8_UNorm);

			AnimForm	ss	=SetUpForms(gd.GD, matLib, sk, comPrims);

			Vector3	pos				=Vector3.One * 5f;
			Vector3	lightDir		=-Vector3.UnitY;
			bool	bMouseLookOn	=false;
			long	lastTime		=Stopwatch.GetTimestamp();

			RenderLoop.Run(gd.RendForm, () =>
			{
				gd.CheckResize();

				if(bMouseLookOn)
				{
					gd.ResetCursorPos();
				}

				List<Input.InputAction>	actions	=inp.GetAction();
				if(!gd.RendForm.Focused)
				{
					actions.Clear();
				}
				else
				{
					foreach(Input.InputAction act in actions)
					{
						if(act.mAction.Equals(MyActions.ToggleMouseLookOn))
						{
							bMouseLookOn	=true;
							Debug.WriteLine("Mouse look: " + bMouseLookOn);

							gd.SetCapture(true);

							inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
							inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
						}
						else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
						{
							bMouseLookOn	=false;
							Debug.WriteLine("Mouse look: " + bMouseLookOn);

							gd.SetCapture(false);

							inp.UnMapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
							inp.UnMapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
						}
						else if(act.mAction.Equals(MyActions.BoostSpeedOn))
						{
							pSteering.Speed	=2;
						}
						else if(act.mAction.Equals(MyActions.BoostSpeedOff))
						{
							pSteering.Speed	=0.5f;
						}
					}
				}

				ChangeLight(actions, ref lightDir);

				//light direction is backwards now for some strange reason
				matLib.SetParameterForAll("mLightDirection", -lightDir);
				
				pos	=pSteering.Update(pos, gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, actions);
				
				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.SetParameterForAll("mView", gd.GCam.View);
				matLib.SetParameterForAll("mEyePos", gd.GCam.Position);
				matLib.SetParameterForAll("mProjection", gd.GCam.Projection);

				comPrims.Update(gd.GCam, lightDir);

				//Clear views
				gd.ClearViews();

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;

				ss.RenderUpdate((float)delta / (float)Stopwatch.Frequency);

				post.SetTargets(gd, "SceneDepthMatNorm", "SceneDepth");

				post.ClearTarget(gd, "SceneDepthMatNorm", Color.White);
				post.ClearDepth(gd, "SceneDepth");

				ss.RenderDMN(gd.DC);

				post.SetTargets(gd, "SceneColor", "SceneDepth");

				post.ClearTarget(gd, "SceneColor", Color.CornflowerBlue);
				post.ClearDepth(gd, "SceneDepth");

				ss.Render(gd.DC);

				if(ss.GetDrawAxis())
				{
					comPrims.DrawAxis(gd.DC);
				}

				if(ss.GetDrawBox())
				{
					comPrims.DrawBox(gd.DC);
				}

				if(ss.GetDrawSphere())
				{
					comPrims.DrawSphere(gd.DC);
				}

				post.SetTargets(gd, "Outline", "null");
				post.SetParameter("mNormalTex", "SceneDepthMatNorm");
				post.DrawStage(gd, "Outline");

				post.SetTargets(gd, "Bleach", "null");
				post.SetParameter("mColorTex", "SceneColor");
				post.DrawStage(gd, "BleachBypass");

				post.SetTargets(gd, "Bloom1", "null");
				post.SetParameter("mBlurTargetTex", "Bleach");
				post.DrawStage(gd, "BloomExtract");

				post.SetTargets(gd, "Bloom2", "null");
				post.SetParameter("mBlurTargetTex", "Bloom1");
				post.DrawStage(gd, "GaussianBlurX");

				post.SetTargets(gd, "Bloom1", "null");
				post.SetParameter("mBlurTargetTex", "Bloom2");
				post.DrawStage(gd, "GaussianBlurY");

				post.SetTargets(gd, "SceneColor", "null");
				post.SetParameter("mBlurTargetTex", "Bloom1");
				post.SetParameter("mColorTex", "Bleach");
				post.DrawStage(gd, "BloomCombine");

				post.SetTargets(gd, "BackColor", "BackDepth");
				post.SetParameter("mBlurTargetTex", "Outline");
				post.SetParameter("mColorTex", "SceneColor");
				post.DrawStage(gd, "Modulate");
				
				gd.Present();

				lastTime	=timeNow;
			}, true);	//true here is slow but needed for winforms events

			Settings.Default.Save();
			
			//Release all resources
			gd.ReleaseAll();
		}

		static AnimForm SetUpForms(Device gd, MatLib matLib, StuffKeeper sk, CommonPrims ep)
		{
			MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();
			AnimForm		ss		=new AnimForm(gd, matLib, animLib);
			StripElements	se		=new StripElements();
			SkeletonEditor	skel	=new SkeletonEditor();
			SeamEditor		seam	=new SeamEditor();

			SharedForms.MaterialForm	matForm	=new SharedForms.MaterialForm(matLib, sk);
			SharedForms.CelTweakForm	celForm	=new SharedForms.CelTweakForm(gd, matLib);

			//save positions
			matForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "MaterialFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			ss.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "AnimFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			skel.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "SkeletonEditorFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			skel.DataBindings.Add(new System.Windows.Forms.Binding("Size",
				Settings.Default, "SkeletonEditorFormSize", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			seam.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "SeamEditorFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			seam.DataBindings.Add(new System.Windows.Forms.Binding("Size",
				Settings.Default, "SeamEditorFormSize", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			celForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "CelTweakFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			ss.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => ss.NukeMeshPart(sender as MeshLib.Mesh);
			matForm.eStripElements	+=(sender, args) =>
				{	if(se.Visible){	return;	}
					se.Populate(sender as List<MeshLib.Mesh>);	};
			matForm.eFindSeams		+=(sender, args) =>
				{	seam.Initialize(sender as List<Mesh>, gd);	};
			matForm.eSeamFound		+=(sender, args) =>
				{	seam.AddSeam(sender as EditorMesh.WeightSeam);	};
			matForm.eSeamsDone		+=(sender, args) =>
				{	seam.SizeColumns();
					seam.Visible	=true;	};
			se.eDeleteElement		+=(sender, args) =>
				{	DeleteVertElement(gd, sender as List<int>, se.GetMeshes());
					se.Populate(null);	se.Visible	=false;
					matForm.RefreshMeshPartList();	};
			se.eEscape				+=(sender, args) =>
				{	se.Populate(null);	se.Visible	=false;	};
			ss.eSkeletonChanged		+=(sender, args) => skel.Initialize(sender as MeshLib.Skeleton);
			ss.eBoundsChanged		+=(sender, args) => ep.ReBuildBoundsDrawData(gd, sender);			

			ss.Visible		=true;
			matForm.Visible	=true;
			skel.Visible	=true;
			celForm.Visible	=true;

			return	ss;
		}

		static Input SetUpInput()
		{
			Input	inp	=new InputLib.Input();
			
			inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold, Modifiers.None, 16);
			inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold, Modifiers.None, 17);
			inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold, Modifiers.None, 18);
			inp.MapAction(MyActions.MoveLeft, ActionTypes.ContinuousHold, Modifiers.None, 30);
			inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold, Modifiers.None, 31);
			inp.MapAction(MyActions.MoveRight, ActionTypes.ContinuousHold, Modifiers.None, 32);
			inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, 36);
			inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, 37);
			inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, 38);

			inp.MapToggleAction(MyActions.BoostSpeedOn,
				MyActions.BoostSpeedOff, Modifiers.None,
				42);

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff, Modifiers.None,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

			inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadLeft);
			inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadDown);
			inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, Input.VariousButtons.GamePadDPadRight);

			return	inp;
		}

		static PlayerSteering SetUpSteering()
		{
			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveLeftRight, MyActions.MoveLeft, MyActions.MoveRight,
				MyActions.MoveForwardBack, MyActions.MoveForward, MyActions.MoveBack);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);

			pSteering.Speed	=0.5f;

			return	pSteering;
		}

		static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
		{
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(MyActions.LightX))
				{
					Matrix	rot	=Matrix.RotationX(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightY))
				{
					Matrix	rot	=Matrix.RotationY(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightZ))
				{
					Matrix	rot	=Matrix.RotationZ(act.mMultiplier * 0.001f);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
			}
		}

		static void DeleteVertElement(Device gd, List<int> inds, List<Mesh> meshes)
		{
			Type	firstType	=meshes[0].VertexType;

			foreach(Mesh m in meshes)
			{
				if(m.VertexType == firstType)
				{
					EditorMesh	em	=m as EditorMesh;

					em.NukeVertexElement(inds, gd);
				}
			}
		}
	}
}
