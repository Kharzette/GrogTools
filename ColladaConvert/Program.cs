﻿using System.Numerics;
using System.Diagnostics;
using UtilityLib;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using InputLib;
using MaterialLib;
using MeshLib;

//renderform and renderloop
using SharpDX.Windows;

using MatLib	=MaterialLib.MaterialLib;
using Color		=Vortice.Mathematics.Color;


namespace ColladaConvert;

internal static class Program
{
	internal enum MyActions
	{
		MoveForwardBack, MoveForward, MoveBack,
		MoveLeftRight, MoveLeft, MoveRight,
		MoveForwardFast, MoveBackFast,
		MoveLeftFast, MoveRightFast,
		Turn, TurnLeft, TurnRight,
		Pitch, PitchUp, PitchDown,
		LightX, LightY, LightZ,
		ToggleMouseLookOn, ToggleMouseLookOff,
		SensitivityUp, SensitivityDown,
		BoneRadiusUp, BoneLengthUp, BoneDepthUp, 
		BoneRadiusDown, BoneLengthDown, BoneDepthDown,
		BoneDone, BoneMirror, BoneSphereSnap,
		ColMoveX, ColMoveY, ColMoveZ, 
		ColMoveNX, ColMoveNY, ColMoveNZ, 
		Exit
	};

	const float	MaxTimeDelta	=0.1f;
	const float	MoveScalar		=1.25f;


	[STAThread]
	static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		Icon	testIcon	=new Icon("1281737606553.ico");

		//turn this on for help with leaky stuff
		//Configuration.EnableObjectTracking	=true;

		GraphicsDevice	gd	=new GraphicsDevice("Collada Conversion Tool",
			testIcon, FeatureLevel.Level_11_0, 0.1f, 3000f);

		//save renderform position
		gd.RendForm.DataBindings.Add(new System.Windows.Forms.Binding("Location",
				Properties.Settings.Default,
				"MainWindowPos", true,
				System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));

		gd.RendForm.Location	=Properties.Settings.Default.MainWindowPos;

		SharedForms.ShaderCompileHelper.mTitle	="Compiling Shaders...";

		StuffKeeper	sk		=new StuffKeeper();

		sk.eCompileNeeded	+=SharedForms.ShaderCompileHelper.CompileNeededHandler;
		sk.eCompileDone		+=SharedForms.ShaderCompileHelper.CompileDoneHandler;

		sk.Init(gd, ".");

		PlayerSteering	pSteering		=SetUpSteering();
		Input			inp				=SetUpInput(gd.RendForm);
		Random			rand			=new Random();
		bool			bMouseLookOn	=false;
		CBKeeper		cbk				=sk.GetCBKeeper();
		UserSettings	sets			=new UserSettings();
		FormStuff		fstuff			=new FormStuff(gd.GD, sk);
		ScreenText		st				=new ScreenText(gd, sk, "CGA", "CGA", 64);
		Matrix4x4		textProj		=Matrix4x4.CreateOrthographicOffCenter(
			0, gd.RendForm.Width, gd.RendForm.Height, 0, 0f, 1f);

		//turn on sprint
		pSteering.SprintEnabled	=true;

		//set up post processing module
		PostProcess	post	=new PostProcess(gd, sk);

		//default cel
		{
			Vector4	min	=new Vector4(0.0f, 0.3f, 0.6f, 1.0f);
			Vector4	max	=new Vector4(0.3f, 0.6f, 1.0f, 5.0f);
			Vector4	stp	=new Vector4(0.3f, 0.5f, 0.9f, 1.4f);

			cbk.SetCelSteps(min, max, stp, 4);
			cbk.UpdateCelStuff(gd.DC);
		}

		EventHandler	actHandler	=new EventHandler(
			delegate(object ?s, EventArgs ea)
			{	inp.ClearInputs();	});

		EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
			delegate(object ?s, EventArgs ea)
			{
				gd.SetCapture(false);
				bMouseLookOn	=false;
			});

		EventHandler	lostHandler	=new EventHandler(
			delegate(object ?s, EventArgs ea)
			{
				post.FreeAll(gd);
				post	=new PostProcess(gd, sk);
			});

		gd.eDeviceLost				+=lostHandler;
		gd.RendForm.Activated		+=actHandler;
		gd.RendForm.AppDeactivated	+=deActHandler;

		int	resx	=gd.RendForm.ClientRectangle.Width;
		int	resy	=gd.RendForm.ClientRectangle.Height;

		Vector3	pos			=Vector3.One * 5f;
		Vector3	lightDir	=-Vector3.UnitY;

		st.AddString("LightDir: " + lightDir.ToString(), "LightDir",
			Misc.SystemColorToV4Color(System.Drawing.Color.DarkRed),
			Vector2.One * 10f, Vector2.One * 2f);

		UpdateTimer	time	=new UpdateTimer(true, false);

		time.SetFixedTimeStepSeconds(1f / 60f);	//60fps update rate
		time.SetMaxDeltaSeconds(MaxTimeDelta);

		cbk.SetTransposedProjection(gd.GCam.ProjectionTransposed);

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

			time.Stamp();
			while(time.GetUpdateDeltaSeconds() > 0f)
			{
				acts	=UpdateInput(inp, sets, gd,
					time.GetUpdateDeltaSeconds(), ref bMouseLookOn);

				if(!gd.RendForm.Focused)
				{
					acts.Clear();
					bMouseLookOn	=false;
					gd.SetCapture(false);
					inp.UnMapAxisAction(Input.MoveAxis.MouseYAxis);
					inp.UnMapAxisAction(Input.MoveAxis.MouseXAxis);
				}

				Vector3	deltaMove	=pSteering.Update(pos,
					gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

				deltaMove	*=MoveScalar * fstuff.GetScaleFactor();
				pos			+=deltaMove;
				
				ChangeLight(acts, ref lightDir);
				fstuff.AdjustBone(acts);

				time.UpdateDone();
			}

			cbk.SetCommonCBToShaders(gd.DC);

			gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

			cbk.SetTransposedView(gd.GCam.ViewTransposed, pos);
			cbk.UpdateFrame(gd.DC);

			st.ModifyStringText("LightDir: " + lightDir.ToString(), "LightDir");

			fstuff.RenderUpdate(gd.GCam, lightDir, time.GetRenderUpdateDeltaSeconds());
			st.Update();

			post.SetTargets(gd, "BackColor", "BackDepth");

			post.ClearTarget(gd, "BackColor",
				Misc.SystemColorToDXColor(System.Drawing.Color.CornflowerBlue));
			post.ClearDepth(gd, "BackDepth");

			fstuff.Render();

			//set proj for 2D
			cbk.SetProjection(textProj);
			cbk.UpdateFrame(gd.DC);

			st.Draw();

			//change back to 3D
			cbk.SetTransposedProjection(gd.GCam.ProjectionTransposed);
			cbk.UpdateFrame(gd.DC);

			gd.Present();

			acts.Clear();
		}, true);   //true here is slow but needed for winforms events

		sets.SaveSettings();
		Properties.Settings.Default.Save();

		gd.RendForm.Activated		-=actHandler;
		gd.RendForm.AppDeactivated	-=deActHandler;

		inp.FreeAll(gd.RendForm);
		post.FreeAll(gd);
		fstuff.FreeAll();

		sk.eCompileDone		-=SharedForms.ShaderCompileHelper.CompileDoneHandler;
		sk.eCompileNeeded	-=SharedForms.ShaderCompileHelper.CompileNeededHandler;

		sk.FreeAll();
		
		//Release all resources
		gd.ReleaseAll();
	}


	static Input SetUpInput(RenderForm hwnd)
	{
		Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency, hwnd);
		
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
		inp.MapAction(MyActions.ColMoveY, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Up);
		inp.MapAction(MyActions.ColMoveNY, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Down);
		inp.MapAction(MyActions.ColMoveZ, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Up);
		inp.MapAction(MyActions.ColMoveNZ, ActionTypes.ContinuousHold,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Down);
		inp.MapAction(MyActions.ColMoveX, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Left);
		inp.MapAction(MyActions.ColMoveNX, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Right);

		inp.MapAction(MyActions.PitchUp, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.Q);
		inp.MapAction(MyActions.PitchDown, ActionTypes.ContinuousHold,
			Modifiers.None, System.Windows.Forms.Keys.E);

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

		//sensitivity adjust
		inp.MapAction(MyActions.SensitivityDown, ActionTypes.PressAndRelease,
			Modifiers.None, System.Windows.Forms.Keys.OemMinus);
		//for numpad
		inp.MapAction(MyActions.SensitivityUp, ActionTypes.PressAndRelease,
			Modifiers.None, System.Windows.Forms.Keys.Oemplus);
		//non numpad will have shift held too
		inp.MapAction(MyActions.SensitivityUp, ActionTypes.PressAndRelease,
			Modifiers.ShiftHeld, System.Windows.Forms.Keys.Oemplus);

		inp.MapAction(MyActions.BoneLengthUp, ActionTypes.ContinuousHold, Modifiers.None, Keys.T);
		inp.MapAction(MyActions.BoneRadiusUp, ActionTypes.ContinuousHold, Modifiers.None, Keys.R);
		inp.MapAction(MyActions.BoneDepthUp, ActionTypes.ContinuousHold, Modifiers.None, Keys.Y);
		inp.MapAction(MyActions.BoneLengthDown, ActionTypes.ContinuousHold, Modifiers.ShiftHeld, Keys.T);
		inp.MapAction(MyActions.BoneRadiusDown, ActionTypes.ContinuousHold, Modifiers.ShiftHeld, Keys.R);
		inp.MapAction(MyActions.BoneDepthDown, ActionTypes.ContinuousHold, Modifiers.ShiftHeld, Keys.Y);
		inp.MapAction(MyActions.BoneDone, ActionTypes.ActivateOnce, Modifiers.None, Keys.X);
		inp.MapAction(MyActions.BoneMirror, ActionTypes.ActivateOnce, Modifiers.None, Keys.M);
		inp.MapAction(MyActions.BoneSphereSnap, ActionTypes.ActivateOnce, Modifiers.None, Keys.C);

		inp.MapAction(MyActions.Exit, ActionTypes.ActivateOnce, Modifiers.ControlHeld, Keys.X);

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

	static List<Input.InputAction> UpdateInput(Input inp, UserSettings sets,
		GraphicsDevice gd, float delta, ref bool bMouseLookOn)
	{
		List<Input.InputAction>	actions	=inp.GetAction();

		//check for exit
		foreach(Input.InputAction act in actions)
		{
			if(act.mAction.Equals(MyActions.Exit))
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
					act.mMultiplier	*=UserSettings.MouseTurnMultiplier
						* sets.mTurnSensitivity;
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

		//sensitivity adjust
		foreach(Input.InputAction act in actions)
		{
			float	sense	=sets.mTurnSensitivity;
			if(act.mAction.Equals(MyActions.SensitivityUp))
			{
				sense	+=0.025f;
			}
			else if(act.mAction.Equals(MyActions.SensitivityDown))
			{
				sense	-=0.025f;
			}
			else
			{
				continue;
			}
			sets.mTurnSensitivity	=Math.Clamp(sense, 0.025f, 10f);
		}

		

		return	actions;
	}


	static void AdjustBone(List<Input.InputAction> acts)
	{
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(MyActions.BoneLengthUp))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneRadiusUp))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneLengthDown))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneRadiusDown))
			{
			}
			else if(act.mAction.Equals(MyActions.BoneDone))
			{
			}
		}
	}


	static void ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
	{
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(MyActions.LightX))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationX(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
			else if(act.mAction.Equals(MyActions.LightY))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationY(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
			else if(act.mAction.Equals(MyActions.LightZ))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationZ(act.mMultiplier);
				lightDir		=Mathery.TransformCoordinate(lightDir, ref rot);
				lightDir		=Vector3.Normalize(lightDir);
			}
		}
	}

	static void DeleteVertElement(ID3D11Device gd, List<int> inds, List<Mesh> meshes)
	{
		Type	firstType	=meshes[0].VertexType;

		foreach(Mesh m in meshes)
		{
			if(m.VertexType == firstType)
			{
				m.DeleteVertElement(gd, inds);
			}
		}
	}
}