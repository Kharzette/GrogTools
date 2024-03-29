﻿using System.Numerics;
using System.Diagnostics;
using UtilityLib;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using InputLib;
using MaterialLib;

//renderform and renderloop
using SharpDX.Windows;

namespace	EarlyTest;

internal class Program
{
	enum MyActions
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
		Exit
	};

	const float	MaxTimeDelta	=0.1f;
	const float	MoveScalar		=25f;


	[STAThread]
	static unsafe void Main(string []args)
	{
		//I have no idea what this does
		Application.EnableVisualStyles();

		Console.WriteLine("Hello, World!");

		Icon	testIcon	=new Icon("1281737606553.ico");

		FeatureLevel	f	=FeatureLevel.Level_11_0;

		GraphicsDevice	gd	=new GraphicsDevice("Goblin Test",
			testIcon, f, 0.1f, 2000f);

		PlayerSteering	pSteering		=SetUpSteering();
		Input			inp				=SetUpInput(gd.RendForm);
		UserSettings	sets			=new UserSettings();
		bool			bMouseLookOn	=false;

		//turn on sprint
		pSteering.SprintEnabled	=true;

		EventHandler	actHandler	=new EventHandler(
			delegate(object ?s, EventArgs ea)
			{	inp.ClearInputs();	});

		EventHandler<EventArgs>	deActHandler	=new EventHandler<EventArgs>(
			delegate(object ?s, EventArgs ea)
			{
				gd.SetCapture(false);
				bMouseLookOn	=false;
			});

		gd.RendForm.Activated		+=actHandler;
		gd.RendForm.AppDeactivated	+=deActHandler;

		StuffKeeper	sk	=new StuffKeeper();

		sk.Init(gd, ".");

		List<string>	fonts	=sk.GetFontList();

		ScreenText	st	=new ScreenText(gd, sk, fonts[1], fonts[1], 512);

		Matrix4x4	textProj	=Matrix4x4.CreateOrthographicOffCenter(
			0, gd.RendForm.Width, gd.RendForm.Height, 0, 0f, 1f);

		byte	[]vsBytes	=sk.GetVSCompiledCode("WNormWPosTexVS");

		//make some prims to draw
		PrimObject	prism	=PrimFactory.CreatePrism(gd.GD, vsBytes, 5f);
		PrimObject	sphere	=PrimFactory.CreateSphere(gd.GD, vsBytes, Vector3.Zero, 5f);
		PrimObject	box		=PrimFactory.CreateCube(gd.GD, vsBytes, 5f);
		PrimObject	cyl		=PrimFactory.CreateCylinder(gd.GD, vsBytes, 2f, 5f);

		CBKeeper	skcb	=sk.GetCBKeeper();

		Vector3	lightDir	=-Vector3.UnitY;

		skcb.SetTrilights(Vector3.One, Vector3.One * 0.3f,
			Vector3.One * 0.2f, lightDir);

		skcb.SetSpecular(Vector4.One, 15f);

		//grab shaders
		ID3D11VertexShader	vs	=sk.GetVertexShader("WNormWPosTexVS");
		ID3D11PixelShader	ps	=sk.GetPixelShader("TriTex0SpecPS");

		//grab a texture
		ID3D11ShaderResourceView	srv	=sk.GetSRV("RoughStone");

		gd.DC.VSSetShader(vs);
		gd.DC.PSSetShader(ps);
		gd.DC.PSSetShaderResource(0, srv);

		Random	rnd	=new Random();

		//randomize colours of the objects
		Vector4	prismColour		=Mathery.RandomColorVector4(rnd);
		Vector4	sphereColour	=Mathery.RandomColorVector4(rnd);
		Vector4	boxColour		=Mathery.RandomColorVector4(rnd);
		Vector4	cylColour		=Mathery.RandomColorVector4(rnd);

		//set A to 1
		prismColour.W	=sphereColour.W	=boxColour.W	=cylColour.W	=1f;

		Vector3	yawPitchRoll	=Vector3.Zero;
		Vector3	pos				=Vector3.One * 5f;

		st.AddString("Camera Location: " + pos,
			"Position",									//id
			Vector4.UnitX + Vector4.UnitW,				//color
			Vector2.UnitX * 20f + Vector2.UnitY * 400f,	//position
			Vector2.One);								//scale

		st.AddString("Blort: " + yawPitchRoll.X,
			"DummyJunx00",								//id
			Vector4.UnitX + Vector4.UnitW,				//color
			Vector2.UnitX * 20f + Vector2.UnitY * 420f,	//position
			Vector2.One);								//scale
		st.AddString("Blort: " + yawPitchRoll.Y,
			"DummyJunx01",								//id
			Vector4.UnitX + Vector4.UnitW,				//color
			Vector2.UnitX * 20f + Vector2.UnitY * 440f,	//position
			Vector2.One);								//scale
		st.AddString("Blort: " + yawPitchRoll.Z,
			"DummyJunx02",								//id
			Vector4.UnitX + Vector4.UnitW,				//color
			Vector2.UnitX * 20f + Vector2.UnitY * 460f,	//position
			Vector2.One);								//scale

		//using this to sort of visualize my coordinate system
		sphere.World	=Matrix4x4.CreateTranslation(Vector3.Zero);
		box.World		=Matrix4x4.CreateTranslation(Vector3.UnitX * 25f);
		prism.World		=Matrix4x4.CreateTranslation(Vector3.UnitY * 25f);
		cyl.World		=Matrix4x4.CreateTranslation(Vector3.UnitZ * 25f);

		//make samplers for 3d and 2d
		SamplerDescription	sd	=new SamplerDescription(
			Filter.MinMagMipPoint,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			TextureAddressMode.Wrap,
			0f, 16,
			ComparisonFunction.Less,
			0, float.MaxValue);

		ID3D11SamplerState	ss3D	=gd.GD.CreateSamplerState(sd);

		sd.Filter				=Filter.MinMagMipLinear;
		sd.AddressU				=TextureAddressMode.Clamp;
		sd.AddressV				=TextureAddressMode.Clamp;
		sd.ComparisonFunction	=ComparisonFunction.Never;

		ID3D11SamplerState	ss2D	=gd.GD.CreateSamplerState(sd);

		//depth stencil 3D
		DepthStencilDescription	dsd		=new DepthStencilDescription(true, DepthWriteMask.All, ComparisonFunction.Less);
		ID3D11DepthStencilState	dss3D	=gd.GD.CreateDepthStencilState(dsd);

		//2D
		dsd	=new DepthStencilDescription(false, DepthWriteMask.All, ComparisonFunction.Always);
		ID3D11DepthStencilState	dss2D	=gd.GD.CreateDepthStencilState(dsd);

		//blendstate
		BlendDescription	bd			=new BlendDescription(Blend.One, Blend.Zero);
		ID3D11BlendState	bsOpaque	=gd.GD.CreateBlendState(bd);

		bd	=new BlendDescription(Blend.One, Blend.InverseSourceAlpha, Blend.One, Blend.One);
		ID3D11BlendState	bsAlpha		=gd.GD.CreateBlendState(bd);

		Vortice.Mathematics.Color	zeroCol	=new Vortice.Mathematics.Color(0);
		Vortice.Mathematics.Color	oneCol	=new Vortice.Mathematics.Color(0xff);
		
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

			gd.ClearViews();
//			if(gd.RendForm.WindowState == FormWindowState.Minimized)
//			{
//				return;
//			}
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

				deltaMove	*=MoveScalar;
				pos			+=deltaMove;

				st.ModifyStringText("Camera Location: " + pos, "Position");
				st.ModifyStringText("Yaw	: " + pSteering.Yaw, "DummyJunx00");
				st.ModifyStringText("Pitch	: " + pSteering.Pitch, "DummyJunx01");
				st.ModifyStringText("Roll	: " + pSteering.Roll, "DummyJunx02");
				st.Update();

				if(ChangeLight(acts, ref lightDir))
				{
					skcb.SetTrilights(Vector3.One, Vector3.One * 0.3f,
						Vector3.One * 0.2f, lightDir);
				}
				
				time.UpdateDone();
			}

			//these can get unset if there's a resize/minimize/etc
			gd.DC.VSSetShader(vs);
			gd.DC.PSSetShader(ps);

			//samplers
			gd.DC.PSSetSampler(0, ss2D);
			gd.DC.PSSetSampler(3, ss3D);

			gd.DC.OMSetDepthStencilState(dss3D);

			skcb.SetCommonCBToShaders(gd.DC);

			gd.DC.PSSetShaderResource(0, srv);
			gd.DC.OMSetBlendState(bsOpaque, zeroCol);


			//for automatic spinny light
			/*
			yawPitchRoll.X	+=0.0001f;
			yawPitchRoll.Y	+=0.00005f;
			yawPitchRoll.Z	+=0.00007f;

			Mathery.WrapAngleDegrees(ref yawPitchRoll.X);
			Mathery.WrapAngleDegrees(ref yawPitchRoll.Y);
			Mathery.WrapAngleDegrees(ref yawPitchRoll.Z);

			perObject.mLightDirection	=Vector3.TransformNormal(Vector3.UnitX,
				Matrix4x4.CreateFromYawPitchRoll(yawPitchRoll.X,
					yawPitchRoll.Y, yawPitchRoll.Z));*/

			gd.GCam.Update(pos, pSteering.Pitch, pSteering.Yaw, pSteering.Roll);

			//update perframe data
			skcb.SetTransposedView(gd.GCam.ViewTransposed, pos);
			skcb.UpdateFrame(gd.DC);

			//per object shader vars
			skcb.SetWorldMat(prism.World);
			skcb.SetSolidColour(prismColour);
			skcb.UpdateObject(gd.DC);
			prism.Draw(gd.DC);

			skcb.SetWorldMat(sphere.World);
			skcb.SetSolidColour(sphereColour);
			skcb.UpdateObject(gd.DC);
			sphere.Draw(gd.DC);

			skcb.SetWorldMat(box.World);
			skcb.SetSolidColour(boxColour);
			skcb.UpdateObject(gd.DC);
			box.Draw(gd.DC);

			skcb.SetWorldMat(cyl.World);
			skcb.SetSolidColour(cylColour);
			skcb.UpdateObject(gd.DC);
			cyl.Draw(gd.DC);

			//set proj for 2D
			skcb.SetProjection(textProj);
			skcb.UpdateFrame(gd.DC);

			gd.DC.OMSetDepthStencilState(dss2D);
			gd.DC.OMSetBlendState(bsAlpha, oneCol);

			st.Draw();

			//change back to 3D
			skcb.SetTransposedProjection(gd.GCam.ProjectionTransposed);
			skcb.UpdateFrame(gd.DC);

			gd.Present();

			acts.Clear();
		});

		sets.SaveSettings();
		inp.FreeAll(gd.RendForm);
		sk.FreeAll();
		gd.ReleaseAll();
	}


	static Input SetUpInput(RenderForm rForm)
	{
		Input	inp	=new InputLib.Input(1f / Stopwatch.Frequency, rForm);
		
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

		inp.MapAction(MyActions.Exit, ActionTypes.ActivateOnce, Modifiers.None, Keys.Escape);

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

	static bool ChangeLight(List<Input.InputAction> acts, ref Vector3 lightDir)
	{
		bool	bChanged	=false;
		foreach(Input.InputAction act in acts)
		{
			if(act.mAction.Equals(MyActions.LightX))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationX(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
				bChanged	=true;
			}
			else if(act.mAction.Equals(MyActions.LightY))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationY(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
				bChanged	=true;
			}
			else if(act.mAction.Equals(MyActions.LightZ))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationZ(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
				bChanged	=true;
			}
		}
		return	bChanged;
	}
}