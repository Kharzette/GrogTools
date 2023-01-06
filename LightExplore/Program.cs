using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using InputLib;
using MaterialLib;
using UtilityLib;
using MeshLib;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using MatLib = MaterialLib.MaterialLib;


namespace LightExplore
{
	static class Program
	{
		public enum MyActions
		{
			MoveForwardBack, MoveForward, MoveBack,
			MoveLeftRight, MoveLeft, MoveRight,
			MoveForwardFast, MoveBackFast,
			MoveLeftFast, MoveRightFast,
			Turn, TurnLeft, TurnRight,
			Pitch, PitchUp, PitchDown,
			LightX, LightY, LightZ,
			ToggleMouseLookOn, ToggleMouseLookOff,
			IncrementFaceIndex, DecrementFaceIndex,
			BigIncrementFaceIndex, BigDecrementFaceIndex,
			ToggleWorld, SnapIndexToAimed, Close
		};

		const float	MaxTimeDelta	=0.1f;
		
		
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//turn this on for help with leaky stuff
			//Configuration.EnableObjectTracking	=true;

			GraphicsDevice	gd	=new GraphicsDevice("BSP Light Explorer",
				FeatureLevel.Level_11_0, 0.1f, 3000f);

			//save renderform position
			gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
					Settings.Default,
					"MainWindowPos", true,
					System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

			gd.RendForm.Location	=Settings.Default.MainWindowPos;

			SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

			StuffKeeper	sk		=new StuffKeeper();

			sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
			sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

			sk.Init(gd, ".");

			MatLib		matLib	=new MatLib(gd, sk);

			PlayerSteering	pSteering		=SetUpSteering();
			Input			inp				=SetUpInput();
			Random			rand			=new Random();
			CommonPrims		comPrims		=new CommonPrims(gd, sk);
			bool			bMouseLookOn	=false;
			LightExplorer	lex				=new LightExplorer(gd, sk);

			EventHandler	actHandler	=new EventHandler(
				delegate(object s, EventArgs ea)
				{	inp.ClearInputs();	});

			EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
				delegate(object s, EventArgs ea)
				{
					gd.SetCapture(false);
					bMouseLookOn	=false;
				});

			gd.RendForm.Activated		+=actHandler;
			gd.RendForm.AppDeactivated	+=deActHandler;

			int	resx	=gd.RendForm.ClientRectangle.Width;
			int	resy	=gd.RendForm.ClientRectangle.Height;

			Vector3	pos			=Vector3.One * 5f;
			Vector3	lightDir	=-Vector3.UnitY;

			UpdateTimer	time	=new UpdateTimer(true, false);

			time.SetFixedTimeStepSeconds(1f / 60f);	//60fps update rate
			time.SetMaxDeltaSeconds(MaxTimeDelta);

			List<Input.InputAction>	acts	=new List<Input.InputAction>();
			RenderLoop.Run(gd.RendForm, () =>
			{
				if(!gd.RendForm.Focused)
				{
					Thread.Sleep(33);
				}

				gd.CheckResize();

				if(bMouseLookOn && gd.RendForm.Focused)
				{
					gd.ResetCursorPos();
				}

				//Clear views
				gd.ClearViews();

				time.Stamp();
				while(time.GetUpdateDeltaSeconds() > 0f)
				{
					acts	=UpdateInput(inp, gd,
						time.GetUpdateDeltaSeconds(), ref bMouseLookOn);
					if(!gd.RendForm.Focused)
					{
						acts.Clear();
						bMouseLookOn	=false;
						gd.SetCapture(false);
						inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
						inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
					}

					//check for speediness
					//psteering only allows speedy ground sprinting
					bool	bFast	=false;
					for(int i=0;i < acts.Count;i++)
					{
						if(acts[i].mAction.Equals(MyActions.MoveForwardFast))
						{
							bFast	=true;
						}
					}

					Vector3	deltaMove	=pSteering.Update(pos,
						gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

					if(bFast)
					{
						deltaMove	*=400f;
					}
					else
					{
						deltaMove	*=200f;
					}
					pos			-=deltaMove;
					
					ChangeLight(acts, ref lightDir);

					lex.UpdateActions(acts);

					time.UpdateDone();
				}

				//light direction is backwards now for some strange reason
				matLib.SetParameterForAll("mLightDirection", -lightDir);				
				
				gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

				matLib.UpdateWVP(Matrix.Identity, gd.GCam.View, gd.GCam.Projection, gd.GCam.Position);

				comPrims.Update(gd.GCam, lightDir);

				lex.Update(time.GetUpdateDeltaMilliSeconds(), gd);
				lex.Render(gd);

				gd.Present();

				acts.Clear();
			}, true);	//true here is slow but needed for winforms events

			Settings.Default.Save();

			gd.RendForm.Activated		-=actHandler;
			gd.RendForm.AppDeactivated	-=deActHandler;

			lex.FreeAll();
			comPrims.FreeAll();
			inp.FreeAll();
			matLib.FreeAll();

			sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
			sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;

			sk.FreeAll();
			
			//Release all resources
			gd.ReleaseAll();
//			Application.Run(new Form1());
		}


