using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using InputLib;
using MaterialLib;
using UtilityLib;
using ParticleLib;
using MeshLib;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Buffer	=SharpDX.Direct3D11.Buffer;
using Device	=SharpDX.Direct3D11.Device;
using MapFlags	=SharpDX.Direct3D11.MapFlags;
using MatLib	=MaterialLib.MaterialLib;


namespace ParticleEdit
{
	static class Program
	{
		enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			ToggleMouseLookOn, ToggleMouseLookOff
		};

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			GraphicsDevice	gd	=new GraphicsDevice("Particle Editing Tool", FeatureLevel.Level_11_0);

			//save renderform position
			gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;

#if DEBUG
			string	rootDir	="C:\\Games\\CurrentGame";
#else
			string	rootDir	=AppDomain.CurrentDomain.BaseDirectory;
#endif

			StuffKeeper	sk		=new StuffKeeper(gd, rootDir);
			MatLib		matLib	=new MatLib(gd, sk);
			CommonPrims	cprims	=new CommonPrims(gd, sk);

			matLib.InitCelShading(1);
			matLib.GenerateCelTexturePreset(gd.GD,
				gd.GD.FeatureLevel == FeatureLevel.Level_9_3, false, 0);
			matLib.SetCelTexture(0);

			PlayerSteering	pSteering	=SetUpSteering();
			Input			inp			=SetUpInput();
			Random			rand		=new Random();
			ParticleForm	partForm	=SetUpForms(gd.GD, matLib, sk);
			ParticleEditor	partEdit	=new ParticleEditor(gd, partForm, matLib);

			Vector3	pos			=Vector3.One * 5f;
			Vector3	lightDir	=-Vector3.UnitY;
			long	lastTime	=Stopwatch.GetTimestamp();

			RenderLoop.Run(gd.RendForm, () =>
			{
				gd.CheckResize();

				List<Input.InputAction>	actions	=UpdateInput(inp, gd);

				pos	=pSteering.Update(pos, gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, actions);
				
				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.SetParameterForAll("mView", gd.GCam.View);
				matLib.SetParameterForAll("mEyePos", gd.GCam.Position);
				matLib.SetParameterForAll("mProjection", gd.GCam.Projection);

				cprims.Update(gd.GCam, lightDir);

				//Clear views
				gd.ClearViews();

				long	timeNow	=Stopwatch.GetTimestamp();
				long	delta	=timeNow - lastTime;
				float	msFreq	=Stopwatch.Frequency / 1000f;

				float	msDelta	=((float)delta / msFreq);

				cprims.DrawAxis(gd.DC);

				partEdit.Update(msDelta);
				partEdit.Draw();

				gd.Present();

				lastTime	=timeNow;
			}, true);	//true here is slow but needed for winforms events

			matLib.FreeAll();

			Settings.Default.Save();
			
			//Release all resources
			gd.ReleaseAll();
		}

		
		static ParticleForm SetUpForms(Device gd, MatLib matLib, StuffKeeper sk)
		{
			ParticleForm	partForm	=new ParticleForm(matLib, sk);

			//save positions
			partForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Settings.Default, "ParticleFormPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			partForm.Visible	=true;

			return	partForm;
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

			inp.MapToggleAction(MyActions.ToggleMouseLookOn,
				MyActions.ToggleMouseLookOff,
				Input.VariousButtons.RightMouseButton);

			inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.GamePadRightYAxis);
			inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.GamePadRightXAxis);
			inp.MapAxisAction(MyActions.MoveLeftRight, Input.MoveAxis.GamePadLeftXAxis);
			inp.MapAxisAction(MyActions.MoveForwardBack, Input.MoveAxis.GamePadLeftYAxis);

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

		static List<Input.InputAction> UpdateInput(Input inp, GraphicsDevice gd)
		{
			if(gd.RendForm.Capture)
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
						gd.SetCapture(true);

						inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
						inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
					}
					else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
					{
						gd.SetCapture(false);

						inp.UnMapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
						inp.UnMapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
					}
				}
			}
			return	actions;
		}
	}
}
