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
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MatLib = MaterialLib.MaterialLib;


namespace ColladaStartSmall
{
	internal static class Program
	{
		static bool	mbResized;

		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLookOn, ToggleMouseLookOff
		};

		static void OnRenderFormResize(object sender, EventArgs ea)
		{
			mbResized	=true;
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


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			RenderForm	renderForm	=new RenderForm("Collada Conversion Tool");

			GraphicsDevice	gd	=new GraphicsDevice(renderForm, FeatureLevel.Level_11_0);

			renderForm.UserResized	+=OnRenderFormResize;

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

			Random	rand	=new Random();

			UtilityLib.GameCamera	gcam	=new UtilityLib.GameCamera(renderForm.ClientSize.Width, renderForm.ClientSize.Height, 16f/9f, 0.1f, 2000f);

			Input	inp	=new InputLib.Input();

			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveLeftRight, MyActions.MoveLeft, MyActions.MoveRight,
				MyActions.MoveForwardBack, MyActions.MoveForward, MyActions.MoveBack);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);
			
			inp.MapAction(MyActions.PitchUp, 16);
			inp.MapAction(MyActions.MoveForward, 17);
			inp.MapAction(MyActions.PitchDown, 18);
			inp.MapAction(MyActions.MoveLeft, 30);
			inp.MapAction(MyActions.MoveBack, 31);
			inp.MapAction(MyActions.MoveRight, 32);
			inp.MapAction(MyActions.LightX, 36);
			inp.MapAction(MyActions.LightY, 37);
			inp.MapAction(MyActions.LightZ, 38);

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

			MeshLib.AnimLib	animLib	=new MeshLib.AnimLib();

			ExtraPrims	extraPrims	=new ExtraPrims(gd.GD, shaderModel);

			StartSmall		ss		=new StartSmall(gd.GD, matLib, animLib);
			MaterialForm	matForm	=new MaterialForm(matLib);
			StripElements	se		=new StripElements();
			SkeletonEditor	skel	=new SkeletonEditor();
			CelTweakForm	celForm	=new CelTweakForm(gd.GD, matLib);

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

			renderForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			renderForm.Location	=Settings.Default.MainWindowPos;

			ss.eMeshChanged			+=(sender, args) => matForm.SetMesh(sender);
			matForm.eNukedMeshPart	+=(sender, args) => ss.NukeMeshPart(sender as MeshLib.Mesh);
			matForm.eStripElements	+=(sender, args) =>
				{	if(se.Visible){	return;	}
					se.Populate(sender as List<MeshLib.Mesh>);	};
			se.eDeleteElement		+=(sender, args) =>
				{	DeleteVertElement(gd.GD, sender as List<int>, se.GetMeshes());
					se.Populate(null);	se.Visible	=false;
					matForm.RefreshMeshPartList();	};
			se.eEscape				+=(sender, args) =>
				{	se.Populate(null);	se.Visible	=false;	};
			ss.eSkeletonChanged		+=(sender, args) => skel.Initialize(sender as MeshLib.Skeleton);
			ss.eBoundsChanged		+=(sender, args) => extraPrims.ReBuildBoundsDrawData(gd.GD, sender);

			ss.Visible		=true;
			matForm.Visible	=true;
			skel.Visible	=true;
			celForm.Visible	=true;

			Vector3	pos				=Vector3.One * 5f;
			Vector3	lightDir		=-Vector3.UnitY;
			bool	bMouseLookOn	=false;
			long	lastTime		=Stopwatch.GetTimestamp();

			//keep track of mouse pos during mouse look
			System.Drawing.Point		StoredMousePos	=System.Drawing.Point.Empty;
			System.Drawing.Rectangle	StoredClipRect	=Cursor.Clip;


			RenderLoop.Run(renderForm, () =>
			{
				if(mbResized)
				{
					gd.HandleResize(renderForm.ClientSize.Width,
						renderForm.ClientSize.Height, ref gcam);
					mbResized	=false;
				}

				if(bMouseLookOn)
				{
					Cursor.Position	=StoredMousePos;
				}

				List<Input.InputAction>	actions	=inp.GetAction();
				if(!renderForm.Focused)
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

							renderForm.Capture	=bMouseLookOn;

							inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
							inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
							Cursor.Hide();

							StoredMousePos	=Cursor.Position;

							Cursor.Clip	=renderForm.RectangleToScreen(renderForm.ClientRectangle);
						}
						else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
						{
							bMouseLookOn	=false;
							Debug.WriteLine("Mouse look: " + bMouseLookOn);

							renderForm.Capture	=bMouseLookOn;

							inp.UnMapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
							inp.UnMapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
							Cursor.Show();
							Cursor.Clip	=StoredClipRect;
						}
					}
				}

				ChangeLight(actions, ref lightDir);

				//light direction is backwards now for some strange reason
				matLib.SetParameterForAll("mLightDirection", -lightDir);
				
				pos	=pSteering.Update(pos, gcam.Forward, gcam.Left, gcam.Up, actions);
				
				gcam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.SetParameterForAll("mView", gcam.View);
				matLib.SetParameterForAll("mEyePos", gcam.Position);
				matLib.SetParameterForAll("mProjection", gcam.Projection);

				extraPrims.Update(gcam, lightDir);

				matLib.SetCelTexture(0);

				//Clear views
				gd.ClearViews();

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;

				ss.Render(gd.DC, (float)delta / (float)Stopwatch.Frequency);
				
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

				gd.Present();

				lastTime	=timeNow;
			});

			Settings.Default.Save();
			
			//Release all resources
			gd.ReleaseAll();
		}
	}
}