		static List<Input.InputAction> UpdateInput(Input inp,
			GraphicsDevice gd, float delta, ref bool bMouseLookOn)
		{
			List<Input.InputAction>	actions	=inp.GetAction();

			//check for exit
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.Close))
				{
					gd.RendForm.Close();
					return	actions;
				}
			}

			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.ToggleMouseLookOn))
				{
					bMouseLookOn	=true;
					gd.SetCapture(true);
					inp.MapAxisAction(MyActions.Pitch, Input.MoveAxis.MouseYAxis);
					inp.MapAxisAction(MyActions.Turn, Input.MoveAxis.MouseXAxis);
				}
				else if(act.mAction.Equals(MyActions.ToggleMouseLookOff))
				{
					bMouseLookOn	=false;
					gd.SetCapture(false);
					inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
					inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
				}
			}

			//delta scale analogs, since there's no timestamp stuff in gamepad code
			foreach(Input.InputAction act in actions)
			{
				if(!act.mbTime && act.mDevice == Input.InputAction.DeviceType.ANALOG)
				{
					//analog needs a time scale applied
					act.mMultiplier	*=delta;
				}
			}

			//scale inputs to user prefs
			foreach(Input.InputAction act in actions)
			{
				if(act.mAction.Equals(MyActions.Turn)
					|| act.mAction.Equals(MyActions.TurnLeft)
					|| act.mAction.Equals(MyActions.TurnRight)
					|| act.mAction.Equals(MyActions.Pitch)
					|| act.mAction.Equals(MyActions.PitchDown)
					|| act.mAction.Equals(MyActions.PitchUp))
				{
					if(act.mDevice == Input.InputAction.DeviceType.MOUSE)
					{
						act.mMultiplier	*=UserSettings.MouseTurnMultiplier;
					}
					else if(act.mDevice == Input.InputAction.DeviceType.ANALOG)
					{
						act.mMultiplier	*=UserSettings.AnalogTurnMultiplier;
					}
					else if(act.mDevice == Input.InputAction.DeviceType.KEYS)
					{
						act.mMultiplier	*=UserSettings.KeyTurnMultiplier;
					}
				}
			}
			return	actions;
		}


		static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
		{
			foreach(Input.InputAction act in acts)
			{
				if(act.mAction.Equals(MyActions.LightX))
				{
					Matrix	rot	=Matrix.RotationX(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightY))
				{
					Matrix	rot	=Matrix.RotationY(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
				else if(act.mAction.Equals(MyActions.LightZ))
				{
					Matrix	rot	=Matrix.RotationZ(act.mMultiplier);
					lightDir	=Vector3.TransformCoordinate(lightDir, rot);
					lightDir.Normalize();
				}
			}
		}


		static Input SetUpInput()
		{
			Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency);
			
			inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.W);
			inp.MapAction(MyActions.MoveLeft, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.A);
			inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.S);
			inp.MapAction(MyActions.MoveRight, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.D);
			inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.W);
			inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.S);
			inp.MapAction(MyActions.MoveLeftFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.A);
			inp.MapAction(MyActions.MoveRightFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.D);

			//arrow keys
			inp.MapAction(MyActions.MoveForward, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Up);
			inp.MapAction(MyActions.MoveBack, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Down);
			inp.MapAction(MyActions.MoveForwardFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.Up);
			inp.MapAction(MyActions.MoveBackFast, ActionTypes.ContinuousHold,
				Modifiers.ShiftHeld, System.Windows.Forms.Keys.Down);
			inp.MapAction(MyActions.TurnLeft, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Left);
			inp.MapAction(MyActions.TurnRight, ActionTypes.ContinuousHold,
				Modifiers.None, System.Windows.Forms.Keys.Right);

			inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold, Modifiers.None, 16);
			inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold, Modifiers.None, 18);
			inp.MapAction(MyActions.LightX, ActionTypes.ContinuousHold, Modifiers.None, 36);
			inp.MapAction(MyActions.LightY, ActionTypes.ContinuousHold, Modifiers.None, 37);
			inp.MapAction(MyActions.LightZ, ActionTypes.ContinuousHold, Modifiers.None, 38);

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

			inp.MapAction(MyActions.IncrementFaceIndex, ActionTypes.PressAndRelease, Modifiers.None, Keys.PageUp);
			inp.MapAction(MyActions.DecrementFaceIndex, ActionTypes.PressAndRelease, Modifiers.None, Keys.PageDown);
			inp.MapAction(MyActions.BigIncrementFaceIndex, ActionTypes.PressAndRelease, Modifiers.ShiftHeld, Keys.PageUp);
			inp.MapAction(MyActions.BigDecrementFaceIndex, ActionTypes.PressAndRelease, Modifiers.ShiftHeld, Keys.PageDown);
			inp.MapAction(MyActions.SnapIndexToAimed, ActionTypes.PressAndRelease, Modifiers.None, Keys.E);

			inp.MapAction(MyActions.ToggleWorld, ActionTypes.PressAndRelease, Modifiers.None, Keys.X);

			inp.MapAction(MyActions.Close, ActionTypes.PressAndRelease, Modifiers.None, Keys.Escape);

			return	inp;
		}

		static PlayerSteering SetUpSteering()
		{
			PlayerSteering	pSteering	=new PlayerSteering();
			pSteering.Method			=PlayerSteering.SteeringMethod.Fly;

			pSteering.SetMoveEnums(MyActions.MoveForwardBack, MyActions.MoveLeftRight,
				MyActions.MoveForward, MyActions.MoveBack, MyActions.MoveLeft,
				MyActions.MoveRight, MyActions.MoveForwardFast, MyActions.MoveBackFast,
				MyActions.MoveLeftFast, MyActions.MoveRightFast);

			pSteering.SetTurnEnums(MyActions.Turn, MyActions.TurnLeft, MyActions.TurnRight);

			pSteering.SetPitchEnums(MyActions.Pitch, MyActions.PitchUp, MyActions.PitchDown);

			return	pSteering;
		}
	}
}
