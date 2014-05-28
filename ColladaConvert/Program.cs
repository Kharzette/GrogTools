using System;
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
			
			MatLib.ShaderModel	shaderModel;

			switch(gd.GD.FeatureLevel)
			{
				case	FeatureLevel.Level_11_0:
					shaderModel	=MatLib.ShaderModel.SM5;
					break;
				case	FeatureLevel.Level_10_1:
					shaderModel	=MatLib.ShaderModel.SM41;
					break;
				case	FeatureLevel.Level_10_0:
					shaderModel	=MatLib.ShaderModel.SM4;
					break;
				case	FeatureLevel.Level_9_3:
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
				default:
					Debug.Assert(false);	//only support the above
					shaderModel	=MatLib.ShaderModel.SM2;
					break;
			}

			MatLib	matLib		=new MatLib(gd.GD, shaderModel, true);

			matLib.InitCelShading(1);
			matLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);

			RenderTargetView	[]backBuf	=new RenderTargetView[1];
			DepthStencilView	backDepth;

			backBuf	=gd.DC.OutputMerger.GetRenderTargets(1, out backDepth);

			//set up post processing module
			PostProcess	post	=new PostProcess(gd, matLib.GetEffect("Post.fx"),
				gd.RendForm.ClientRectangle.Width, gd.RendForm.ClientRectangle.Height,
				backBuf[0], backDepth);

			PlayerSteering	pSteering	=SetUpSteering();
			Input			inp			=SetUpInput();
			Random			rand		=new Random();
			ExtraPrims		extraPrims	=new ExtraPrims(gd.GD, shaderModel);

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			post.MakePostTarget(gd, "SceneColor", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostDepth(gd, "SceneColor", resx, resy,
				(gd.GD.FeatureLevel != FeatureLevel.Level_9_3)?
					Format.D32_Float_S8X24_UInt : Format.D24_UNorm_S8_UInt);
			post.MakePostTarget(gd, "SceneDepthMatNorm", resx, resy, Format.R16G16B16A16_Float);
			post.MakePostTarget(gd, "Bleach", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Outline", resx, resy, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Bloom1", resx/2, resy/2, Format.R8G8B8A8_UNorm);
			post.MakePostTarget(gd, "Bloom2", resx/2, resy/2, Format.R8G8B8A8_UNorm);

			AnimForm	ss	=SetUpForms(gd.GD, matLib, extraPrims);

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

							gd.ToggleCapture(true);

							inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
							inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
						}
						else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
						{
							bMouseLookOn	=false;
							Debug.WriteLine("Mouse look: " + bMouseLookOn);

							gd.ToggleCapture(false);

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

				extraPrims.Update(gd.GCam, lightDir);

				matLib.SetCelTexture(0);

				//Clear views
				gd.ClearViews();

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;

				ss.RenderUpdate((float)delta / (float)Stopwatch.Frequency);

				post.SetTargets(gd, "SceneDepthMatNorm", "SceneColor");

				post.ClearTarget(gd, "SceneDepthMatNorm", Color.White);
				post.ClearDepth(gd, "SceneColor");

				ss.RenderDMN(gd.DC);

				post.SetTargets(gd, "SceneColor", "SceneColor");

				post.ClearTarget(gd, "SceneColor", Color.CornflowerBlue);
				post.ClearDepth(gd, "SceneColor");

				ss.Render(gd.DC);

				if(ss.GetDrawAxis())
				{
					extraPrims.DrawAxis(gd.DC);
				}

				if(ss.GetDrawBox())
				{
					extraPrims.DrawBox(gd.DC);
				}

				if(ss.GetDrawSphere())
				{
					extraPrims.DrawSphere(gd.DC);
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
			});

			Settings.Default.Save();
			
			//Release all resources
			gd.ReleaseAll();
		}

		static AnimForm SetUpForms(Device gd, MatLib matLib, ExtraPrims ep)
		{
			MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();
			AnimForm		ss		=new AnimForm(gd, matLib, animLib);
			StripElements	se		=new StripElements();
			SkeletonEditor	skel	=new SkeletonEditor();

			SharedForms.MaterialForm	matForm	=new SharedForms.MaterialForm(matLib);
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

			celForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "CelTweakFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			ss.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => ss.NukeMeshPart(sender as MeshLib.Mesh);
			matForm.eStripElements	+=(sender, args) =>
				{	if(se.Visible){	return;	}
					se.Populate(sender as List<MeshLib.Mesh>);	};
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
			
			inp.MapAction(MyActions.PitchUp, 16);
			inp.MapAction(MyActions.MoveForward, 17);
			inp.MapAction(MyActions.PitchDown, 18);
			inp.MapAction(MyActions.MoveLeft, 30);
			inp.MapAction(MyActions.MoveBack, 31);
			inp.MapAction(MyActions.MoveRight, 32);
			inp.MapAction(MyActions.LightX, 36);
			inp.MapAction(MyActions.LightY, 37);
			inp.MapAction(MyActions.LightZ, 38);

			inp.MapToggleAction(MyActions.BoostSpeedOn,
				MyActions.BoostSpeedOff,
				42);

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

			inp.MapAction(MyActions.LightX, Input.VariousButtons.GamePadDPadLeft);
			inp.MapAction(MyActions.LightY, Input.VariousButtons.GamePadDPadDown);
			inp.MapAction(MyActions.LightZ, Input.VariousButtons.GamePadDPadRight);

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
