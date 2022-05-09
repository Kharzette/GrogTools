using System;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UtilityLib;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.D3DCompiler;
using SharpGen.Runtime;
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
		Exit
	};

	const float	MaxTimeDelta	=0.1f;

	//CommonFunctions.hlsli
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct PerFrame
	{
		internal Matrix4x4	mView;
		internal Matrix4x4	mLightViewProj;	//for shadows
		internal Vector3	mEyePos;
		internal UInt32		mPadding;		//pad to 16 boundary
	}

	//CommonFunctions.hlsli
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct PerObject
	{
		internal Matrix4x4	mWorld;
		internal Vector4	mSolidColour;
		internal Vector4	mSpecColor;

		//These are considered directional (no falloff)
		internal Vector4	mLightColor0;		//trilights need 3 colors
		internal Vector4	mLightColor1;		//trilights need 3 colors
		internal Vector4	mLightColor2;		//trilights need 3 colors

		internal Vector3	mLightDirection;
		internal float		mSpecPower;

		//material id for borders etc
		internal int		mMaterialID;
		internal UInt32		mPadding0;
		internal UInt32		mPadding1;
		internal UInt32		mPadding2;	//pad to 16
	}

	//CommonFunctions.hlsli
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	struct ChangeLess
	{
		internal Matrix4x4	mProjection;
	}

	[STAThread]
	static unsafe void Main(string []args)
	{
		//I have no idea what this does
		Application.EnableVisualStyles();

		Console.WriteLine("Hello, World!");

		Icon	testIcon	=new Icon("1281737606553.ico");

		FeatureLevel	f	=FeatureLevel.Level_11_0;

		GraphicsDevice	gd	=new GraphicsDevice("Goblin Test", testIcon, f, 0.1f, 2000f);

		PlayerSteering	pSteering		=SetUpSteering();
		Input			inp				=SetUpInput(gd.RendForm);
		bool			bMouseLookOn	=false;

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
			0, gd.RendForm.Width, gd.RendForm.Height, 0, -5f, 5f);

		byte	[]vsBytes	=sk.GetVSCompiledCode("WNormWPosTexVS");

		//make some prims to draw
		PrimObject	prism	=PrimFactory.CreatePrism(gd.GD, vsBytes, 5f);
		PrimObject	sphere	=PrimFactory.CreateSphere(gd.GD, vsBytes, Vector3.Zero, 5f);
		PrimObject	box		=PrimFactory.CreateCube(gd.GD, vsBytes, 5f);
		PrimObject	cyl		=PrimFactory.CreateCylinder(gd.GD, vsBytes, 2f, 5f);

		//create constant buffers
		ID3D11Buffer	perObjectBuf	=StuffKeeper.MakeConstantBuffer(gd.GD, sizeof(PerObject));
		ID3D11Buffer	perFrameBuf		=StuffKeeper.MakeConstantBuffer(gd.GD, sizeof(PerFrame));
		ID3D11Buffer	changeLessBuf	=StuffKeeper.MakeConstantBuffer(gd.GD, sizeof(ChangeLess));

		//alloc C# side constant buffer data
		PerObject	perObject	=new PerObject();
		PerFrame	perFrame	=new PerFrame();
		ChangeLess	changeLess	=new ChangeLess();

		perObject.mLightColor0	=Vector4.One;
		perObject.mLightColor1	=Vector4.One * 0.3f;
		perObject.mLightColor2	=Vector4.One * 0.2f;

		perObject.mLightColor1.W	=perObject.mLightColor2.W	=1f;

		perObject.mLightDirection	=-Vector3.UnitY;
		perObject.mSolidColour		=Vector4.One;
		perObject.mSpecColor		=Vector4.One;
		perObject.mSpecPower		=5f;

		changeLess.mProjection		=Matrix4x4.Transpose(gd.GCam.Projection);

		//put values in changeLess
		gd.DC.UpdateSubresource<ChangeLess>(changeLess, changeLessBuf);

		//assign cbuffers to shaders
		gd.DC.VSSetConstantBuffer(0, perObjectBuf);
		gd.DC.PSSetConstantBuffer(0, perObjectBuf);
		gd.DC.VSSetConstantBuffer(1, perFrameBuf);
		gd.DC.PSSetConstantBuffer(1, perFrameBuf);
		gd.DC.VSSetConstantBuffer(2, changeLessBuf);
		gd.DC.PSSetConstantBuffer(2, changeLessBuf);

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

		st.AddString("Camera Location: " + pos, "Position", Vector4.UnitY + Vector4.UnitW, Vector2.UnitX * 20f + Vector2.UnitY * 400f, Vector2.One);

		prism.World		=Matrix4x4.CreateTranslation(Vector3.UnitX * 15f);
		sphere.World	=Matrix4x4.CreateTranslation(Vector3.UnitX * -15f);
		box.World		=Matrix4x4.CreateTranslation(Vector3.UnitZ * 15f);
		cyl.World		=Matrix4x4.CreateTranslation(Vector3.UnitZ * -15f);

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
		DepthStencilDescription	dsd	=new DepthStencilDescription(true, DepthWriteMask.All, ComparisonFunction.Less);
		ID3D11DepthStencilState	dss3D	=gd.GD.CreateDepthStencilState(dsd);

		//2D
		dsd	=new DepthStencilDescription(false, DepthWriteMask.All, ComparisonFunction.Always);
		ID3D11DepthStencilState	dss2D	=gd.GD.CreateDepthStencilState(dsd);

		//blendstate
		BlendDescription	bd	=new BlendDescription(Blend.One, Blend.Zero);
		ID3D11BlendState	bsOpaque	=gd.GD.CreateBlendState(bd);

		bd	=new BlendDescription(Blend.One, Blend.InverseSourceAlpha, Blend.Zero, Blend.Zero);
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

				Vector3	deltaMove	=pSteering.Update(pos,
					gd.GCam.Forward, gd.GCam.Left, gd.GCam.Up, acts);

				deltaMove	*=200f;
				pos			+=deltaMove;

				st.ModifyStringText("Camera Location: " + pos, "Position");
				st.Update();

				ChangeLight(acts, ref perObject.mLightDirection);
				
				time.UpdateDone();
			}

			//these can get unset if there's a resize/minimize/etc
			gd.DC.VSSetShader(vs);
			gd.DC.PSSetShader(ps);

			gd.DC.PSSetSampler(0, ss3D);
			gd.DC.OMSetDepthStencilState(dss3D);

			gd.DC.VSSetConstantBuffer(0, perObjectBuf);
			gd.DC.PSSetConstantBuffer(0, perObjectBuf);
			gd.DC.VSSetConstantBuffer(1, perFrameBuf);
			gd.DC.PSSetConstantBuffer(1, perFrameBuf);
			gd.DC.VSSetConstantBuffer(2, changeLessBuf);
			gd.DC.PSSetConstantBuffer(2, changeLessBuf);

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
			perFrame.mEyePos		=gd.GCam.Position;
			perFrame.mLightViewProj	=Matrix4x4.Transpose(Matrix4x4.Identity);
			perFrame.mView			=Matrix4x4.Transpose(gd.GCam.View);

			//update values in perFrame
			gd.DC.UpdateSubresource<PerFrame>(perFrame, perFrameBuf);

			//per object shader vars
			perObject.mWorld		=Matrix4x4.Transpose(prism.World);
			perObject.mSolidColour	=prismColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			prism.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(sphere.World);
			perObject.mSolidColour	=sphereColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			sphere.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(box.World);
			perObject.mSolidColour	=boxColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			box.Draw(gd);

			perObject.mWorld		=Matrix4x4.Transpose(cyl.World);
			perObject.mSolidColour	=cylColour;
			gd.DC.UpdateSubresource<PerObject>(perObject, perObjectBuf);
			cyl.Draw(gd);

			//set up for 2D
			changeLess.mProjection	=Matrix4x4.Transpose(textProj);
			gd.DC.UpdateSubresource<ChangeLess>(changeLess, changeLessBuf);

			perFrame.mView	=Matrix4x4.Transpose(Matrix4x4.Identity);
			gd.DC.UpdateSubresource<PerFrame>(perFrame, perFrameBuf);

			gd.DC.PSSetSampler(0, ss2D);
			gd.DC.OMSetDepthStencilState(dss2D);
			gd.DC.OMSetBlendState(bsAlpha, oneCol);

			st.Draw(Matrix4x4.Identity, textProj);

			//change back to 3D
			changeLess.mProjection	=Matrix4x4.Transpose(gd.GCam.Projection);
			gd.DC.UpdateSubresource<ChangeLess>(changeLess, changeLessBuf);

			gd.Present();

			acts.Clear();
		});

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

	static List<Input.InputAction> UpdateInput(Input inp,
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
				Matrix4x4	rot	=Matrix4x4.CreateRotationX(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
			}
			else if(act.mAction.Equals(MyActions.LightY))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationY(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
			}
			else if(act.mAction.Equals(MyActions.LightZ))
			{
				Matrix4x4	rot	=Matrix4x4.CreateRotationZ(act.mMultiplier);
				lightDir	=Vector3.TransformNormal(lightDir, rot);
			}
		}
	}
}