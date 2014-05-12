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


namespace BSPBuilder
{
	static class Program
	{
		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLookOn, ToggleMouseLookOff
		};


		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			GraphicsDevice	gd	=new GraphicsDevice("Binary Space Partitioning tree building tools",
				FeatureLevel.Level_11_0);

			//save renderform position
			gd.RendForm.DataBindings.Add(new Binding("Location",
				Settings.Default, "MainWindowPos", true,
				DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;
			
			PlayerSteering	pSteering	=SetUpSteering();
			Input			inp			=SetUpInput();
			Random			rand		=new Random();

			BSPBuilder	bspBuild	=new BSPBuilder(gd);

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
					}
				}

				pos	=pSteering.Update(pos, gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, actions);
				
				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				//Clear views
				gd.ClearViews();

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;

				bspBuild.Update((float)delta / (float)Stopwatch.Frequency, gd);

				bspBuild.Render(gd);
				
				gd.Present();

				lastTime	=timeNow;
			});

			Settings.Default.Save();
			
			//Release all resources
			gd.ReleaseAll();
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

			return	pSteering;
		}
	}
}
